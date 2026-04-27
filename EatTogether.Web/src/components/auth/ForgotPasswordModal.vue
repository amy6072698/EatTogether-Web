<script setup>
import { ref, onMounted } from 'vue'
import { Modal } from 'bootstrap'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'

// 'form' | 'success'
const status = ref('form')

const email = ref('')
const emailError = ref('')
const isLoading = ref(false)

function validateEmail() {
    emailError.value = ''
    const trimmed = email.value.trim()
    if (!trimmed) {
        emailError.value = 'Email 為必填'
        return false
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
        emailError.value = 'Email 格式不正確'
        return false
    }
    return true
}

async function handleSubmit() {
    if (!validateEmail()) return
    isLoading.value = true
    try {
        await apiFetch('/auth/forgot-password', {
            method: 'POST',
            body: JSON.stringify({ email: email.value.trim() }),
        })
        // 無論後端回傳成功或找不到 Email，一律切換至成功狀態（防枚舉）
        status.value = 'success'
    } catch {
        // 網路錯誤由 apiFetch 統一 Toast 處理，不切換狀態
    } finally {
        isLoading.value = false
    }
}

function goToLogin() {
    const selfEl = document.querySelector('#forgotPasswordModal')
    const authModalEl = document.querySelector('#authModal')
    Modal.getOrCreateInstance(selfEl).hide()
    // 等 forgotPasswordModal 完全關閉後才開啟 authModal，避免兩個 Modal 同時存在
    selfEl.addEventListener(
        'hidden.bs.modal',
        () => {
            Modal.getOrCreateInstance(authModalEl).show()
        },
        { once: true }
    )
}

onMounted(() => {
    const modalEl = document.querySelector('#forgotPasswordModal')
    // Modal 關閉時重置所有狀態，確保下次開啟從頭開始
    modalEl.addEventListener('hidden.bs.modal', () => {
        status.value = 'form'
        email.value = ''
        emailError.value = ''
        isLoading.value = false
    })
})
</script>

<template>
    <div
        id="forgotPasswordModal"
        class="modal fade"
        data-bs-backdrop="static"
        data-bs-keyboard="false"
        tabindex="-1"
        aria-labelledby="forgotPasswordModalLabel"
        aria-hidden="true"
    >
        <div class="modal-dialog modal-dialog-centered modal-fullscreen-sm-down fp-modal-dialog">
            <div class="modal-content fp-modal-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm me-2 mt-2 ms-auto"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>

                <div class="modal-header justify-content-between border-0 px-4 py-0">
                    <div class="w-100 d-flex justify-content-center">
                        <h2
                            id="forgotPasswordModalLabel"
                            class="eat-h3 fw-bolder fst-normal text-center fs-5 mb-0"
                        >
                            {{ status === 'form' ? '忘記密碼' : '密碼重設信已寄出' }}
                        </h2>
                    </div>
                </div>

                <div class="modal-body px-4 pt-3 pb-4">
                    <!-- 狀態一：輸入 Email 表單 -->
                    <div v-if="status === 'form'" class="d-flex flex-column gap-2">
                        <p class="eat-body-muted mb-1 text-center" style="font-size: 0.9rem">
                            請輸入您的 Email，我們將寄送密碼重設信給您
                        </p>

                        <div>
                            <label for="fp-modal-email" class="eat-label d-block mb-1">
                                Email
                            </label>
                            <input
                                id="fp-modal-email"
                                v-model="email"
                                type="email"
                                class="form-eat w-100"
                                :class="{ 'form-eat--error': emailError }"
                                placeholder="請輸入您的 Email"
                                autocomplete="email"
                                :disabled="isLoading"
                                @blur="validateEmail"
                                @keyup.enter="handleSubmit"
                                :aria-describedby="emailError ? 'fp-modal-email-error' : undefined"
                                :aria-invalid="emailError ? 'true' : undefined"
                            />
                            <p
                                v-if="emailError"
                                id="fp-modal-email-error"
                                class="fp-field-error mt-1 mb-0"
                            >
                                {{ emailError }}
                            </p>
                        </div>

                        <Button
                            variant="primary"
                            class="btn-eat-md mt-2"
                            :loading="isLoading"
                            @click="handleSubmit"
                        >
                            {{ isLoading ? '寄送中...' : '寄送重設信' }}
                        </Button>

                        <div class="text-center mt-1">
                            <button
                                type="button"
                                class="eat-link-btn fp-back-link"
                                @click="goToLogin"
                            >
                                ← 返回登入
                            </button>
                        </div>
                    </div>

                    <!-- 狀態二：寄送成功 -->
                    <div v-else class="text-center py-2">
                        <div class="icon-eat icon-eat--success mb-3">
                            <i class="bi bi-envelope"></i>
                        </div>
                        <p class="eat-body-muted mb-0" role="alert" style="font-size: 0.9rem">
                            請檢查您的收件匣<br />
                            若未收到，請確認 Email 是否正確<br />
                            或檢查垃圾郵件資料夾
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.fp-modal-dialog {
    max-width: 480px;
}

.fp-modal-content {
    background: var(--eat-surface-container);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
}

.form-eat {
    background: var(--eat-surface-high);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-sm);
    color: var(--eat-on-surface);
    font-family: var(--font-body);
    font-size: 1rem;
    padding: 0.375rem 0.75rem;
    outline: none;
    transition: border-color 0.25s;
    line-height: 1.5;
}

.form-eat:focus {
    border-color: var(--eat-primary);
}

.form-eat--error {
    border-color: var(--eat-error);
}

.form-eat::placeholder {
    color: color-mix(in srgb, var(--eat-on-surface) 35%, transparent);
}

.form-eat:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.fp-field-error {
    color: var(--eat-error);
    font-family: var(--font-label);
    font-size: 0.75rem;
    letter-spacing: 0.08em;
}

.eat-link-btn {
    background: none;
    border: none;
    color: var(--eat-primary);
    font-family: var(--font-body);
    font-style: italic;
    font-size: inherit;
    padding: 0;
    cursor: pointer;
    text-decoration: underline;
    text-underline-offset: 3px;
}

.eat-link-btn:hover {
    color: var(--eat-primary-fixed);
}

.fp-back-link {
    font-size: 0.85rem;
}

.fp-success-icon {
    font-size: 2.5rem;
    line-height: 1;
    color: var(--eat-primary);
}
</style>
