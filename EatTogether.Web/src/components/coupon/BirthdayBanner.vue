<template>
  <Transition name="banner-slide">
    <div v-if="visible && info.hasBirthdayCoupon" class="birthday-banner">
      <div class="banner-content">
        <span class="banner-emoji">🎂</span>
        <div class="banner-text">
          <div class="banner-title">生日快樂！專屬優惠等著你</div>
          <div class="banner-desc">
            {{ info.couponName }}・{{ info.discountDescription }}
            <template v-if="info.daysUntilExpiry !== null">
              ・<span :class="info.daysUntilExpiry <= 3 ? 'text-warning' : ''">
                剩 {{ info.daysUntilExpiry }} 天到期
              </span>
            </template>
          </div>
        </div>
        <router-link to="/coupons?birthday=true" class="banner-btn">去領取</router-link>
        <button class="banner-close" @click="dismiss">✕</button>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiFetch from '@/utils/apiFetch.js'
import { useAuthStore } from '@/stores/auth.js'

const authStore = useAuthStore()
const visible   = ref(false)
const info      = ref({
  hasBirthdayCoupon: false,
  couponName:        '',
  code:              '',
  discountDescription: '',
  endDate:           null,
  daysUntilExpiry:   null
})

async function checkBirthday() {
  // 同次訪問已關閉過則不再顯示
  if (sessionStorage.getItem('birthdayBannerDismissed')) return

  if (!authStore.isLoggedIn) return

  try {
    const res = await apiFetch('/Coupons/BirthdayCheck')
    if (res.ok) {
      const data = await res.json()
      info.value  = data
      visible.value = data.hasBirthdayCoupon
    }
  } catch {}
}

function dismiss() {
  visible.value = false
  sessionStorage.setItem('birthdayBannerDismissed', '1')
}

onMounted(checkBirthday)
</script>

<style scoped>
.birthday-banner {
  position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%);
  z-index: 1050; width: min(92vw, 680px);
}
.banner-content {
  display: flex; align-items: center; gap: 12px;
  background: linear-gradient(135deg, #2a1205, #3d1a08);
  border: 1px solid rgba(201,169,110,.5); border-radius: 16px;
  padding: 16px 20px; box-shadow: 0 8px 32px rgba(0,0,0,.5);
}
.banner-emoji { font-size: 1.8rem; flex-shrink: 0; }
.banner-text { flex: 1; min-width: 0; }
.banner-title { font-size: .95rem; color: var(--eat-primary); font-weight: 600; margin-bottom: 3px; }
.banner-desc  { font-size: .82rem; color: var(--eat-text-secondary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.banner-btn {
  flex-shrink: 0; padding: 7px 18px; border-radius: 50px; font-size: .85rem;
  background: var(--eat-primary); color: #fff; text-decoration: none; white-space: nowrap;
  transition: opacity .2s;
}
.banner-btn:hover { opacity: .85; }
.banner-close {
  background: none; border: none; color: var(--eat-text-muted);
  font-size: .9rem; cursor: pointer; padding: 4px; flex-shrink: 0;
}
.banner-close:hover { color: var(--eat-text-primary); }

.banner-slide-enter-active, .banner-slide-leave-active { transition: all .35s ease; }
.banner-slide-enter-from, .banner-slide-leave-to { opacity: 0; transform: translateX(-50%) translateY(20px); }
</style>
