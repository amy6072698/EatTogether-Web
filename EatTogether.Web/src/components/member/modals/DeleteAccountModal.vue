<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Modal } from 'bootstrap'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

const props = defineProps({
    hasPassword: { type: Boolean, required: true },
})

const router = useRouter()
const authStore = useAuthStore()
const { show: showToast } = useToast()

const password = ref('')
const showPassword = ref(false)
const passwordError = ref('')
const isDeleting = ref(false)

function resetForm() {
    password.value = ''
    showPassword.value = false
    passwordError.value = ''
    isDeleting.value = false
}

async function submit() {
    passwordError.value = ''

    if (props.hasPassword && !password.value) {
        passwordError.value = '請輸入密碼以確認刪除'
        return
    }

    isDeleting.value = true
    try {
        const fetchOptions = { method: 'DELETE' }
        if (props.hasPassword) {
            fetchOptions.body = JSON.stringify({ password: password.value })
        }

        const res = await apiFetch('/members/me', fetchOptions)

        if (res.ok) {
            authStore.clearAuth()
            showToast('帳號已刪除', 'success')
            router.push('/')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'wrong_password') {
                passwordError.value = '密碼不正確'
            } else {
                showToast(data?.message || '刪除失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[DeleteAccountModal.submit]', err)
    } finally {
        isDeleting.value = false
    }
}

const handleHidden = () => resetForm()

onMounted(() => {
    const el = document.querySelector('#deleteAccountModal')
    el?.addEventListener('hidden.bs.modal', handleHidden)
})

onUnmounted(() => {
    const el = document.querySelector('#deleteAccountModal')
    el?.removeEventListener('hidden.bs.modal', handleHidden)
    Modal.getInstance(document.querySelector('#deleteAccountModal'))?.dispose()
})
</script>

<template>
    <div
        id="deleteAccountModal"
        class="modal fade"
        tabindex="-1"
        data-bs-backdrop="static"
        data-bs-keyboard="false"
    >
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm ms-auto me-2 mt-2"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>
                <div class="modal-header border-0 px-4 py-3">
                    <h5 class="eat-h3 fst-normal fw-bold fs-5 mb-0">刪除帳號</h5>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <p class="eat-body-muted mb-0">此操作無法復原，所有資料將永久刪除。</p>
                        <!-- HAS_PASSWORD：需密碼確認 -->
                        <div v-if="hasPassword">
                            <label class="form-label">請輸入密碼確認</label>
                            <div class="position-relative">
                                <input
                                    v-model="password"
                                    :type="showPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="請輸入目前密碼"
                                    autocomplete="current-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showPassword = !showPassword"
                                    :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                                </button>
                            </div>
                            <FormErrorMessage :message="passwordError" :show="!!passwordError" />
                        </div>
                        <button
                            type="button"
                            class="btn-eat-danger"
                            :disabled="isDeleting"
                            @click="submit"
                        >
                            <span v-if="isDeleting" class="spinner-border-eat spinner-sm"></span>
                            {{ isDeleting ? '刪除中...' : '確認刪除' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
