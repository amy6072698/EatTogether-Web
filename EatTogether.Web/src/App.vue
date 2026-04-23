<template>
    <Navbar />
    <RouterView />
    <Footer />
    <ToastContainer />

    <!-- 測試用，確認後刪除 -->
    <div style="position:fixed; bottom:1rem; left:1rem; display:flex; gap:0.5rem; z-index:9999;">
        <button @click="show('資料已儲存', 'success')">✅ success</button>
        <button @click="show('載入失敗，請重新整理', 'error')">❌ error</button>
        <button @click="show('驗證信已寄出', 'info')">ℹ️ info</button>
    </div>
</template>

<script setup>
import Navbar from '@/components/common/Navbar.vue'
import Footer from '@/components/common/Footer.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'

import { useToast } from '@/composables/useToast.js'  // 測試用，確認後刪除

const { show } = useToast()  // 測試用，確認後刪除
</script>