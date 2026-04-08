import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    component: () => import('@/views/Home.vue')
  },
  {
    path: '/login',
    component: () => import('@/views/auth/Login.vue'),
    meta: { guestOnly: true }
  },
  {
    path: '/register',
    component: () => import('@/views/auth/Register.vue'),
    meta: { guestOnly: true }
  },
  {
    path: '/member',
    component: () => import('@/views/member/Member.vue'),
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// ── 登入驗證完成後取消以下註解 ──────────────────────────
// import { useAuthStore } from '@/stores/auth'
// router.beforeEach(async (to) => {
//   const auth = useAuthStore()
//   if (auth.member === null) await auth.fetchMe()
//   if (to.meta.requiresAuth && !auth.isLoggedIn)
//     return { path: '/login', query: { redirect: to.fullPath } }
//   if (to.meta.guestOnly && auth.isLoggedIn)
//     return { path: '/' }
// })

export default router