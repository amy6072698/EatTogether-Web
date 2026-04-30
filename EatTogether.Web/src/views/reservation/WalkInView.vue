<template>
  <div class="walkin-page">

    <!-- Hero -->
    <div class="walkin-hero">
      <div class="hero-glow"></div>
      <div class="container position-relative">
        <p class="eat-label mb-2" style="color:var(--eat-secondary)">
          <i class="bi bi-people me-2"></i>WALK-IN QUEUE
        </p>
        <h1 class="eat-h1 fst-normal mb-2">現場候位</h1>
        <p class="eat-body-muted mb-0">
          目前等待：<span style="color:var(--eat-primary);font-weight:600">{{ todayStatus.waitingCount }}</span> 組&ensp;
          已叫號：<span style="color:var(--eat-secondary);font-weight:600">{{ todayStatus.calledCount }}</span> 組
        </p>
      </div>
    </div>

    <div class="container pb-5">

      <!-- Tab 切換 -->
      <div class="walkin-tabs mb-4">
        <button
          class="walkin-tab"
          :class="{ active: activeTab === 'register' }"
          @click="activeTab = 'register'"
        >
          <i class="bi bi-person-plus me-1"></i>登記候位
        </button>
        <button
          class="walkin-tab"
          :class="{ active: activeTab === 'query' }"
          @click="activeTab = 'query'; resetQuery()"
        >
          <i class="bi bi-search me-1"></i>查詢號碼
        </button>
      </div>

      <!-- ═══ 登記候位 Tab ═══ -->
      <Transition name="fade-slide" mode="out-in">
        <div v-if="activeTab === 'register'" key="register">

          <!-- 登記成功卡片 -->
          <div v-if="registered" class="ticket-card mb-4">
            <div class="ticket-header">
              <i class="bi bi-ticket-perforated me-2"></i>候位號碼牌
            </div>
            <div class="ticket-number">{{ registered.queueNumber }}</div>
            <div class="ticket-name">{{ registered.name }}・{{ registered.adultsCount + registered.childrenCount }} 人</div>
            <div class="ticket-status">
              <span class="status-badge status-waiting">
                <i class="bi bi-hourglass-split me-1"></i>{{ registered.statusText }}
              </span>
            </div>
            <div v-if="registered.groupsAhead > 0" class="ticket-ahead">
              前方還有 <strong>{{ registered.groupsAhead }}</strong> 組
            </div>
            <div v-else class="ticket-ahead" style="color:var(--eat-primary)">
              <i class="bi bi-star me-1"></i>您是下一組！請注意叫號
            </div>
            <div class="ticket-time">登記時間：{{ formatTime(registered.registeredAt) }}</div>
            <div class="d-flex justify-content-center gap-3 mt-4">
              <Button variant="secondary" @click="leaveQueue(registered.id)" :loading="leaving">
                <i class="bi bi-x-circle me-1"></i>取消候位
              </Button>
              <Button variant="primary" @click="registered = null">
                <i class="bi bi-plus-circle me-1"></i>重新登記
              </Button>
            </div>
          </div>

          <!-- 登記表單 -->
          <div v-else class="card-eat p-4 p-md-5">
            <h2 class="eat-h3 mb-4">填寫候位資訊</h2>

            <div class="row g-3">
              <!-- 姓名 -->
              <div class="col-12 col-md-6">
                <label class="form-label-eat">姓名 <span class="text-danger">*</span></label>
                <input
                  v-model="form.name"
                  type="text"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': errors.name }"
                  placeholder="請輸入姓名"
                  maxlength="50"
                />
                <div v-if="errors.name" class="invalid-feedback">{{ errors.name }}</div>
              </div>

              <!-- 電話 -->
              <div class="col-12 col-md-6">
                <label class="form-label-eat">手機號碼 <span class="text-danger">*</span></label>
                <input
                  v-model="form.phone"
                  type="tel"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': errors.phone }"
                  placeholder="09xxxxxxxx"
                  maxlength="10"
                />
                <div v-if="errors.phone" class="invalid-feedback">{{ errors.phone }}</div>
              </div>

              <!-- 大人人數 -->
              <div class="col-6 col-md-3">
                <label class="form-label-eat">大人人數 <span class="text-danger">*</span></label>
                <select
                  v-model="form.adultsCount"
                  class="form-select form-select-eat"
                  :class="{ 'is-invalid': errors.people }"
                >
                  <option v-for="n in 10" :key="n" :value="n">{{ n }} 位</option>
                </select>
              </div>

              <!-- 小孩人數 -->
              <div class="col-6 col-md-3">
                <label class="form-label-eat">兒童人數</label>
                <select v-model="form.childrenCount" class="form-select form-select-eat">
                  <option :value="0">0 位</option>
                  <option v-for="n in 10" :key="n" :value="n">{{ n }} 位</option>
                </select>
              </div>

              <!-- 備註 -->
              <div class="col-12">
                <label class="form-label-eat">備註（選填）</label>
                <input
                  v-model="form.remark"
                  type="text"
                  class="form-control form-control-eat"
                  placeholder="如：需嬰兒椅、過敏食材…"
                  maxlength="200"
                />
              </div>

              <!-- 人數錯誤 -->
              <div v-if="errors.people" class="col-12">
                <div class="text-danger small">{{ errors.people }}</div>
              </div>
            </div>

            <!-- 送出錯誤 -->
            <div v-if="submitError" class="alert-eat alert-eat--error mt-3">
              <i class="bi bi-exclamation-circle me-2"></i>{{ submitError }}
            </div>

            <div class="d-flex justify-content-center mt-4">
              <Button variant="primary" size="lg" @click="submitRegister" :loading="submitting">
                <i class="bi bi-ticket-perforated me-2"></i>確認登記候位
              </Button>
            </div>
          </div>

        </div>
      </Transition>

      <!-- ═══ 查詢號碼 Tab ═══ -->
      <Transition name="fade-slide" mode="out-in">
        <div v-if="activeTab === 'query'" key="query">
          <div class="card-eat p-4 p-md-5">
            <h2 class="eat-h3 mb-4">查詢候位狀態</h2>

            <div class="row g-3 align-items-end">
              <div class="col-12 col-md-8">
                <label class="form-label-eat">手機號碼</label>
                <input
                  v-model="queryPhone"
                  type="tel"
                  class="form-control form-control-eat"
                  :class="{ 'is-invalid': queryError }"
                  placeholder="09xxxxxxxx"
                  maxlength="10"
                  @keyup.enter="submitQuery"
                />
                <div v-if="queryError" class="invalid-feedback d-block">{{ queryError }}</div>
              </div>
              <div class="col-12 col-md-4">
                <Button variant="primary" class="w-100" @click="submitQuery" :loading="querying">
                  <i class="bi bi-search me-1"></i>查詢
                </Button>
              </div>
            </div>

            <!-- 查詢結果 -->
            <Transition name="fade-slide">
              <div v-if="queryResult" class="mt-4">
                <div class="ticket-card">
                  <div class="ticket-header">
                    <i class="bi bi-ticket-perforated me-2"></i>您的候位資訊
                  </div>
                  <div class="ticket-number">{{ queryResult.queueNumber }}</div>
                  <div class="ticket-name">{{ queryResult.name }}・{{ queryResult.adultsCount + queryResult.childrenCount }} 人</div>
                  <div class="ticket-status">
                    <span class="status-badge" :class="statusClass(queryResult.status)">
                      <i class="bi me-1" :class="statusIcon(queryResult.status)"></i>
                      {{ queryResult.statusText }}
                    </span>
                  </div>
                  <div v-if="queryResult.status === 0">
                    <div v-if="queryResult.groupsAhead > 0" class="ticket-ahead">
                      前方還有 <strong>{{ queryResult.groupsAhead }}</strong> 組
                    </div>
                    <div v-else class="ticket-ahead" style="color:var(--eat-primary)">
                      <i class="bi bi-star me-1"></i>您是下一組！請注意叫號
                    </div>
                  </div>
                  <div class="ticket-time">登記時間：{{ formatTime(queryResult.registeredAt) }}</div>
                  <div
                    v-if="queryResult.status === 0"
                    class="d-flex justify-content-center mt-4"
                  >
                    <Button variant="secondary" @click="leaveQueue(queryResult.id)" :loading="leaving">
                      <i class="bi bi-x-circle me-1"></i>取消候位
                    </Button>
                  </div>
                </div>
              </div>
            </Transition>
          </div>
        </div>
      </Transition>

    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import Button from '@/components/common/Button.vue'

// ── 今日摘要 ──────────────────────────────────────
const todayStatus = ref({ waitingCount: 0, calledCount: 0 })
let refreshTimer = null

async function fetchTodayStatus() {
  try {
    const data = await apiFetch('/api/WalkInQueues/TodayStatus')
    todayStatus.value = data
  } catch {
    // 靜默失敗，不影響頁面
  }
}

onMounted(() => {
  fetchTodayStatus()
  refreshTimer = setInterval(fetchTodayStatus, 30_000)
})
onUnmounted(() => clearInterval(refreshTimer))

// ── Tab 切換 ──────────────────────────────────────
const activeTab = ref('register')

// ── 登記候位 ──────────────────────────────────────
const form = reactive({
  name: '',
  phone: '',
  adultsCount: 2,
  childrenCount: 0,
  remark: '',
})
const errors = reactive({ name: '', phone: '', people: '' })
const submitting = ref(false)
const submitError = ref('')
const registered = ref(null)  // 登記成功後的 WalkInStatusDto

function validateRegister() {
  errors.name = ''
  errors.phone = ''
  errors.people = ''

  let valid = true

  if (!form.name || form.name.length < 2 || form.name.length > 50) {
    errors.name = '請輸入正確姓名（2～50 字）'
    valid = false
  }

  if (!form.phone || !/^09\d{8}$/.test(form.phone)) {
    errors.phone = '請輸入有效手機號碼（格式：09xxxxxxxx）'
    valid = false
  }

  const total = form.adultsCount + form.childrenCount
  if (total < 1 || total > 10) {
    errors.people = '候位人數須在 1～10 人之間'
    valid = false
  }

  return valid
}

async function submitRegister() {
  submitError.value = ''
  if (!validateRegister()) return

  submitting.value = true
  try {
    const data = await apiFetch('/api/WalkInQueues', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.name.trim(),
        phone: form.phone.trim(),
        adultsCount: form.adultsCount,
        childrenCount: form.childrenCount,
        remark: form.remark.trim(),
      }),
    })
    registered.value = data
    await fetchTodayStatus()
  } catch (err) {
    submitError.value = err?.message || '登記失敗，請稍後再試'
  } finally {
    submitting.value = false
  }
}

// ── 取消候位 ─────────────────────────────────────
const leaving = ref(false)

async function leaveQueue(id) {
  if (!confirm('確定要取消候位嗎？')) return
  leaving.value = true
  try {
    await apiFetch(`/api/WalkInQueues/${id}/Leave`, { method: 'PUT' })
    registered.value = null
    queryResult.value = null
    await fetchTodayStatus()
  } catch (err) {
    alert(err?.message || '取消失敗，請稍後再試')
  } finally {
    leaving.value = false
  }
}

// ── 查詢候位 ─────────────────────────────────────
const queryPhone = ref('')
const queryError = ref('')
const querying = ref(false)
const queryResult = ref(null)

function resetQuery() {
  queryPhone.value = ''
  queryError.value = ''
  queryResult.value = null
}

async function submitQuery() {
  queryError.value = ''
  queryResult.value = null

  if (!queryPhone.value || !/^09\d{8}$/.test(queryPhone.value)) {
    queryError.value = '請輸入有效手機號碼（格式：09xxxxxxxx）'
    return
  }

  querying.value = true
  try {
    const data = await apiFetch(`/api/WalkInQueues/My?phone=${encodeURIComponent(queryPhone.value)}`)
    queryResult.value = data
  } catch (err) {
    queryError.value = err?.message || '查無今日候位紀錄'
  } finally {
    querying.value = false
  }
}

// ── 工具函式 ─────────────────────────────────────
function formatTime(dtStr) {
  const d = new Date(dtStr)
  const pad = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}/${pad(d.getMonth()+1)}/${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function statusClass(status) {
  return {
    0: 'status-waiting',
    1: 'status-called',
    2: 'status-seated',
    3: 'status-left',
    4: 'status-missed',
  }[status] ?? ''
}

function statusIcon(status) {
  return {
    0: 'bi-hourglass-split',
    1: 'bi-megaphone',
    2: 'bi-check-circle',
    3: 'bi-x-circle',
    4: 'bi-skip-forward',
  }[status] ?? 'bi-question-circle'
}
</script>

<style scoped>
/* ── 頁面佈局 ── */
.walkin-page {
  min-height: 100vh;
  background: var(--eat-surface);
}

/* ── Hero ── */
.walkin-hero {
  position: relative;
  padding: 8rem 0 3rem;
  overflow: hidden;
}
.hero-glow {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 60% 40% at 50% 0%, rgba(227,199,107,.18) 0%, transparent 70%),
    radial-gradient(ellipse 40% 60% at 80% 50%, rgba(180,100,40,.1) 0%, transparent 70%);
  pointer-events: none;
}

/* ── Tabs ── */
.walkin-tabs {
  display: flex;
  gap: 0.5rem;
  border-bottom: 1px solid rgba(180,120,30,.2);
  padding-bottom: 0;
}
.walkin-tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: rgba(226,210,185,.6);
  font-family: var(--font-body);
  font-size: 1rem;
  padding: 0.6rem 1.25rem;
  cursor: pointer;
  transition: color .2s, border-color .2s;
  margin-bottom: -1px;
}
.walkin-tab:hover {
  color: rgba(226,210,185,.9);
}
.walkin-tab.active {
  color: var(--eat-primary);
  border-bottom-color: var(--eat-primary);
  font-weight: 600;
}

/* ── 表單 ── */
.form-label-eat {
  display: block;
  font-size: .875rem;
  color: rgba(226,210,185,.75);
  margin-bottom: .35rem;
  letter-spacing: .04em;
}
.form-control-eat,
.form-select-eat {
  background: rgba(255,255,255,.05);
  border: 1px solid rgba(180,120,30,.25);
  border-radius: .375rem;
  color: var(--eat-on-surface);
  padding: .6rem .85rem;
  width: 100%;
  font-size: .95rem;
  transition: border-color .2s, box-shadow .2s;
}
.form-control-eat::placeholder { color: rgba(226,210,185,.35); }
.form-control-eat:focus,
.form-select-eat:focus {
  outline: none;
  border-color: rgba(227,199,107,.55);
  box-shadow: 0 0 0 3px rgba(227,199,107,.12);
}
.form-control-eat.is-invalid,
.form-select-eat.is-invalid {
  border-color: #e05a5a;
}
.invalid-feedback {
  color: #e05a5a;
  font-size: .8rem;
  margin-top: .25rem;
}
.form-select-eat option {
  background: #1e120b;
  color: var(--eat-on-surface);
}

/* ── 錯誤提示框 ── */
.alert-eat {
  padding: .75rem 1rem;
  border-radius: .375rem;
  font-size: .9rem;
}
.alert-eat--error {
  background: rgba(224,90,90,.12);
  border: 1px solid rgba(224,90,90,.3);
  color: #e88;
}

/* ── 號碼牌卡片 ── */
.ticket-card {
  background: linear-gradient(135deg, rgba(41,24,17,.9) 0%, rgba(55,32,18,.85) 100%);
  border: 1px solid rgba(227,199,107,.3);
  border-radius: 1rem;
  padding: 2rem;
  text-align: center;
  box-shadow:
    0 8px 32px rgba(0,0,0,.35),
    inset 0 1px 0 rgba(255,255,255,.05);
}
.ticket-header {
  font-size: .85rem;
  letter-spacing: .1em;
  color: rgba(226,210,185,.55);
  text-transform: uppercase;
  margin-bottom: .75rem;
}
.ticket-number {
  font-size: 4rem;
  font-weight: 800;
  font-family: var(--font-display, monospace);
  color: var(--eat-primary);
  line-height: 1;
  letter-spacing: .1em;
  margin-bottom: .5rem;
  text-shadow: 0 0 30px rgba(227,199,107,.4);
}
.ticket-name {
  font-size: 1.1rem;
  color: rgba(226,210,185,.85);
  margin-bottom: .75rem;
}
.ticket-status {
  margin-bottom: .75rem;
}
.ticket-ahead {
  font-size: .95rem;
  color: rgba(226,210,185,.7);
  margin-bottom: .5rem;
}
.ticket-time {
  font-size: .8rem;
  color: rgba(226,210,185,.4);
}

/* ── 狀態標籤 ── */
.status-badge {
  display: inline-flex;
  align-items: center;
  padding: .3rem .85rem;
  border-radius: 999px;
  font-size: .85rem;
  font-weight: 600;
}
.status-waiting  { background: rgba(227,199,107,.15); color: var(--eat-primary); border: 1px solid rgba(227,199,107,.35); }
.status-called   { background: rgba(100,180,255,.12); color: #7bc8ff; border: 1px solid rgba(100,180,255,.3); }
.status-seated   { background: rgba(100,220,130,.12); color: #7de0a0; border: 1px solid rgba(100,220,130,.3); }
.status-left     { background: rgba(180,180,180,.1);  color: rgba(226,210,185,.5); border: 1px solid rgba(180,180,180,.2); }
.status-missed   { background: rgba(224,90,90,.12);   color: #e88; border: 1px solid rgba(224,90,90,.3); }

/* ── 頁面切換動畫 ── */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity .2s ease, transform .2s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
