<template>
    <Navbar />
    <AuthModal />
    <ForgotPasswordModal />
    <ResendResetPasswordModal />
    <RouterView />
    <Footer />
    <ToastContainer />
</template>

<script setup>
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.js'
import Navbar from '@/components/common/Navbar.vue'
import Footer from '@/components/common/Footer.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'
import AuthModal from '@/components/auth/AuthModal.vue'
import ForgotPasswordModal from '@/components/auth/ForgotPasswordModal.vue'
import ResendResetPasswordModal from '@/components/auth/ResendResetPasswordModal.vue'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

onMounted(() => {
    // ── redirect 處理 ───────────────────────────────────
    // 路由守衛將未登入的使用者導回首頁時，會附上 ?redirect=原目標路由
    // 登入成功後，Navbar 的登入邏輯呼叫 fetchMe() 更新 isLoggedIn
    // 此處在 App 掛載時檢查一次，處理頁面重新整理後仍帶有 redirect query 的情況
    if (authStore.isLoggedIn && route.query.redirect) {
        router.push(route.query.redirect)
    }

    // ── auth:expired 事件監聽 ────────────────────────────
    // apiFetch refresh token 失敗時會發出此事件
    // 由 App.vue 統一處理導頁與開啟登入 Modal，確保只執行一次
    window.addEventListener('auth:expired', async () => {
        await router.push('/')
        const modalEl = document.querySelector('#authModal')
        if (modalEl) {
            const { default: bootstrap } = await import('bootstrap')
            bootstrap.Modal.getOrCreateInstance(modalEl).show()
        }
    })
})
</script>
