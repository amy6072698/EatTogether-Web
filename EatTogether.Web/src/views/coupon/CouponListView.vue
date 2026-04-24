<template>
  <div class="coupon-list-page">
    <div class="container py-5">
      <h1 class="page-title text-center mb-2">優惠券專區</h1>
      <p class="page-subtitle text-center mb-5">領取專屬優惠，享受更多折扣</p>

      <!-- 分類篩選 -->
      <div class="filter-bar mb-4">
        <button
          v-for="f in filters"
          :key="f.key"
          class="filter-btn"
          :class="{ active: activeFilter === f.key }"
          @click="activeFilter = f.key"
        >
          {{ f.label }}
        </button>
        <label class="filter-toggle ms-auto">
          <input type="checkbox" v-model="birthdayOnly" />
          <span>生日專屬</span>
        </label>
      </div>

      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border" style="color:var(--eat-primary)"></div>
      </div>

      <div v-else-if="!filteredCoupons.length" class="empty-state">
        <p>目前沒有符合條件的優惠券</p>
      </div>

      <div v-else class="coupon-grid">
        <CouponCard
          v-for="c in filteredCoupons"
          :key="c.id"
          :coupon="c"
          @claimed="onClaimed"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import CouponCard from '@/components/coupon/CouponCard.vue'
import apiFetch from '@/utils/apiFetch.js'

const coupons      = ref([])
const loading      = ref(true)
const activeFilter = ref('all')
const birthdayOnly = ref(false)

const filters = [
  { key: 'all',    label: '全部' },
  { key: '0',      label: '折金額' },
  { key: '1',      label: '折百分比' },
]

const filteredCoupons = computed(() => {
  let list = coupons.value
  if (activeFilter.value !== 'all')
    list = list.filter(c => String(c.discountType) === activeFilter.value)
  if (birthdayOnly.value)
    list = list.filter(c => c.code?.startsWith('BDAY'))
  return list
})

async function fetchCoupons() {
  loading.value = true
  try {
    const res = await apiFetch('/Coupons')
    if (res.ok) coupons.value = await res.json()
  } finally {
    loading.value = false
  }
}

function onClaimed(couponId) {
  const idx = coupons.value.findIndex(c => c.id === couponId)
  if (idx !== -1) coupons.value[idx] = { ...coupons.value[idx], isClaimed: true }
}

onMounted(fetchCoupons)
</script>

<style scoped>
.coupon-list-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }
.page-subtitle { color: var(--eat-text-muted); }

.filter-bar {
  display: flex; align-items: center; gap: 10px; flex-wrap: wrap;
}
.filter-btn {
  padding: 6px 18px; border-radius: 50px; font-size: .9rem; cursor: pointer;
  border: 1px solid rgba(180,120,30,.3); color: var(--eat-text-secondary);
  background: transparent; transition: all .2s;
}
.filter-btn:hover, .filter-btn.active {
  border-color: var(--eat-primary); color: var(--eat-primary);
  background: rgba(192,57,43,.1);
}
.filter-toggle {
  display: flex; align-items: center; gap: 8px;
  color: var(--eat-text-secondary); font-size: .9rem; cursor: pointer;
}
.filter-toggle input { accent-color: var(--eat-primary); }

.coupon-grid { display: flex; flex-direction: column; gap: 14px; max-width: 700px; margin: 0 auto; }

.empty-state { text-align: center; padding: 60px; color: var(--eat-text-muted); }
</style>
