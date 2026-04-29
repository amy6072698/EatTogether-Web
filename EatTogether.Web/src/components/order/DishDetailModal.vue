<template>
    <div v-if="dish" @click.self="$emit('close')" class="modal-wrap open">
        <div class="modal-box">
            <div class="detail-img-wrap">
                <img :src="dish.imageUrl" :alt="dish.productName" />
                <button
                    @click="$emit('close')"
                    class="close-btn absolute top-4 right-4 z-10 w-8 h-8 rounded-full backdrop-blur-sm flex items-center justify-center text-lg transition-colors"
                >
                    ✕
                </button>
            </div>
            <div class="detail-body">
                <!-- 第一排：餐點名稱（左）+ 標籤（右） -->
                <div class="detail-row-1">
                    <h2 class="font-headline detail-name italic" style="color: #e3c76b">
                        {{ dish.productName }}
                    </h2>
                    <div class="detail-tags">
                        <span v-for="tag in tags" :key="tag.key" :class="['badge', tag.cls]">{{
                            tag.label
                        }}</span>
                    </div>
                </div>
                <!-- 第二排：描述 -->
                <p
                    class="font-body detail-desc italic leading-relaxed"
                    style="color: rgba(249, 221, 211, 0.7)"
                >
                    {{ dish.description }}
                </p>
                <!-- 第三排：價格（左）+ 數量控制（右） -->
                <div class="detail-row-3">
                    <span class="font-label detail-price" style="color: #d5b478"
                        >NT$ {{ dish.unitPrice?.toLocaleString() }}</span
                    >
                    <div class="detail-qty">
                        <button class="qty-btn" @click="localQty = Math.max(1, localQty - 1)">
                            −
                        </button>
                        <span class="font-label detail-qty-num" style="color: #f9ddd3">{{
                            localQty
                        }}</span>
                        <button class="qty-btn" @click="localQty++">+</button>
                    </div>
                </div>
                <!-- 備註欄 -->
                <div class="detail-note-wrap">
                    <textarea
                        v-model="note"
                        class="font-body detail-note resize-none"
                        rows="2"
                        placeholder="備註 : 過敏食材、特殊需求…"
                    ></textarea>
                </div>
                <!-- 底部：加入 / 確認修改 按鈕 -->
                <button @click="onConfirm" class="submit-btn font-label detail-submit uppercase">
                    {{ editMode ? '確認修改' : '加入本桌訂單' }}
                </button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
    dish: { type: Object, default: null },
    initialQty: { type: Number, default: 1 },
    initialNote: { type: String, default: '' },
    editMode: { type: Boolean, default: false },
})
const emit = defineEmits(['close', 'confirm'])

const localQty = ref(props.initialQty)
const note = ref(props.initialNote)

// 每次 dish / initialQty / initialNote 任一改變時同步（含編輯模式帶入備註）
watch([() => props.dish, () => props.initialQty, () => props.initialNote], ([, qty, initNote]) => {
    localQty.value = qty || 1
    note.value = initNote || ''
})

const tags = computed(() => {
    if (!props.dish) return []
    const t = []
    if (props.dish.isRecommended) t.push({ key: 'rec', cls: 'badge-new', label: '推薦' })
    if (props.dish.isPopular) t.push({ key: 'pop', cls: 'badge-chef', label: '主廚特選' })
    if (props.dish.isVegetarian) t.push({ key: 'veg', cls: 'badge-veg', label: '素食' })
    if (props.dish.spicyLevel > 0) t.push({ key: 'spicy', cls: 'badge-spicy', label: '辣味' })
    return t
})

function onConfirm() {
    emit('confirm', props.dish, localQty.value, note.value)
}
</script>

<style scoped>
.modal-wrap {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(24, 11, 6, 0.87);
    backdrop-filter: blur(6px);
    z-index: 1050;
    align-items: center;
    justify-content: center;
}
.modal-wrap.open {
    display: flex;
}
.modal-box {
    background: #2b1c16;
    width: min(580px, 95vw);
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.6);
}
.detail-img-wrap {
    position: relative;
    height: 240px;
    overflow: hidden;
}
.detail-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}
.detail-img-wrap::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, #2b1c16 0%, transparent 55%);
}
.close-btn {
    background: rgba(24, 11, 6, 0.7);
    border: 1px solid rgba(77, 70, 58, 0.4);
    color: rgba(208, 197, 181, 0.8);
}
.close-btn:hover {
    color: #e3c76b;
}
.qty-btn {
    width: 28px;
    height: 28px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
}
.qty-btn:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
.submit-btn {
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    display: block;
}
.submit-btn:hover {
    filter: brightness(1.1);
}
.submit-btn:active {
    transform: scale(0.97);
}

/* ── 內容區整體 ── */
.detail-body {
    display: flex;
    flex-direction: column;
    gap: 0;
    padding: 1.25rem 2rem 0;
}

/* 第一排：名稱＋標籤同行 */
.detail-row-1 {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 0.75rem;
    margin-bottom: 0.6rem;
}
.detail-name {
    font-size: 1.6rem;
    line-height: 1.2;
    margin: 0;
    flex: 1;
    min-width: 0;
}
.detail-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    flex-shrink: 0;
    align-items: flex-start;
    padding-top: 0.2rem;
}

/* 第二排：描述 */
.detail-desc {
    font-size: 1rem;
    margin: 0 0 1.1rem;
}

/* 第三排：價格＋數量並排 */
.detail-row-3 {
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid rgba(77, 70, 58, 0.3);
    border-bottom: 1px solid rgba(77, 70, 58, 0.3);
    padding: 0.75rem 0;
    margin-bottom: 1rem;
}
.detail-price {
    font-size: 1.2rem;
    letter-spacing: 0.08em;
}
.detail-qty {
    display: flex;
    align-items: center;
    gap: 0.6rem;
}
.detail-qty-num {
    font-size: 1.1rem;
    width: 2rem;
    text-align: center;
}

/* 備註欄 */
.detail-note-wrap {
    margin-bottom: 0.85rem;
}
.detail-note-label {
    display: block;
    font-size: 0.7rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    margin-bottom: 0.35rem;
}
.detail-note {
    width: 100%;
    background: rgba(24, 11, 6, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.55);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.95rem;
    padding: 0.55rem 0.75rem;
    outline: none;
    transition: border-color 0.25s;
    box-sizing: border-box;
}
.detail-note:focus {
    border-color: rgba(227, 199, 107, 0.5);
}
.detail-note::placeholder {
    color: rgba(208, 197, 181, 0.28);
}

/* 底部按鈕 */
.detail-submit {
    width: 100%;
    display: block;
    padding: 0.9rem 0;
    font-size: 0.85rem;
    letter-spacing: 0.28em;
    margin-bottom: 1.5rem;
    border: none;
    cursor: pointer;
    transition:
        filter 0.2s,
        transform 0.15s;
}
.badge-new {
    background: rgba(227, 199, 107, 0.18);
    border: 1px solid rgba(227, 199, 107, 0.45);
    color: #e3c76b;
}
.badge-veg {
    background: rgba(66, 49, 43, 0.5);
    border: 1px solid rgba(120, 180, 80, 0.4);
    color: #a3d977;
}
.badge-spicy {
    background: rgba(147, 0, 10, 0.25);
    border: 1px solid rgba(255, 100, 80, 0.4);
    color: #ffb4ab;
}
.badge-chef {
    background: rgba(93, 69, 20, 0.4);
    border: 1px solid rgba(228, 194, 133, 0.35);
    color: #e4c285;
}
.badge {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.58rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    padding: 0.18rem 0.5rem;
}
.font-headline {
    font-family: 'Noto Serif TC', serif;
}
.font-label {
    font-family: 'Work Sans', sans-serif;
}
.font-body {
    font-family: 'Newsreader', serif;
}
</style>
