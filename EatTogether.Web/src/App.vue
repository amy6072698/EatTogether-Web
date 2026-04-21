<template>
    <Navbar v-if="!route.meta.hideChrome" />
    <RouterView />
    <Footer v-if="!route.meta.hideChrome" />
</template>

<script setup>
import { useRoute } from 'vue-router';
import Navbar from "@/components/Navbar.vue";
import Footer from "@/components/Footer.vue";

const route = useRoute();
</script>
