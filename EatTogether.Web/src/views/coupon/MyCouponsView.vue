<template>
  <div class="my-coupons-page">
    <div class="container py-5">
      <h1 class="page-title mb-5">我的優惠券</h1>

      <!-- Tab -->
      <div class="tab-bar mb-4">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: activeTab === tab.key }"
          @click="activeTab = tab.key"
        >
          {{ tab.label }}
          <span class="tab-count">{{ tabCount(tab.key) }}</span>
        </button>
      </div>

      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border" style="color:var(--eat-primary)"></div>
      </div>

      <div v-else-if="!filteredCoupons.length" class="empty-state">
        <template v-if="activeTab === 'usable'">
          <p>目前沒有可使用的優惠券</p>
          <router-link to="/coupons" class="btn-eat-primary">前往領取</router-link>
        </template>
        <p v-else>暫無紀錄</p>
      </div>

      <div v-else class="coupon-list">
        <div
          v-for="mc in filteredCoupons"
          :key="mc.id"
          class="my-coupon-card"
          :class="activeTab"
        >
          <div class="mc-left">
            <div class="mc-discount">{{ mc.discountDescription }}</div>
          </div>
          <div class="mc-divider">
            <span class="dot top"></span>
            <span class="dot bottom"></span>
          </div>
          <div class="mc-right">
            <div class="mc-name">{{ mc.couponName }}</div>
            <div class="mc-code">
              <i class="bi bi-tag me-1"></i>{{ mc.code }}
            </div>
            <div class="mc-expire" v-if="mc.endDate">
              <template v-if="activeTab === 'usable'">
                {{ daysLeft(mc.endDate) }}
              </template>
              <template v-else-if="activeTab === 'used'">
                使用於 {{ formatDate(mc.usedDate) }}
              </template>
              <template v-else>
                效期 {{ formatDate(mc.endDate) }} 已過期
              </template>
            </div>
          </div>
          <div class="mc-status-badge">
            <span class="badge" :class="mc.statusBadgeClass">{{ mc.statusText }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'

const myCoupons = ref([])
const loading   = ref(true)
const activeTab = ref('usable')

const tabs = [
  { key: 'usable',  label: '可使用' },
  { key: 'used',    label: '已使用' },
  { key: 'expired', label: '已過期' },
]

const filteredCoupons = computed(() => {
  const now = new Date()
  return myCoupons.value.filter(mc => {
    const isExpired = mc.endDate && new Date(mc.endDate) < now
    if (activeTab.value === 'usable')  return !mc.isUsed && !isExpired
    if (activeTab.value === 'used')    return mc.isUsed
    if (activeTab.value === 'expired') return !mc.isUsed && isExpired
    return false
  })
})

function tabCount(key) {
  const now = new Date()
  return myCoupons.value.filter(mc => {
    const isExpired = mc.endDate && new Date(mc.endDate) < now
    if (key === 'usable')  return !mc.isUsed && !isExpired
    if (key === 'used')    return mc.isUsed
    if (key === 'expired') return !mc.isUsed && isExpired
    return false
  }).length
}

function daysLeft(dt) {
  const diff = Math.ceil((new Date(dt) - Date.now()) / 86400000)
  if (diff <= 0) return '今日到期'
  if (diff <= 3) return `⚠ 僅剩 ${diff} 天到期`
  return `效期至 ${new Date(dt).toLocaleDateString('zh-TW')}`
}

function formatDate(dt) {
  if (!dt) return ''
  return new Date(dt).toLocaleDateString('zh-TW')
}

async function fetchMyCoupons() {
  loading.value = true
  try {
    const res = await apiFetch('/Coupons/My')
    if (res.ok) myCoupons.value = await res.json()
  } finally {
    loading.value = false
  }
}

onMounted(fetchMyCoupons)
</script>

<style scoped>
.my-coupons-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }

.tab-bar { display: flex; gap: 8px; }
.tab-btn {
  padding: 8px 20px; border-radius: 50px; cursor: pointer;
  border: 1px solid rgba(180,120,30,.25); color: var(--eat-text-secondary);
  background: transparent; font-size: .9rem; transition: all .2s;
  display: flex; align-items: center; gap: 6px;
}
.tab-btn:hover, .tab-btn.active {
  border-color: var(--eat-primary); color: var(--eat-primary);
  background: rgba(192,57,43,.08);
}
.tab-count {
  background: rgba(180,120,30,.2); color: var(--eat-primary);
  border-radius: 50px; padding: 1px 8px; font-size: .75rem;
}

.coupon-list { display: flex; flex-direction: column; gap: 12px; max-width: 680px; }
.my-coupon-card {
  display: flex; align-items: stretch;
  background: rgba(255,255,255,.04); border: 1px solid rgba(180,120,30,.2);
  border-radius: 14px; overflow: hidden;
}
.my-coupon-card.used, .my-coupon-card.expired { opacity: .65; }

.mc-left {
  width: 100px; flex-shrink: 0;
  background: linear-gradient(135deg, rgba(192,57,43,.2), rgba(180,120,30,.15));
  display: flex; align-items: center; justify-content: center; padding: 16px;
}
.mc-discount { font-size: 1.1rem; font-weight: 700; color: var(--eat-primary); text-align: center; }

.mc-divider {
  width: 1px; background: rgba(180,120,30,.15); position: relative;
  display: flex; flex-direction: column; justify-content: space-between;
}
.dot { width: 12px; height: 12px; border-radius: 50%; background: var(--eat-bg); display: block; margin: -6px -6px; }

.mc-right { flex: 1; padding: 14px 16px; display: flex; flex-direction: column; gap: 4px; }
.mc-name { font-size: .95rem; color: var(--eat-text-primary); font-weight: 600; }
.mc-code { font-size: .82rem; color: var(--eat-primary); letter-spacing: 1px; }
.mc-expire { font-size: .78rem; color: var(--eat-text-muted); }

.mc-status-badge { padding: 14px 12px; display: flex; align-items: flex-start; }

.empty-state {
  text-align: center; padding: 60px; color: var(--eat-text-muted);
  display: flex; flex-direction: column; align-items: center; gap: 16px;
}
</style>
