<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { Modal } from 'bootstrap'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'

defineProps({
    currentEmail: { type: String, default: '' },
})

const emit = defineEmits(['success'])

const { show: showToast } = useToast()

const newEmail = ref('')
const newEmailError = ref('')
const isSubmitting = ref(false)

function resetForm() {
    newEmail.value = ''
    newEmailError.value = ''
    isSubmitting.value = false
}

async function submit() {
    newEmailError.value = ''

    const trimmed = newEmail.value.trim()
    if (!trimmed) {
        newEmailError.value = '請輸入新 Email'
        return
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
        newEmailError.value = 'Email 格式不正確'
        return
    }
    if (trimmed.length > 100) {
        newEmailError.value = 'Email 最長 100 字元'
        return
    }

    isSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/email', {
            method: 'POST',
            body: JSON.stringify({ newEmail: trimmed }),
        })

        if (res.ok) {
            Modal.getOrCreateInstance(document.querySelector('#changeEmailModal')).hide()
            showToast('驗證信已寄出，請至新信箱確認', 'success')
            emit('success', { newEmail: trimmed })
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'email_taken') {
                newEmailError.value = '此 Email 已被使用'
            } else if (data?.errorCode === 'same_email') {
                newEmailError.value = '新 Email 與目前相同'
            } else {
                showToast(data?.message || '申請失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[ChangeEmailModal.submit]', err)
    } finally {
        isSubmitting.value = false
    }
}

const handleHidden = () => resetForm()

onMounted(() => {
    const el = document.querySelector('#changeEmailModal')
    el?.addEventListener('hidden.bs.modal', handleHidden)
})

onUnmounted(() => {
    const el = document.querySelector('#changeEmailModal')
    el?.removeEventListener('hidden.bs.modal', handleHidden)
    Modal.getInstance(document.querySelector('#changeEmailModal'))?.dispose()
})
</script>

<template>
    <div id="changeEmailModal" class="modal fade" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm ms-auto me-2 mt-2"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>
                <div class="modal-header border-0 px-4 py-3">
                    <h5 class="eat-h3 fst-normal fw-bold fs-5 mb-0">修改 Email</h5>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <div>
                            <label class="form-label">新 Email</label>
                            <input
                                v-model="newEmail"
                                type="email"
                                class="form-control"
                                placeholder="example@email.com"
                                autocomplete="email"
                            />
                            <FormErrorMessage :message="newEmailError" :show="!!newEmailError" />
                        </div>
                        <button
                            type="button"
                            class="btn-eat-primary"
                            :disabled="isSubmitting"
                            @click="submit"
                        >
                            <span v-if="isSubmitting" class="spinner-border-eat spinner-sm"></span>
                            {{ isSubmitting ? '送出中...' : '送出申請' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
