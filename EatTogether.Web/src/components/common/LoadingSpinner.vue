<template>
    <!-- 全螢幕覆蓋版 -->
    <div v-if="fullscreen" class="spinner-fullscreen">
        <div class="spinner-candle">
            <div class="candle-flame"></div>
            <span class="candle-text">{{ message || '稍候片刻' }}</span>
        </div>
    </div>

    <!-- 行內版 -->
    <div v-else class="spinner-inline">
        <div
            class="spinner-border-eat"
            :class="{
                'spinner-sm': size === 'sm',
                'spinner-lg': size === 'lg',
            }"
        ></div>
        <span v-if="message" class="spinner-msg">{{ message }}</span>
    </div>
</template>

<!--
  LoadingSpinner.vue — 載入中動畫元件
  支援行內版（局部區塊）與全螢幕覆蓋版兩種模式。

  Props:
    message    (String, default '')     — 說明文字；全螢幕版無文字時預設顯示「稍候片刻」
    fullscreen (Boolean, default false) — true 時全螢幕半透明覆蓋＋燭火動畫
    size       (String, default 'md')   — 行內版尺寸：'sm' | 'md' | 'lg'

  行內版（局部 loading）:
    <LoadingSpinner />
    <LoadingSpinner size="sm" />
    <LoadingSpinner message="資料載入中..." />

  全螢幕版（整頁 loading）:
    <LoadingSpinner :fullscreen="isLoading" />
    <LoadingSpinner :fullscreen="isLoading" message="送出中，請稍候" />
-->
<script setup>
defineProps({
    message: { type: String, default: '' },
    fullscreen: { type: Boolean, default: false },
    size: { type: String, default: 'md' },
})
</script>

<style scoped>
.spinner-inline {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.spinner-msg {
    font-family: var(--font-label);
    font-size: 0.8rem;
    color: var(--eat-on-surface-variant);
}

.spinner-fullscreen {
    position: fixed;
    inset: 0;
    z-index: 2000;
    background: rgba(30, 16, 11, 0.85);
    backdrop-filter: blur(8px);
    -webkit-backdrop-filter: blur(8px);
    display: flex;
    align-items: center;
    justify-content: center;
}
</style>
