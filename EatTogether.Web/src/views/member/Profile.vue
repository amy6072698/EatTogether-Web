<script setup>
import { ref, onMounted, onUnmounted, onBeforeUnmount } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import { useToast } from '@/composables/useToast.js'
import apiFetch from '@/utils/apiFetch.js'
import { validateImageFile, previewImage } from '@/utils/imageUpload.js'
import { generateState, buildGoogleOAuthUrl } from '@/utils/googleOAuth.js'
import AvatarInitial from '@/components/member/AvatarInitial.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import ChangeEmailModal from '@/components/member/modals/ChangeEmailModal.vue'
import ChangePasswordModal from '@/components/member/modals/ChangePasswordModal.vue'
import CreateAccountModal from '@/components/member/modals/CreateAccountModal.vue'
import DeleteAccountModal from '@/components/member/modals/DeleteAccountModal.vue'

const authStore = useAuthStore()
const { show: showToast } = useToast()

// ── 基本資料表單（phone/birthDate 不放 authStore）─────────
const profileName = ref('')
const profilePhone = ref('')
const profileBirthDate = ref(null) // null 初始值，VueDatePicker onMounted 不觸發 format
const nameError = ref('')
const phoneError = ref('')
const isSavingProfile = ref(false)

// ── 頭像上傳 ──────────────────────────────────────────────
const previewUrl = ref(null)
const isUploading = ref(false)
const fileInput = ref(null)

// ── Email 變更提示 ────────────────────────────────────────
const emailChangeSent = ref(false)

let isMounted = true
let birthDateTimer = null

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
        if (!isMounted) return
        profileName.value = data.name
        profilePhone.value = data.phone ?? ''
        // setTimeout (macrotask) 保證在 VueDatePicker onMounted 之後才設值
        if (data.birthDate) {
            birthDateTimer = setTimeout(() => {
                if (!isMounted) return
                profileBirthDate.value = new Date(data.birthDate + 'T00:00:00')
            }, 0)
        }
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

// ── Modal success handlers ────────────────────────────────
function onEmailChangeSuccess() {
    emailChangeSent.value = true
}

function onPasswordChangeSuccess() {
    // Toast 由 ChangePasswordModal 處理，此處無需額外動作
}

function onCreateAccountSuccess() {
    authStore.member.hashedPasswordStatus = 'HAS_PASSWORD'
}

// ── Lifecycle ─────────────────────────────────────────────
onMounted(async () => {
    isMounted = true
    loadProfile()
})

onBeforeUnmount(() => {
    isMounted = false
    clearTimeout(birthDateTimer)
})

// 原本的 onUnmounted 移除掉 profileBirthDate 那行
onUnmounted(() => {
    // 釋放頭像預覽 Object URL，避免記憶體洩漏
    if (previewUrl.value) {
        URL.revokeObjectURL(previewUrl.value)
    }
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
                            :formats="{ input: 'yyyy/MM/dd' }"
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
            <p v-if="emailChangeSent" class="eat-body-muted mt-2 mb-0">
                驗證信已寄出，請至新信箱點擊連結完成變更
            </p>
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
    <ChangeEmailModal :currentEmail="authStore.member.email" @success="onEmailChangeSuccess" />
    <ChangePasswordModal @success="onPasswordChangeSuccess" />
    <CreateAccountModal @success="onCreateAccountSuccess" />
    <DeleteAccountModal :hasPassword="authStore.member.hashedPasswordStatus === 'HAS_PASSWORD'" />
</template>

<style scoped>
.avatar-preview {
    width: 100px;
    height: 100px;
    border-radius: 50%;
    object-fit: cover;
}
</style>
