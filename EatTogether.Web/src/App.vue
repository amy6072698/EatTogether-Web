<template>
    <Navbar />
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

const authStore = useAuthStore()
const route    = useRoute()
const router   = useRouter()

onMounted(async () => {
    // 確認登入狀態（已登入則直接跳過 API 呼叫）
    await authStore.checkAuth()

    // 若帶有 redirect query（由路由守衛附上），登入後自動跳回目標頁
    if (authStore.isLoggedIn && route.query.redirect) {
        router.push(route.query.redirect)
    }
})
</script>