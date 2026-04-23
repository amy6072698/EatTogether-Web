import { createRouter, createWebHistory } from "vue-router";
import { nextTick } from 'vue'
import { useAuthStore } from '@/stores/auth.js'

const routes = [
    {
        path: "/",
        component: () => import("@/views/Home.vue"),
    },
    // {
    //     path: "/login",
    //     component: () => import("@/views/auth/Login.vue"),
    //     meta: { guestOnly: true },
    // },
    // {
    //     path: "/register",
    //     component: () => import("@/views/auth/Register.vue"),
    //     meta: { guestOnly: true },
    // },
    // {
    //     path: "/member",
    //     component: () => import("@/views/member/Member.vue"),
    //     meta: { requiresAuth: true },
    // },
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
    // 攔截尚未實作的路由，避免 null component crash
    { path: '/:pathMatch(.*)*', redirect: '/' },
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

export default router;
