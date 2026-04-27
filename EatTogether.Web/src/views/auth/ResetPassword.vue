<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Modal } from 'bootstrap'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'

const route = useRoute()
const router = useRouter()

// 'loading' | 'form' | 'invalid' | 'success'
const status = ref('loading')

const newPassword = ref('')
const confirmPassword = ref('')
const newPasswordError = ref('')
const confirmPasswordError = ref('')
const isLoading = ref(false)
const showRpNewPassword = ref(false)
const showRpConfirmPassword = ref(false)

function validateNewPassword() {
    newPasswordError.value = ''
    if (!newPassword.value) {
        newPasswordError.value = '新密碼為必填'
        return false
    }
    if (newPassword.value.length < 8) {
        newPasswordError.value = '密碼至少 8 個字元'
        return false
    }
    return true
}

function validateConfirmPassword() {
    confirmPasswordError.value = ''
    if (!confirmPassword.value) {
        confirmPasswordError.value = '確認密碼為必填'
        return false
    }
    if (confirmPassword.value !== newPassword.value) {
        confirmPasswordError.value = '兩次密碼輸入不一致'
        return false
    }
    return true
}

async function handleSubmit() {
    const v1 = validateNewPassword()
    const v2 = validateConfirmPassword()
    if (!v1 || !v2) return

    isLoading.value = true
    try {
        const token = route.query.token
        const res = await apiFetch('/auth/reset-password', {
            method: 'POST',
            body: JSON.stringify({ token, newPassword: newPassword.value }),
        })

        if (!res.ok) {
            // token 無效或已使用（後端 400）
            status.value = 'invalid'
            return
        }

        status.value = 'success'
        window.scrollTo({ top: 0 })
        setTimeout(() => {
            router.push({ name: 'Home' }).then(() => {
                const modalEl = document.querySelector('#authModal')
                if (modalEl) {
                    const modal = Modal.getInstance(modalEl) || new Modal(modalEl)
                    modal.show()
                }
            })
        }, 3000)
    } catch {
        // 網路錯誤由 apiFetch 統一 Toast 處理
    } finally {
        isLoading.value = false
    }
}

// token 無效狀態：導回首頁並開啟重新申請 Modal，讓使用者重新申請
function goToForgotPassword() {
    router.push({ name: 'Home' }).then(() => {
        const modalEl = document.querySelector('#resendResetPasswordModal')
        if (modalEl) {
            const modal = Modal.getInstance(modalEl) || new Modal(modalEl)
            modal.show()
        }
    })
}

onMounted(async () => {
    const token = route.query.token
    if (!token) {
        status.value = 'invalid'
        return
    }
    try {
        const res = await apiFetch(`/auth/validate-reset-token?token=${encodeURIComponent(token)}`)
        if (res.ok) {
            status.value = 'form'
        } else {
            status.value = 'invalid'
        }
    } catch {
        status.value = 'invalid'
    }
})
</script>

<template>
    <main class="reset-password-main d-flex align-items-center">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-10 col-md-7 col-lg-5">
                    <div class="rp-card">
                        <!-- 驗證中 -->
                        <div v-if="status === 'loading'" class="text-center py-4">
                            <div class="spinner-border rp-spinner mb-3" role="status">
                                <span class="visually-hidden">驗證中...</span>
                            </div>
                            <p class="eat-body mb-0">驗證連結中，請稍候...</p>
                        </div>

                        <!-- 連結無效 -->
                        <div v-else-if="status === 'invalid'" class="text-center py-2">
                            <div class="icon-eat icon-eat--error mb-2">
                                <i class="bi bi-x-lg"></i>
                            </div>
                            <h1 class="eat-h3 fw-bolder fst-normal mb-2">連結無效或已過期</h1>
                            <p class="eat-body-muted mb-3">此重設密碼連結已失效，請重新申請</p>
                            <button class="rp-back-link" @click="goToForgotPassword">
                                重新申請重設密碼
                            </button>
                        </div>

                        <!-- 重設表單 -->
                        <div v-else-if="status === 'form'">
                            <div class="text-center mb-3">
                                <div class="icon-eat mb-2">
                                    <i class="bi bi-lock"></i>
                                </div>
                                <h1 class="eat-h3 fw-bolder fst-normal mb-2">重設密碼</h1>
                                <p class="eat-body-muted mb-0">請輸入您的新密碼</p>
                            </div>

                            <div class="mb-3">
                                <label for="rp-new-password" class="eat-label d-block mb-1">
                                    新密碼
                                </label>
                                <div class="position-relative">
                                    <input
                                        id="rp-new-password"
                                        v-model="newPassword"
                                        :type="showRpNewPassword ? 'text' : 'password'"
                                        class="form-control rp-input"
                                        placeholder="至少 8 個字元"
                                        :disabled="isLoading"
                                        @blur="validateNewPassword"
                                        autocomplete="new-password"
                                        :aria-describedby="
                                            newPasswordError ? 'rp-new-password-error' : undefined
                                        "
                                        :aria-invalid="newPasswordError ? 'true' : undefined"
                                    />
                                    <button
                                        type="button"
                                        class="btn-eat-password-toggle"
                                        @click="showRpNewPassword = !showRpNewPassword"
                                        :aria-label="showRpNewPassword ? '隱藏密碼' : '顯示密碼'"
                                    >
                                        <i
                                            :class="
                                                showRpNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                            "
                                        ></i>
                                    </button>
                                </div>
                                <p
                                    v-if="newPasswordError"
                                    id="rp-new-password-error"
                                    class="rp-field-error mt-1 mb-0"
                                >
                                    {{ newPasswordError }}
                                </p>
                            </div>

                            <div class="mb-3">
                                <label for="rp-confirm-password" class="eat-label d-block mb-1">
                                    確認新密碼
                                </label>
                                <div class="position-relative">
                                    <input
                                        id="rp-confirm-password"
                                        v-model="confirmPassword"
                                        :type="showRpConfirmPassword ? 'text' : 'password'"
                                        class="form-control rp-input"
                                        placeholder="再次輸入新密碼"
                                        :disabled="isLoading"
                                        @blur="validateConfirmPassword"
                                        autocomplete="new-password"
                                        :aria-describedby="
                                            confirmPasswordError
                                                ? 'rp-confirm-password-error'
                                                : undefined
                                        "
                                        :aria-invalid="confirmPasswordError ? 'true' : undefined"
                                        @keyup.enter="handleSubmit"
                                    />
                                    <button
                                        type="button"
                                        class="btn-eat-password-toggle"
                                        @click="showRpConfirmPassword = !showRpConfirmPassword"
                                        :aria-label="
                                            showRpConfirmPassword ? '隱藏密碼' : '顯示密碼'
                                        "
                                    >
                                        <i
                                            :class="
                                                showRpConfirmPassword
                                                    ? 'bi bi-eye-slash'
                                                    : 'bi bi-eye'
                                            "
                                        ></i>
                                    </button>
                                </div>
                                <p
                                    v-if="confirmPasswordError"
                                    id="rp-confirm-password-error"
                                    class="rp-field-error mt-1 mb-0"
                                >
                                    {{ confirmPasswordError }}
                                </p>
                            </div>

                            <Button
                                variant="primary"
                                class="w-100 btn-eat-md mt-2"
                                :loading="isLoading"
                                @click="handleSubmit"
                            >
                                {{ isLoading ? '更新中...' : '確認重設密碼' }}
                            </Button>
                        </div>

                        <!-- 成功畫面 -->
                        <div v-else class="text-center py-2">
                            <div class="icon-eat icon-eat--success mb-3">
                                <i class="bi bi-check-lg"></i>
                            </div>
                            <h1 class="eat-h3 fw-bolder fst-normal mb-2">密碼已重設</h1>
                            <p class="eat-body-muted mb-4">您的密碼已成功更新，請使用新密碼登入</p>
                            <p class="eat-body-muted mt-1">3 秒後自動返回登入...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.reset-password-main {
    min-height: calc(100vh - 80px - 80px);
    padding: 2rem 0;
}

.rp-card {
    background: var(--eat-surface-container);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
    padding: 2rem;
}

.rp-spinner {
    color: var(--eat-primary);
    width: 3rem;
    height: 3rem;
}

.rp-input {
    background: var(--eat-surface-high);
    border-color: var(--eat-outline-variant);
    color: var(--eat-on-surface);
}

.rp-input:focus {
    background: var(--eat-surface-high);
    border-color: var(--eat-primary);
    box-shadow: 0 0 0 0.2rem rgba(227, 199, 107, 0.2);
    color: var(--eat-on-surface);
}

.rp-input:disabled {
    opacity: 0.6;
}

.rp-field-error {
    font-size: 0.8rem;
    color: var(--eat-error);
    font-family: var(--font-label);
}

.rp-back-link {
    background: none;
    border: none;
    padding: 0;
    font-family: var(--font-body);
    font-size: 0.9rem;
    color: var(--eat-primary);
    cursor: pointer;
    text-decoration: underline;
    text-underline-offset: 3px;
    transition: opacity 0.2s;
}

.rp-back-link:hover {
    opacity: 0.75;
}
</style>
