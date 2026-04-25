<template>
    <nav class="navbar navbar-expand-lg navbar-eat px-lg-3 px-0">
        <div class="container-fluid">
            <!-- Brand Logo -->
            <RouterLink class="navbar-brand py-0" to="/">
                <img
                    src="@/assets/images/logo.svg"
                    alt="義起吃 Eat Together"
                    height="50"
                    class="d-inline-block align-middle"
                />
            </RouterLink>

            <!-- Mobile Toggler -->
            <button
                class="navbar-toggler"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#navbarMain"
                aria-controls="navbarMain"
                aria-expanded="false"
                aria-label="Toggle navigation"
            >
                <span class="navbar-toggler-icon"></span>
            </button>

            <!-- Nav Links -->
            <div class="collapse navbar-collapse" id="navbarMain" ref="navbarCollapse">
                <div
                    class="w-100 d-flex justify-content-between flex-column-reverse flex-lg-row px-2 px-lg-0"
                >
                    <ul
                        class="navbar-nav w-100 gap-2 gap-lg-4 align-items-stretch align-items-lg-center justify-content-center"
                    >
                        <template v-for="link in navLinks" :key="link.to ?? link.label">
                            <!-- Dropdown -->
                            <li
                                v-if="link.children"
                                class="nav-item dropdown-wrap"
                                :class="{ 'is-open': openDropdown === link.label }"
                            >
                                <button
                                    class="nav-link nav-dropdown-trigger"
                                    :class="{ active: link.children.some((c) => isActive(c.to)) }"
                                    @click="toggleDropdown(link.label)"
                                >
                                    {{
                                        link.children?.find((c) => isActive(c.to))?.label ??
                                        link.label
                                    }}
                                    <svg
                                        class="dropdown-chevron"
                                        width="10"
                                        height="6"
                                        viewBox="0 0 10 6"
                                        fill="none"
                                    >
                                        <path
                                            d="M1 1l4 4 4-4"
                                            stroke="currentColor"
                                            stroke-width="1.5"
                                            stroke-linecap="round"
                                            stroke-linejoin="round"
                                        />
                                    </svg>
                                </button>
                                <Transition name="dropdown">
                                    <div
                                        v-if="openDropdown === link.label"
                                        class="dropdown-menu-eat"
                                    >
                                        <RouterLink
                                            v-for="child in link.children"
                                            :key="child.to"
                                            class="dropdown-item-eat"
                                            :to="child.to"
                                            :class="{ active: isActive(child.to) }"
                                            @click="handleLinkClick"
                                            >{{ child.label }}</RouterLink
                                        >
                                    </div>
                                </Transition>
                            </li>
                            <!-- Normal link -->
                            <li v-else class="nav-item">
                                <RouterLink
                                    class="nav-link"
                                    :to="link.to"
                                    :class="{ active: isActive(link.to) }"
                                    @click="handleLinkClick"
                                >
                                    {{ link.label }}
                                </RouterLink>
                            </li>
                        </template>
                    </ul>

                    <!-- CTA Button -->
                    <div class="d-flex my-3 my-lg-0">
                        <Button
                            class="py-2 px-3 w-100 w-lg-auto"
                            variant="primary"
                            @click="openAuthModal"
                            aria-label="開啟登入或註冊視窗"
                        >
                            登入｜註冊
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </nav>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { RouterLink } from 'vue-router'
import Button from '@/components/common/Button.vue'
import { Modal, Collapse } from 'bootstrap'

const route = useRoute()
const openDropdown = ref(null)
const navbarCollapse = ref(null)

const navLinks = [
    { label: '最新消息', to: '/news' },
    {
        label: '菜單',
        children: [
            { label: '本季限定', to: '/limited' },
            { label: '菜單', to: '/menu' },
            { label: '套餐', to: '/setmeal' },
        ],
    },
    { label: '優惠券', to: '/coupons' },
    { label: '訂位', to: '/reservation' },
    { label: '訂位查詢', to: '/reservation/query' },
    { label: '會員', to: '/member' },
    { label: '外帶點餐', to: '/takeout' },
    { label: '內用', to: '/In' },
]

function isActive(path) {
    if (path === '/') return route.path === '/'
    return route.path.startsWith(path)
}

function toggleDropdown(label) {
    openDropdown.value = openDropdown.value === label ? null : label
}

function openAuthModal() {
    const modalEl = document.querySelector('#authModal')
    const modal = new Modal(modalEl)
    modal.show()
}

function handleLinkClick() {
    // 1. 關閉 Vue 控制的子選單
    openDropdown.value = null

    // 2. 關閉 Bootstrap 控制的手機版選單
    if (navbarCollapse.value) {
        // 取得或建立 Bootstrap Collapse 實例
        const bsCollapse =
            Collapse.getInstance(navbarCollapse.value) ||
            new Collapse(navbarCollapse.value, { toggle: false })
        bsCollapse.hide()
    }
}

// 點擊外部關閉
if (typeof window !== 'undefined') {
    window.addEventListener('click', (e) => {
        if (!e.target.closest('.dropdown-wrap')) openDropdown.value = null
    })
}

onMounted(() => {
    const navbarEl = document.getElementById('navbarMain')

    // 當選單「開始展開」時
    navbarEl.addEventListener('show.bs.collapse', () => {
        document.body.style.overflow = 'hidden'
    })

    // 當選單「開始收合」時
    navbarEl.addEventListener('hide.bs.collapse', () => {
        document.body.style.overflow = ''
    })
})
</script>

<style scoped>
.navbar-eat {
    background: rgba(41, 24, 17, 0.72);
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
    border-bottom: 1px solid rgba(180, 120, 30, 0.12);
    /* 固定在頂部，讓 Hero 可以透出 navbar */
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 1030;
}

.navbar-eat .nav-link {
    display: block;
    font-family: var(--font-body);
    text-align: center;
    font-size: 1rem;
    letter-spacing: 0.06em;
    color: rgba(226, 210, 185, 0.85);
    transition: color 0.3s ease;
    padding: 0.5rem 1rem;
    position: relative;
    text-transform: none;
    text-decoration: none;
    width: 100%;
}
.navbar-eat .nav-link:hover {
    color: rgba(254, 225, 130, 0.9);
}
.navbar-eat .nav-link.active {
    color: var(--eat-primary);
    font-weight: 700;
}
/* active 底線 */
.navbar-eat .nav-link.active::after {
    content: '';
    display: block;
    height: 2px;
    background: var(--eat-primary);
    position: absolute;
    bottom: 2px;
    left: 0;
    right: 0;
}

@media (max-width: 992px) {
    .navbar-eat .nav-link.active::after {
        display: none;
    }
}

/* ── Dropdown ── */
.dropdown-wrap {
    position: relative;
}
.nav-dropdown-trigger {
    background: none;
    border: none;
    display: flex;
    align-items: center;
    gap: 0.35rem;
    cursor: pointer;
}
.dropdown-chevron {
    transition: transform 0.25s ease;
    opacity: 0.6;
}
.is-open .dropdown-chevron {
    transform: rotate(180deg);
}
.dropdown-menu-eat {
    position: absolute;
    top: calc(100% + 0.75rem);
    left: 50%;
    transform: translateX(-50%);
    background: rgba(30, 16, 10, 0.96);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(180, 120, 30, 0.2);
    border-radius: 0.125rem;
    padding: 0.4rem;
    min-width: 130px;
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
    box-shadow: 0 16px 40px rgba(0, 0, 0, 0.5);
    z-index: 100;
}

@media (max-width: 992px) {
    .dropdown-menu-eat {
        position: static;
        transform: none;
        left: auto;
        top: auto;

        /* 讓寬度撐滿父層，並調整內距 */
        width: 100%;
        min-width: unset;
        /* margin-top: 0.5rem; */
        padding: 0.5rem; /* 增加選單內框距離 */

        box-shadow: none;
        border-radius: 0.125rem;
        border: none;
        background: rgba(255, 255, 255, 0.05); /* 給個微弱背影深淺色差，區隔感會更好 */
    }
}

.dropdown-item-eat {
    display: block;
    font-family: var(--font-body);
    text-align: center;
    font-size: 0.95rem;
    letter-spacing: 0.06em;
    color: rgba(226, 210, 185, 0.85);
    text-decoration: none;
    padding: 0.8rem 1.5rem;
    border-radius: 8px;
    transition:
        background 0.2s,
        color 0.2s;
    white-space: nowrap;
}
.dropdown-item-eat:hover {
    background: rgba(227, 199, 107, 0.08);
    color: rgba(254, 225, 130, 0.9);
}
.dropdown-item-eat.active {
    color: var(--eat-primary);
    font-weight: 700;
}

/* Dropdown 動畫 */
.dropdown-enter-active {
    transition:
        opacity 0.2s ease,
        transform 0.2s cubic-bezier(0.22, 1, 0.36, 1);
}
.dropdown-leave-active {
    transition:
        opacity 0.15s ease,
        transform 0.15s ease;
}
.dropdown-enter-from {
    opacity: 0;
    transform: translateX(-50%) translateY(-6px);
}
.dropdown-leave-to {
    opacity: 0;
    transform: translateX(-50%) translateY(-4px);
}

/* Mobile toggler */
.navbar-eat .navbar-toggler {
    border-color: var(--eat-outline-variant);
}
.navbar-eat .navbar-toggler-icon {
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba(249,221,211,0.8)' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e");
}

@media (max-width: 992px) {
    .navbar-collapse {
        /* 設定最大高度，避免選單太長滑不到底 */
        max-height: 80vh;
        overflow-y: auto; /* 讓選單內部可以捲動 */
        padding-bottom: 2rem;

        /* 順滑捲動體驗 */
        -webkit-overflow-scrolling: touch;
    }
}
</style>
