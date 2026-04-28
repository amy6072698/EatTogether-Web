<template>
    <!-- 桌機版：左側欄 -->
    <nav class="sidebar d-none d-lg-block">
        <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="sidebar-item"
            :class="{ active: route.path === item.to }"
        >
            {{ item.label }}
        </RouterLink>
    </nav>

    <!-- 行動版：頂部 Tab 列 -->
    <nav class="tab-bar d-flex d-lg-none">
        <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="tab-item"
            :class="{ active: route.path === item.to }"
        >
            {{ item.label }}
        </RouterLink>
    </nav>
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
      <div class="flex-grow-1"><!-- 頁面主內容 --></div>
    </div>
-->
<script setup>
import { useRoute } from 'vue-router'
import { RouterLink } from 'vue-router'

const route = useRoute()

const navItems = [
    { label: '個人資料', to: '/member' },
    { label: '收藏餐點', to: '/member/favorites' },
    { label: '訂單紀錄', to: '/member/orders' },
]
</script>

<style scoped>
/* ── 桌機版側欄 ── */
.sidebar {
    background: var(--eat-surface-container);
    border-radius: var(--eat-radius-lg);
    padding: 1.5rem 0;
}

.sidebar-item {
    display: block;
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

/* ── 行動版 Tab 列 ── */
.tab-bar {
    border-radius: var(--eat-radius-lg);
    overflow: hidden;
    background: var(--eat-surface-container);
    gap: 0.25rem;
    padding: 0.35rem;
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
