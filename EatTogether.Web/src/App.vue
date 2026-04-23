<template>
    <Navbar />
    <RouterView />
    <Footer />
    <ToastContainer />

</template>

<script setup>
import Navbar from '@/components/common/Navbar.vue'
import Footer from '@/components/common/Footer.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'

import { useToast } from '@/composables/useToast.js'  // 測試用，確認後刪除

const { show } = useToast()  // 測試用，確認後刪除
</script>