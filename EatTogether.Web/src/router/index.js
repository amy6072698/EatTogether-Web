import { createRouter, createWebHistory } from "vue-router";
import { nextTick } from 'vue'
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
        path: '/forgot-password',
        name: 'ForgotPassword',
        component: () => import('@/views/auth/ForgotPassword.vue'),
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
        path: '/member/profile',
        name: 'MemberProfile',
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
        path: "/menu",
        name: "Menu",
        component: () => import("@/views/menu/Menu.vue"),
    },
    {
        path: "/setmeal",
        name: "SetMeal",
        component: () => import("@/views/menu/SetMeal.vue"),
    },
    {
        path: "/limited",
        name: "Limited",
        component: () => import("@/views/menu/Limited.vue"),
    },
    {
        path: "/in",
        name: "DineIn",
        component: () => import("@/views/order/In.vue"),
        meta: { hideChrome: true },
    },

    {
        path: '/news',
        name: 'NewsList',
        component:()=> import('@/views/news/NewsListView.vue'),

    },
    {
        path: '/news/:id',
        name: 'NewsDetail',
        component:()=> import('@/views/news/NewsDetailView.vue'),

    },

    // ── 404 Not Found ─────────────────────────────────────
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFound.vue'),
    },

    


];

const router = createRouter({
    history: createWebHistory(),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition   // 瀏覽器上一頁/下一頁時還原位置
        }
        return { top: 0 }          // 一般換頁回到頂部
    },
})

// ── 路由守衛（0-F8）────────────────────────────────────
router.beforeEach(async (to) => {
    const authStore = useAuthStore()

    // 等待 App.vue onMounted 的 checkAuth 完成（isLoading 由 false → true → false）
    // 首次進入時 isLoading 可能尚未被設定為 true，因此搭配 App.vue onMounted 使用
    while (authStore.isLoading) {
        await nextTick()
    }

    // 不需登入的頁面直接放行
    if (!to.meta.requiresAuth) return true

    // 需登入但未登入 → 導回首頁，並帶上 redirect query 供登入後跳回
    if (!authStore.isLoggedIn) {
        return { name: 'Home', query: { redirect: to.fullPath } }
    }

    return true
})

export default router;