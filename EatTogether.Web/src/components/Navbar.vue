<template>
  <nav class="navbar navbar-expand-lg navbar-eat">
    <div class="container-fluid">

      <!-- Brand Logo -->
      <RouterLink class="navbar-brand" to="/">
        <svg height="48" viewBox="0 0 400 120" xmlns="http://www.w3.org/2000/svg">
          <circle cx="60" cy="60" fill="#1A0D08" r="56"/>
          <circle cx="60" cy="60" fill="none" opacity="0.5" r="52" stroke="#F5D87A" stroke-width="1"/>
          <line stroke="#F5D87A" stroke-linecap="round" stroke-width="3" x1="49" x2="49" y1="28" y2="92"/>
          <line stroke="#F5D87A" stroke-linecap="round" stroke-width="2.2" x1="43" x2="43" y1="28" y2="48"/>
          <line stroke="#F5D87A" stroke-linecap="round" stroke-width="2.2" x1="49" x2="49" y1="28" y2="48"/>
          <line stroke="#F5D87A" stroke-linecap="round" stroke-width="2.2" x1="55" x2="55" y1="28" y2="48"/>
          <path d="M43 48 Q49 58 55 48" fill="none" stroke="#F5D87A" stroke-width="2.2"/>
          <line stroke="#F5D87A" stroke-linecap="round" stroke-width="3" x1="71" x2="71" y1="28" y2="92"/>
          <path d="M71 28 Q83 37 83 52 Q83 61 71 64" fill="#F5D87A" opacity="0.18"/>
          <path d="M71 28 Q83 37 83 52 Q83 61 71 64" fill="none" stroke="#F5D87A" stroke-width="2.2"/>
          <circle cx="60" cy="93" fill="#C9A96E" r="2"/>
          <line opacity="0.35" stroke="#F5D87A" stroke-width="1" x1="130" x2="130" y1="28" y2="92"/>
          <text fill="#F5D87A" font-family="'Noto Serif TC','Songti SC',serif" font-size="44" font-weight="700" letter-spacing="8" x="158" y="74">義起吃</text>
          <text fill="#C9A96E" font-family="'Cormorant Garamond','Georgia',serif" font-size="14" font-style="italic" letter-spacing="5" opacity="0.85" x="162" y="97">Eat Together</text>
        </svg>
      </RouterLink>

      <!-- Mobile Toggler -->
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarMain"
        aria-controls="navbarMain"
        aria-expanded="false"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <!-- Nav Links -->
      <div class="collapse navbar-collapse justify-content-center" id="navbarMain">
        <ul class="navbar-nav gap-4">
          <li class="nav-item" v-for="link in navLinks" :key="link.to">
            <RouterLink
              class="nav-link"
              :to="link.to"
              :class="{ active: isActive(link.to) }"
            >
              {{ link.label }}
            </RouterLink>
          </li>
        </ul>
      </div>

      <!-- CTA Button -->
      <RouterLink to="/reservation" class="btn-eat-primary ms-auto">
        立即訂位
      </RouterLink>

    </div>
  </nav>
</template>

<script setup>
import { useRoute } from 'vue-router'
import { RouterLink } from 'vue-router'

const route = useRoute()

const navLinks = [
  { label: '菜單',   to: '/menu' },
  { label: '套餐',   to: '/setmeal' },
  { label: '訂位',   to: '/reservation' },
  { label: '會員',   to: '/member' },
  { label: '外帶點餐', to: '/takeout' },
]

// 判斷目前路由是否 active（首頁特別處理）
function isActive(path) {
  if (path === '/') return route.path === '/'
  return route.path.startsWith(path)
}
</script>

<style scoped>
.navbar-eat {
  background: rgba(41, 24, 17, 0.72);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-bottom: 1px solid rgba(180, 120, 30, 0.12);
  padding: 1.25rem 3rem;
  /* 固定在頂部，讓 Hero 可以透出 navbar */
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1030;
}

.navbar-eat .nav-link {
  font-family: var(--font-body);
  font-style: italic;
  font-size: 1rem;
  letter-spacing: 0.06em;
  color: rgba(226, 210, 185, 0.85);
  transition: color 0.3s ease;
  padding: 0.25rem 0;
  position: relative;
  text-transform: none;
  text-decoration: none;
}
.navbar-eat .nav-link:hover {
  color: rgba(254, 225, 130, 0.9);
}
.navbar-eat .nav-link.active {
  color: var(--eat-primary);
  font-weight: 700;
}
/* active 底線 */
.navbar-eat .nav-link.active::after {
  content: '';
  display: block;
  height: 2px;
  background: var(--eat-primary);
  position: absolute;
  bottom: -4px;
  left: 0;
  right: 0;
}

/* Mobile toggler */
.navbar-eat .navbar-toggler {
  border-color: var(--eat-outline-variant);
}
.navbar-eat .navbar-toggler-icon {
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba(249,221,211,0.8)' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e");
}
</style>
