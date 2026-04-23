<template>
  <div class="coupon-card" :class="{ claimed: coupon.isClaimed }">
    <div class="coupon-left">
      <div class="discount-label">{{ coupon.discountDescription }}</div>
      <div class="min-spend" v-if="coupon.minSpend > 0">
        滿 NT${{ coupon.minSpend }} 可用
      </div>
    </div>
    <div class="coupon-divider">
      <span class="dot top"></span>
      <span class="dot bottom"></span>
    </div>
    <div class="coupon-right">
      <div class="coupon-name">{{ coupon.name }}</div>
      <div class="coupon-expire" v-if="coupon.endDate">
        {{ formatExpire(coupon.endDate) }}
      </div>
      <div class="coupon-remain" v-if="coupon.limitCount">
        剩餘 {{ coupon.limitCount - coupon.receivedCount }} 張
      </div>
      <button
        class="claim-btn"
        :class="{
          'claimed-btn': coupon.isClaimed,
          'login-btn':   !isLoggedIn && !coupon.isClaimed
        }"
        :disabled="coupon.isClaimed || claiming"
        @click="handleClaim"
      >
        <span v-if="claiming" class="spinner-border spinner-border-sm me-1"></span>
        <template v-if="coupon.isClaimed">已領取</template>
        <template v-else-if="isLoggedIn">立即領取</template>
        <template v-else>登入後領取</template>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const props = defineProps({
  coupon: { type: Object, required: true }
})
const emit = defineEmits(['claimed'])

const authStore = useAuthStore()
const { show }  = useToast()
const isLoggedIn = computed(() => authStore.isLoggedIn)
const claiming   = ref(false)

async function handleClaim() {
  if (!isLoggedIn) {
    // 觸發 AuthModal（若存在）
    const modalEl = document.querySelector('#authModal')
    if (modalEl) {
      const { default: bootstrap } = await import('bootstrap')
      bootstrap.Modal.getOrCreateInstance(modalEl).show()
    }
    return
  }

  claiming.value = true
  try {
    const res = await apiFetch(`/Coupons/${props.coupon.id}/Claim`, { method: 'POST' })
    if (res.ok) {
      show('領取成功！已加入您的優惠券', 'success')
      emit('claimed', props.coupon.id)
    } else {
      const err = await res.json()
      show(err.message || '領取失敗', 'error')
    }
  } finally {
    claiming.value = false
  }
}

function formatExpire(dt) {
  const d = new Date(dt)
  const diff = Math.ceil((d - Date.now()) / 86400000)
  if (diff <= 0) return '已過期'
  if (diff <= 3) return `⚠ 僅剩 ${diff} 天`
  return `效期至 ${d.toLocaleDateString('zh-TW')}`
}
</script>

<style scoped>
.coupon-card {
  display: flex; background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.3); border-radius: 14px; overflow: hidden;
  transition: border-color .2s, transform .15s;
}
.coupon-card:hover { border-color: rgba(180,120,30,.6); transform: translateY(-2px); }
.coupon-card.claimed { opacity: .6; }

.coupon-left {
  width: 120px; flex-shrink: 0;
  background: linear-gradient(135deg, rgba(192,57,43,.25), rgba(180,120,30,.2));
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 20px 12px; text-align: center;
}
.discount-label { font-size: 1.25rem; font-weight: 700; color: var(--eat-primary); line-height: 1.2; }
.min-spend { font-size: .72rem; color: var(--eat-text-muted); margin-top: 6px; }

.coupon-divider {
  width: 1px; background: rgba(180,120,30,.2); position: relative;
  display: flex; flex-direction: column; justify-content: space-between; padding: 0;
}
.dot { width: 14px; height: 14px; border-radius: 50%; background: var(--eat-bg); display: block; margin: -7px -7px; }

.coupon-right {
  flex: 1; padding: 16px 20px;
  display: flex; flex-direction: column; justify-content: center; gap: 6px;
}
.coupon-name { font-size: 1rem; color: var(--eat-text-primary); font-weight: 600; }
.coupon-expire { font-size: .78rem; color: var(--eat-text-muted); }
.coupon-remain { font-size: .78rem; color: #c9a96e; }

.claim-btn {
  align-self: flex-start; margin-top: 8px;
  padding: 6px 18px; border-radius: 50px; font-size: .85rem; cursor: pointer;
  border: 1px solid var(--eat-primary); color: var(--eat-primary);
  background: transparent; transition: all .2s;
}
.claim-btn:hover:not(:disabled) { background: var(--eat-primary); color: #fff; }
.claim-btn.claimed-btn { border-color: rgba(255,255,255,.2); color: var(--eat-text-muted); cursor: default; }
.claim-btn:disabled { cursor: not-allowed; }
</style>
