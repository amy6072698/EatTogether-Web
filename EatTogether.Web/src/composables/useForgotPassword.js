import { ref } from 'vue'
import apiFetch from '@/utils/apiFetch.js'

export function useForgotPassword() {
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

    // 重置所有狀態至初始值，供 Modal hidden.bs.modal 事件呼叫
    function resetState() {
        status.value = 'form'
        email.value = ''
        emailError.value = ''
        isLoading.value = false
    }

    return {
        status,
        email,
        emailError,
        isLoading,
        validateEmail,
        handleSubmit,
        resetState,
    }
}
