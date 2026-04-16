<template>
    <!-- ═══════════════════════════════════════════
       Hero
    ════════════════════════════════════════════ -->
    <header class="news-hero">
        <h1 class="eat-display news-hero-title">最新消息</h1>
        <div class="news-hero-sub">
            <div class="news-hero-line"></div>
            <span class="news-hero-eyebrow">Il Manoscritto · News &amp; Stories</span>
            <div class="news-hero-line"></div>
        </div>
    </header>
 
    <!-- ═══════════════════════════════════════════
       主體：側邊欄 + 文章列表
    ════════════════════════════════════════════ -->
    <main class="news-layout">
        <!-- 側邊分類導覽 -->
        <aside class="news-sidebar">
            <div class="news-sidebar-inner">
                <div class="news-sidebar-heading">
                    <h2 class="news-sidebar-title">Il Manoscritto</h2>
                    <p class="news-sidebar-sub">News &amp; Stories</p>
                </div>
                <nav class="news-sidebar-nav">
                    <a
                        v-for="cat in categories"
                        :key="cat.id ?? 'all'"
                        class="news-sidebar-link"
                        :class="{ active: selectedCategory === cat.id }"
                        href="#"
                        @click.prevent="selectCategory(cat.id)"
                    >
                        <span class="material-symbols-outlined">{{ cat.icon }}</span>
                        <span>{{ cat.label }}</span>
                    </a>
                </nav>
            </div>
        </aside>
 
        <!-- 文章區塊 -->
        <section class="news-articles">
            <!-- 載入中 -->
            <div v-if="loading" class="news-state">
                <span class="news-state-text">載入中…</span>
            </div>
 
            <!-- 無文章 -->
            <div v-else-if="articles.length === 0" class="news-state">
                <span class="news-state-text">目前尚無消息，請稍後再回來。</span>
            </div>
 
            <template v-else>
                <template v-for="(article, index) in articles" :key="article.id">
 
                    <!-- 文章卡片：偶數左圖右文、奇數左文右圖 -->
                    <article
                        class="news-card"
                        :class="index % 2 === 0 ? 'news-card--normal' : 'news-card--reverse'"
                    >
                        <!-- 圖片欄 -->
                        <div class="news-card-img-wrap">
                            <img
                                v-if="article.coverImageUrl"
                                :src="article.coverImageUrl"
                                :alt="article.title"
                                class="news-card-img"
                            />
                            <div v-else class="news-card-img-placeholder">
                                <span class="material-symbols-outlined">article</span>
                            </div>
                        </div>
 
                        <!-- 文字欄 -->
                        <div
                            class="news-card-body"
                            :class="index % 2 !== 0 ? 'news-card-body--right' : ''"
                        >
                            <div class="news-card-date-bg">
                                {{ formatDateBg(article.publishDate) }}
                            </div>
                            <span class="news-card-category">{{ article.categoryName }}</span>
                            <h3 class="news-card-title">{{ article.title }}</h3>
                            <p class="news-card-summary">{{ stripTags(article.summary) }}</p>
                            <div class="news-card-meta">
                                <span class="material-symbols-outlined news-card-meta-icon">visibility</span>
                                <span>{{ article.viewCount }}</span>
                            </div>
                            <RouterLink
                                :to="`/news/${article.id}`"
                                class="news-card-link"
                                :class="index % 2 !== 0 ? 'news-card-link--reverse' : ''"
                            >
                                <template v-if="index % 2 !== 0">
                                    <span class="material-symbols-outlined">arrow_back</span>
                                    閱讀全文
                                </template>
                                <template v-else>
                                    閱讀全文
                                    <span class="material-symbols-outlined">arrow_forward</span>
                                </template>
                            </RouterLink>
                        </div>
                    </article>
 
                    <!-- 羽毛分隔線 -->
                    <div v-if="index < articles.length - 1" class="news-divider">
                        <svg width="200" height="20" viewBox="0 0 200 20" fill="none">
                            <path
                                d="M0 10C50 10 70 2 100 2C130 2 150 18 200 18"
                                stroke="#C9A96E"
                                stroke-width="0.5"
                                stroke-dasharray="4 4"
                            />
                        </svg>
                    </div>
                </template>
 
                <!-- 分頁 -->
                <div class="news-pagination">
                    <button
                        class="news-page-btn"
                        :disabled="page === 1"
                        @click="changePage(page - 1)"
                    >
                        <span class="material-symbols-outlined">chevron_left</span>
                    </button>
                    <div class="news-page-numbers">
                        <span
                            v-for="p in totalPages"
                            :key="p"
                            class="news-page-num"
                            :class="{ active: p === page }"
                            @click="changePage(p)"
                        >{{ p }}</span>
                    </div>
                    <button
                        class="news-page-btn"
                        :disabled="page === totalPages"
                        @click="changePage(page + 1)"
                    >
                        <span class="material-symbols-outlined">chevron_right</span>
                    </button>
                </div>
            </template>
        </section>
    </main>
</template>
 
<script setup>
import { ref, watch, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import apiFetch from '@/utils/apiFetch.js'
 
// ── 分類（對應後台 ArticleCategory 名稱）
const categories = [
    { id: null,     label: '所有消息', icon: 'dashboard' },
    { id: '活動介紹', label: '活動介紹', icon: 'event' },
    { id: '餐廳公告', label: '餐廳公告', icon: 'campaign' },
    { id: '季節限定', label: '季節限定', icon: 'thermostat_arrow_up' },
    { id: '品牌故事', label: '品牌故事', icon: 'history' },
    { id: '新品上市', label: '新品上市', icon: 'new_releases' },
    { id: '媒體報導', label: '媒體報導', icon: 'menu_book' },
]
 
// ── 狀態
const articles         = ref([])
const loading          = ref(false)
const page             = ref(1)
const totalPages       = ref(1)
const pageSize         = 5
const selectedCategory = ref(null)
 
// ── 取得文章列表
async function fetchNews() {
    loading.value = true
    try {
        const params = new URLSearchParams({ page: page.value, pageSize })
        if (selectedCategory.value) {
            params.append('categoryName', selectedCategory.value)
        }
        const res  = await apiFetch(`/News?${params}`)
        const data = await res.json()
        articles.value   = data.data        // 不再前端 filter
        totalPages.value = data.totalPages  // 後端給的才是正確的
    } catch (e) {
        console.error('載入最新消息失敗', e)
        articles.value = []
    } finally {
        loading.value = false
    }
}

 
function changePage(p) {
    if (p < 1 || p > totalPages.value) return
    page.value = p
    window.scrollTo({ top: 0, behavior: 'smooth' })
}
 
function selectCategory(id) {
    selectedCategory.value = id
    page.value = 1
}
 
// ── 工具函式
function stripTags(html) {
    if (!html) return ''
    return html.replace(/<[^>]*>/g, '')
}
 
function formatDateBg(dateStr) {
    if (!dateStr) return ''
    const d   = new Date(dateStr)
    const y   = d.getFullYear()
    const m   = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}.${m}.${day}`
}
 
// ── 換頁或換分類時重新 fetch
watch([page, selectedCategory], fetchNews)
onMounted(fetchNews)
</script>
 
<style scoped>
/* ── Hero ─────────────────────────────────────────── */
.news-hero {
    padding: 5rem 2rem 4rem;
    text-align: center;
    background-color: var(--eat-surface-lowest);
}
.news-hero-title {
    color: var(--eat-primary);
    font-style: italic;
    margin-bottom: 1.25rem;
}
.news-hero-sub {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 1.25rem;
}
.news-hero-line {
    flex: 0 0 3rem;
    height: 1px;
    background: var(--eat-secondary);
    opacity: 0.3;
}
.news-hero-eyebrow {
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    white-space: nowrap;
}
 
/* ── Layout ───────────────────────────────────────── */
.news-layout {
    display: flex;
    flex-direction: column;
    gap: 3rem;
    max-width: 1280px;
    margin: 0 auto;
    padding: 2rem 2rem 8rem;
}
@media (min-width: 768px) {
    .news-layout {
        flex-direction: row;
        gap: 5rem;
        align-items: flex-start;
    }
}
 
/* ── Sidebar ──────────────────────────────────────── */
.news-sidebar {
    width: 100%;
    flex-shrink: 0;
}
@media (min-width: 768px) {
    .news-sidebar { width: 15rem; }
}
.news-sidebar-inner {
    position: sticky;
    top: 6rem;
    display: flex;
    flex-direction: column;
    gap: 2rem;
}
.news-sidebar-title {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 1.8rem;
    color: var(--eat-primary);
    margin-bottom: 0;
}
.news-sidebar-sub {
    font-family: var(--font-label);
    font-size: 0.62rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.4;
    margin-top: 0.3rem;
}
.news-sidebar-nav {
    display: flex;
    flex-direction: column;
    gap: 1.25rem;
}
.news-sidebar-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    font-family: var(--font-label);
    font-size: 1rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.4;
    text-decoration: none;
    transition: all 0.25s ease;
}
.news-sidebar-link:hover,
.news-sidebar-link.active {
    color: var(--eat-primary);
    opacity: 1;
    transform: translateX(6px);
}
.news-sidebar-link.active { font-weight: 600; }
.news-sidebar-link .material-symbols-outlined { font-size: 1.1rem; }
 
/* ── Articles ─────────────────────────────────────── */
.news-articles {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 5rem;
}
 
/* ── Card ─────────────────────────────────────────── */
.news-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2.5rem;
}
@media (min-width: 768px) {
    .news-card--normal  { flex-direction: row; }
    .news-card--reverse { flex-direction: row-reverse; }
}
 
.news-card-img-wrap {
    width: 100%;
    overflow: hidden;
    border-radius: var(--eat-radius);
    box-shadow: 0 25px 50px -12px rgba(24, 11, 6, 0.5);
    flex-shrink: 0;
}
@media (min-width: 768px) {
    .news-card-img-wrap { width: 48%; }
}
.news-card-img {
    width: 100%;
    height: 380px;
    object-fit: cover;
    display: block;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.news-card:hover .news-card-img { transform: scale(1.05); }
 
.news-card-img-placeholder {
    width: 100%;
    height: 380px;
    background: var(--eat-surface-high);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--eat-on-surface-variant);
    opacity: 0.25;
}
.news-card-img-placeholder .material-symbols-outlined { font-size: 3rem; }
 
.news-card-body {
    width: 100%;
    padding: 2rem;
    padding-top: 4rem;
    background: var(--eat-surface-high);
    border-radius: var(--eat-radius);
    position: relative;
}
.news-card-body--right { text-align: right; }
 
.news-card-date-bg {
    position: absolute;
    top: 1rem;
    left: 2rem;
    font-family: var(--font-headline);
    font-size: 2rem;
    font-style: italic;
    color: var(--eat-on-surface);
    opacity: 0.08;
    pointer-events: none;
    user-select: none;
}
.news-card-body--right .news-card-date-bg {
    left: auto;
    right: 2rem;
}
 
.news-card-category {
    display: block;
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.4em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 1rem;
}
.news-card-title {
    font-family: var(--font-headline);
    font-size: 1.65rem;
    color: var(--eat-on-surface);
    line-height: 1.35;
    margin-bottom: 1.25rem;
}
.news-card-summary {
    font-family: var(--font-body);
    font-size: 1rem;
    line-height: 1.85;
    color: var(--eat-on-surface);
    opacity: 0.75;
    font-style: italic;
    margin-bottom: 1.25rem;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
.news-card-meta {
    display: flex;
    align-items: center;
    gap: 0.3rem;
    font-family: var(--font-label);
    font-size: 0.72rem;
    color: var(--eat-on-surface-variant);
    opacity: 0.45;
    margin-bottom: 1.75rem;
}
.news-card-body--right .news-card-meta { justify-content: flex-end; }
.news-card-meta-icon { font-size: 1rem; }
 
.news-card-link {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--eat-primary);
    text-decoration: none;
    transition: gap 0.3s ease;
}
.news-card-link:hover { gap: 0.85rem; }
.news-card-link--reverse { flex-direction: row-reverse; }
 
/* ── Divider ──────────────────────────────────────── */
.news-divider {
    display: flex;
    justify-content: center;
    opacity: 0.3;
}
 
/* ── Pagination ───────────────────────────────────── */
.news-pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 2rem;
    padding-top: 4rem;
}
.news-page-btn {
    width: 3rem;
    height: 3rem;
    border-radius: 50%;
    border: 1px solid rgba(201, 169, 110, 0.2);
    background: transparent;
    color: var(--eat-secondary);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s ease;
}
.news-page-btn:hover:not(:disabled) {
    background: var(--eat-primary);
    color: var(--eat-on-primary);
}
.news-page-btn:disabled { opacity: 0.25; cursor: not-allowed; }
.news-page-numbers {
    display: flex;
    gap: 1rem;
    font-family: var(--font-label);
    font-size: 0.82rem;
    letter-spacing: 0.12em;
}
.news-page-num {
    cursor: pointer;
    color: var(--eat-on-surface);
    opacity: 0.35;
    transition: all 0.2s ease;
    padding-bottom: 2px;
}
.news-page-num:hover { color: var(--eat-primary); opacity: 1; }
.news-page-num.active {
    color: var(--eat-primary);
    opacity: 1;
    border-bottom: 1px solid var(--eat-primary);
}
 
/* ── Loading / Empty ──────────────────────────────── */
.news-state {
    padding: 6rem 0;
    text-align: center;
}
.news-state-text {
    font-family: var(--font-body);
    font-style: italic;
    color: var(--eat-on-surface-variant);
    opacity: 0.45;
}
 
/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .news-hero    { padding: 8rem 1.5rem 3rem; }
    .news-layout  { padding: 2.5rem 1.5rem 5rem; }
    .news-card-body { padding: 1.5rem; padding-top: 3.5rem; }
}
</style>
 