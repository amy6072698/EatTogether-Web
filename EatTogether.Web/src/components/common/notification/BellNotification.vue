<template>
    <div class="bell-wrapper">
        <button class="bell-btn" @click="toggleDropdown">
            <i class="fa-solid fa-bell"></i>
            <span v-if="unreadCount > 0" class="bell-badge">{{ unreadCount }}</span>
        </button>
    </div>
</template>

<script setup></script>

<style scoped></style>
