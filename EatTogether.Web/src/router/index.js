import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.js'

const routes = [
    {
        path: '/',
        name: 'Home',
        component: () => import('@/views/Home.vue'),
    },
    {
        path: '/verify-email',
        name: 'VerifyEmail',
        component: () => import('@/views/auth/VerifyEmail.vue'),
    },
    {
        path: '/reset-password',
        name: 'ResetPassword',
        component: () => import('@/views/auth/ResetPassword.vue'),
    },
    {
        path: '/auth/google/callback',
        name: 'GoogleCallback',
        component: () => import('@/views/auth/GoogleCallback.vue'),
    },

    // ── 會員專區（需登入）────────────────────────────────
    {
        path: '/member',
        name: 'Member',
        component: () => import('@/views/member/Profile.vue'),
        meta: { requiresAuth: true },
    },
    {
        path: '/member/favorites',
        name: 'MemberFavorites',
        component: () => import('@/views/member/Favorites.vue'),
        meta: { requiresAuth: true },
    },
    {
        path: '/member/orders',
        name: 'MemberOrders',
        component: () => import('@/views/member/OrderHistory.vue'),
        meta: { requiresAuth: true },
    },

    {
        path: '/menu',
        name: 'Menu',
        component: () => import('@/views/menu/Menu.vue'),
    },
    {
        path: '/setmeal',
        name: 'SetMeal',
        component: () => import('@/views/menu/SetMeal.vue'),
    },
    {
        path: '/limited',
        name: 'Limited',
        component: () => import('@/views/menu/Limited.vue'),
    },
    {
        path: '/in',
        name: 'DineIn',
        component: () => import('@/views/order/In.vue'),
        meta: { hideChrome: true },
    },

    // ── 訂位 ────────────────────────────────────────────
    {
        path: '/reservation',
        name: 'Reservation',
        component: () => import('@/views/reservation/BookingView.vue'),
    },
    {
        path: '/reservation/query',
        name: 'ReservationQuery',
        component: () => import('@/views/reservation/ReservationQueryView.vue'),
    },
    {
        path: '/member/reservations',
        name: 'MyReservations',
        component: () => import('@/views/reservation/MyReservationsView.vue'),
        meta: { requiresAuth: true },
    },

    // ── 優惠券 ──────────────────────────────────────────
    {
        path: '/coupons',
        name: 'CouponList',
        component: () => import('@/views/coupon/CouponListView.vue'),
    },
    {
        path: '/member/coupons',
        name: 'MyCoupons',
        component: () => import('@/views/coupon/MyCouponsView.vue'),
        meta: { requiresAuth: true },
    },
    {
        path: '/member/coupon-usage',
        name: 'CouponUsage',
        component: () => import('@/views/coupon/CouponUsageView.vue'),
        meta: { requiresAuth: true },
    },

    // ── 最新消息 ─────────────────────────────────────────
    {
        path: '/news',
        name: 'NewsList',
        component: () => import('@/views/news/NewsListView.vue'),
    },
    {
        path: '/news/:id',
        name: 'NewsDetail',
        component: () => import('@/views/news/NewsDetailView.vue'),
    },

    // ── 404 Not Found ─────────────────────────────────────
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFound.vue'),
    },
]

const router = createRouter({
    history: createWebHistory(),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition // 瀏覽器上一頁/下一頁時還原位置
        }
        return { top: 0 } // 一般換頁回到頂部
    },
})

// ── 路由守衛 ───────────────────────────────────────────
// 用模組層級的 singleton Promise 確保 checkAuth 只執行一次
// 無論使用者首次進入哪個路由，守衛都會等待登入狀態確定後再判斷
// 後續的路由切換因 Promise 已 resolved，await 會立即通過，不會重複打 API
let authInitPromise = null

router.beforeEach(async (to) => {
    const authStore = useAuthStore()

    if (!authInitPromise) {
        authInitPromise = authStore.checkAuth()
    }
    await authInitPromise

    // 不需登入的頁面直接放行
    if (!to.meta.requiresAuth) return true

    // 需登入但未登入 → 導回首頁，並帶上 redirect query 供登入後跳回
    if (!authStore.isLoggedIn) {
        return { name: 'Home', query: { redirect: to.fullPath } }
    }

    return true
})

export default router
