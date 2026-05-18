<template>
    <div class="container py-4 py-lg-5">
        <div class="row g-4">
            <div class="col-lg-3">
                <MemberSidebar />
            </div>
            <div class="col-lg-9 d-flex flex-column gap-4">
                <RouterView />
            </div>
        </div>
    </div>
</template>

<script setup>
import MemberSidebar from '@/components/member/MemberSidebar.vue'
</script>
