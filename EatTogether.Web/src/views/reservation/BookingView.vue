<template>
  <div class="booking-page">
    <div class="container py-5">
      <h1 class="page-title text-center mb-5">線上訂位</h1>

      <!-- 各桌型即時剩餘空位 -->
      <div v-if="tableAvailability.length" class="table-availability mb-5">
        <h5 class="section-subtitle mb-3">各桌型即時座位狀況</h5>
        <div class="row g-3">
          <div
            v-for="slot in tableAvailability"
            :key="slot.seatCount"
            class="col-6 col-md-3"
          >
            <div class="seat-card" :class="slot.available > 0 ? 'available' : 'full'">
              <div class="seat-type">{{ slot.tableType }}</div>
              <div class="seat-count">
                <span class="available-num">{{ slot.available }}</span>
                <span class="total-num"> / {{ slot.total }}</span>
              </div>
              <div class="seat-label">剩餘桌數</div>
            </div>
          </div>
        </div>
      </div>

      <!-- 訂位表單 -->
      <div class="booking-form-card">
        <form @submit.prevent="submitBooking" novalidate>
          <div class="row g-4">

            <!-- 日期 -->
            <div class="col-md-6">
              <label class="form-label required">訂位日期</label>
              <VueDatePicker
                v-model="form.date"
                :min-date="minDate"
                :max-date="maxDate"
                :enable-time-picker="false"
                :format="'yyyy/MM/dd'"
                placeholder="選擇日期"
                @update:model-value="onDateTimeChange"
                auto-apply
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
import { ref, computed } from 'vue'
import { VueDatePicker } from '@vuepic/vue-datepicker'
import '@vuepic/vue-datepicker/dist/main.css'
import apiFetch from '@/utils/apiFetch.js'
import { useToast } from '@/composables/useToast.js'

const { show } = useToast()

// 時段選項
const hours   = Array.from({ length: 9 }, (_, i) => i + 11)  // 11~19
const minutes = [0, 15, 30, 45]

// 日期範圍
const minDate = computed(() => {
  const d = new Date()
  d.setMinutes(d.getMinutes() + 30)
  return d
})
const maxDate = computed(() => {
  const d = new Date()
  d.setDate(d.getDate() + 90)
  return d
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
const tableAvailability = ref([])
const submitting  = ref(false)
const successModal = ref(false)
const bookingNumber = ref('')

let debounceTimer = null

// 日期時間變更時觸發即時查詢
function onDateTimeChange() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    fetchTableAvailability()
    if (form.value.date && form.value.hour !== '' && form.value.minute !== '') {
      fetchAvailability()
    }
  }, 500)
}

async function fetchTableAvailability() {
  if (!form.value.date || form.value.hour === '' || form.value.minute === '') return
  const dt = buildDateTime()
  if (!dt) return
  try {
    const res = await apiFetch(`/Reservations/TableAvailability?date=${encodeURIComponent(dt.toISOString())}`)
    if (res.ok) tableAvailability.value = await res.json()
  } catch {}
}

async function fetchAvailability() {
  const dt = buildDateTime()
  if (!dt) return
  try {
    const res = await apiFetch(
      `/Reservations/Availability?date=${encodeURIComponent(dt.toISOString())}&adults=${form.value.adults}&children=${form.value.children}`
    )
    if (res.ok) availability.value = await res.json()
  } catch {}
}

function buildDateTime() {
  if (!form.value.date || form.value.hour === '' || form.value.minute === '') return null
  const d = new Date(form.value.date)
  d.setHours(Number(form.value.hour), Number(form.value.minute), 0, 0)
  return d
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
    const hour = Number(form.value.hour)
    if (hour < 11 || hour > 19) e.time = '訂位時段須在 11:00 ~ 19:45 之間'
    else if (hour === 19 && Number(form.value.minute) > 45) e.time = '最晚訂位時段為 19:45'
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
      reservationDate: dt.toISOString(),
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
      } catch {}
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


</script>

<style scoped>
.booking-page { min-height: 100vh; padding-top: 80px; background: var(--eat-bg); }
.page-title { font-family: var(--font-display); color: var(--eat-primary); font-size: 2rem; }
.section-subtitle { color: var(--eat-primary-light); letter-spacing: .05em; }

.seat-card {
  border-radius: 12px; padding: 16px; text-align: center;
  border: 1px solid rgba(180,120,30,.2); background: rgba(255,255,255,.04);
}
.seat-card.available { border-color: #3cb371; }
.seat-card.full { border-color: #c0392b; opacity: .7; }
.seat-type { font-size: .85rem; color: var(--eat-text-muted); margin-bottom: 6px; }
.available-num { font-size: 2rem; font-weight: 700; color: var(--eat-primary); }
.total-num { font-size: 1rem; color: var(--eat-text-muted); }
.seat-label { font-size: .75rem; color: var(--eat-text-muted); margin-top: 4px; }

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
.form-control:focus, .form-select:focus {
  background: rgba(255,255,255,.08); border-color: var(--eat-primary); box-shadow: none; color: var(--eat-text-primary);
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
