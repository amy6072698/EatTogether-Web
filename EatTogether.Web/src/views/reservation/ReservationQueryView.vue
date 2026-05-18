<template>
  <div class="query-page">
    <div class="container py-5" style="max-width:600px;">
      <h1 class="page-title text-center mb-5">查詢訂位</h1>

      <!-- 查詢表單 -->
      <div class="query-card" v-if="!reservation">
        <p class="hint-text mb-4">請輸入訂位單號與 Email 查詢您的訂位紀錄</p>
        <div class="mb-3">
          <label class="form-label">訂位單號</label>
          <input
            v-model.trim="queryForm.bookingNumber"
            type="text"
            class="form-control"
            placeholder="例：R260311001"
            @keyup.enter="queryReservation"
          />
        </div>
        <div class="mb-4">
          <label class="form-label">Email</label>
          <input
            v-model.trim="queryForm.email"
            type="email"
            class="form-control"
            placeholder="訂位時填寫的 Email"
            @keyup.enter="queryReservation"
          />
        </div>
        <div v-if="queryError" class="alert alert-danger">{{ queryError }}</div>
        <button
          class="btn-eat-primary w-100 py-3"
          :disabled="querying"
          @click="queryReservation"
        >
          <span v-if="querying" class="spinner-border spinner-border-sm me-2"></span>
          查詢
        </button>
      </div>

      <!-- 查詢結果 -->
      <div class="result-card" v-if="reservation">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h4 class="mb-0" style="color:var(--eat-primary)">訂位詳情</h4>
          <span class="badge" :class="statusBadgeClass">{{ reservation.statusText }}</span>
        </div>

        <div class="detail-row">
          <span class="label">訂位單號</span>
          <span class="value booking-num">{{ reservation.bookingNumber }}</span>
        </div>
        <div class="detail-row">
          <span class="label">姓名</span>
          <span class="value">{{ reservation.name }}</span>
        </div>
        <div class="detail-row">
          <span class="label">電話</span>
          <span class="value">{{ reservation.phone }}</span>
        </div>
        <div class="detail-row">
          <span class="label">訂位日期</span>
          <span class="value">{{ formatDate(reservation.reservationDate) }}</span>
        </div>
        <div class="detail-row">
          <span class="label">訂位時間</span>
          <span class="value">{{ formatTime(reservation.reservationDate) }}</span>
        </div>
        <div class="detail-row">
          <span class="label">人數</span>
          <span class="value">大人 {{ reservation.adultsCount }} 位 / 小孩 {{ reservation.childrenCount }} 位</span>
        </div>
        <div class="detail-row" v-if="reservation.remark">
          <span class="label">備註</span>
          <span class="value">{{ reservation.remark }}</span>
        </div>
        <div class="detail-row" v-if="reservation.cancelledAt">
          <span class="label">取消時間</span>
          <span class="value">{{ formatDate(reservation.cancelledAt) }} {{ formatTime(reservation.cancelledAt) }}</span>
        </div>

        <div class="mt-4 d-flex gap-3">
          <button class="btn btn-outline-secondary flex-fill" @click="reservation = null">重新查詢</button>
          <button
            v-if="reservation.canCancel"
            class="btn btn-danger flex-fill"
            :disabled="cancelling"
            @click="showCancelConfirm = true"
          >
            取消訂位
          </button>
        </div>
      </div>

      <!-- 取消確認 Dialog -->
      <div v-if="showCancelConfirm" class="modal-overlay" @click.self="showCancelConfirm = false">
        <div class="confirm-dialog">
          <h5>確認取消訂位？</h5>
          <p class="text-muted">取消後將寄送確認信至您的 Email，此操作無法復原。</p>
          <div class="d-flex gap-3 mt-4">
            <button class="btn btn-outline-secondary flex-fill" @click="showCancelConfirm = false">返回</button>
            <button class="btn btn-danger flex-fill" :disabled="cancelling" @click="cancelReservation">
              <span v-if="cancelling" class="spinner-border spinner-border-sm me-2"></span>
              確認取消
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const { show } = useToast()

const queryForm = ref({ bookingNumber: '', email: '' })
const reservation = ref(null)
const queryError  = ref('')
const querying    = ref(false)
const cancelling  = ref(false)
const showCancelConfirm = ref(false)

const statusBadgeClass = computed(() => reservation.value?.statusBadgeClass ?? 'bg-secondary')

async function queryReservation() {
  if (!queryForm.value.bookingNumber || !queryForm.value.email) {
    queryError.value = '請填寫訂位單號與 Email'
    return
  }
  queryError.value = ''
  querying.value = true
  try {
    const res = await apiFetch(
      `/Reservations/Query?bookingNumber=${encodeURIComponent(queryForm.value.bookingNumber)}&email=${encodeURIComponent(queryForm.value.email)}`
    )
    if (res.ok) {
      reservation.value = await res.json()
    } else if (res.status === 404) {
      queryError.value = '找不到訂位紀錄，請確認訂位單號與 Email'
    } else {
      queryError.value = '查詢失敗，請稍後再試'
    }
  } finally {
    querying.value = false
  }
}

async function cancelReservation() {
  if (!reservation.value) return
  cancelling.value = true
  try {
    const res = await apiFetch(`/Reservations/${reservation.value.id}/Cancel`, {
      method: 'PUT',
      body: JSON.stringify({
        bookingNumber: queryForm.value.bookingNumber,
        email:         queryForm.value.email
      })
    })
    if (res.ok) {
      show('訂位已取消，取消確認信已寄出', 'success')
      showCancelConfirm.value = false
      reservation.value = null
      queryForm.value = { bookingNumber: '', email: '' }
    } else {
      const err = await res.json()
      show(err.message || '取消失敗，請稍後再試', 'error')
      showCancelConfirm.value = false
    }
  } finally {
    cancelling.value = false
  }
}

function formatDate(dt) {
  return new Date(dt).toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function formatTime(dt) {
  return new Date(dt).toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.query-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }
.hint-text { color: var(--eat-text-muted); text-align: center; }

.query-card, .result-card {
  background: rgba(255,255,255,.04); border: 1px solid rgba(180,120,30,.15);
  border-radius: 20px; padding: 40px;
}
.form-label { color: var(--eat-text-secondary); font-size: .9rem; }
.form-control {
  background: rgba(255,255,255,.06); border: 1px solid rgba(180,120,30,.25);
  color: var(--eat-text-primary); border-radius: 8px;
}
.form-control:focus { background: rgba(255,255,255,.08); border-color: var(--eat-primary); box-shadow: none; color: var(--eat-text-primary); }

.detail-row {
  display: flex; gap: 16px; padding: 12px 0;
  border-bottom: 1px solid rgba(255,255,255,.06);
}
.detail-row:last-of-type { border-bottom: none; }
.label { width: 110px; flex-shrink: 0; color: var(--eat-text-muted); font-size: .9rem; }
.value { color: var(--eat-text-primary); flex: 1; }
.booking-num { font-weight: 700; letter-spacing: 2px; color: var(--eat-primary); font-size: 1.1rem; }

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
