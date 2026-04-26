import { useToast } from '@/composables/useToast.js'

// ── 環境變數保護 ────────────────────────────────────────
// 若 .env.development 未設定此變數，立即報出有意義的錯誤
// 而非讓所有請求默默打到 undefined/xxx
if (!import.meta.env.VITE_API_BASE_URL) {
    throw new Error('[apiFetch] VITE_API_BASE_URL 未定義，請檢查 .env.development')
}
const BASE_URL = import.meta.env.VITE_API_BASE_URL

// ── 401 Refresh 競態防護 ────────────────────────────────
// 模組層級變數，所有 apiFetch 呼叫共用
// 確保多個同時 401 的請求只觸發一次 refresh，避免 refresh token 輪換衝突
let refreshPromise = null

// ── auth:expired 重複觸發防護 ───────────────────────────
// 確保多個同時 refresh 失敗的請求只執行一次清空與事件派發
let authExpiredHandling = false

/**
 * 封裝原生 fetch，統一處理：
 *  - credentials: 'include'（JWT 存於 HttpOnly Cookie，跨域必須帶）
 *  - 網路錯誤（connection refused / timeout）統一顯示 Toast 並 throw
 *  - 401 自動 Refresh Token（含競態防護）→ 重送原始請求
 *  - 5xx 顯示「伺服器發生錯誤」Toast
 *
 * @param {string} path         - 相對路徑，例如 '/auth/login'（不加 /api 前綴，BASE_URL 已含）
 * @param {RequestInit} options - 同原生 fetch options，body 由呼叫端自行 JSON.stringify
 * @returns {Promise<Response>}  200 / 400 / 403 / 404 等交由呼叫端自行處理
 * @throws {Error}              網路錯誤時 throw，呼叫端的 catch 可顯示頁面層級錯誤狀態
 */
async function apiFetch(path, options = {}) {
    // useToast 在函式內取用，確保 Vue 已初始化後才呼叫
    const { show } = useToast()
    const url = `${BASE_URL}${path}`

    // ── 組合 Headers ────────────────────────────────────────
    // 展開呼叫端傳入的 headers，再依 body 類型決定是否補上 Content-Type
    const headers = { ...options.headers }

    // FormData 必須讓瀏覽器自動設定 multipart/form-data boundary
    // 手動設定 Content-Type 會導致 boundary 遺失，後端無法解析上傳的檔案
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
        credentials: 'include', // JWT 存於 HttpOnly Cookie，跨域傳遞必須帶
    }

    // ── 發送請求（捕捉網路層錯誤）──────────────────────────
    // fetch 在網路中斷、connection refused、DNS 失敗時會 throw（不回傳 response）
    // 此處統一攔截，顯示 Toast 後重新 throw，讓呼叫端 catch 可處理頁面層級的錯誤狀態
    let response
    try {
        response = await fetch(url, fetchOptions)
    } catch {
        show('網路錯誤，請確認網路連線或稍後再試', 'error')
        throw new Error(`[apiFetch] 網路錯誤：${path}`)
    }

    // ── 401：Token 過期，嘗試 Refresh ───────────────────────
    if (response.status === 401) {
        // 防無限遞迴：refresh 本身回傳 401 時直接回傳，不再嘗試
        if (path === '/auth/refresh') {
            return response
        }

        // 競態防護：多個請求同時 401 時，共用同一個 refresh Promise
        // 第一個進來的請求建立 Promise，後續的直接等待同一個
        // finally 確保 refresh 結束後清空，下次 token 過期時可重新觸發
        if (!refreshPromise) {
            refreshPromise = fetch(`${BASE_URL}/auth/refresh`, {
                method: 'POST',
                credentials: 'include',
            }).finally(() => {
                refreshPromise = null
            })
        }

        let refreshResponse
        try {
            refreshResponse = await refreshPromise
        } catch {
            // refresh 請求本身遇到網路錯誤
            handleAuthExpired()
            return response
        }

        if (refreshResponse.ok) {
            // Refresh 成功 → 重送原始請求
            // 重新建構 headers，為 P3 CSRF Token 補上後的擴充預留位置
            // TODO: P3 CSRF Token 補上後，在此處重新讀取最新的 csrf_token Cookie
            // const csrfToken = document.cookie.split('; ')
            //   .find(row => row.startsWith('csrf_token='))?.split('=')[1]
            // if (csrfToken) retryHeaders['X-CSRF-Token'] = csrfToken
            const retryHeaders = { ...headers }
            try {
                return await fetch(url, { ...fetchOptions, headers: retryHeaders })
            } catch {
                show('網路錯誤，請確認網路連線或稍後再試', 'error')
                throw new Error(`[apiFetch] 重送請求網路錯誤：${path}`)
            }
        }

        // Refresh 失敗（refresh token 已過期或無效）
        // → 清空登入狀態，透過自訂事件通知 App.vue 處理導頁與開啟登入 Modal
        handleAuthExpired()
        return response
    }

    // ── 5xx：伺服器錯誤 ─────────────────────────────────────
    if (response.status >= 500) {
        show('伺服器發生錯誤，請稍後再試', 'error')
        return response
    }

    // ── 其他（200、400、403、404 等）：交由呼叫端自行處理 ────────
    return response
}

/**
 * Refresh Token 失敗後的處理
 * - 透過 store 的 clearAuth action 清空前端登入狀態（不呼叫 logout API）
 * - 發出 auth:expired 自訂事件，由 App.vue 監聽後統一處理導頁與開啟登入 Modal
 * - authExpiredHandling flag 確保多個同時失敗的請求只執行一次
 */
async function handleAuthExpired() {
    if (authExpiredHandling) return
    authExpiredHandling = true
    try {
        // 動態 import 避免循環依賴（apiFetch → auth store → apiFetch）
        const { useAuthStore } = await import('@/stores/auth.js')
        const authStore = useAuthStore()
        authStore.clearAuth()
        window.dispatchEvent(new CustomEvent('auth:expired'))
    } finally {
        authExpiredHandling = false
    }
}

export default apiFetch
