<template>
  <div class="table-status-page">

    <!-- Hero -->
    <div class="status-hero">
      <div class="hero-glow"></div>
      <div class="container position-relative">
        <p class="eat-label mb-2" style="color:var(--eat-secondary)">
          <i class="bi bi-grid-3x3-gap me-2"></i>LIVE TABLE STATUS
        </p>
        <h1 class="eat-h1 fst-normal mb-2">即時桌況</h1>
        <p class="eat-body-muted mb-0">
          目前時間：<span style="color:var(--eat-primary);font-weight:600">{{ currentTimeStr }}</span>
        </p>
      </div>
    </div>

    <div class="container pb-5">

      <!-- 載入中 -->
      <div v-if="loading" class="d-flex justify-content-center py-5">
        <LoadingSpinner message="查詢桌況中..." />
      </div>

      <!-- 桌況卡片 -->
      <div v-else-if="slots.length" class="row g-3 mb-4">
        <div v-for="slot in slots" :key="slot.tableType" class="col-6 col-md-3">
          <div class="seat-card" :class="slot.available > 0 ? 'available' : 'full'">
            <div class="seat-badge" :class="slot.available > 0 ? 'badge-open' : 'badge-full'">
              {{ slot.available > 0 ? '有位' : '已滿' }}
            </div>
            <div class="seat-type">{{ slot.tableType }}</div>
            <div class="seat-count">
              <span class="available-num">{{ slot.available }}</span>
              <span class="total-num"> / {{ slot.total }}</span>
            </div>
            <div class="seat-label">剩餘桌數</div>
          </div>
        </div>
      </div>

      <!-- 非營業時間 -->
      <div v-else class="card-eat p-5 text-center mb-4">
        <i class="bi bi-moon-stars" style="font-size:2.5rem;opacity:.35;color:var(--eat-on-surface-variant)"></i>
        <p class="eat-body-muted mt-3 mb-0">目前非訂位服務時段</p>
      </div>

      <!-- 重新整理 + 訂位按鈕 -->
      <div class="d-flex justify-content-center gap-3 mb-5">
        <Button variant="secondary" @click="fetchStatus" :loading="loading">
          <i class="bi bi-arrow-clockwise me-1"></i>重新整理
        </Button>
        <Button variant="primary" :to="{ name: 'Reservation' }">
          <i class="bi bi-calendar-plus me-2"></i>立即訂位
        </Button>
      </div>


    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const slots   = ref([])
const loading = ref(false)
const currentTimeStr = ref('')

let refreshTimer = null

function getNow() {
  const now = new Date()
  const pad = n => String(n).padStart(2, '0')
  currentTimeStr.value = `${now.getFullYear()}/${pad(now.getMonth()+1)}/${pad(now.getDate())} ${pad(now.getHours())}:${pad(now.getMinutes())}`
  return now
}

function toLocalISO(d) {
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`
}

async function fetchStatus() {
  loading.value = true
  const now = getNow()
  try {
    const res = await apiFetch(`/Reservations/TableAvailability?date=${encodeURIComponent(toLocalISO(now))}`)
    if (res.ok) slots.value = await res.json()
  } catch { /* 忽略網路錯誤 */ }
  finally { loading.value = false }
}

onMounted(() => {
  fetchStatus()
  // 每分鐘自動更新
  refreshTimer = setInterval(fetchStatus, 60_000)
})

onUnmounted(() => {
  clearInterval(refreshTimer)
})
</script>

<style scoped>
.table-status-page {
  min-height: 100vh;
  padding-top: 80px;
  background: var(--eat-bg);
}

/* ── Hero ── */
.status-hero {
  position: relative;
  overflow: hidden;
  padding: 3.5rem 0 2.5rem;
  background: linear-gradient(160deg, #1a0a05 0%, #2b1c16 60%, #1e100b 100%);
  border-bottom: 1px solid var(--eat-outline-variant);
  margin-bottom: 2.5rem;
}
.hero-glow {
  position: absolute;
  top: -60px; left: 50%;
  transform: translateX(-50%);
  width: 600px; height: 300px;
  background: radial-gradient(ellipse, rgba(227,199,107,.08) 0%, transparent 70%);
  pointer-events: none;
}

/* ── 桌況卡片 ── */
.seat-card {
  position: relative;
  border-radius: var(--eat-radius-lg);
  padding: 1.5rem 1rem;
  text-align: center;
  border: 1px solid rgba(180,120,30,.2);
  background: var(--eat-surface-container);
  transition: transform .2s, box-shadow .2s;
}
.seat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 32px rgba(0,0,0,.4);
}
.seat-card.available { border-color: rgba(60,179,113,.5); }
.seat-card.full      { border-color: rgba(192,57,43,.4); opacity: .75; }

.seat-badge {
  position: absolute;
  top: 10px; right: 10px;
  font-family: var(--font-label);
  font-size: .6rem;
  letter-spacing: .1em;
  padding: .15rem .5rem;
  border-radius: 50px;
}
.badge-open { background: rgba(60,179,113,.2);  color: #3cb371; border: 1px solid rgba(60,179,113,.4); }
.badge-full { background: rgba(192,57,43,.2);   color: #e05a50; border: 1px solid rgba(192,57,43,.4); }

.seat-type {
  font-family: var(--font-label);
  font-size: .8rem;
  letter-spacing: .08em;
  color: var(--eat-on-surface-variant);
  margin-bottom: .5rem;
}
.seat-count { line-height: 1; margin-bottom: .4rem; }
.available-num {
  font-family: var(--font-headline);
  font-size: 2.2rem;
  font-weight: 700;
  color: var(--eat-primary);
}
.total-num {
  font-size: 1rem;
  color: var(--eat-on-surface-variant);
}
.seat-label {
  font-size: .72rem;
  color: var(--eat-on-surface-variant);
  opacity: .7;
}

</style>
