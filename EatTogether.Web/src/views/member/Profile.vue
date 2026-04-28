<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Modal } from 'bootstrap'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import { validateImageFile, previewImage } from '@/utils/imageUpload.js'
import { generateState, buildGoogleOAuthUrl } from '@/utils/googleOAuth.js'
import AvatarInitial from '@/components/member/AvatarInitial.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const router = useRouter()
const authStore = useAuthStore()
const { show: showToast } = useToast()

// ── 基本資料表單（phone/birthDate 不放 authStore）─────────
const profileName = ref('')
const profilePhone = ref('')
const profileBirthDate = ref(null) // Date | null，供 VueDatePicker 使用
const nameError = ref('')
const phoneError = ref('')
const isSavingProfile = ref(false)

// ── 頭像上傳 ──────────────────────────────────────────────
const previewUrl = ref(null)
const isUploading = ref(false)
const fileInput = ref(null)

// ── 修改 Email Modal ──────────────────────────────────────
const newEmail = ref('')
const newEmailError = ref('')
const isEmailSubmitting = ref(false)

// ── 修改密碼 Modal ────────────────────────────────────────
const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmNewPassword = ref(false)
const currentPasswordError = ref('')
const newPasswordError = ref('')
const confirmNewPasswordError = ref('')
const isPasswordSubmitting = ref(false)

// ── 建立帳號 Modal ────────────────────────────────────────
const newAccount = ref('')
const newAccountPassword = ref('')
const confirmAccountPassword = ref('')
const showAccountPassword = ref(false)
const showConfirmAccountPassword = ref(false)
const accountError = ref('')
const accountPasswordError = ref('')
const confirmAccountPasswordError = ref('')
const isCreateAccountSubmitting = ref(false)

// ── 刪除帳號 Modal ────────────────────────────────────────
const deletePassword = ref('')
const showDeletePassword = ref(false)
const deletePasswordError = ref('')
const isDeleting = ref(false)

let isMounted = true

// ── 工具：Date → YYYY-MM-DD（避免 toISOString 時區偏移）──
function toDateString(date) {
    if (!date) return null
    const d = new Date(date)
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// ── 初始化：取得完整會員資料（含 phone/birthDate）─────────
async function loadProfile() {
    try {
        const res = await apiFetch('/members/me')
        if (!res.ok) return
        const data = await res.json()
        if (!isMounted) return // 元件已卸載，不再更新狀態
        profileName.value = data.name
        profilePhone.value = data.phone ?? ''
        profileBirthDate.value = data.birthDate ? new Date(data.birthDate + 'T00:00:00') : null
    } catch (err) {
        if (import.meta.env.DEV) console.error('[loadProfile]', err)
    }
}

// ── 頭像上傳 ──────────────────────────────────────────────
function triggerFileInput() {
    fileInput.value?.click()
}

async function handleFileChange(event) {
    const file = event.target.files?.[0]
    if (!file) return
    event.target.value = '' // 清除 input，讓同一檔案可重複觸發

    const validation = validateImageFile(file)
    if (!validation.valid) {
        showToast(validation.error, 'error')
        return
    }

    const objectUrl = previewImage(file)
    previewUrl.value = objectUrl
    await uploadAvatar(file, objectUrl)
}

async function uploadAvatar(file, objectUrl) {
    isUploading.value = true
    try {
        const formData = new FormData()
        formData.append('file', file)

        const res = await apiFetch('/members/me/avatar', {
            method: 'POST',
            body: formData,
        })

        URL.revokeObjectURL(objectUrl)
        previewUrl.value = null

        if (res.ok) {
            await authStore.fetchMe()
            showToast('頭像已更新', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            showToast(data?.message || '上傳失敗，請稍後再試', 'error')
        }
    } catch (err) {
        URL.revokeObjectURL(objectUrl)
        previewUrl.value = null
        if (import.meta.env.DEV) console.error('[uploadAvatar]', err)
    } finally {
        isUploading.value = false
    }
}

// ── 基本資料 ──────────────────────────────────────────────
async function saveProfile() {
    nameError.value = ''
    phoneError.value = ''

    const trimmedName = profileName.value.trim()
    if (!trimmedName) {
        nameError.value = '姓名為必填'
        return
    }
    if (trimmedName.length > 50) {
        nameError.value = '姓名最多 50 字元'
        return
    }
    if (profilePhone.value && !/^\d{10}$/.test(profilePhone.value)) {
        phoneError.value = '手機格式不正確（請輸入 10 位數字）'
        return
    }

    isSavingProfile.value = true
    try {
        const res = await apiFetch('/members/me/profile', {
            method: 'PUT',
            body: JSON.stringify({
                name: trimmedName,
                phone: profilePhone.value || null,
                birthDate: toDateString(profileBirthDate.value),
            }),
        })

        if (res.ok) {
            await authStore.fetchMe()
            showToast('個人資料已更新', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'invalid_birth_date') {
                showToast('生日不可為未來日期', 'error')
            } else {
                showToast(data?.message || '更新失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[saveProfile]', err)
    } finally {
        isSavingProfile.value = false
    }
}

// ── 申請 Email 變更 ───────────────────────────────────────
async function submitEmailChange() {
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

    isEmailSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/email', {
            method: 'POST',
            body: JSON.stringify({ newEmail: trimmed }),
        })

        if (res.ok) {
            Modal.getInstance(document.querySelector('#changeEmailModal'))?.hide()
            newEmail.value = ''
            showToast('驗證信已寄出，請至新信箱確認', 'success')
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
        if (import.meta.env.DEV) console.error('[submitEmailChange]', err)
    } finally {
        isEmailSubmitting.value = false
    }
}

// ── 修改密碼 ──────────────────────────────────────────────
function resetPasswordForm() {
    currentPassword.value = ''
    newPassword.value = ''
    confirmNewPassword.value = ''
    showCurrentPassword.value = false
    showNewPassword.value = false
    showConfirmNewPassword.value = false
    currentPasswordError.value = ''
    newPasswordError.value = ''
    confirmNewPasswordError.value = ''
}

async function submitPasswordChange() {
    currentPasswordError.value = ''
    newPasswordError.value = ''
    confirmNewPasswordError.value = ''

    let hasError = false
    if (!currentPassword.value) {
        currentPasswordError.value = '請輸入目前密碼'
        hasError = true
    }
    if (!newPassword.value) {
        newPasswordError.value = '請輸入新密碼'
        hasError = true
    } else if (newPassword.value.length < 8) {
        newPasswordError.value = '新密碼至少 8 個字元'
        hasError = true
    } else if (newPassword.value.length > 128) {
        newPasswordError.value = '新密碼最多 128 個字元'
        hasError = true
    }
    if (!confirmNewPassword.value) {
        confirmNewPasswordError.value = '請確認新密碼'
        hasError = true
    } else if (confirmNewPassword.value !== newPassword.value) {
        confirmNewPasswordError.value = '兩次密碼輸入不一致'
        hasError = true
    }
    if (hasError) return

    isPasswordSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/password', {
            method: 'PUT',
            body: JSON.stringify({
                currentPassword: currentPassword.value,
                newPassword: newPassword.value,
            }),
        })

        if (res.ok) {
            Modal.getInstance(document.querySelector('#changePasswordModal'))?.hide()
            showToast('密碼已更新', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'wrong_password') {
                currentPasswordError.value = '舊密碼不正確'
            } else {
                showToast(data?.message || '修改失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[submitPasswordChange]', err)
    } finally {
        isPasswordSubmitting.value = false
    }
}

// ── 建立一般帳號 ──────────────────────────────────────────
async function submitCreateAccount() {
    accountError.value = ''
    accountPasswordError.value = ''
    confirmAccountPasswordError.value = ''

    let hasError = false
    if (!newAccount.value.trim()) {
        accountError.value = '帳號為必填'
        hasError = true
    } else if (!/^[a-zA-Z0-9_]{3,50}$/.test(newAccount.value)) {
        accountError.value = '帳號限英數字及底線，3–50 字元'
        hasError = true
    }
    if (!newAccountPassword.value) {
        accountPasswordError.value = '密碼為必填'
        hasError = true
    } else if (newAccountPassword.value.length < 8) {
        accountPasswordError.value = '密碼至少 8 個字元'
        hasError = true
    } else if (newAccountPassword.value.length > 128) {
        accountPasswordError.value = '密碼最多 128 個字元'
        hasError = true
    }
    if (!confirmAccountPassword.value) {
        confirmAccountPasswordError.value = '請確認密碼'
        hasError = true
    } else if (confirmAccountPassword.value !== newAccountPassword.value) {
        confirmAccountPasswordError.value = '兩次密碼輸入不一致'
        hasError = true
    }
    if (hasError) return

    isCreateAccountSubmitting.value = true
    try {
        const res = await apiFetch('/members/me/create-account', {
            method: 'POST',
            body: JSON.stringify({
                account: newAccount.value,
                password: newAccountPassword.value,
            }),
        })

        if (res.ok) {
            Modal.getInstance(document.querySelector('#createAccountModal'))?.hide()
            authStore.member.hashedPasswordStatus = 'HAS_PASSWORD'
            showToast('帳號建立成功', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            if (data?.errorCode === 'account_taken') {
                accountError.value = '帳號已被使用，請換一個'
            } else {
                showToast(data?.message || '建立失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[submitCreateAccount]', err)
    } finally {
        isCreateAccountSubmitting.value = false
    }
}

// ── Google 連結 ───────────────────────────────────────────
async function unlinkGoogle() {
    try {
        const res = await apiFetch('/members/me/google-link', { method: 'DELETE' })

        if (res.ok) {
            await authStore.fetchMe()
            showToast('已取消與 Google 帳號的連結', 'success')
            return
        }

        if (res.status < 500) {
            const data = await res.json()
            showToast(data?.message || '取消連結失敗，請稍後再試', 'error')
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[unlinkGoogle]', err)
    }
}

function linkGoogle() {
    const state = generateState('/member')
    const url = buildGoogleOAuthUrl(state)
    window.location.href = url
}

// ── 刪除帳號 ──────────────────────────────────────────────
async function deleteAccount() {
    deletePasswordError.value = ''

    if (authStore.member.hashedPasswordStatus === 'HAS_PASSWORD' && !deletePassword.value) {
        deletePasswordError.value = '請輸入密碼以確認刪除'
        return
    }

    isDeleting.value = true
    try {
        const fetchOptions = { method: 'DELETE' }
        if (authStore.member.hashedPasswordStatus === 'HAS_PASSWORD') {
            fetchOptions.body = JSON.stringify({ password: deletePassword.value })
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
                deletePasswordError.value = '密碼不正確'
            } else {
                showToast(data?.message || '刪除失敗，請稍後再試', 'error')
            }
        }
    } catch (err) {
        if (import.meta.env.DEV) console.error('[deleteAccount]', err)
    } finally {
        isDeleting.value = false
    }
}

// ── Lifecycle ─────────────────────────────────────────────
const handlePasswordModalHidden = resetPasswordForm
const handleDeleteModalHidden = () => {
    deletePassword.value = ''
    showDeletePassword.value = false
    deletePasswordError.value = ''
}

onMounted(async () => {
    isMounted = true
    await loadProfile()

    // 修改密碼 Modal：關閉時重置欄位與錯誤狀態
    const pwModalEl = document.querySelector('#changePasswordModal')
    pwModalEl?.addEventListener('hidden.bs.modal', handlePasswordModalHidden)

    // 刪除帳號 Modal：關閉時清空密碼欄位
    const deleteModalEl = document.querySelector('#deleteAccountModal')
    deleteModalEl?.addEventListener('hidden.bs.modal', handleDeleteModalHidden)
})

onUnmounted(() => {
    isMounted = false
    // 先清空 VueDatePicker 的值，避免元件卸載時內部 watcher 報錯
    profileBirthDate.value = null

    // 釋放頭像預覽 Object URL，避免記憶體洩漏
    if (previewUrl.value) {
        URL.revokeObjectURL(previewUrl.value)
    }

    // 強制關閉所有可能開著的 Modal，避免 Bootstrap 操作已卸載的 DOM
    ;[
        '#changeEmailModal',
        '#changePasswordModal',
        '#createAccountModal',
        '#deleteAccountModal',
    ].forEach((id) => {
        const el = document.querySelector(id)
        if (el) {
            const instance = Modal.getInstance(el)
            instance?.hide()
        }
    })

    if (previewUrl.value) {
        URL.revokeObjectURL(previewUrl.value)
    }

    const pwModalEl = document.querySelector('#changePasswordModal')
    pwModalEl?.removeEventListener('hidden.bs.modal', handlePasswordModalHidden)

    const deleteModalEl = document.querySelector('#deleteAccountModal')
    deleteModalEl?.removeEventListener('hidden.bs.modal', handleDeleteModalHidden)
})
</script>

<template>
    <!-- 主內容 -->
    <div class="d-flex flex-column gap-4">
        <!-- ── 頭像區塊 ── -->
        <div class="card-eat p-4 text-center">
            <div class="mb-3 d-flex justify-content-center">
                <div v-if="isUploading">
                    <LoadingSpinner size="sm" message="上傳中..." />
                </div>
                <img
                    v-else-if="previewUrl"
                    :src="previewUrl"
                    alt="頭像預覽"
                    class="avatar-preview"
                />
                <AvatarInitial
                    v-else
                    :avatarFileName="authStore.member.avatarFileName"
                    :name="authStore.member.name"
                    size="100px"
                    interactive
                    @click="triggerFileInput"
                />
            </div>
            <input
                ref="fileInput"
                type="file"
                accept="image/jpeg,image/png,image/webp"
                class="d-none"
                @change="handleFileChange"
            />
            <button
                type="button"
                class="eat-body-muted"
                style="background: none; border: none; cursor: pointer"
                @click="triggerFileInput"
            >
                更換頭像
            </button>
        </div>

        <!-- ── 基本資料 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3 mb-4">基本資料</h2>
            <div class="form-eat">
                <div class="d-flex flex-column gap-3">
                    <!-- 姓名 -->
                    <div>
                        <label class="form-label">姓名</label>
                        <input
                            v-model="profileName"
                            type="text"
                            class="form-control"
                            placeholder="請輸入姓名"
                            autocomplete="name"
                        />
                        <FormErrorMessage :message="nameError" :show="!!nameError" />
                    </div>
                    <!-- 手機 -->
                    <div>
                        <label class="form-label">手機</label>
                        <input
                            v-model="profilePhone"
                            type="tel"
                            class="form-control"
                            placeholder="0912345678"
                            autocomplete="tel"
                        />
                        <FormErrorMessage :message="phoneError" :show="!!phoneError" />
                    </div>
                    <!-- 生日 -->
                    <div>
                        <label class="form-label">生日</label>
                        <VueDatePicker
                            v-model="profileBirthDate"
                            :max-date="new Date()"
                            :enable-time-picker="false"
                            locale="zh-TW"
                            auto-apply
                        />
                    </div>
                </div>
                <button
                    type="button"
                    class="btn-eat-primary mt-4"
                    :disabled="isSavingProfile"
                    @click="saveProfile"
                >
                    <span v-if="isSavingProfile" class="spinner-border-eat spinner-sm"></span>
                    {{ isSavingProfile ? '儲存中...' : '儲存變更' }}
                </button>
            </div>
        </div>

        <!-- ── Email 設定 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3 mb-3">Email 設定</h2>
            <div class="form-eat">
                <label class="form-label">目前 Email</label>
                <input class="form-control" :value="authStore.member.email" disabled />
            </div>
            <button
                type="button"
                class="btn-eat-secondary btn-eat-sm mt-3"
                data-bs-toggle="modal"
                data-bs-target="#changeEmailModal"
            >
                修改 Email
            </button>
        </div>

        <!-- ── 帳號設定 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3 mb-3">帳號設定</h2>
            <template v-if="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'">
                <p class="eat-body-muted mb-0">您的帳號已設定密碼</p>
                <button
                    type="button"
                    class="btn-eat-secondary btn-eat-sm mt-2"
                    data-bs-toggle="modal"
                    data-bs-target="#changePasswordModal"
                >
                    修改密碼
                </button>
            </template>
            <template v-else>
                <p class="eat-body-muted mb-0">您目前使用 Google 帳號登入，尚未建立一般帳號</p>
                <button
                    type="button"
                    class="btn-eat-primary btn-eat-sm mt-2"
                    data-bs-toggle="modal"
                    data-bs-target="#createAccountModal"
                >
                    建立一般帳號
                </button>
            </template>
        </div>

        <!-- ── Google 帳號連結 ── -->
        <div class="card-eat p-4">
            <h2 class="eat-h3 mb-3">Google 帳號連結</h2>
            <template v-if="authStore.member.googleLinked">
                <span class="badge-eat-new mb-3 d-inline-block">已連結 Google</span>
                <div>
                    <button
                        type="button"
                        class="btn-eat-danger btn-eat-sm"
                        :disabled="
                            authStore.member.hashedPasswordStatus === 'EXTERNAL_LOGIN_NO_PASSWORD'
                        "
                        :title="
                            authStore.member.hashedPasswordStatus === 'EXTERNAL_LOGIN_NO_PASSWORD'
                                ? '請先建立一般帳號才能取消連結'
                                : ''
                        "
                        @click="unlinkGoogle"
                    >
                        取消連結
                    </button>
                </div>
            </template>
            <template v-else>
                <p class="eat-body-muted mb-0">尚未連結 Google 帳號</p>
                <button type="button" class="btn-eat-secondary btn-eat-sm mt-2" @click="linkGoogle">
                    前往連結 Google
                </button>
            </template>
        </div>

        <!-- ── 危險操作 ── -->
        <div class="card-eat p-4" style="border-color: var(--eat-error)">
            <h2 class="eat-h3 mb-2" style="color: var(--eat-error)">危險操作</h2>
            <p class="eat-body-muted mb-0">刪除帳號後所有資料將永久移除，無法復原。</p>
            <button
                type="button"
                class="btn-eat-danger mt-3"
                data-bs-toggle="modal"
                data-bs-target="#deleteAccountModal"
            >
                刪除帳號
            </button>
        </div>
    </div>

    <!-- ════════════════ Modals ════════════════ -->

    <!-- 修改 Email Modal -->
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
                            :disabled="isEmailSubmitting"
                            @click="submitEmailChange"
                        >
                            <span
                                v-if="isEmailSubmitting"
                                class="spinner-border-eat spinner-sm"
                            ></span>
                            {{ isEmailSubmitting ? '送出中...' : '送出申請' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 修改密碼 Modal -->
    <div id="changePasswordModal" class="modal fade" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm ms-auto me-2 mt-2"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>
                <div class="modal-header border-0 px-4 py-3">
                    <h5 class="eat-h3 fst-normal fw-bold fs-5 mb-0">修改密碼</h5>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <!-- 目前密碼 -->
                        <div>
                            <label class="form-label">目前密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="currentPassword"
                                    :type="showCurrentPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="請輸入目前密碼"
                                    autocomplete="current-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showCurrentPassword = !showCurrentPassword"
                                    :aria-label="showCurrentPassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i
                                        :class="
                                            showCurrentPassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                        "
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="currentPasswordError"
                                :show="!!currentPasswordError"
                            />
                        </div>
                        <!-- 新密碼 -->
                        <div>
                            <label class="form-label">新密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="newPassword"
                                    :type="showNewPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="至少 8 個字元"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showNewPassword = !showNewPassword"
                                    :aria-label="showNewPassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i
                                        :class="showNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="newPasswordError"
                                :show="!!newPasswordError"
                            />
                        </div>
                        <!-- 確認新密碼 -->
                        <div>
                            <label class="form-label">確認新密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="confirmNewPassword"
                                    :type="showConfirmNewPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="再次輸入新密碼"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showConfirmNewPassword = !showConfirmNewPassword"
                                    :aria-label="showConfirmNewPassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i
                                        :class="
                                            showConfirmNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                        "
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="confirmNewPasswordError"
                                :show="!!confirmNewPasswordError"
                            />
                        </div>
                        <button
                            type="button"
                            class="btn-eat-primary mt-2"
                            :disabled="isPasswordSubmitting"
                            @click="submitPasswordChange"
                        >
                            <span
                                v-if="isPasswordSubmitting"
                                class="spinner-border-eat spinner-sm"
                            ></span>
                            {{ isPasswordSubmitting ? '修改中...' : '確認修改' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 建立一般帳號 Modal -->
    <div id="createAccountModal" class="modal fade" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-eat-dialog">
            <div class="modal-content modal-eat-content">
                <button
                    type="button"
                    class="btn-close btn-close-white btn-sm ms-auto me-2 mt-2"
                    data-bs-dismiss="modal"
                    aria-label="關閉"
                ></button>
                <div class="modal-header border-0 px-4 py-3">
                    <h5 class="eat-h3 fst-normal fw-bold fs-5 mb-0">建立一般帳號</h5>
                </div>
                <div class="modal-body px-4 pb-4">
                    <div class="form-eat d-flex flex-column gap-3">
                        <!-- 帳號 -->
                        <div>
                            <label class="form-label">帳號</label>
                            <input
                                v-model="newAccount"
                                type="text"
                                class="form-control"
                                placeholder="限英數字及底線，3–50 字元"
                                autocomplete="username"
                            />
                            <FormErrorMessage :message="accountError" :show="!!accountError" />
                        </div>
                        <!-- 密碼 -->
                        <div>
                            <label class="form-label">密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="newAccountPassword"
                                    :type="showAccountPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="至少 8 個字元"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showAccountPassword = !showAccountPassword"
                                    :aria-label="showAccountPassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i
                                        :class="
                                            showAccountPassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                        "
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="accountPasswordError"
                                :show="!!accountPasswordError"
                            />
                        </div>
                        <!-- 確認密碼 -->
                        <div>
                            <label class="form-label">確認密碼</label>
                            <div class="position-relative">
                                <input
                                    v-model="confirmAccountPassword"
                                    :type="showConfirmAccountPassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="再次輸入密碼"
                                    autocomplete="new-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="
                                        showConfirmAccountPassword = !showConfirmAccountPassword
                                    "
                                    :aria-label="
                                        showConfirmAccountPassword ? '隱藏密碼' : '顯示密碼'
                                    "
                                >
                                    <i
                                        :class="
                                            showConfirmAccountPassword
                                                ? 'bi bi-eye-slash'
                                                : 'bi bi-eye'
                                        "
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="confirmAccountPasswordError"
                                :show="!!confirmAccountPasswordError"
                            />
                        </div>
                        <button
                            type="button"
                            class="btn-eat-primary mt-2"
                            :disabled="isCreateAccountSubmitting"
                            @click="submitCreateAccount"
                        >
                            <span
                                v-if="isCreateAccountSubmitting"
                                class="spinner-border-eat spinner-sm"
                            ></span>
                            {{ isCreateAccountSubmitting ? '建立中...' : '建立帳號' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 刪除帳號 Modal（data-bs-backdrop="static" 防止點外部關閉）-->
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
                        <div v-if="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'">
                            <label class="form-label">請輸入密碼確認</label>
                            <div class="position-relative">
                                <input
                                    v-model="deletePassword"
                                    :type="showDeletePassword ? 'text' : 'password'"
                                    class="form-control"
                                    placeholder="請輸入目前密碼"
                                    autocomplete="current-password"
                                />
                                <button
                                    type="button"
                                    class="btn-eat-password-toggle"
                                    @mousedown.prevent
                                    @click="showDeletePassword = !showDeletePassword"
                                    :aria-label="showDeletePassword ? '隱藏密碼' : '顯示密碼'"
                                >
                                    <i
                                        :class="
                                            showDeletePassword ? 'bi bi-eye-slash' : 'bi bi-eye'
                                        "
                                    ></i>
                                </button>
                            </div>
                            <FormErrorMessage
                                :message="deletePasswordError"
                                :show="!!deletePasswordError"
                            />
                        </div>
                        <button
                            type="button"
                            class="btn-eat-danger"
                            :disabled="isDeleting"
                            @click="deleteAccount"
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

<style scoped>
.avatar-preview {
    width: 100px;
    height: 100px;
    border-radius: 50%;
    object-fit: cover;
}
</style>
