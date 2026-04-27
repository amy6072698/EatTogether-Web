<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Modal } from 'bootstrap'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'

const router = useRouter()

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
        // 無論後端回傳成功或找不到 Email，都顯示相同成功畫面（防枚舉）
        status.value = 'success'
    } catch {
        // 網路錯誤由 apiFetch 統一 Toast 處理
    } finally {
        isLoading.value = false
    }
}

function goToLogin() {
    router.push({ name: 'Home' }).then(() => {
        const modalEl = document.querySelector('#authModal')
        if (modalEl) {
            const modal = Modal.getInstance(modalEl) || new Modal(modalEl)
            modal.show()
        }
    })
}
</script>

<template>
    <main class="forgot-password-main d-flex align-items-center">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-12 col-sm-10 col-md-7 col-lg-5">
                    <div class="fp-card">
                        <!-- 輸入 Email 表單 -->
                        <div v-if="status === 'form'">
                            <div class="text-center mb-4">
                                <div class="fp-icon mb-3">
                                    <i class="bi bi-envelope-open"></i>
                                </div>
                                <h1 class="eat-h2 mb-2">忘記密碼</h1>
                                <p class="eat-body-muted mb-0">
                                    請輸入您的 Email，我們將寄送重設密碼連結。
                                </p>
                            </div>

                            <div class="mb-3">
                                <label for="fp-email" class="eat-label d-block mb-1">Email</label>
                                <input
                                    id="fp-email"
                                    v-model="email"
                                    type="email"
                                    class="form-control fp-input"
                                    placeholder="請輸入您的 Email"
                                    :disabled="isLoading"
                                    @blur="validateEmail"
                                    autocomplete="email"
                                    :aria-describedby="emailError ? 'fp-email-error' : undefined"
                                    :aria-invalid="emailError ? 'true' : undefined"
                                    @keyup.enter="handleSubmit"
                                />
                                <p
                                    v-if="emailError"
                                    id="fp-email-error"
                                    class="fp-field-error mt-1 mb-0"
                                >
                                    {{ emailError }}
                                </p>
                            </div>

                            <Button
                                variant="primary"
                                class="w-100 fs-6 py-2 mt-2"
                                :loading="isLoading"
                                @click="handleSubmit"
                            >
                                {{ isLoading ? '寄送中...' : '寄送重設連結' }}
                            </Button>

                            <div class="text-center mt-3">
                                <button class="fp-back-link" @click="goToLogin">返回登入</button>
                            </div>
                        </div>

                        <!-- 成功畫面 -->
                        <div v-else class="text-center py-2">
                            <div class="fp-icon fp-icon--success mb-3">
                                <i class="bi bi-check-lg"></i>
                            </div>
                            <h1 class="eat-h2 mb-2">信件已寄出</h1>
                            <p class="eat-body mb-3">
                                若此 Email 已在義起吃註冊，重設密碼連結將於幾分鐘內寄達。<br />
                                請記得檢查垃圾郵件資料夾。
                            </p>
                            <button class="fp-back-link" @click="goToLogin">返回登入</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.forgot-password-main {
    min-height: calc(100vh - 80px - 80px);
    padding: 3rem 0;
}

.fp-card {
    background: var(--eat-surface-container);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-lg);
    padding: 2.5rem 2rem;
}

.fp-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 4rem;
    height: 4rem;
    border-radius: 50%;
    background: var(--eat-primary-container);
    color: var(--eat-on-primary);
    font-size: 1.75rem;
    line-height: 1;
}

.fp-icon--success {
    background: var(--eat-primary-container);
    color: var(--eat-on-primary);
}

.fp-input {
    background: var(--eat-surface-high);
    border-color: var(--eat-outline-variant);
    color: var(--eat-on-surface);
}

.fp-input:focus {
    background: var(--eat-surface-high);
    border-color: var(--eat-primary);
    box-shadow: 0 0 0 0.2rem rgba(227, 199, 107, 0.2);
    color: var(--eat-on-surface);
}

.fp-input:disabled {
    opacity: 0.6;
}

.fp-field-error {
    font-size: 0.8rem;
    color: var(--eat-error);
    font-family: var(--font-label);
}

.fp-back-link {
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

.fp-back-link:hover {
    opacity: 0.75;
}
</style>
