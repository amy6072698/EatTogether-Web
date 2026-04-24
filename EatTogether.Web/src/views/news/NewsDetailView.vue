<template>
    <div class="detail-root">
        <div v-if="loading" class="detail-state" style="padding-top: 8rem">
            <span class="detail-state-text">載入中…</span>
        </div>

        <div v-else-if="error" class="detail-state" style="padding-top: 8rem">
            <span class="detail-state-text">文章載入失敗，請稍後再試。</span>
        </div>

        <template v-else>
            <!-- Intro：圖左文右 -->
            <header class="detail-intro">
                <div class="detail-intro-img">
                    <img
                        v-if="article.coverImageUrl"
                        :src="article.coverImageUrl"
                        :alt="article.title"
                    />
                    <div v-else class="detail-intro-img-placeholder"></div>
                </div>
                <div class="detail-intro-text">
                    <div class="detail-title-row">
                        <i v-if="article.isPinned" class="bi bi-pin-fill detail-pin-icon"></i>
                        <h1 class="detail-title">{{ article.title }}</h1>
                    </div>
                    <span class="detail-eyebrow">{{ article.categoryName }}</span>
                    <div class="detail-meta-row">
                        <span>{{ formatDate(article.publishDate) }}</span>
                        <span class="detail-sep">·</span>
                        <span>{{ article.viewCount }} 次閱覽</span>
                    </div>
                </div>
            </header>

            <!-- 麵包屑 -->
            <nav class="detail-breadcrumb">
                <RouterLink to="/" class="detail-bc-link">Home</RouterLink>
                <span class="detail-bc-sep">/</span>
                <RouterLink to="/news" class="detail-bc-link">News</RouterLink>
                <span class="detail-bc-sep">/</span>
                <span class="detail-bc-current">{{ article.title }}</span>
            </nav>

            <!-- 主體 -->
            <main class="detail-main">
                <!-- eslint-disable-next-line vue/no-v-html -->
                <article class="detail-body" v-html="article.description"></article>
                <!-- 這裡保留是文章非純文字會含有html標籤，使用v-html呈現。以白名單方式預防XSS攻擊，後端會過濾掉不安全的標籤與屬性。 -->

                <div class="detail-divider">
                    <svg width="200" height="20" viewBox="0 0 200 20" fill="none">
                        <path
                            d="M0 10C50 10 70 2 100 2C130 2 150 18 200 18"
                            stroke="#C9A96E"
                            stroke-width="0.5"
                            stroke-dasharray="4 4"
                        />
                    </svg>
                </div>

                <div v-if="article.categoryName" class="detail-tags">
                    <span class="detail-tag"># {{ article.categoryName }}</span>
                </div>

                <nav class="detail-nav">
                    <RouterLink
                        v-if="prevArticle"
                        :to="{ name: 'NewsDetail', params: { id: prevArticle.id } }"
                        class="detail-nav-link"
                    >
                        <span class="detail-nav-arrow">←</span>
                        <div class="detail-nav-info">
                            <span class="detail-nav-label">上一篇</span>
                            <span class="detail-nav-ttl">{{ prevArticle.title }}</span>
                        </div>
                    </RouterLink>
                    <div v-else></div>

                    <RouterLink
                        v-if="nextArticle"
                        :to="{ name: 'NewsDetail', params: { id: nextArticle.id } }"
                        class="detail-nav-link detail-nav-link--right"
                    >
                        <div class="detail-nav-info detail-nav-info--right">
                            <span class="detail-nav-label">下一篇</span>
                            <span class="detail-nav-ttl">{{ nextArticle.title }}</span>
                        </div>
                        <span class="detail-nav-arrow">→</span>
                    </RouterLink>
                    <div v-else></div>
                </nav>

                <div class="detail-back-wrap">
                    <RouterLink to="/news" class="detail-back-btn">回列表頁</RouterLink>
                </div>
            </main>
        </template>
    </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import apiFetch from '@/utils/apiFetch.js'

const route = useRoute()

const article = ref({
    title: '',
    categoryName: '',
    description: '',
    coverImageUrl: null,
    publishDate: null,
    viewCount: 0,
})
const prevArticle = ref(null)
const nextArticle = ref(null)
const loading = ref(false)
const error = ref(false)

async function fetchDetail(id) {
    loading.value = true
    error.value = false
    try {
        const res = await apiFetch(`/News/${id}`)
        if (!res.ok) {
            // 404、500 都會在這裡被攔截
            error.value = true
            return
        }
        const data = await res.json()
        article.value = data.article
        prevArticle.value = data.prev ?? null
        nextArticle.value = data.next ?? null
    } catch (e) {
        console.error('載入文章失敗', e)
        error.value = true
    } finally {
        loading.value = false
    }
}

function formatDate(dateStr) {
    if (!dateStr) return ''
    const d = new Date(dateStr)
    return `${d.getFullYear()}.${String(d.getMonth() + 1).padStart(2, '0')}.${String(d.getDate()).padStart(2, '0')}`
}

watch(
    () => route.params.id,
    (id) => {
        if (id) fetchDetail(id)
    }
)

onMounted(() => {
    fetchDetail(route.params.id)
})
</script>

<style scoped>
.detail-root {
    background-color: var(--eat-surface-lowest);
    min-height: 100vh;
    padding-bottom: 6rem;
}

/* ── Intro 滿版橫列 ───────────────────────────────── */
.detail-intro {
    display: flex;
    flex-direction: row;
    width: 100%;
    height: 320px;
    margin: 0;
    padding: 0;
    gap: 0;
}

.detail-intro-img {
    width: 30%;
    height: 100%;
    overflow: hidden;
    flex-shrink: 0;
    border-radius: 0;
    aspect-ratio: unset;
}

.detail-intro-img img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    object-position: center;
    display: block;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.detail-intro:hover .detail-intro-img img {
    transform: scale(1.04);
}

.detail-intro-img-placeholder {
    width: 100%;
    height: 100%;
    background: var(--eat-surface-high);
}

.detail-intro-text {
    flex: 1;
    background: var(--eat-surface-high);
    padding: 2.5rem 3rem 2rem;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 0.75rem;
    position: relative;
    border-radius: 0;
}

/* RWD：手機才改直排 */
@media (max-width: 767px) {
    .detail-intro {
        flex-direction: column;
        height: auto;
    }
    .detail-intro-img {
        width: 100%;
        height: 260px;
    }
}

/* ── 共用文字元件 ──────────────────────────────────── */
.detail-eyebrow {
    display: block;
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.4em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-left: 0.5rem;
}

.detail-title {
    font-family: var(--font-headline);
    font-size: clamp(1.6rem, 3.5vw, 2.6rem);
    color: var(--eat-on-surface);
    line-height: 3.5;
    font-style: italic;
    margin: 0;
}

.detail-title-row {
    position: relative;
    padding-left: 0;
}

.detail-pin-icon {
    position: absolute;
    top: -1rem; /* 在標題上方 */
    left: 0.5rem;
    font-size: 1rem;
    color: var(--eat-secondary);
    opacity: 0.8;
    rotate: 45deg;
}

.detail-meta-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.75rem;
    color: var(--eat-on-surface-variant);
    opacity: 0.5;
    margin-left: 0.5rem;
}
.detail-sep {
    opacity: 0.4;
}

/* ── 麵包屑 ───────────────────────────────────────── */
.detail-breadcrumb {
    max-width: 1100px;
    margin: 0 auto;
    padding: 1.25rem 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    font-family: var(--font-label);
    font-size: 0.7rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.5;
    flex-wrap: wrap;
}
.detail-bc-link {
    color: inherit;
    text-decoration: none;
    transition: color 0.2s;
}
.detail-bc-link:hover {
    color: var(--eat-primary);
    opacity: 1;
}
.detail-bc-sep {
    opacity: 0.3;
}
.detail-bc-current {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 240px;
}

/* ── 主體 ─────────────────────────────────────────── */
.detail-main {
    max-width: 860px;
    margin: 0 auto;
    padding: 2.5rem 2rem 4rem;
}

/* ── 內文排版 ──────────────────────────────────────── */
.detail-body {
    font-family: var(--font-body);
    font-size: 1.1rem;
    line-height: 2.5;
    color: var(--eat-on-surface);
    opacity: 0.85;
}
.detail-body :deep(p) {
    margin-bottom: 3rem;
}
.detail-body :deep(strong) {
    font-weight: 600;
}
.detail-body :deep(em) {
    font-style: italic;
    color: var(--eat-secondary);
}
.detail-body :deep(u) {
    text-underline-offset: 4px;
}
.detail-body :deep(a) {
    color: var(--eat-primary);
    text-decoration: underline;
    text-underline-offset: 3px;
    transition: opacity 0.2s;
}
.detail-body :deep(a:hover) {
    opacity: 0.7;
}
.detail-body :deep(.ql-size-small) {
    font-size: 0.85rem;
}
.detail-body :deep(.ql-size-large) {
    font-size: 1.3rem;
}
.detail-body :deep(.ql-size-huge) {
    font-size: 1.6rem;
}

/* ── Quill文字大小 ───────────────────────────────────────── */
.ql-size-small {
    font-size: 0.75em;
}

.ql-size-large {
    font-size: 1.5em;
}

.ql-size-huge {
    font-size: 2.5em;
}

/* ── 分隔線 ───────────────────────────────────────── */
.detail-divider {
    display: flex;
    justify-content: center;
    margin: 3rem 0 2rem;
    opacity: 0.35;
}

/* ── 標籤 ─────────────────────────────────────────── */
.detail-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-bottom: 3rem;
}
.detail-tag {
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.2em;
    padding: 0.3rem 0.9rem;
    background: var(--eat-surface-high);
    color: var(--eat-on-surface-variant);
    border-radius: var(--eat-radius);
    transition:
        background 0.2s,
        color 0.2s;
}
.detail-tag:hover {
    background: var(--eat-surface-highest);
    color: var(--eat-secondary);
}

/* ── 上下篇 ───────────────────────────────────────── */
.detail-nav {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 1.5rem;
    border-top: 1px solid rgba(201, 169, 110, 0.12);
    padding-top: 2rem;
    margin-bottom: 3rem;
}
.detail-nav-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    text-decoration: none;
    color: var(--eat-on-surface-variant);
    opacity: 0.5;
    transition: all 0.25s ease;
    max-width: 45%;
}
.detail-nav-link:hover {
    opacity: 1;
    color: var(--eat-primary);
}
.detail-nav-link--right {
    flex-direction: row;
    justify-content: flex-end;
}
.detail-nav-arrow {
    font-size: 1.1rem;
    flex-shrink: 0;
    color: var(--eat-secondary);
}
.detail-nav-info {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.detail-nav-info--right {
    text-align: right;
}
.detail-nav-label {
    font-family: var(--font-label);
    font-size: 0.62rem;
    letter-spacing: 0.25em;
    text-transform: uppercase;
    opacity: 0.6;
}
.detail-nav-ttl {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 0.9rem;
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

/* ── 回列表 ───────────────────────────────────────── */
.detail-back-wrap {
    display: flex;
    justify-content: center;
}
.detail-back-btn {
    display: inline-block;
    padding: 0.7rem 3rem;
    border: 1px solid rgba(201, 169, 110, 0.25);
    color: var(--eat-secondary);
    font-family: var(--font-label);
    font-size: 0.7rem;
    letter-spacing: 0.25em;
    text-transform: uppercase;
    text-decoration: none;
    transition: all 0.4s ease;
    border-radius: var(--eat-radius);
}
.detail-back-btn:hover {
    background: var(--eat-secondary);
    color: var(--eat-on-primary);
}

/* ── 載入/錯誤 ────────────────────────────────────── */
.detail-state {
    padding: 6rem 0;
    text-align: center;
}
.detail-state-text {
    font-family: var(--font-body);
    font-style: italic;
    color: var(--eat-on-surface-variant);
    opacity: 0.45;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 767px) {
    .detail-intro {
        margin-top: 1.5rem;
        padding: 0 1rem;
    }
    .detail-main {
        padding: 1.5rem 1.25rem 4rem;
    }
    .detail-body {
        font-size: 1rem;
    }
    .detail-nav {
        flex-direction: column;
        gap: 1.5rem;
    }
    .detail-nav-link {
        max-width: 100%;
    }
    .detail-nav-link--right {
        justify-content: flex-start;
    }
    .detail-nav-info--right {
        text-align: left;
    }
    .detail-bc-current {
        max-width: 160px;
    }
}
</style>
