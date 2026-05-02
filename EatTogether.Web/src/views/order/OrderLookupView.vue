<template>
    <div class="ol-page">
        <div class="ol-container">
            <!-- ══ 頁面標題 ══ -->
            <div class="ol-hero">
                <div class="ol-icon">
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="28"
                        height="28"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                        />
                    </svg>
                </div>
                <div>
                    <h1 class="font-headline ol-title">訂單查詢</h1>
                    <p class="font-label ol-subtitle">查詢今日外帶訂單進度</p>
                </div>
            </div>

            <!-- ══ 查詢表單 ══ -->
            <div class="ol-form-card">
                <div class="ol-fields">
                    <div class="ol-field">
                        <label class="font-label ol-label">訂單編號</label>
                        <input
                            v-model="lkOrderNum"
                            class="ol-input font-label"
                            placeholder="例：20260503-0001"
                            @input="(lkName = '')((lkPhone = ''))"
                            @keyup.enter="doLookup"
                        />
                    </div>
                    <div class="ol-divider-or"><span class="font-label">或</span></div>
                    <div class="ol-field">
                        <label class="font-label ol-label">訂購人姓名</label>
                        <input
                            v-model="lkName"
                            class="ol-input font-label"
                            placeholder="請輸入完整姓名"
                            @input="(lkOrderNum = '')((lkPhone = ''))"
                            @keyup.enter="doLookup"
                        />
                    </div>
                    <div class="ol-divider-or"><span class="font-label">或</span></div>
                    <div class="ol-field">
                        <label class="font-label ol-label">電話號碼</label>
                        <input
                            v-model="lkPhone"
                            class="ol-input font-label"
                            placeholder="請輸入完整電話"
                            @input="(lkOrderNum = '')((lkName = ''))"
                            @keyup.enter="doLookup"
                        />
                    </div>
                </div>

                <button
                    class="ol-search-btn font-label"
                    :disabled="!canSearch || searching"
                    @click="doLookup"
                >
                    <svg
                        v-if="!searching"
                        xmlns="http://www.w3.org/2000/svg"
                        width="14"
                        height="14"
                        fill="currentColor"
                        viewBox="0 0 16 16"
                    >
                        <path
                            d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                        />
                    </svg>
                    {{ searching ? '查詢中…' : '查詢' }}
                </button>

                <p v-if="error" class="font-label ol-error">{{ error }}</p>
            </div>

            <!-- ══ 結果列表 ══ -->
            <template v-if="searched && !searching">
                <div v-if="results.length > 0" class="ol-results">
                    <p class="font-label ol-results-hint">
                        找到 {{ results.length }} 筆當日未完成訂單，點擊查看進度
                    </p>

                    <div
                        v-for="r in results"
                        :key="r.orderNumber"
                        class="ol-result-card"
                        :class="{ 'is-expanded': selectedOrder?.orderNumber === r.orderNumber }"
                        @click="selectOrder(r)"
                    >
                        <!-- 摘要列 -->
                        <div class="ol-rc-summary">
                            <div class="ol-rc-left">
                                <span class="font-label ol-rc-num">{{ r.orderNumber }}</span>
                                <span class="font-label ol-rc-name">{{ r.customerName }}</span>
                            </div>
                            <div class="ol-rc-right">
                                <span v-if="r.pickupTime" class="font-label ol-rc-pickup"
                                    >取餐 {{ r.pickupTime }}</span
                                >
                                <span class="font-label ol-rc-total"
                                    >NT$ {{ r.totalAmount.toLocaleString() }}</span
                                >
                                <svg
                                    class="ol-rc-chevron"
                                    :class="{
                                        rotated: selectedOrder?.orderNumber === r.orderNumber,
                                    }"
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="12"
                                    height="12"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z"
                                    />
                                </svg>
                            </div>
                        </div>

                        <!-- 品項預覽（收合時） -->
                        <div
                            v-if="selectedOrder?.orderNumber !== r.orderNumber"
                            class="font-label ol-rc-items-preview"
                        >
                            {{
                                r.items
                                    .slice(0, 3)
                                    .map((i) => i.productName + (i.qty > 1 ? ` ×${i.qty}` : ''))
                                    .join('、')
                            }}{{ r.items.length > 3 ? '…' : '' }}
                        </div>

                        <!-- 展開：完整訂單進度 -->
                        <Transition name="ol-expand">
                            <div
                                v-if="selectedOrder?.orderNumber === r.orderNumber"
                                class="ol-detail"
                                @click.stop
                            >
                                <!-- 進度條 -->
                                <div class="ol-progress">
                                    <div :class="['ol-prog-step', 'ol-prog-done']">
                                        <div class="ol-prog-dot">
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                width="14"
                                                height="14"
                                                fill="currentColor"
                                                viewBox="0 0 16 16"
                                            >
                                                <path
                                                    d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"
                                                />
                                            </svg>
                                        </div>
                                        <span class="font-label ol-prog-lbl">訂單已接收</span>
                                    </div>
                                    <div class="ol-prog-line ol-prog-line-lit"></div>
                                    <div :class="['ol-prog-step', 'ol-prog-active']">
                                        <div class="ol-prog-dot"><span>2</span></div>
                                        <span class="font-label ol-prog-lbl">餐點製作中</span>
                                    </div>
                                    <div class="ol-prog-line"></div>
                                    <div class="ol-prog-step">
                                        <div class="ol-prog-dot"><span>3</span></div>
                                        <span class="font-label ol-prog-lbl">餐點已完成</span>
                                    </div>
                                </div>

                                <!-- 預計取餐時間 -->
                                <div v-if="r.pickupTime" class="ol-pickup-banner">
                                    <span
                                        class="font-label"
                                        style="color: rgba(208, 197, 181, 0.5); font-size: 0.8rem"
                                        >預計取餐時間</span
                                    >
                                    <span
                                        class="font-headline"
                                        style="
                                            font-size: 1.25rem;
                                            font-style: italic;
                                            color: #e3c76b;
                                        "
                                        >今日 {{ r.pickupTime }}</span
                                    >
                                </div>

                                <!-- 品項明細 -->
                                <div class="ol-items">
                                    <div v-for="(item, idx) in r.items" :key="idx" class="ol-item">
                                        <div class="ol-item-left">
                                            <div
                                                v-if="item.isSetMeal"
                                                class="setmeal-badge-sm font-label"
                                            >
                                                🍱 套餐
                                            </div>
                                            <span class="font-body ol-item-name">{{
                                                item.productName
                                            }}</span>
                                            <div v-if="item.subItems?.length" class="ol-subitems">
                                                <span
                                                    v-for="s in item.subItems"
                                                    :key="s"
                                                    class="ol-subitem font-label"
                                                    >{{ s }}</span
                                                >
                                            </div>
                                            <span
                                                v-if="item.itemNote"
                                                class="font-label ol-item-note"
                                                >{{ item.itemNote }}</span
                                            >
                                        </div>
                                        <div class="ol-item-right">
                                            <span
                                                class="font-label"
                                                style="
                                                    color: rgba(208, 197, 181, 0.5);
                                                    font-size: 0.82rem;
                                                "
                                                >× {{ item.qty }}</span
                                            >
                                            <span
                                                class="font-label"
                                                style="color: #d5b478; font-size: 0.9rem"
                                                >NT$
                                                {{
                                                    (item.unitPrice * item.qty).toLocaleString()
                                                }}</span
                                            >
                                        </div>
                                    </div>
                                </div>

                                <div class="feather-divider" style="margin: 0.75rem 0"></div>

                                <!-- 金額 / 付款 / 取餐 -->
                                <div class="ol-meta-rows">
                                    <div class="ol-meta-row">
                                        <span
                                            class="font-label"
                                            style="color: rgba(208, 197, 181, 0.5)"
                                            >金額總計</span
                                        >
                                        <span
                                            class="font-label"
                                            style="color: #e3c76b; font-size: 1rem"
                                            >NT$ {{ r.totalAmount.toLocaleString() }}</span
                                        >
                                    </div>
                                    <div class="ol-meta-row">
                                        <span
                                            class="font-label"
                                            style="color: rgba(208, 197, 181, 0.5)"
                                            >付款方式</span
                                        >
                                        <span class="font-label" style="color: #f9ddd3"
                                            >現場付款</span
                                        >
                                    </div>
                                    <div class="ol-meta-row">
                                        <span
                                            class="font-label"
                                            style="color: rgba(208, 197, 181, 0.5)"
                                            >取餐方式</span
                                        >
                                        <span class="font-label" style="color: #f9ddd3"
                                            >臨櫃自取</span
                                        >
                                    </div>
                                    <div v-if="r.note" class="ol-meta-row">
                                        <span
                                            class="font-label"
                                            style="color: rgba(208, 197, 181, 0.5)"
                                            >備註</span
                                        >
                                        <span class="font-label" style="color: #f9ddd3">{{
                                            r.note
                                        }}</span>
                                    </div>
                                </div>

                                <!-- 店家資訊 -->
                                <div class="ol-store-row">
                                    <span
                                        class="font-label"
                                        style="color: rgba(208, 197, 181, 0.45); font-size: 0.8rem"
                                        >台北市大安區慢食街 88 號</span
                                    >
                                    <a
                                        href="tel:0223456789"
                                        class="font-label ol-store-tel"
                                        @click.stop
                                        >Tel: (02) 2345-6789</a
                                    >
                                </div>
                            </div>
                        </Transition>
                    </div>
                </div>

                <!-- 查無結果 -->
                <div v-else class="ol-empty">
                    <div class="ol-empty-icon">🔍</div>
                    <p class="font-body ol-empty-text">查無當日未完成外帶訂單</p>
                    <p class="font-label ol-empty-hint">請確認資料是否填寫完整，或訂單已完成</p>
                    <RouterLink to="/takeout" class="font-label ol-back-link"
                        >前往外帶點餐 →</RouterLink
                    >
                </div>
            </template>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { RouterLink } from 'vue-router'
import apiFetch from '@/utils/apiFetch'

// ── 查詢欄位 ────────────────────────────────────────
const lkOrderNum = ref('')
const lkName = ref('')
const lkPhone = ref('')
const searching = ref(false)
const searched = ref(false)
const error = ref('')
const results = ref([])
const selectedOrder = ref(null)

const canSearch = computed(() => {
    const filled = [lkOrderNum.value.trim(), lkName.value.trim(), lkPhone.value.trim()].filter(
        Boolean
    )
    return filled.length === 1
})

async function doLookup() {
    if (!canSearch.value || searching.value) return
    searching.value = true
    error.value = ''
    results.value = []
    selectedOrder.value = null
    searched.value = false

    try {
        let type, q
        if (lkOrderNum.value.trim()) {
            type = 'orderNumber'
            q = lkOrderNum.value.trim()
        } else if (lkName.value.trim()) {
            type = 'name'
            q = lkName.value.trim()
        } else {
            type = 'phone'
            q = lkPhone.value.trim()
        }

        const res = await apiFetch(
            `/Orders/Lookup?type=${encodeURIComponent(type)}&q=${encodeURIComponent(q)}`
        )
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        results.value = await res.json()
    } catch (e) {
        error.value = '查詢失敗，請稍後再試'
    } finally {
        searching.value = false
        searched.value = true
    }
}

function selectOrder(r) {
    if (selectedOrder.value?.orderNumber === r.orderNumber) {
        selectedOrder.value = null // 再次點擊收合
    } else {
        selectedOrder.value = r
    }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Serif+TC:wght@400;700&family=Newsreader:ital,wght@0,400;0,600;1,400&family=Work+Sans:wght@300;400&display=swap');

/* ── 頁面容器 ── */
.ol-page {
    min-height: 100vh;
    background: #1e100b;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
    padding-top: 90px; /* navbar 高度 */
    padding-bottom: 4rem;
}
.ol-container {
    max-width: 640px;
    margin: 0 auto;
    padding: 2.5rem 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 1.75rem;
}

/* ── Hero ── */
.ol-hero {
    display: flex;
    align-items: center;
    gap: 1rem;
}
.ol-icon {
    width: 56px;
    height: 56px;
    border-radius: 50%;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    display: flex;
    align-items: center;
    justify-content: center;
    color: #3b2f00;
    flex-shrink: 0;
}
.ol-title {
    font-size: 1.9rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0;
}
.ol-subtitle {
    font-size: 0.78rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.45);
    margin: 0.2rem 0 0;
}

/* ── 表單卡片 ── */
.ol-form-card {
    background: #271813;
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 0.65rem;
    padding: 1.75rem 1.75rem 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}
.ol-fields {
    display: flex;
    flex-direction: column;
    gap: 0;
}
.ol-field {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
}
.ol-label {
    font-size: 0.8rem;
    letter-spacing: 0.12em;
    color: rgba(208, 197, 181, 0.55);
}
.ol-input {
    background: rgba(24, 11, 6, 0.6);
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 0.3rem;
    padding: 0.7rem 1rem;
    color: #f9ddd3;
    font-size: 0.95rem;
    outline: none;
    transition: border-color 0.2s;
}
.ol-input:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.ol-input::placeholder {
    color: rgba(208, 197, 181, 0.28);
}
.ol-divider-or {
    text-align: center;
    padding: 0.5rem 0;
    font-size: 0.72rem;
    color: rgba(208, 197, 181, 0.3);
    letter-spacing: 0.2em;
}
.ol-search-btn {
    width: 100%;
    padding: 0.85rem;
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.3rem;
    font-size: 0.92rem;
    letter-spacing: 0.2em;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    transition:
        filter 0.2s,
        opacity 0.2s;
}
.ol-search-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}
.ol-search-btn:not(:disabled):hover {
    filter: brightness(1.08);
}
.ol-error {
    color: #e07070;
    font-size: 0.85rem;
    text-align: center;
    margin: 0;
}

/* ── 結果區 ── */
.ol-results {
    display: flex;
    flex-direction: column;
    gap: 0.3rem;
}
.ol-results-hint {
    font-size: 0.78rem;
    color: rgba(208, 197, 181, 0.4);
    margin: 0 0 0.65rem;
    letter-spacing: 0.06em;
}

/* 結果卡片 */
.ol-result-card {
    background: #271813;
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.55rem;
    overflow: hidden;
    cursor: pointer;
    transition: border-color 0.2s;
}
.ol-result-card:hover,
.ol-result-card.is-expanded {
    border-color: rgba(227, 199, 107, 0.4);
}
.ol-rc-summary {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.9rem 1.1rem;
    gap: 1rem;
}
.ol-rc-left {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
}
.ol-rc-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.15rem;
    flex-shrink: 0;
}
.ol-rc-num {
    font-size: 0.92rem;
    color: #e3c76b;
    letter-spacing: 0.1em;
    font-family: 'Work Sans', sans-serif;
}
.ol-rc-name {
    font-size: 0.82rem;
    color: rgba(208, 197, 181, 0.7);
}
.ol-rc-pickup {
    font-size: 0.75rem;
    color: rgba(208, 197, 181, 0.5);
}
.ol-rc-total {
    font-size: 0.88rem;
    color: #d5b478;
}
.ol-rc-chevron {
    color: rgba(208, 197, 181, 0.4);
    transition: transform 0.25s;
    flex-shrink: 0;
    margin-top: 0.2rem;
}
.ol-rc-chevron.rotated {
    transform: rotate(180deg);
}
.ol-rc-items-preview {
    padding: 0 1.1rem 0.85rem;
    font-size: 0.78rem;
    color: rgba(208, 197, 181, 0.4);
    line-height: 1.4;
}

/* 展開詳情 */
.ol-expand-enter-active {
    transition: all 0.28s ease;
}
.ol-expand-leave-active {
    transition: all 0.2s ease;
}
.ol-expand-enter-from,
.ol-expand-leave-to {
    opacity: 0;
    transform: translateY(-6px);
}

.ol-detail {
    padding: 0 1.1rem 1.1rem;
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    display: flex;
    flex-direction: column;
    gap: 0.85rem;
    padding-top: 1rem;
}

/* 進度條 */
.ol-progress {
    display: flex;
    align-items: center;
    width: 100%;
}
.ol-prog-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.35rem;
    flex: 0 0 auto;
    min-width: 80px;
}
.ol-prog-dot {
    width: 38px;
    height: 38px;
    border-radius: 50%;
    border: 2px solid rgba(77, 70, 58, 0.4);
    background: #1e100b;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.88rem;
    color: rgba(208, 197, 181, 0.4);
    transition: all 0.4s;
}
.ol-prog-done .ol-prog-dot {
    background: linear-gradient(135deg, #e3c76b, #c6ab53);
    border-color: #e3c76b;
    color: #3b2f00;
}
.ol-prog-active .ol-prog-dot {
    border-color: #e3c76b;
    background: rgba(227, 199, 107, 0.1);
    color: #e3c76b;
    animation: ol-pulse 1.4s ease-in-out infinite;
}
@keyframes ol-pulse {
    0%,
    100% {
        box-shadow: 0 0 0 0 rgba(227, 199, 107, 0.4);
    }
    50% {
        box-shadow: 0 0 0 8px rgba(227, 199, 107, 0);
    }
}
.ol-prog-lbl {
    font-size: 0.7rem;
    letter-spacing: 0.06em;
    color: rgba(208, 197, 181, 0.4);
    white-space: nowrap;
}
.ol-prog-done .ol-prog-lbl,
.ol-prog-active .ol-prog-lbl {
    color: #e3c76b;
}
.ol-prog-line {
    flex: 1;
    height: 2px;
    background: rgba(77, 70, 58, 0.4);
    margin-bottom: 1.4rem;
    transition: background 0.4s;
}
.ol-prog-line-lit {
    background: linear-gradient(90deg, #e3c76b, #c6ab53);
}

/* 預計取餐時間 */
.ol-pickup-banner {
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(227, 199, 107, 0.2);
    border-radius: 0.4rem;
    padding: 0.75rem 1rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

/* 品項 */
.ol-items {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
}
.ol-item {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 1rem;
    padding: 0.55rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.18);
}
.ol-item:last-child {
    border-bottom: none;
}
.ol-item-left {
    flex: 1;
}
.ol-item-right {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.15rem;
    flex-shrink: 0;
}
.ol-item-name {
    font-size: 0.95rem;
    color: #f9ddd3;
}
.ol-item-note {
    font-size: 0.78rem;
    color: rgba(208, 197, 181, 0.4);
    margin-top: 0.1rem;
    display: block;
}
.ol-subitems {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    margin-top: 0.25rem;
}
.ol-subitem {
    font-size: 0.72rem;
    color: rgba(208, 197, 181, 0.5);
    background: rgba(77, 70, 58, 0.25);
    padding: 0.1rem 0.45rem;
    border-radius: 999px;
}
.setmeal-badge-sm {
    font-size: 0.68rem;
    color: #e3c76b;
    margin-bottom: 0.1rem;
}

/* 金額 Meta */
.ol-meta-rows {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}
.ol-meta-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.88rem;
}

/* 店家資訊 */
.ol-store-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.5rem;
    padding-top: 0.25rem;
}
.ol-store-tel {
    font-size: 0.82rem;
    color: #e3c76b;
    text-decoration: none;
}
.ol-store-tel:hover {
    text-decoration: underline;
}

/* ── 查無結果 ── */
.ol-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.55rem;
    padding: 3rem 1rem;
    text-align: center;
}
.ol-empty-icon {
    font-size: 2.5rem;
}
.ol-empty-text {
    font-size: 1.1rem;
    color: rgba(208, 197, 181, 0.6);
    margin: 0;
}
.ol-empty-hint {
    font-size: 0.8rem;
    color: rgba(208, 197, 181, 0.38);
    margin: 0;
}
.ol-back-link {
    font-size: 0.82rem;
    color: #e3c76b;
    text-decoration: none;
    margin-top: 0.5rem;
}
.ol-back-link:hover {
    text-decoration: underline;
}

/* feather-divider（共用） */
.feather-divider {
    height: 1px;
    background: linear-gradient(90deg, transparent, #e4c285 50%, transparent);
    position: relative;
}
.feather-divider::after {
    content: '◈';
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
    color: #e4c285;
    background: #271813;
    padding: 0 0.35rem;
    font-size: 0.7rem;
}

/* ── 手機版 ── */
@media (max-width: 600px) {
    .ol-container {
        padding: 1.5rem 1rem;
        gap: 1.25rem;
    }
    .ol-title {
        font-size: 1.5rem;
    }
    .ol-prog-step {
        min-width: 62px;
    }
    .ol-prog-dot {
        width: 32px;
        height: 32px;
    }
    .ol-prog-lbl {
        font-size: 0.62rem;
    }
}
</style>
