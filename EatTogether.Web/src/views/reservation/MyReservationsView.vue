<template>
  <div class="my-reservations-page">
    <div class="container py-5">
      <h1 class="page-title mb-5">我的訂位</h1>

      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border" style="color:var(--eat-primary)"></div>
      </div>

      <template v-else>
        <!-- 即將到來 -->
        <section class="mb-5">
          <h5 class="section-title">
            <i class="bi bi-calendar-check me-2"></i>即將到來
            <span class="count-badge">{{ upcoming.length }}</span>
          </h5>

          <div v-if="!upcoming.length" class="empty-state">
            <p>目前沒有即將到來的訂位</p>
            <router-link to="/reservation" class="btn-eat-primary">立即訂位</router-link>
          </div>

          <div v-else class="reservation-list">
            <div
              v-for="r in upcoming"
              :key="r.id"
              class="reservation-card"
            >
              <div class="card-header-row">
                <span class="booking-number">{{ r.bookingNumber }}</span>
                <span class="badge" :class="r.statusBadgeClass">{{ r.statusText }}</span>
              </div>
              <div class="card-body-row">
                <div class="info-item">
                  <i class="bi bi-calendar3"></i>
                  {{ formatDate(r.reservationDate) }}
                </div>
                <div class="info-item">
                  <i class="bi bi-people"></i>
                  大人 {{ r.adultsCount }} 位 / 小孩 {{ r.childrenCount }} 位
                </div>
              </div>
              <div class="card-actions" v-if="canCancel(r)">
                <button
                  class="btn btn-sm btn-outline-danger"
                  @click="confirmCancel(r)"
                >
                  取消訂位
                </button>
              </div>
            </div>
          </div>
        </section>

        <!-- 歷史紀錄 -->
        <section>
          <h5 class="section-title">
            <i class="bi bi-clock-history me-2"></i>歷史紀錄
            <span class="count-badge">{{ history.length }}</span>
          </h5>

          <div v-if="!history.length" class="empty-state">
            <p>尚無歷史訂位紀錄</p>
          </div>

          <div v-else class="reservation-list">
            <div
              v-for="r in history"
              :key="r.id"
              class="reservation-card history"
            >
              <div class="card-header-row">
                <span class="booking-number">{{ r.bookingNumber }}</span>
                <span class="badge" :class="r.statusBadgeClass">{{ r.statusText }}</span>
              </div>
              <div class="card-body-row">
                <div class="info-item">
                  <i class="bi bi-calendar3"></i>
                  {{ formatDate(r.reservationDate) }}
                </div>
                <div class="info-item">
                  <i class="bi bi-people"></i>
                  大人 {{ r.adultsCount }} 位 / 小孩 {{ r.childrenCount }} 位
                </div>
                <div class="info-item" v-if="r.cancelledAt">
                  <i class="bi bi-x-circle"></i>
                  取消於 {{ formatDate(r.cancelledAt) }}
                </div>
              </div>
            </div>
          </div>
        </section>
      </template>
    </div>

    <!-- 取消確認 Dialog -->
    <div v-if="cancelTarget" class="modal-overlay" @click.self="cancelTarget = null">
      <div class="confirm-dialog">
        <h5>確認取消訂位？</h5>
        <p class="text-muted">
          <strong>{{ cancelTarget?.bookingNumber }}</strong><br>
          {{ formatDate(cancelTarget?.reservationDate) }}
        </p>
        <p class="text-muted small">取消後將寄送確認信，此操作無法復原。</p>
        <div class="d-flex gap-3 mt-4">
          <button class="btn btn-outline-secondary flex-fill" @click="cancelTarget = null">返回</button>
          <button class="btn btn-danger flex-fill" :disabled="cancelling" @click="doCancel">
            <span v-if="cancelling" class="spinner-border spinner-border-sm me-2"></span>
            確認取消
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const { show } = useToast()

const reservations = ref([])
const loading      = ref(true)
const cancelTarget = ref(null)
const cancelling   = ref(false)

const upcoming = computed(() =>
  reservations.value.filter(r =>
    (r.status === 0 || r.status === 1) &&
    new Date(r.reservationDate) >= new Date()
  )
)

const history = computed(() =>
  reservations.value.filter(r =>
    r.status === 2 || r.status === 3 ||
    new Date(r.reservationDate) < new Date()
  )
)

function canCancel(r) {
  return r.status === 0 &&
    new Date(r.reservationDate) > new Date(Date.now() + 60 * 60 * 1000)
}

async function fetchReservations() {
  loading.value = true
  try {
    const res = await apiFetch('/Reservations/My')
    if (res.ok) reservations.value = await res.json()
  } finally {
    loading.value = false
  }
}

function confirmCancel(r) {
  cancelTarget.value = r
}

async function doCancel() {
  if (!cancelTarget.value) return
  cancelling.value = true
  try {
    const res = await apiFetch(`/Reservations/${cancelTarget.value.id}/Cancel`, {
      method: 'PUT',
      body:   JSON.stringify({})
    })
    if (res.ok) {
      show('訂位已取消', 'success')
      cancelTarget.value = null
      await fetchReservations()
    } else {
      const err = await res.json()
      show(err.message || '取消失敗', 'error')
      cancelTarget.value = null
    }
  } finally {
    cancelling.value = false
  }
}

function formatDate(dt) {
  if (!dt) return ''
  return new Date(dt).toLocaleString('zh-TW', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}

onMounted(fetchReservations)
</script>

<style scoped>
.my-reservations-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }

.section-title {
  color: var(--eat-text-secondary); font-size: 1rem;
  letter-spacing: .08em; text-transform: uppercase;
  display: flex; align-items: center; gap: 8px; margin-bottom: 16px;
}
.count-badge {
  background: rgba(180,120,30,.2); color: var(--eat-primary);
  border-radius: 50px; padding: 2px 10px; font-size: .8rem;
}

.reservation-list { display: flex; flex-direction: column; gap: 12px; }
.reservation-card {
  background: rgba(255,255,255,.04); border: 1px solid rgba(180,120,30,.15);
  border-radius: 14px; padding: 20px;
  transition: border-color .2s;
}
.reservation-card:hover { border-color: rgba(180,120,30,.4); }
.reservation-card.history { opacity: .75; }

.card-header-row {
  display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;
}
.booking-number { font-weight: 700; letter-spacing: 2px; color: var(--eat-primary); }

.card-body-row { display: flex; flex-wrap: wrap; gap: 16px; margin-bottom: 12px; }
.info-item { display: flex; align-items: center; gap: 8px; color: var(--eat-text-secondary); font-size: .9rem; }

.card-actions { display: flex; justify-content: flex-end; }

.empty-state {
  text-align: center; padding: 32px; color: var(--eat-text-muted);
  display: flex; flex-direction: column; align-items: center; gap: 16px;
}

.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.7);
  display: flex; align-items: center; justify-content: center; z-index: 9999;
}
.confirm-dialog {
  background: #1e120b; border: 1px solid rgba(180,120,30,.3);
  border-radius: 16px; padding: 32px; max-width: 380px; width: 90%; text-align: center;
}
.confirm-dialog h5 { color: var(--eat-primary); margin-bottom: 12px; }
</style>
