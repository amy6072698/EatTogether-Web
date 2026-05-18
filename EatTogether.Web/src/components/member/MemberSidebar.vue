<template>
    <div class="sidebar-wrapper">
        <div class="d-flex flex-column align-items-center mb-4">
            <AvatarInitial
                class="mb-3"
                :avatarFileName="authStore.member.avatarFileName"
                :googleAvatarUrl="authStore.member.googleAvatarUrl"
                :name="authStore.member.name"
                size="64px"
            />
            <div class="mb-2 fw-bold">{{ authStore.member.name }}</div>
            <div class="text-eat-muted mb-4">{{ authStore.member.email }}</div>
            <div class="feather-divider on-container my-2"></div>
        </div>

        <!-- 桌機版：左側欄 -->
        <nav class="sidebar d-none d-lg-block">
            <RouterLink
                v-for="item in navItems"
                :key="item.name"
                :to="{ name: item.name }"
                class="sidebar-item"
                exact-active-class="active"
            >
                <i class="me-2 sidebar-item-icon align-middle" :class="item.icon"></i>
                {{ item.label }}
            </RouterLink>
        </nav>

        <!-- 行動版：頂部 Tab 列 -->
        <nav class="tab-bar d-flex d-lg-none">
            <RouterLink
                v-for="item in navItems"
                :key="item.name"
                :to="{ name: item.name }"
                class="tab-item"
                exact-active-class="active"
            >
                {{ item.label }}
            </RouterLink>
        </nav>
    </div>
</template>

<!--
  MemberSidebar.vue — 會員中心導覽元件
  桌機版顯示左側欄，行動版（< 992px）自動切換為頂部水平 Tab 列。
  不需傳入任何 Props，active 狀態依 route.path 完全比對自動偵測。

  使用範例（直接放入頁面版型即可）:
    <MemberSidebar />

  建議版型結構:
    <div class="d-lg-flex gap-4">
      <MemberSidebar />
      <div class="flex-grow-1">頁面主內容</div>
    </div>
-->
<script setup>
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.js'
import AvatarInitial from '@/components/member/AvatarInitial.vue'

const authStore = useAuthStore()

const navItems = [
    { label: '個人資料',  name: 'MemberProfile',  icon: 'bi bi-person' },
    { label: '收藏餐點',  name: 'MemberFavorites', icon: 'bi bi-suit-heart' },
    { label: '訂單紀錄',  name: 'MemberOrders',    icon: 'bi bi-file-earmark-text' },
    { label: '我的訂位',  name: 'MyReservations',  icon: 'bi bi-calendar-check' },
    { label: '我的候位',  name: 'MyWalkIn',        icon: 'bi bi-people' },
    { label: '我的優惠券', name: 'MyCoupons',      icon: 'bi bi-ticket-perforated' },
    { label: '優惠券明細', name: 'CouponUsage',    icon: 'bi bi-clock-history' },
]
</script>

<style scoped>
.sidebar-wrapper {
    background: var(--eat-surface-container);
    border-radius: var(--eat-radius);
    padding: 1.5rem 0;
}

.feather-divider {
    width: 90%;
    height: 1px;
}

/* ── 桌機版側欄 ── */
.sidebar-item {
    display: flex;
    align-items: center;
    justify-content: start;
    font-family: var(--font-body);
    font-size: 0.95rem;
    color: var(--eat-on-surface-variant);
    text-decoration: none;
    padding: 0.75rem 1.5rem;
    border-left: 2px solid transparent;
    transition:
        color 0.2s ease,
        border-color 0.2s ease;
}

.sidebar-item:hover {
    color: var(--eat-on-surface);
}

.sidebar-item.active {
    color: var(--eat-primary);
    border-left-color: var(--eat-primary);
    font-weight: 600;
}

.sidebar-item-icon {
    font-size: 1.2rem;
}

/* ── 行動版 Tab 列 ── */
.tab-bar {
    display: flex;
    overflow-x: auto;
    overflow-y: hidden;
    -webkit-overflow-scrolling: touch;
    scrollbar-width: none; /* Firefox 隱藏滾軸 */
    border-radius: var(--eat-radius-lg);
    background: var(--eat-surface-container);
    gap: 0.25rem;
    padding: 0.35rem;
}

.tab-bar::-webkit-scrollbar {
    display: none; /* Chrome/Safari 隱藏滾軸 */
}

.tab-item {
    flex: 1;
    text-align: center;
    font-family: var(--font-body);
    font-size: 0.875rem;
    color: var(--eat-on-surface-variant);
    text-decoration: none;
    padding: 0.55rem 0.5rem;
    border-radius: var(--eat-radius);
    transition:
        background 0.2s ease,
        color 0.2s ease;
    white-space: nowrap;
}

.tab-item:hover {
    color: var(--eat-on-surface);
}

.tab-item.active {
    background: var(--eat-primary);
    color: var(--eat-on-primary);
    font-weight: 600;
}
</style>
