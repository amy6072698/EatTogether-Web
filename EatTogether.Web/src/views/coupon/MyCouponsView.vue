<template>
  <div class="d-flex flex-column gap-4">

    <div class="d-flex justify-content-between align-items-start flex-wrap gap-3">
      <div>
        <h2 class="eat-h3 fst-normal mb-1">我的優惠券</h2>
        <p class="eat-body-muted mb-0">管理您的專屬折扣券</p>
      </div>
      <Button variant="secondary" :to="{ name: 'CouponList' }">
        <i class="bi bi-gift me-1"></i>領取更多優惠券
      </Button>
    </div>

    <!-- 優惠碼輸入 -->
    <div class="card-eat p-4">
      <div class="eat-label mb-2">輸入優惠碼</div>
      <div class="d-flex gap-2">
        <input
          v-model.trim="claimCode"
          type="text"
          class="form-control"
          placeholder="請輸入優惠碼"
          :disabled="claiming"
          @keyup.enter="claimByCode"
        />
        <Button variant="primary" :loading="claiming" :disabled="!claimCode" @click="claimByCode">
          兌換
        </Button>
      </div>
      <div v-if="claimMsg" class="mt-2" :class="claimSuccess ? 'text-eat-primary' : 'text-danger'" style="font-size:.85rem">
        <i :class="claimSuccess ? 'bi bi-check-circle me-1' : 'bi bi-x-circle me-1'"></i>{{ claimMsg }}
      </div>
    </div>

    <!-- Tab 篩選 -->
    <div class="d-flex gap-2 flex-wrap">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        class="chip-eat"
        :class="{ active: activeTab === tab.key }"
        @click="activeTab = tab.key"
      >
        {{ tab.label }}
        <span class="ms-1" style="opacity:.7">({{ tabCount(tab.key) }})</span>
      </button>
    </div>

    <!-- 載入中 -->
    <LoadingSpinner v-if="loading" message="載入優惠券中..." />

    <!-- 空狀態 -->
    <div v-else-if="!filteredCoupons.length" class="card-eat p-5 text-center">
      <template v-if="activeTab === 'usable'">
        <p class="eat-body-muted mb-3">目前沒有可使用的優惠券</p>
        <Button variant="primary" :to="{ name: 'CouponList' }">前往領取</Button>
      </template>
      <p v-else class="eat-body-muted mb-0">暫無紀錄</p>
    </div>

    <!-- 券列表 -->
    <div v-else class="d-flex flex-column gap-3">
      <div
        v-for="mc in filteredCoupons"
        :key="mc.id"
        class="card-eat coupon-row"
        :class="{ dimmed: activeTab !== 'usable' }"
      >
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
            <template v-if="activeTab === 'usable'">{{ daysLeft(mc.endDate) }}</template>
            <template v-else-if="activeTab === 'used'">使用於 {{ formatDate(mc.usedDate) }}</template>
            <template v-else>效期 {{ formatDate(mc.endDate) }} 已過期</template>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import Button from '@/components/common/Button.vue'

const myCoupons   = ref([])
const loading     = ref(true)
const activeTab   = ref('usable')
const claimCode   = ref('')
const claiming    = ref(false)
const claimMsg    = ref('')
const claimSuccess = ref(false)

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

async function claimByCode() {
  if (!claimCode.value) return
  claiming.value = true
  claimMsg.value = ''
  try {
    const res = await apiFetch('/Coupons/ClaimByCode', {
      method: 'POST',
      body: JSON.stringify({ code: claimCode.value })
    })
    const data = await res.json().catch(() => ({}))
    if (res.ok) {
      claimSuccess.value = true
      claimMsg.value = data.message || '領取成功！'
      claimCode.value = ''
      await fetchMyCoupons()
    } else {
      claimSuccess.value = false
      claimMsg.value = data.message || '兌換失敗，請確認優惠碼是否正確'
    }
  } catch {
    claimSuccess.value = false
    claimMsg.value = '網路錯誤，請稍後再試'
  } finally {
    claiming.value = false
  }
}

function daysLeft(dt) {
  if (!dt) return ''
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
.coupon-row {
  display: flex;
  align-items: stretch;
  overflow: hidden;
  padding: 0;
}
.coupon-row.dimmed { opacity: .6; }

.coupon-left {
  width: 100px;
  flex-shrink: 0;
  background: linear-gradient(135deg, rgba(192,57,43,.2), rgba(227,199,107,.12));
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
