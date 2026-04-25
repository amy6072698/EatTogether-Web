<script setup>
import { ref } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'

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

// 密碼顯示切換
const showPassword = ref(false)
const showConfirmPassword = ref(false)

function validateAccount() {
    regAccountError.value = ''
    if (!regAccount.value.trim()) {
        regAccountError.value = '帳號為必填'
    } else if (!/^[a-zA-Z0-9]{3,50}$/.test(regAccount.value)) {
        regAccountError.value = '帳號限英數字，3–50 字元'
    }
}

function validateName() {
    regNameError.value = ''
    const trimmed = regName.value.trim()
    if (!trimmed) {
        regNameError.value = '姓名為必填'
    } else if (trimmed.length > 50) {
        regNameError.value = '姓名最多 50 字'
    }
}

function validateEmail() {
    regEmailError.value = ''
    const trimmed = regEmail.value.trim()
    if (!trimmed) {
        regEmailError.value = 'Email 為必填'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
        regEmailError.value = 'Email 格式不正確'
    } else if (trimmed.length > 100) {
        regEmailError.value = 'Email 最長 100 字元'
    }
}

function validatePassword() {
    regPasswordError.value = ''
    if (!regPassword.value) {
        regPasswordError.value = '密碼為必填'
    } else if (regPassword.value.length < 8) {
        regPasswordError.value = '密碼至少需要 8 個字元'
    } else if (regPassword.value.length > 128) {
        regPasswordError.value = '密碼最多 128 個字元'
    }
}

function validateConfirmPassword() {
    regConfirmPasswordError.value = ''
    if (!regConfirmPassword.value) {
        regConfirmPasswordError.value = '請再次輸入密碼'
    } else if (regConfirmPassword.value !== regPassword.value) {
        regConfirmPasswordError.value = '兩次密碼輸入不一致'
    }
}

function validateRegisterForm() {
    validateAccount()
    validateName()
    validateEmail()
    validatePassword()
    validateConfirmPassword()

    return (
        !regAccountError.value &&
        !regNameError.value &&
        !regEmailError.value &&
        !regPasswordError.value &&
        !regConfirmPasswordError.value
    )
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

    showPassword.value = false
}

function switchToLogin() {
    activeTab.value = 'login'

    // 重置註冊表單欄位
    regAccount.value = ''
    regName.value = ''
    regEmail.value = ''
    regPassword.value = ''
    regConfirmPassword.value = ''
    // 重置註冊表單錯誤訊息
    regAccountError.value = ''
    regNameError.value = ''
    regEmailError.value = ''
    regPasswordError.value = ''
    regConfirmPasswordError.value = ''
    regFormError.value = ''
    // 重置眼睛開關
    showPassword.value = false
    showConfirmPassword.value = false
    // 重置成功狀態
    registerSuccess.value = false
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
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm me-2 mt-2 ms-auto"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>
                <!-- Header -->
                <div class="modal-header justify-content-between border-0 px-4 py-0">
                    <div class="w-100 d-flex justify-content-center">
                        <h2 id="authModalLabel" class="text-eat-primary text-center fs-5 mb-0">
                            {{ activeTab === 'login' ? '會員登入' : '註冊會員' }}
                        </h2>
                    </div>
                </div>

                <!-- Tab 切換 -->
                <div class="px-4 pt-3 pb-2">
                    <div v-show="activeTab === 'login'">
                        <p class="text-center eat-body-muted mb-0 auth-footer-text">
                            還沒有帳號？
                            <button type="button" class="eat-link-btn" @click="switchToRegister">
                                前往註冊
                            </button>
                        </p>
                    </div>
                    <div v-show="activeTab === 'register'">
                        <p class="text-center eat-body-muted mb-0 auth-footer-text">
                            已有帳號？
                            <button type="button" class="eat-link-btn" @click="switchToLogin">
                                前往登入
                            </button>
                        </p>
                    </div>
                </div>

                <!-- Modal Body -->
                <div class="modal-body px-4 pt-2 pb-4" style="overflow-y: auto; max-height: 70vh">
                    <!-- ── 登入 Tab（UI 佔位，1-3 實作串接）── -->
                    <div v-show="activeTab === 'login'">
                        <div class="d-flex flex-column gap-2">
                            <Button variant="secondary" class="fs-6 py-2 mb-2" :disabled="true">
                                使用 Google 登入
                            </Button>

                            <div
                                class="auth-divider-content feather-divider on-container my-3"
                            ></div>

                            <div>
                                <label class="eat-label d-block mb-1">帳號</label>
                                <input
                                    type="text"
                                    class="form-eat w-100"
                                    placeholder="請輸入帳號"
                                    disabled
                                />
                            </div>

                            <div>
                                <label class="eat-label d-block mb-1">密碼</label>
                                <div class="position-relative">
                                    <input
                                        :type="showPassword ? 'text' : 'password'"
                                        class="form-eat w-100"
                                        placeholder="請輸入密碼"
                                    />
                                    <button
                                        type="button"
                                        class="password-toggle"
                                        @click="showPassword = !showPassword"
                                        :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
                                    >
                                        <i
                                            :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"
                                        ></i>
                                    </button>
                                </div>
                            </div>

                            <Button variant="primary" class="fs-6 py-2 mt-3" :disabled="true">
                                登入
                            </Button>

                            <div class="text-center">
                                <a href="/forgot-password" class="eat-body-muted auth-small-link"
                                    >忘記密碼？</a
                                >
                            </div>
                        </div>
                    </div>

                    <!-- ── 註冊 Tab ── -->
                    <div v-show="activeTab === 'register'">
                        <div class="d-flex flex-column gap-2">
                            <!-- 成功狀態 -->
                            <div v-if="registerSuccess" class="auth-success-box text-center py-3">
                                <p class="eat-h3 mb-2">✉</p>
                                <p class="eat-body mb-1">驗證信已寄出</p>
                                <p class="eat-body-muted mb-0">請至信箱點擊驗證連結以完成註冊。</p>
                            </div>

                            <!-- 註冊表單 -->
                            <template v-else>
                                <Button variant="primary" class="fs-6 py-2 mb-2" :disabled="true">
                                    使用 Google 快速註冊
                                </Button>

                                <div
                                    class="auth-divider-content feather-divider on-container my-3"
                                ></div>

                                <!-- 通用錯誤橫幅 -->
                                <div v-if="regFormError" class="auth-error-banner">
                                    {{ regFormError }}
                                </div>

                                <!-- 帳號 -->
                                <div>
                                    <label class="eat-label d-block mb-1">帳號</label>
                                    <input
                                        v-model="regAccount"
                                        @blur="validateAccount"
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
                                <div>
                                    <label class="eat-label d-block mb-1">姓名</label>
                                    <input
                                        v-model="regName"
                                        @blur="validateName"
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
                                <div>
                                    <label class="eat-label d-block mb-1">Email</label>
                                    <input
                                        v-model="regEmail"
                                        @blur="validateEmail"
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
                                <div>
                                    <label class="eat-label d-block mb-1">密碼</label>
                                    <div class="position-relative">
                                        <input
                                            v-model="regPassword"
                                            @blur="validatePassword"
                                            :type="showPassword ? 'text' : 'password'"
                                            class="form-eat w-100"
                                            :class="{ 'form-eat--error': regPasswordError }"
                                            placeholder="至少 8 個字元"
                                            autocomplete="new-password"
                                        />
                                        <button
                                            type="button"
                                            class="password-toggle"
                                            @click="showPassword = !showPassword"
                                            :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
                                        >
                                            <i
                                                :class="
                                                    showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                                "
                                            ></i>
                                        </button>
                                    </div>

                                    <p v-if="regPasswordError" class="auth-field-error mt-1 mb-0">
                                        {{ regPasswordError }}
                                    </p>
                                </div>

                                <!-- 確認密碼 -->
                                <div>
                                    <label class="eat-label d-block mb-1">確認密碼</label>
                                    <div class="position-relative">
                                        <input
                                            v-model="regConfirmPassword"
                                            @input="validateConfirmPassword"
                                            :type="showConfirmPassword ? 'text' : 'password'"
                                            class="form-eat w-100"
                                            :class="{ 'form-eat--error': regConfirmPasswordError }"
                                            placeholder="再次輸入密碼"
                                            autocomplete="new-password"
                                        />
                                        <button
                                            type="button"
                                            class="password-toggle"
                                            @click="showConfirmPassword = !showConfirmPassword"
                                            :aria-label="
                                                showConfirmPassword ? '隱藏密碼' : '顯示密碼'
                                            "
                                        >
                                            <i
                                                :class="
                                                    showConfirmPassword
                                                        ? 'bi bi-eye-slash'
                                                        : 'bi bi-eye'
                                                "
                                            ></i>
                                        </button>
                                    </div>
                                    <p
                                        v-if="regConfirmPasswordError"
                                        class="auth-field-error mt-1 mb-0"
                                    >
                                        {{ regConfirmPasswordError }}
                                    </p>
                                </div>

                                <Button
                                    variant="primary"
                                    class="fs-6 py-2 mt-3"
                                    :loading="isSubmitting"
                                    @click="handleRegister"
                                >
                                    {{ isSubmitting ? '註冊中...' : '註冊' }}
                                </Button>
                            </template>
                        </div>
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
.auth-divider-content.feather-divider::after {
    content: '或';
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

/* ── 密碼顯示切換按鈕 ── */
.password-toggle {
    position: absolute;
    right: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: var(--eat-on-surface-variant);
    cursor: pointer;
    padding: 0;
    line-height: 1;
    transition: color 0.2s;
}

.password-toggle:hover {
    color: var(--eat-on-surface);
}
</style>
