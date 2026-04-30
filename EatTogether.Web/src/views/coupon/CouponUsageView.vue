<template>
  <div class="d-flex flex-column gap-4">

    <div>
      <h2 class="eat-h3 fst-normal mb-1">優惠券明細</h2>
      <p class="eat-body-muted mb-0">歷史折扣使用紀錄</p>
    </div>

    <!-- 載入中 -->
    <LoadingSpinner v-if="loading" message="載入紀錄中..." />

    <!-- 空狀態 -->
    <div v-else-if="!history.length" class="card-eat p-5 text-center">
      <p class="eat-body-muted mb-3">尚無優惠券使用紀錄</p>
      <Button variant="primary" :to="{ name: 'CouponList' }">前往領取優惠券</Button>
    </div>

    <!-- 明細列表 -->
    <div v-else class="d-flex flex-column gap-3">
      <div v-for="mc in history" :key="mc.id" class="card-eat coupon-row">
        <!-- 左側折扣色塊 -->
        <div class="coupon-left">
          <span class="coupon-discount">{{ mc.discountDescription }}</span>
        </div>

        <!-- 鋸齒分隔 -->
        <div class="coupon-tear"></div>

        <!-- 右側資訊 -->
        <div class="coupon-right">
          <div class="eat-body fw-semibold mb-1">{{ mc.couponName }}</div>
          <div class="eat-label mb-1">
            <i class="bi bi-tag me-1"></i>{{ mc.code }}
          </div>
          <div class="eat-body-muted" style="font-size:.82rem">
            <i class="bi bi-clock me-1"></i>使用於 {{ formatDate(mc.usedDate) }}
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const history = ref([])
const loading = ref(true)

function formatDate(dt) {
  if (!dt) return ''
  return new Date(dt).toLocaleString('zh-TW', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}

async function fetchHistory() {
  loading.value = true
  try {
    const res = await apiFetch('/Coupons/My/History')
    if (res.ok) history.value = await res.json()
  } finally {
    loading.value = false
  }
}

onMounted(fetchHistory)
</script>

<style scoped>
.coupon-row {
  display: flex;
  align-items: stretch;
  overflow: hidden;
  padding: 0;
}

.coupon-left {
  width: 100px;
  flex-shrink: 0;
  background: linear-gradient(135deg, rgba(192,57,43,.15), rgba(227,199,107,.1));
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
.coupon-discount {
  font-family: var(--font-label);
  font-size: 1rem;
  font-weight: 700;
  color: var(--eat-primary);
  text-align: center;
  line-height: 1.3;
}

.coupon-tear {
  width: 1px;
  background: repeating-linear-gradient(
    to bottom,
    transparent,
    transparent 4px,
    var(--eat-outline-variant) 4px,
    var(--eat-outline-variant) 8px
  );
  flex-shrink: 0;
}

.coupon-right {
  flex: 1;
  padding: 1rem 1.25rem;
}
</style>
