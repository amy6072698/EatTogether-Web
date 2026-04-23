import { ref } from 'vue'

// 模組層級共享狀態，確保所有呼叫端操作同一份 toasts 陣列
const toasts = ref([])

/**
 * 全域 Toast 通知 composable
 * 供 apiFetch.js 及各頁面元件使用
 */
export function useToast() {
    /**
     * 新增一則 Toast，3500ms 後自動移除
     * @param {string} message
     * @param {'success'|'error'|'info'} type
     */
    function show(message, type = 'info') {
        const id = Date.now()
        toasts.value.push({ id, message, type })
        setTimeout(() => hide(id), 3500)
    }

    /**
     * 依 id 手動移除（供關閉按鈕使用）
     * @param {number} id
     */
    function hide(id) {
        toasts.value = toasts.value.filter(t => t.id !== id)
    }

    return { toasts, show, hide }
}