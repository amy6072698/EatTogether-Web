/**
 * imageUpload.js — 頭像上傳工具
 *
 * 提供圖片檔案驗證與本地預覽 URL 產生功能，供 Profile.vue 使用。
 *
 * 使用方式：
 *
 *   import { validateImageFile, previewImage } from '@/utils/imageUpload.js'
 *
 *   // 1. 驗證
 *   const result = validateImageFile(file)
 *   if (!result.valid) {
 *     errorMessage.value = result.error
 *     return
 *   }
 *
 *   // 2. 本地預覽
 *   const previewUrl = previewImage(file)
 *   avatarPreview.value = previewUrl
 *   // 上傳完成或元件 unmount 時釋放記憶體：
 *   // URL.revokeObjectURL(previewUrl)
 */

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE_BYTES = 2 * 1024 * 1024 // 2MB

/**
 * 驗證圖片檔案的格式與大小。
 *
 * @param {File} file - 使用者選取的 File 物件
 * @returns {{ valid: true } | { valid: false, error: string }}
 */
export function validateImageFile(file) {
    if (!ALLOWED_TYPES.includes(file.type)) {
        return { valid: false, error: '僅支援 JPG、PNG、WebP 格式' }
    }
    if (file.size > MAX_SIZE_BYTES) {
        return { valid: false, error: '圖片大小不可超過 2MB' }
    }
    return { valid: true }
}

/**
 * 產生圖片的本地預覽 URL（Object URL）。
 *
 * ⚠️ 呼叫端須在適當時機（上傳完成或元件 unmount）呼叫
 *    URL.revokeObjectURL(url) 釋放記憶體，避免記憶體洩漏。
 *
 * @param {File} file - 使用者選取的 File 物件
 * @returns {string} Object URL 字串
 */
export function previewImage(file) {
    return URL.createObjectURL(file)
}
