<template>
  <div class="coupon-usage-page">
    <div class="container py-5">
      <h1 class="page-title mb-2">優惠券使用明細</h1>
      <p class="page-subtitle mb-5">歷史折扣使用紀錄</p>

      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border" style="color:var(--eat-primary)"></div>
      </div>

      <div v-else-if="!history.length" class="empty-state">
        <p>尚無優惠券使用紀錄</p>
        <router-link to="/coupons" class="btn-eat-primary">前往領取優惠券</router-link>
      </div>

      <div v-else class="usage-list">
        <div v-for="mc in history" :key="mc.id" class="usage-card">
          <div class="usage-left">
            <div class="usage-discount">{{ mc.discountDescription }}</div>
          </div>
          <div class="usage-right">
            <div class="usage-name">{{ mc.couponName }}</div>
            <div class="usage-code"><i class="bi bi-tag me-1"></i>{{ mc.code }}</div>
            <div class="usage-meta">
              <span><i class="bi bi-clock me-1"></i>使用於 {{ formatDate(mc.usedDate) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'

const history = ref([])
const loading = ref(true)

async function fetchHistory() {
  loading.value = true
  try {
    const res = await apiFetch('/Coupons/My/History')
    if (res.ok) history.value = await res.json()
  } finally {
    loading.value = false
  }
}

function formatDate(dt) {
  if (!dt) return ''
  return new Date(dt).toLocaleString('zh-TW', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}

onMounted(fetchHistory)
</script>

<style scoped>
.coupon-usage-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }
.page-subtitle { color: var(--eat-text-muted); }

.usage-list { display: flex; flex-direction: column; gap: 12px; max-width: 680px; }
.usage-card {
  display: flex; gap: 0;
  background: rgba(255,255,255,.04); border: 1px solid rgba(180,120,30,.2);
  border-radius: 14px; overflow: hidden;
}
.usage-left {
  width: 100px; flex-shrink: 0;
  background: rgba(192,57,43,.12);
  display: flex; align-items: center; justify-content: center; padding: 16px;
}
.usage-discount { font-size: 1.1rem; font-weight: 700; color: var(--eat-primary); text-align: center; }
.usage-right { flex: 1; padding: 14px 20px; display: flex; flex-direction: column; gap: 5px; }
.usage-name { font-size: .95rem; color: var(--eat-text-primary); font-weight: 600; }
.usage-code { font-size: .82rem; color: var(--eat-primary); letter-spacing: 1px; }
.usage-meta { font-size: .8rem; color: var(--eat-text-muted); display: flex; gap: 16px; }

.empty-state {
  text-align: center; padding: 60px; color: var(--eat-text-muted);
  display: flex; flex-direction: column; align-items: center; gap: 16px;
}
</style>
