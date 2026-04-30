<template>
  <div class="booking-page">
    <div class="container py-5">
      <h1 class="page-title text-center mb-5">線上訂位</h1>

      <!-- 訂位表單 -->
      <div class="booking-form-card">
        <form @submit.prevent="submitBooking" novalidate>
          <div class="row g-4">

            <!-- 日期 -->
            <div class="col-md-6">
              <label class="form-label required">訂位日期</label>
              <input
                v-model="form.date"
                type="date"
                class="form-control"
                :class="{ 'is-invalid': errors.date }"
                :min="minDateStr"
                :max="maxDateStr"
                @change="onDateTimeChange"
              />
              <div v-if="errors.date" class="invalid-feedback d-block">{{ errors.date }}</div>
            </div>

            <!-- 時段 -->
            <div class="col-md-6">
              <label class="form-label required">訂位時段</label>
              <div class="d-flex gap-2">
                <select
                  v-model="form.hour"
                  class="form-select"
                  :class="{ 'is-invalid': errors.time }"
                  @change="onDateTimeChange"
                >
                  <option value="">時</option>
                  <option v-for="h in hours" :key="h" :value="h">{{ String(h).padStart(2,'0') }} 時</option>
                </select>
                <select
                  v-model="form.minute"
                  class="form-select"
                  :class="{ 'is-invalid': errors.time }"
                  @change="onDateTimeChange"
                >
                  <option value="">分</option>
                  <option v-for="m in minutes" :key="m" :value="m">{{ String(m).padStart(2,'0') }} 分</option>
                </select>
              </div>
              <div v-if="errors.time" class="invalid-feedback d-block">{{ errors.time }}</div>
            </div>

            <!-- 大人人數 -->
            <div class="col-md-6">
              <label class="form-label required">大人人數</label>
              <div class="number-input">
                <button type="button" @click="form.adults = Math.max(1, form.adults - 1); onDateTimeChange()">−</button>
                <span>{{ form.adults }}</span>
                <button type="button" @click="form.adults = Math.min(10, form.adults + 1); onDateTimeChange()">+</button>
              </div>
              <div v-if="errors.people" class="invalid-feedback d-block">{{ errors.people }}</div>
            </div>

            <!-- 小孩人數 -->
            <div class="col-md-6">
              <label class="form-label">小孩人數</label>
              <div class="number-input">
                <button type="button" @click="form.children = Math.max(0, form.children - 1); onDateTimeChange()">−</button>
                <span>{{ form.children }}</span>
                <button type="button" @click="form.children = Math.min(10, form.children + 1); onDateTimeChange()">+</button>
              </div>
            </div>

            <!-- 即時可用性回饋 -->
            <div class="col-12" v-if="availability">
              <div
                class="availability-feedback"
                :class="availability.isAvailable ? 'available' : 'unavailable'"
              >
                <i :class="availability.isAvailable ? 'bi bi-check-circle-fill' : 'bi bi-x-circle-fill'"></i>
                {{ availability.message }}
              </div>
            </div>

            <!-- 姓名 -->
            <div class="col-md-6">
              <label class="form-label required">姓名</label>
              <input
                v-model.trim="form.name"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': errors.name }"
                placeholder="請輸入姓名"
              />
              <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
            </div>

            <!-- 電話 -->
            <div class="col-md-6">
              <label class="form-label required">電話</label>
              <input
                v-model.trim="form.phone"
                type="tel"
                class="form-control"
                :class="{ 'is-invalid': errors.phone }"
                placeholder="09xxxxxxxx"
              />
              <div v-if="errors.phone" class="invalid-feedback">{{ errors.phone }}</div>
            </div>

            <!-- Email -->
            <div class="col-12">
              <label class="form-label">Email <span class="text-muted">(選填，用於接收確認信)</span></label>
              <input
                v-model.trim="form.email"
                type="email"
                class="form-control"
                :class="{ 'is-invalid': errors.email }"
                placeholder="example@email.com"
              />
              <div v-if="errors.email" class="invalid-feedback">{{ errors.email }}</div>
            </div>

            <!-- 備註 -->
            <div class="col-12">
              <label class="form-label">備註</label>
              <textarea
                v-model.trim="form.remark"
                class="form-control"
                rows="3"
                placeholder="過敏食材、特殊需求等"
              ></textarea>
            </div>

            <!-- 提交 -->
            <div class="col-12 text-center">
              <button
                type="submit"
                class="btn-eat-primary px-5 py-3"
                :disabled="submitting || (availability && !availability.isAvailable)"
              >
                <span v-if="submitting" class="spinner-border spinner-border-sm me-2"></span>
                {{ submitting ? '訂位中…' : '確認訂位' }}
              </button>
            </div>

          </div>
        </form>
      </div>
    </div>

    <!-- 訂位查詢連結 -->
    <div class="text-center mt-4 pb-2">
      <span style="color:var(--eat-text-muted);font-size:.9rem">已有訂位？</span>
      <router-link to="/reservation/query" style="color:var(--eat-primary);font-size:.9rem;margin-left:6px">
        查詢 / 取消訂位 <i class="bi bi-arrow-right"></i>
      </router-link>
    </div>

    <!-- 成功 Modal -->
    <div v-if="successModal" class="modal-overlay" @click.self="successModal = false">
      <div class="success-modal">
        <div class="success-icon">🎉</div>
        <h3>訂位成功！</h3>
        <div class="booking-number">
          <span class="label">訂位單號</span>
          <span class="number">{{ bookingNumber }}</span>
        </div>
        <p class="hint">確認信已寄至您的 Email，請記錄訂位單號以便查詢</p>
        <div class="d-flex gap-3 justify-content-center mt-4">
          <router-link to="/reservation/query" class="btn btn-outline-secondary">查詢訂位</router-link>
          <button class="btn-eat-primary" @click="successModal = false">關閉</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'
import { useAuthStore } from '@/stores/auth.js'

const { show }  = useToast()
const authStore = useAuthStore()

// 時段選項
const hours   = Array.from({ length: 10 }, (_, i) => i + 11)  // 11~20
const minutes = [0, 15, 30, 45]

// 日期範圍（原生 input[type=date] 需要 "yyyy-MM-dd" 格式）
function toDateStr(d) {
  return d.toISOString().split('T')[0]
}
const minDateStr = computed(() => {
  const d = new Date()
  d.setMinutes(d.getMinutes() + 30)
  return toDateStr(d)
})
const maxDateStr = computed(() => {
  const d = new Date()
  d.setDate(d.getDate() + 90)
  return toDateStr(d)
})

// 表單狀態
const form = ref({
  date:     null,
  hour:     '',
  minute:   '',
  adults:   2,
  children: 0,
  name:     '',
  phone:    '',
  email:    '',
  remark:   ''
})

const errors      = ref({})
const availability = ref(null)
const submitting  = ref(false)
const successModal = ref(false)
const bookingNumber = ref('')

let debounceTimer = null

// 日期時間變更時觸發即時查詢
function onDateTimeChange() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    if (form.value.date && form.value.hour !== '' && form.value.minute !== '') {
      fetchAvailability()
    }
  }, 500)
}

async function fetchAvailability() {
  const dt = buildDateTime()
  if (!dt) return
  try {
    // 使用 Availability endpoint（含防線⑤桌型組數 + ⑥總容量70%）
    const res = await apiFetch(
      `/Reservations/Availability?date=${encodeURIComponent(toLocalISOString(dt))}&adults=${form.value.adults}&children=${form.value.children}`
    )
    if (res.ok) availability.value = await res.json()
  } catch { /* 忽略網路錯誤，不影響使用者操作 */ }
}

function buildDateTime() {
  if (!form.value.date || form.value.hour === '' || form.value.minute === '') return null
  const [y, m, day] = String(form.value.date).split('-').map(Number)
  return new Date(y, m - 1, day, Number(form.value.hour), Number(form.value.minute), 0, 0)
}

// 送給後端的本地時間字串（不帶 Z），避免 UTC 轉換後時段錯誤
// 例：台灣 17:00 → "2026-05-20T17:00:00"，後端以本地時間解析
function toLocalISOString(d) {
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`
}

// 前端七道驗證
function validate() {
  const e  = {}
  const dt = buildDateTime()
  const now = new Date()

  if (!form.value.date) { e.date = '請選擇訂位日期' }

  if (form.value.hour === '' || form.value.minute === '') {
    e.time = '請選擇訂位時段'
  } else if (dt) {
    const hour   = Number(form.value.hour)
    const minute = Number(form.value.minute)
    if (hour < 11 || hour > 20) e.time = '訂位時段須在 11:00 ~ 20:00 之間'
    else if (hour === 20 && minute > 0) e.time = '最晚訂位時段為 20:00'
    else if (![0, 15, 30, 45].includes(minute)) e.time = '訂位分鐘只允許 00、15、30、45'  // ③ 分鐘格式
    else if (dt < new Date(now.getTime() + 30 * 60000)) e.time = '訂位時間必須在 30 分鐘後'
  }

  const total = form.value.adults + form.value.children
  if (total < 1 || total > 10) e.people = '訂位人數須在 1 ~ 10 人之間'

  if (!form.value.name || form.value.name.length < 2 || form.value.name.length > 50)
    e.name = '請輸入姓名（2 ~ 50 字）'

  if (!/^09\d{8}$/.test(form.value.phone))
    e.phone = '請輸入有效的手機號碼（格式：09xxxxxxxx）'

  if (form.value.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
    e.email = 'Email 格式不正確'

  errors.value = e
  return Object.keys(e).length === 0
}

async function submitBooking() {
  if (!validate()) return

  submitting.value = true
  try {
    const dt = buildDateTime()
    const payload = {
      name:            form.value.name,
      phone:           form.value.phone,
      email:           form.value.email || null,
      reservationDate: toLocalISOString(dt),
      adultsCount:     form.value.adults,
      childrenCount:   form.value.children,
      remark:          form.value.remark || null
    }

    const res = await apiFetch('/Reservations', {
      method: 'POST',
      body:   JSON.stringify(payload)
    })

    if (res.ok) {
      const data = await res.json()
      bookingNumber.value = data.bookingNumber
      successModal.value  = true
      resetForm()
    } else {
      let message = '訂位失敗，請稍後再試'
      try {
        const err = await res.json()
        message = err.message || message
      } catch { /* 回應非 JSON 時使用預設訊息 */ }
      show(message, 'error')
    }
  } catch {
    show('網路連線錯誤，請確認後端服務是否啟動', 'error')
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  form.value = { date: null, hour: '', minute: '', adults: 2, children: 0, name: '', phone: '', email: '', remark: '' }
  errors.value = {}
  availability.value = null
}

// 登入狀態：自動填入會員資料
onMounted(async () => {
  if (!authStore.isLoggedIn) return
  try {
    const res = await apiFetch('/members/me')
    if (!res.ok) return
    const data = await res.json()
    if (data.name)  form.value.name  = data.name
    if (data.phone) form.value.phone = data.phone
    if (data.email) form.value.email = data.email
  } catch { /* 靜默，不影響訂位流程 */ }
})


</script>

<style scoped>
.booking-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }

.booking-form-card {
  background: rgba(255,255,255,.04); border: 1px solid rgba(180,120,30,.15);
  border-radius: 20px; padding: 40px; max-width: 800px; margin: 0 auto;
}
.form-label { color: var(--eat-text-secondary); font-size: .9rem; margin-bottom: 6px; }
.form-label.required::after { content: ' *'; color: #c0392b; }
.form-control, .form-select {
  background: rgba(255,255,255,.06); border: 1px solid rgba(180,120,30,.25);
  color: var(--eat-text-primary); border-radius: 8px;
}
.form-select option {
  background: #1e120b;
  color: var(--eat-text-primary);
}
.form-control:focus, .form-select:focus {
  background: rgba(255,255,255,.08); border-color: var(--eat-primary); box-shadow: none; color: var(--eat-text-primary);
}
/* 原生日期選擇器深色主題 */
input[type="date"]::-webkit-calendar-picker-indicator {
  filter: invert(0.8);
  cursor: pointer;
}

.number-input {
  display: flex; align-items: center; gap: 20px;
  background: rgba(255,255,255,.06); border: 1px solid rgba(180,120,30,.25);
  border-radius: 8px; padding: 8px 16px; width: fit-content;
}
.number-input button {
  background: none; border: none; color: var(--eat-primary);
  font-size: 1.4rem; cursor: pointer; line-height: 1; padding: 0 4px;
}
.number-input span { font-size: 1.2rem; color: var(--eat-text-primary); min-width: 24px; text-align: center; }

.availability-feedback {
  padding: 12px 20px; border-radius: 10px; font-size: .95rem;
  display: flex; align-items: center; gap: 8px;
}
.availability-feedback.available { background: rgba(60,179,113,.15); color: #3cb371; border: 1px solid #3cb371; }
.availability-feedback.unavailable { background: rgba(192,57,43,.15); color: #c0392b; border: 1px solid #c0392b; }

/* 成功 Modal */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.7);
  display: flex; align-items: center; justify-content: center; z-index: 9999;
}
.success-modal {
  background: #1e120b; border: 1px solid rgba(180,120,30,.4);
  border-radius: 20px; padding: 48px 40px; text-align: center; max-width: 420px; width: 90%;
}
.success-icon { font-size: 3rem; margin-bottom: 12px; }
.success-modal h3 { color: var(--eat-primary); margin-bottom: 20px; font-size: 1.6rem; }
.booking-number {
  background: rgba(255,255,255,.06); border-radius: 12px;
  padding: 16px 24px; margin-bottom: 16px; display: flex;
  flex-direction: column; gap: 4px;
}
.booking-number .label { font-size: .8rem; color: var(--eat-text-muted); }
.booking-number .number { font-size: 1.6rem; font-weight: 700; letter-spacing: 3px; color: var(--eat-primary); }
.hint { font-size: .85rem; color: var(--eat-text-muted); }
</style>
