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
      <div
        v-for="mc in history"
        :key="mc.orderNumber"
        class="usage-card"
        :class="{ expanded: expandedOrder === mc.orderNumber }"
      >
        <!-- ── 主列（點擊展開） ── -->
        <div class="usage-main" @click="toggleExpand(mc.orderNumber)">

          <!-- 左側折扣色塊 -->
          <div class="coupon-left">
            <span class="coupon-discount">{{ mc.discountDescription }}</span>
          </div>

          <!-- 鋸齒分隔 -->
          <div class="coupon-tear"></div>

          <!-- 右側資訊 -->
          <div class="coupon-right">
            <div class="d-flex justify-content-between align-items-start gap-2">
              <div class="eat-body fw-semibold mb-1">{{ mc.couponName }}</div>
              <i
                class="bi expand-icon"
                :class="expandedOrder === mc.orderNumber ? 'bi-chevron-up' : 'bi-chevron-down'"
              ></i>
            </div>
            <div class="eat-label mb-1">
              <i class="bi bi-tag me-1"></i>{{ mc.code }}
            </div>
            <div class="eat-body-muted" style="font-size:.82rem">
              <i class="bi bi-clock me-1"></i>使用於 {{ formatDate(mc.usedDate) }}
            </div>
          </div>
        </div>

        <!-- ── 展開的訂單明細 ── -->
        <Transition name="detail-slide">
          <div v-if="expandedOrder === mc.orderNumber" class="order-detail">
            <div class="detail-divider"></div>
            <div class="detail-body">
              <div class="detail-title">
                <i class="bi bi-receipt me-2"></i>訂單明細
              </div>
              <div class="detail-grid">
                <div class="detail-item">
                  <span class="detail-label">訂單編號</span>
                  <span class="detail-value order-num">{{ mc.orderNumber }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">消費日期</span>
                  <span class="detail-value">{{ formatDate(mc.usedDate) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">付款方式</span>
                  <span class="detail-value">{{ mc.payMethod || '—' }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">原始金額</span>
                  <span class="detail-value">${{ mc.originalAmount?.toLocaleString() }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">折扣金額</span>
                  <span class="detail-value" style="color:#e05a5a">
                    − ${{ mc.discountAmount?.toLocaleString() }}
                  </span>
                </div>
                <div class="detail-item total">
                  <span class="detail-label">實付金額</span>
                  <span class="detail-value" style="color:var(--eat-primary);font-weight:700">
                    ${{ mc.totalAmount?.toLocaleString() }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </Transition>

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
const expandedOrder = ref(null)  // 目前展開的訂單編號

function toggleExpand(orderNumber) {
  expandedOrder.value = expandedOrder.value === orderNumber ? null : orderNumber
}

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
/* ── 使用卡片 ── */
.usage-card {
  background: var(--eat-surface-container);
  border: 1px solid rgba(180,120,30,.18);
  border-radius: var(--eat-radius-lg);
  overflow: hidden;
  transition: border-color .2s;
}
.usage-card.expanded {
  border-color: rgba(227,199,107,.35);
}

/* ── 主列 ── */
.usage-main {
  display: flex;
  align-items: stretch;
  cursor: pointer;
  transition: background .15s;
}
.usage-main:hover {
  background: rgba(255,255,255,.02);
}

/* 左側折扣 */
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

/* 鋸齒線 */
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

/* 右側資訊 */
.coupon-right {
  flex: 1;
  padding: 1rem 1.25rem;
}
.expand-icon {
  font-size: .85rem;
  color: rgba(226,210,185,.4);
  flex-shrink: 0;
  margin-top: .1rem;
  transition: color .2s;
}
.usage-main:hover .expand-icon {
  color: var(--eat-primary);
}

/* ── 展開訂單明細 ── */
.detail-divider {
  border: none;
  border-top: 1px dashed rgba(180,120,30,.2);
  margin: 0 1rem;
}
.detail-body {
  padding: 1rem 1.25rem 1.25rem;
}
.detail-title {
  font-family: var(--font-label);
  font-size: .75rem;
  letter-spacing: .1em;
  text-transform: uppercase;
  color: rgba(226,210,185,.45);
  margin-bottom: .85rem;
}
.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: .6rem .5rem;
}
.detail-item {
  display: flex;
  flex-direction: column;
  gap: .15rem;
}
.detail-item.total {
  grid-column: 1 / -1;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  padding-top: .5rem;
  border-top: 1px solid rgba(180,120,30,.15);
  margin-top: .25rem;
}
.detail-label {
  font-size: .75rem;
  color: rgba(226,210,185,.45);
  letter-spacing: .03em;
}
.detail-value {
  font-size: .9rem;
  color: rgba(226,210,185,.85);
}
.order-num {
  font-family: var(--font-display, monospace);
  font-size: .82rem;
  letter-spacing: .05em;
  color: rgba(226,210,185,.6);
}

/* ── 展開動畫 ── */
.detail-slide-enter-active,
.detail-slide-leave-active {
  transition: max-height .25s ease, opacity .2s ease;
  overflow: hidden;
  max-height: 200px;
}
.detail-slide-enter-from,
.detail-slide-leave-to {
  max-height: 0;
  opacity: 0;
}
</style>
