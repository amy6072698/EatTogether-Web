import { useToast } from '@/composables/useToast.js'

const BASE_URL = import.meta.env.VITE_API_BASE_URL
const { show } = useToast()

/**
 * 封裝原生 fetch，統一處理：
 *  - credentials: 'include'（JWT 存於 HttpOnly Cookie，跨域必須帶）
 *  - 401 自動 Refresh Token → 重送原始請求
 *  - 403 顯示「無操作權限」Toast
 *  - 5xx 顯示「伺服器發生錯誤」Toast
 *
 * @param {string} path        - 相對路徑，例如 '/auth/login'
 * @param {RequestInit} options - 同原生 fetch options
 * @returns {Promise<Response>}
 */
async function apiFetch(path, options = {}) {
    const url = `${BASE_URL}${path}`

    // ── 組合 Headers ────────────────────────────────────────
    const headers = { ...options.headers }

    // FormData 讓瀏覽器自動設定 multipart/form-data boundary，其餘預設 JSON
    if (!(options.body instanceof FormData)) {
        headers['Content-Type'] = 'application/json'
    }

    // TODO: P3 Double Submit Cookie
    // X-CSRF-Token Header 擴充位置，待 P3 安全性強化時補上
    // const csrfToken = document.cookie
    //   .split('; ')
    //   .find(row => row.startsWith('csrf_token='))
    //   ?.split('=')[1]
    // if (csrfToken) headers['X-CSRF-Token'] = csrfToken

    const fetchOptions = {
        ...options,
        headers,
        credentials: 'include',  // JWT 存於 HttpOnly Cookie，跨域傳遞必須帶
    }

    const response = await fetch(url, fetchOptions)

    // ── 401：Token 過期，嘗試 Refresh ───────────────────────
    if (response.status === 401) {
        // 防無限遞迴：refresh 本身回傳 401 時直接回傳，不再嘗試
        if (path === '/auth/refresh') {
            return response
        }

        // 嘗試用 refresh_token Cookie 取得新的 access_token
        const refreshResponse = await fetch(`${BASE_URL}/auth/refresh`, {
            method: 'POST',
            credentials: 'include',
        })

        if (refreshResponse.ok) {
            // Refresh 成功 → 重送原始請求
            return fetch(url, fetchOptions)
        }

        // Refresh 失敗 → 清空登入狀態、導回首頁、開啟登入 Modal
        // 動態 import 避免循環依賴（apiFetch ← auth store ← apiFetch）
        const { useAuthStore } = await import('@/stores/auth.js')
        const { default: router } = await import('@/router/index.js')

        const authStore = useAuthStore()
        await authStore.logout()
        await router.push('/')

        const modalEl = document.querySelector('#authModal')
        if (modalEl) {
            const { default: bootstrap } = await import('bootstrap')
            bootstrap.Modal.getOrCreateInstance(modalEl).show()
        }

        return response
    }

    // ── 403：無操作權限 ─────────────────────────────────────
    if (response.status === 403) {
        show('無操作權限', 'error')
        return response
    }

    // ── 5xx：伺服器錯誤 ─────────────────────────────────────
    if (response.status >= 500) {
        show('伺服器發生錯誤，請稍後再試', 'error')
        return response
    }

    // ── 其他（200、400、404 等）：交由呼叫端自行處理 ────────
    return response
}

export default apiFetch;