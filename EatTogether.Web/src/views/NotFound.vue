<!-- <template>
    <div
        style="
            min-height: 100vh;
            background-color: var(--eat-surface);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 1.5rem;
            text-align: center;
            padding: 2rem;
        "
    >
        <h1 class="eat-h1">404 — 找不到頁面</h1>
        <p style="color: var(--eat-muted);">您造訪的頁面不存在或已移除。</p>
        <button class="btn-eat-primary" @click="router.push('/')">返回首頁</button>
    </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()
</script> -->

<template>
    <div class="container py-5">
        <!-- ─── 1. AvatarInitial ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">AvatarInitial</p>
            <div class="d-flex gap-4 align-items-center flex-wrap">
                <!-- 有頭像（會 404 → 降級為預設圖） -->
                <div class="text-center">
                    <AvatarInitial avatarFileName="test.jpg" name="王小明" size="60px" />
                    <small class="eat-body-muted d-block mt-2">有 avatarFileName（404 降級）</small>
                </div>
                <!-- 無頭像、有名字 → 預設圖，圖片正常載入就不會到名字首字 -->
                <div class="text-center">
                    <AvatarInitial name="王小明" size="60px" />
                    <small class="eat-body-muted d-block mt-2">無 avatarFileName、有 name</small>
                </div>
                <!-- 都沒有 → 預設圖 -->
                <div class="text-center">
                    <AvatarInitial size="60px" />
                    <small class="eat-body-muted d-block mt-2">都沒有</small>
                </div>
                <!-- interactive -->
                <div class="text-center">
                    <AvatarInitial name="王小明" size="60px" interactive />
                    <small class="eat-body-muted d-block mt-2">interactive（hover 看效果）</small>
                </div>
            </div>
        </div>

        <!-- ─── 2. FormErrorMessage ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">FormErrorMessage</p>
            <p class="eat-body-muted mb-1">下方應只出現一則錯誤：</p>
            <FormErrorMessage :show="true" message="這是一則錯誤訊息" />
            <FormErrorMessage :show="false" message="這則不應該出現" />
            <FormErrorMessage :show="true" message="" />
        </div>

        <!-- ─── 3. LoadingSpinner 行內版 ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">LoadingSpinner 行內版</p>
            <div class="d-flex flex-column gap-3">
                <LoadingSpinner size="sm" message="小尺寸載入中..." />
                <LoadingSpinner size="md" message="中尺寸載入中..." />
                <LoadingSpinner size="lg" message="大尺寸載入中..." />
            </div>
        </div>

        <!-- ─── 4. LoadingSpinner 全螢幕版 ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">LoadingSpinner 全螢幕版</p>
            <button class="btn-eat-primary" @click="showFullscreen">
                點我顯示全螢幕（2 秒後自動關閉）
            </button>
            <LoadingSpinner v-if="isFullscreenVisible" fullscreen message="資料載入中" />
        </div>

        <!-- ─── 5. EatSelect ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">EatSelect</p>
            <div class="form-eat" style="max-width: 240px">
                <label class="form-label">用餐方式</label>
                <EatSelect v-model="selectedOption" :options="eatOptions" placeholder="請選擇" />
            </div>
            <p class="eat-body-muted mt-3">
                目前選取：<strong class="text-eat-primary">{{
                    selectedOption?.label ?? '尚未選擇'
                }}</strong>
            </p>
        </div>

        <!-- ─── 6. Toast ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-3">Toast 通知</p>
            <div class="d-flex gap-3 flex-wrap">
                <button
                    class="btn-eat-primary btn-eat-sm"
                    @click="toast.show('操作成功！', 'success')"
                >
                    Success
                </button>
                <button
                    class="btn-eat-secondary btn-eat-sm"
                    @click="toast.show('注意事項', 'warning')"
                >
                    Warning
                </button>
                <button class="btn-eat-danger btn-eat-sm" @click="toast.show('發生錯誤', 'error')">
                    Error
                </button>
                <button class="btn-eat-tertiary btn-eat-sm" @click="toast.show('一般通知', 'info')">
                    Info
                </button>
            </div>
        </div>

        <!-- ─── 7. form-eat 表單樣式 ─── -->
        <div class="card-eat p-4 mb-4">
            <p class="eat-label mb-4">form-eat 表單樣式</p>
            <div class="form-eat">
                <div class="row g-4">
                    <!-- 一般 input -->
                    <div class="col-md-6">
                        <label class="form-label">姓名（正常）</label>
                        <input type="text" class="form-control" placeholder="請輸入姓名" />
                    </div>

                    <!-- is-invalid -->
                    <div class="col-md-6">
                        <label class="form-label">Email（錯誤狀態）</label>
                        <input type="email" class="form-control is-invalid" value="not-an-email" />
                        <div class="invalid-feedback">Email 格式不正確</div>
                    </div>

                    <!-- disabled -->
                    <div class="col-md-6">
                        <label class="form-label">帳號（disabled）</label>
                        <input type="text" class="form-control" value="user123" disabled />
                    </div>

                    <!-- textarea -->
                    <div class="col-md-6">
                        <label class="form-label">備註</label>
                        <textarea
                            class="form-control"
                            placeholder="請輸入備註..."
                            rows="3"
                        ></textarea>
                    </div>

                    <!-- checkbox -->
                    <div class="col-md-6">
                        <label class="form-label d-block mb-2">Checkbox</label>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="chk1" checked />
                            <label class="form-check-label" for="chk1">接受電子報（checked）</label>
                        </div>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="chk2" />
                            <label class="form-check-label" for="chk2">記住我（unchecked）</label>
                        </div>
                    </div>

                    <!-- radio -->
                    <div class="col-md-6">
                        <label class="form-label d-block mb-2">Radio</label>
                        <div class="form-check">
                            <input
                                class="form-check-input"
                                type="radio"
                                name="dineType"
                                id="r1"
                                checked
                            />
                            <label class="form-check-label" for="r1">內用</label>
                        </div>
                        <div class="form-check">
                            <input class="form-check-input" type="radio" name="dineType" id="r2" />
                            <label class="form-check-label" for="r2">外帶</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import AvatarInitial from '@/components/member/AvatarInitial.vue'
import FormErrorMessage from '@/components/common/FormErrorMessage.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import EatSelect from '@/components/common/EatSelect.vue'
import { useToast } from '@/composables/useToast.js'

const toast = useToast()

// EatSelect
const selectedOption = ref(null)
const eatOptions = [
    { value: 1, label: '內用' },
    { value: 2, label: '外帶' },
    { value: 3, label: '外送' },
]

// LoadingSpinner 全螢幕
const isFullscreenVisible = ref(false)
function showFullscreen() {
    isFullscreenVisible.value = true
    setTimeout(() => {
        isFullscreenVisible.value = false
    }, 2000)
}
</script>
