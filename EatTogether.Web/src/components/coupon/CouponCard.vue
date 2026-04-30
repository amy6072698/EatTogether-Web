<template>
  <div class="coupon-card" :class="{ claimed: coupon.isClaimed }">

    <!-- 左側主視覺 -->
    <div class="coupon-left">
      <div class="discount-value">{{ coupon.discountDescription }}</div>
      <div v-if="coupon.minSpend > 0" class="min-spend">滿 ${{ coupon.minSpend }}</div>
      <div class="coupon-code-badge">{{ coupon.code }}</div>
    </div>

    <!-- 鋸齒切邊 -->
    <div class="perforation">
      <div class="notch top"></div>
      <div class="dash-line"></div>
      <div class="notch bottom"></div>
    </div>

    <!-- 右側資訊 -->
    <div class="coupon-right">
      <div class="coupon-name">{{ coupon.name }}</div>

      <div class="coupon-meta">
        <span v-if="coupon.endDate" :class="{ 'expiry-warn': isExpiringSoon(coupon.endDate) }">
          <i class="bi bi-clock me-1"></i>{{ formatExpire(coupon.endDate) }}
        </span>
        <span v-if="coupon.limitCount" class="remain">
          <i class="bi bi-people me-1"></i>剩 {{ coupon.limitCount - coupon.receivedCount }} 張
        </span>
      </div>

      <button
        class="claim-btn"
        :class="{
          'claim-btn--done': coupon.isClaimed,
          'claim-btn--loading': claiming
        }"
        :disabled="coupon.isClaimed || claiming"
        @click="handleClaim"
      >
        <span v-if="claiming" class="spinner-border spinner-border-sm me-1" style="width:.85rem;height:.85rem;border-width:2px"></span>
        <i v-else-if="coupon.isClaimed" class="bi bi-check-circle me-1"></i>
        <i v-else class="bi bi-bag-plus me-1"></i>
        <template v-if="coupon.isClaimed">已領取</template>
        <template v-else-if="!claiming && isLoggedIn">立即領取</template>
        <template v-else-if="!claiming">登入後領取</template>
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

const authStore  = useAuthStore()
const { show }   = useToast()
const isLoggedIn = computed(() => authStore.isLoggedIn)
const claiming   = ref(false)

async function handleClaim() {
  if (!isLoggedIn.value) {
    // 未登入：開啟登入 Modal，登入後由 CouponListView 重新抓券
    const modalEl = document.querySelector('#authModal')
    if (modalEl) {
      const { Modal } = await import('bootstrap')
      Modal.getOrCreateInstance(modalEl).show()
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
      const err = await res.json().catch(() => ({}))
      show(err.message || '領取失敗', 'error')
    }
  } catch {
    show('網路錯誤，請稍後再試', 'error')
  } finally {
    claiming.value = false
  }
}

function isExpiringSoon(dt) {
  return Math.ceil((new Date(dt) - Date.now()) / 86400000) <= 3
}

function formatExpire(dt) {
  const diff = Math.ceil((new Date(dt) - Date.now()) / 86400000)
  if (diff <= 0) return '已過期'
  if (diff <= 3) return `僅剩 ${diff} 天`
  return new Date(dt).toLocaleDateString('zh-TW')
}
</script>

<style scoped>
/* ── 外框 ──────────────────────────────────────────── */
.coupon-card {
  display: flex;
  align-items: stretch;
  background: var(--eat-surface-container);
  border: 1px solid var(--eat-outline-variant);
  border-radius: var(--eat-radius-lg);
  overflow: hidden;
  transition: transform .2s ease, box-shadow .2s ease, border-color .2s;
  position: relative;
}
.coupon-card::before {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: var(--eat-radius-lg);
  background: linear-gradient(135deg, rgba(227,199,107,.04) 0%, transparent 60%);
  pointer-events: none;
}
.coupon-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 36px rgba(0,0,0,.45), 0 0 0 1px rgba(227,199,107,.2);
  border-color: rgba(227,199,107,.35);
}
.coupon-card.claimed {
  opacity: .5;
  filter: grayscale(.3);
}
.coupon-card.claimed:hover {
  transform: none;
  box-shadow: none;
}

/* ── 左側色塊 ─────────────────────────────────────── */
.coupon-left {
  width: 130px;
  flex-shrink: 0;
  background: linear-gradient(150deg, #3d1a08 0%, #2b1c16 50%, #1e100b 100%);
  border-right: none;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 1.25rem 0.75rem;
  text-align: center;
  position: relative;
}
.coupon-left::after {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse at 50% 40%, rgba(227,199,107,.12) 0%, transparent 65%);
  pointer-events: none;
}

.discount-value {
  font-family: var(--font-headline);
  font-size: 1.35rem;
  font-weight: 700;
  color: var(--eat-primary);
  line-height: 1.2;
  position: relative;
  z-index: 1;
}
.min-spend {
  font-family: var(--font-label);
  font-size: .65rem;
  letter-spacing: .08em;
  color: var(--eat-on-surface-variant);
  position: relative;
  z-index: 1;
}
.coupon-code-badge {
  margin-top: 4px;
  font-family: var(--font-label);
  font-size: .6rem;
  letter-spacing: .12em;
  color: rgba(227,199,107,.5);
  background: rgba(227,199,107,.06);
  border: 1px solid rgba(227,199,107,.15);
  border-radius: 2px;
  padding: .15rem .45rem;
  position: relative;
  z-index: 1;
}

/* ── 鋸齒分隔 ─────────────────────────────────────── */
.perforation {
  width: 18px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  background: var(--eat-surface-container);
  position: relative;
}
.notch {
  width: 18px;
  height: 9px;
  background: var(--eat-surface);
  flex-shrink: 0;
}
.notch.top {
  border-radius: 0 0 9px 9px;
  border-bottom: 1px solid var(--eat-outline-variant);
  border-left: 1px solid var(--eat-outline-variant);
  border-right: 1px solid var(--eat-outline-variant);
}
.notch.bottom {
  border-radius: 9px 9px 0 0;
  border-top: 1px solid var(--eat-outline-variant);
  border-left: 1px solid var(--eat-outline-variant);
  border-right: 1px solid var(--eat-outline-variant);
}
.dash-line {
  flex: 1;
  width: 1px;
  background: repeating-linear-gradient(
    to bottom,
    var(--eat-outline-variant),
    var(--eat-outline-variant) 4px,
    transparent 4px,
    transparent 8px
  );
  margin: 0 auto;
}

/* ── 右側資訊 ─────────────────────────────────────── */
.coupon-right {
  flex: 1;
  padding: 1.1rem 1.25rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 8px;
}
.coupon-name {
  font-family: var(--font-headline);
  font-size: .95rem;
  color: var(--eat-on-surface);
  font-weight: 600;
  line-height: 1.4;
}
.coupon-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-size: .75rem;
  color: var(--eat-on-surface-variant);
}
.expiry-warn { color: #e09a50; }
.remain { color: var(--eat-secondary); }

/* ── 領取按鈕 ─────────────────────────────────────── */
.claim-btn {
  align-self: flex-start;
  padding: .4rem 1.1rem;
  border-radius: var(--eat-radius-sm);
  font-family: var(--font-label);
  font-size: .78rem;
  letter-spacing: .1em;
  cursor: pointer;
  border: 1px solid var(--eat-primary);
  color: var(--eat-primary);
  background: transparent;
  transition: all .2s;
  display: flex;
  align-items: center;
}
.claim-btn:hover:not(:disabled) {
  background: var(--eat-primary);
  color: var(--eat-on-primary);
  box-shadow: 0 4px 16px rgba(227,199,107,.3);
}
.claim-btn--done {
  border-color: var(--eat-outline-variant);
  color: var(--eat-on-surface-variant);
  cursor: default;
}
.claim-btn:disabled { cursor: not-allowed; }
</style>
