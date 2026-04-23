<template>
    <div class="toast-eat-wrap">
        <div
            v-for="toast in toasts"
            :key="toast.id"
            class="toast-eat"
            :class="`toast-${toast.type}`"
            :role="toast.type === 'error' ? 'alert' : 'status'"
            :aria-live="toast.type === 'error' ? 'assertive' : undefined"
        >
            <div class="toast-header">
                <div class="toast-icon">
                    <i :class="icons[toast.type]"></i>
                </div>
                <span class="toast-title">{{ titles[toast.type] }}</span>
                <button
                    type="button"
                    class="btn-close btn-close-white"
                    @click="hide(toast.id)"
                    aria-label="關閉"
                ></button>
            </div>
            <div class="toast-body">{{ toast.message }}</div>
        </div>
    </div>
</template>

<script setup>
import { useToast } from '@/composables/useToast.js'

const { toasts, hide } = useToast()

const icons = {
    success: 'bi bi-check-lg',
    error:   'bi bi-x-lg',
    info:    'bi bi-info-lg',
}

const titles = {
    success: '操作成功',
    error:   '發生錯誤',
    info:    '系統通知',
}
</script>

<style>
.toast-eat-wrap {
    position: fixed;
    bottom: 2rem;
    right: 2rem;
    z-index: 1090;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    pointer-events: none;
}

/* 基底 Toast */
.toast-eat {
    background: var(--eat-surface-container);
    border: 1px solid rgba(77, 70, 58, 0.5);
    border-radius: var(--eat-radius);
    box-shadow: 0 16px 40px rgba(0, 0, 0, 0.55);
    min-width: 300px;
    max-width: 380px;
    overflow: hidden;
    pointer-events: all;
}

/* Toast Header */
.toast-eat .toast-header {
    background: var(--eat-surface-high);
    border-bottom: 1px solid rgba(77, 70, 58, 0.4);
    padding: 0.7rem 1rem;
    display: flex;
    align-items: center;
    gap: 0.6rem;
    color: var(--eat-primary);
}

.toast-eat .toast-header .toast-icon {
    width: 1.5rem;
    height: 1.5rem;
    border-radius: 50%;
    font-size: 0.85rem;
}

.toast-eat .toast-header .toast-icon i {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
}
.toast-eat .toast-header .toast-title {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 0.95rem;
    color: var(--eat-primary);
    flex: 1;
}

.toast-eat .toast-header .btn-close {
    filter: invert(0.7) sepia(0.3);
    opacity: 0.5;
    margin-left: auto;
}

.toast-eat .toast-header .btn-close:hover {
    opacity: 1;
}

/* Toast Body */
.toast-eat .toast-body {
    padding: 0.85rem 1rem;
    font-family: var(--font-label);
    font-size: 0.82rem;
    color: rgba(249, 221, 211, 0.8);
    font-style: normal;
    line-height: 1.6;
    letter-spacing: 0.04em;
}

/* Toast 語意色 — 成功 */
.toast-eat.toast-success .toast-icon {
    background: rgba(93, 69, 20, 0.5);
    border: 1px solid rgba(228, 194, 133, 0.35);
    color: var(--eat-secondary);
}

.toast-eat.toast-success .toast-header {
    border-bottom-color: rgba(228, 194, 133, 0.2);
}

/* Toast 語意色 — 錯誤 */
.toast-eat.toast-error .toast-icon {
    background: rgba(147, 0, 10, 0.35);
    border: 1px solid rgba(255, 180, 171, 0.3);
    color: var(--eat-error);
}

.toast-eat.toast-error .toast-header {
    border-bottom-color: rgba(255, 180, 171, 0.15);
}

.toast-eat.toast-error .toast-header .toast-title {
    color: var(--eat-error);
}

/* Toast 語意色 — 資訊 */
.toast-eat.toast-info .toast-icon {
    background: rgba(66, 49, 43, 0.5);
    border: 1px solid rgba(152, 143, 129, 0.3);
    color: var(--eat-on-surface-variant);
}

.toast-eat.toast-info .toast-header .toast-title {
    color: var(--eat-on-surface-variant);
}
</style>