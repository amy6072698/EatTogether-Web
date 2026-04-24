<script setup>
import { ref } from 'vue'
import apiFetch from '@/utils/apiFetch.js'

// Tab 狀態：'login' | 'register'
const activeTab = ref('login')

// 註冊成功狀態
const registerSuccess = ref(false)

// 註冊表單欄位
const regAccount = ref('')
const regName = ref('')
const regEmail = ref('')
const regPassword = ref('')
const regConfirmPassword = ref('')

// 欄位錯誤訊息
const regAccountError = ref('')
const regNameError = ref('')
const regEmailError = ref('')
const regPasswordError = ref('')
const regConfirmPasswordError = ref('')
const regFormError = ref('')

const isSubmitting = ref(false)

function validateRegisterForm() {
    let valid = true
    regAccountError.value = ''
    regNameError.value = ''
    regEmailError.value = ''
    regPasswordError.value = ''
    regConfirmPasswordError.value = ''
    regFormError.value = ''

    if (!regAccount.value || !/^[a-zA-Z0-9]{3,50}$/.test(regAccount.value)) {
        regAccountError.value = '帳號為必填，限英數字 3–50 字元'
        valid = false
    }

    const trimmedName = regName.value.trim()
    if (!trimmedName || trimmedName.length > 50) {
        regNameError.value = '姓名為必填，最多 50 字'
        valid = false
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!regEmail.value || !emailRegex.test(regEmail.value) || regEmail.value.length > 100) {
        regEmailError.value = '請輸入有效的 Email 地址'
        valid = false
    }

    if (!regPassword.value || regPassword.value.length < 8) {
        regPasswordError.value = '密碼至少需要 8 個字元'
        valid = false
    }

    if (regConfirmPassword.value !== regPassword.value) {
        regConfirmPasswordError.value = '兩次密碼輸入不一致'
        valid = false
    }

    return valid
}

async function handleRegister() {
    if (!validateRegisterForm()) return

    isSubmitting.value = true
    try {
        const res = await apiFetch('/auth/register', {
            method: 'POST',
            body: JSON.stringify({
                account: regAccount.value,
                name: regName.value,
                email: regEmail.value,
                password: regPassword.value,
            }),
        })

        if (res.ok) {
            registerSuccess.value = true
            return
        }

        const data = await res.json()

        if (res.status === 409) {
            if (data.errorCode === 'account_taken') {
                regAccountError.value = '此帳號已被使用，請換一個'
            } else if (data.errorCode === 'email_taken') {
                regEmailError.value = '此 Email 已有帳號，請直接登入'
            } else {
                regFormError.value = data.message || '發生錯誤，請稍後再試'
            }
            return
        }

        regFormError.value = data?.message || '發生錯誤，請稍後再試'
    } catch {
        // apiFetch 網路錯誤已由其內部 show Toast，此處不重複提示
    } finally {
        isSubmitting.value = false
    }
}

function switchToRegister() {
    activeTab.value = 'register'
}

function switchToLogin() {
    activeTab.value = 'login'
}
</script>

<template>
    <div
        id="authModal"
        class="modal fade"
        data-bs-backdrop="static"
        data-bs-keyboard="false"
        tabindex="-1"
        aria-labelledby="authModalLabel"
        aria-hidden="true"
    >
        <div class="modal-dialog modal-dialog-centered modal-fullscreen-sm-down auth-modal-dialog">
            <div class="modal-content auth-modal-content">
                <!-- Header -->
                <div class="modal-header border-0 px-4 pt-4 pb-0">
                    <h2 id="authModalLabel" class="eat-h2 mb-0">義起吃 Eat Together</h2>
                    <button
                        type="button"
                        class="btn-close btn-close-white"
                        data-bs-dismiss="modal"
                        aria-label="關閉"
                    ></button>
                </div>

                <!-- Tab 切換 -->
                <div class="px-4 pt-3">
                    <ul class="nav auth-tabs mb-0">
                        <li class="nav-item">
                            <button
                                class="auth-tab-btn"
                                :class="{ active: activeTab === 'login' }"
                                type="button"
                                @click="switchToLogin"
                            >
                                登入
                            </button>
                        </li>
                        <li class="nav-item">
                            <button
                                class="auth-tab-btn"
                                :class="{ active: activeTab === 'register' }"
                                type="button"
                                @click="switchToRegister"
                            >
                                註冊
                            </button>
                        </li>
                    </ul>
                </div>

                <!-- Modal Body -->
                <div class="modal-body px-4 pt-4 pb-4">
                    <!-- ── 登入 Tab（UI 佔位，1-3 實作串接）── -->
                    <div v-show="activeTab === 'login'">
                        <button type="button" class="btn-eat-secondary w-100 mb-3" disabled>
                            使用 Google 登入
                        </button>

                        <div class="auth-divider mb-3">
                            <span class="eat-body-muted px-3">或</span>
                        </div>

                        <div class="mb-3">
                            <label class="eat-label d-block mb-1">帳號</label>
                            <input
                                type="text"
                                class="form-eat w-100"
                                placeholder="請輸入帳號"
                                disabled
                            />
                        </div>

                        <div class="mb-3">
                            <label class="eat-label d-block mb-1">密碼</label>
                            <input
                                type="password"
                                class="form-eat w-100"
                                placeholder="請輸入密碼"
                                disabled
                            />
                        </div>

                        <div class="text-end mb-3">
                            <a href="/forgot-password" class="eat-body-muted auth-small-link"
                                >忘記密碼？</a
                            >
                        </div>

                        <button type="button" class="btn-eat-primary w-100 mb-3" disabled>
                            登入
                        </button>

                        <p class="text-center eat-body-muted mb-0 auth-footer-text">
                            還沒有帳號？
                            <button type="button" class="eat-link-btn" @click="switchToRegister">
                                前往註冊
                            </button>
                        </p>
                    </div>

                    <!-- ── 註冊 Tab ── -->
                    <div v-show="activeTab === 'register'">
                        <!-- 成功狀態 -->
                        <div v-if="registerSuccess" class="auth-success-box text-center py-3">
                            <p class="eat-h3 mb-2">✉</p>
                            <p class="eat-body mb-1">驗證信已寄出</p>
                            <p class="eat-body-muted mb-0">請至信箱點擊驗證連結以完成註冊。</p>
                        </div>

                        <!-- 註冊表單 -->
                        <template v-else>
                            <button type="button" class="btn-eat-secondary w-100 mb-3" disabled>
                                使用 Google 快速註冊
                            </button>

                            <div class="auth-divider mb-3">
                                <span class="eat-body-muted px-3">或</span>
                            </div>

                            <!-- 通用錯誤橫幅 -->
                            <div v-if="regFormError" class="auth-error-banner mb-3">
                                {{ regFormError }}
                            </div>

                            <!-- 帳號 -->
                            <div class="mb-3">
                                <label class="eat-label d-block mb-1">帳號</label>
                                <input
                                    v-model="regAccount"
                                    type="text"
                                    class="form-eat w-100"
                                    :class="{ 'form-eat--error': regAccountError }"
                                    placeholder="限英數字，3–50 字元"
                                    autocomplete="username"
                                />
                                <p v-if="regAccountError" class="auth-field-error mt-1 mb-0">
                                    {{ regAccountError }}
                                </p>
                            </div>

                            <!-- 姓名 -->
                            <div class="mb-3">
                                <label class="eat-label d-block mb-1">姓名</label>
                                <input
                                    v-model="regName"
                                    type="text"
                                    class="form-eat w-100"
                                    :class="{ 'form-eat--error': regNameError }"
                                    placeholder="請輸入您的姓名"
                                    autocomplete="name"
                                />
                                <p v-if="regNameError" class="auth-field-error mt-1 mb-0">
                                    {{ regNameError }}
                                </p>
                            </div>

                            <!-- Email -->
                            <div class="mb-3">
                                <label class="eat-label d-block mb-1">Email</label>
                                <input
                                    v-model="regEmail"
                                    type="email"
                                    class="form-eat w-100"
                                    :class="{ 'form-eat--error': regEmailError }"
                                    placeholder="example@email.com"
                                    autocomplete="email"
                                />
                                <p v-if="regEmailError" class="auth-field-error mt-1 mb-0">
                                    {{ regEmailError }}
                                </p>
                            </div>

                            <!-- 密碼 -->
                            <div class="mb-3">
                                <label class="eat-label d-block mb-1">密碼</label>
                                <input
                                    v-model="regPassword"
                                    type="password"
                                    class="form-eat w-100"
                                    :class="{ 'form-eat--error': regPasswordError }"
                                    placeholder="至少 8 個字元"
                                    autocomplete="new-password"
                                />
                                <p v-if="regPasswordError" class="auth-field-error mt-1 mb-0">
                                    {{ regPasswordError }}
                                </p>
                            </div>

                            <!-- 確認密碼 -->
                            <div class="mb-4">
                                <label class="eat-label d-block mb-1">確認密碼</label>
                                <input
                                    v-model="regConfirmPassword"
                                    type="password"
                                    class="form-eat w-100"
                                    :class="{ 'form-eat--error': regConfirmPasswordError }"
                                    placeholder="再次輸入密碼"
                                    autocomplete="new-password"
                                />
                                <p
                                    v-if="regConfirmPasswordError"
                                    class="auth-field-error mt-1 mb-0"
                                >
                                    {{ regConfirmPasswordError }}
                                </p>
                            </div>

                            <button
                                type="button"
                                class="btn-eat-primary w-100 mb-3"
                                :disabled="isSubmitting"
                                @click="handleRegister"
                            >
                                <span
                                    v-if="isSubmitting"
                                    class="spinner-border spinner-border-sm spinner-border-eat me-2"
                                    role="status"
                                    aria-hidden="true"
                                ></span>
                                {{ isSubmitting ? '建立中...' : '建立帳號' }}
                            </button>

                            <p class="text-center eat-body-muted mb-0 auth-footer-text">
                                已有帳號？
                                <button type="button" class="eat-link-btn" @click="switchToLogin">
                                    前往登入
                                </button>
                            </p>
                        </template>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.auth-modal-dialog {
    max-width: 480px;
}

.auth-modal-content {
    background: var(--eat-surface-container);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
}

/* ── Tab ── */
.auth-tabs {
    border-bottom: 1px solid var(--eat-outline-variant);
}

.auth-tab-btn {
    background: none;
    border: none;
    border-bottom: 2px solid transparent;
    color: var(--eat-on-surface-variant);
    font-family: var(--font-label);
    font-size: 0.8rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    padding: 0.5rem 1.25rem;
    margin-bottom: -1px;
    cursor: pointer;
    transition: var(--eat-transition);
}

.auth-tab-btn.active {
    color: var(--eat-primary);
    border-bottom-color: var(--eat-primary);
}

.auth-tab-btn:hover:not(.active) {
    color: var(--eat-on-surface);
}

/* ── Input ── */
.form-eat {
    background: var(--eat-surface-high);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-sm);
    color: var(--eat-on-surface);
    font-family: var(--font-body);
    font-size: 0.95rem;
    padding: 0.65rem 0.85rem;
    outline: none;
    transition: border-color 0.25s;
}

.form-eat:focus {
    border-color: var(--eat-primary);
}

.form-eat--error {
    border-color: var(--eat-error);
}

.form-eat::placeholder {
    color: rgba(249, 221, 211, 0.35);
}

/* ── Error ── */
.auth-error-banner {
    background: var(--eat-error-container);
    color: var(--eat-error);
    padding: 0.65rem 1rem;
    border-radius: var(--eat-radius-sm);
    font-family: var(--font-label);
    font-size: 0.78rem;
    letter-spacing: 0.1em;
}

.auth-field-error {
    color: var(--eat-error);
    font-family: var(--font-label);
    font-size: 0.75rem;
    letter-spacing: 0.08em;
}

/* ── Divider ── */
.auth-divider {
    display: flex;
    align-items: center;
    text-align: center;
}

.auth-divider::before,
.auth-divider::after {
    content: '';
    flex: 1;
    height: 1px;
    background: var(--eat-outline-variant);
}

/* ── Link Button ── */
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

.auth-small-link {
    font-size: 0.85rem;
    text-decoration: underline;
    text-underline-offset: 3px;
}

.auth-footer-text {
    font-size: 0.85rem;
}

/* ── Success ── */
.auth-success-box {
    color: var(--eat-on-surface);
}
</style>
