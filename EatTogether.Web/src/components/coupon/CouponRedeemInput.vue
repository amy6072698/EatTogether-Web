<template>
  <div class="coupon-redeem">
    <label class="redeem-label">優惠折扣碼</label>
    <div class="redeem-row">
      <input
        v-model.trim="code"
        type="text"
        class="form-control redeem-input"
        :class="{ 'is-valid': validated?.isValid, 'is-invalid': validated && !validated.isValid }"
        placeholder="輸入折扣碼"
        :disabled="!!applied"
        @input="clearValidated"
      />
      <button
        class="btn-eat-primary redeem-btn"
        :disabled="!code || validating || !!applied"
        @click="applyCode"
      >
        <span v-if="validating" class="spinner-border spinner-border-sm"></span>
        <span v-else>{{ applied ? '已套用' : '套用' }}</span>
      </button>
    </div>

    <!-- 驗證回饋 -->
    <div v-if="validated" class="redeem-feedback" :class="validated.isValid ? 'success' : 'error'">
      <i :class="validated.isValid ? 'bi bi-check-circle-fill' : 'bi bi-x-circle-fill'"></i>
      {{ validated.message }}
    </div>

    <!-- 移除按鈕 -->
    <div v-if="applied" class="applied-bar">
      <span>已套用折扣 <strong style="color:var(--eat-primary)">NT${{ applied.discount }}</strong></span>
      <button class="remove-btn" @click="removeCode">移除</button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiFetch from '@/utils/apiFetch.js'

const props = defineProps({
  orderAmount: { type: Number, required: true }
})
const emit = defineEmits(['coupon-applied', 'coupon-removed'])

const code      = ref('')
const validating = ref(false)
const validated  = ref(null)
const applied    = ref(null)

async function applyCode() {
  if (!code.value) return
  validating.value = true
  try {
    const res = await apiFetch('/Coupons/Validate', {
      method: 'POST',
      body: JSON.stringify({ code: code.value, orderAmount: props.orderAmount })
    })
    if (res.ok) {
      validated.value = await res.json()
      if (validated.value.isValid) {
        applied.value = validated.value
        emit('coupon-applied', {
          couponId: validated.value.couponId,
          discount: validated.value.discount,
          code:     code.value
        })
      }
    }
  } finally {
    validating.value = false
  }
}

function removeCode() {
  code.value     = ''
  validated.value = null
  applied.value   = null
  emit('coupon-removed')
}

function clearValidated() {
  if (!applied.value) validated.value = null
}
</script>

<style scoped>
.coupon-redeem { display: flex; flex-direction: column; gap: 8px; }
.redeem-label { font-size: .88rem; color: var(--eat-text-secondary); }
.redeem-row { display: flex; gap: 8px; }
.redeem-input {
  flex: 1; background: rgba(255,255,255,.06); border: 1px solid rgba(180,120,30,.25);
  color: var(--eat-text-primary); border-radius: 8px;
}
.redeem-input:focus { background: rgba(255,255,255,.08); border-color: var(--eat-primary); box-shadow: none; color: var(--eat-text-primary); }
.redeem-btn { padding: 8px 20px; white-space: nowrap; border-radius: 8px; }

.redeem-feedback {
  font-size: .85rem; display: flex; align-items: center; gap: 6px; padding: 6px 10px; border-radius: 8px;
}
.redeem-feedback.success { color: #3cb371; background: rgba(60,179,113,.1); }
.redeem-feedback.error   { color: #c0392b; background: rgba(192,57,43,.1); }

.applied-bar {
  display: flex; justify-content: space-between; align-items: center;
  background: rgba(60,179,113,.1); border: 1px solid rgba(60,179,113,.3);
  border-radius: 8px; padding: 8px 14px; font-size: .88rem; color: var(--eat-text-secondary);
}
.remove-btn {
  background: none; border: none; color: #c0392b; font-size: .82rem;
  cursor: pointer; padding: 0;
}
.remove-btn:hover { text-decoration: underline; }
</style>
