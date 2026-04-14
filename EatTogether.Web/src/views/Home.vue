<template>
    <!-- ═══════════════════════════════════════════
       Hero Banner
  ════════════════════════════════════════════ -->
    <section class="hero-section" id="hero">
        <div class="hero-bg" :class="{ loaded: heroBgLoaded }"></div>
        <div class="hero-overlay"></div>

        <div class="hero-content">
            <span class="hero-eyebrow">Since 1924 · 道地義式料理</span>
            <h1 class="eat-display mb-0">義起吃</h1>
            <p class="hero-subtitle">道地義式，與你共享</p>
            <div class="hero-cta">
                <Button variant="primary" to="/reservation">立即訂位</Button>
                <Button variant="secondary" to="/menu">查看菜單</Button>
            </div>
        </div>

        <a class="hero-scroll-hint" href="#brand">
            <span>Scroll</span>
            <div class="scroll-chevron"></div>
        </a>
    </section>

    <!-- ═══════════════════════════════════════════
       品牌介紹 — 左圖右文
  ════════════════════════════════════════════ -->
    <section class="brand-section" id="brand">
        <div class="container">
            <div class="row align-items-center g-5">
                <!-- 左欄：圖片 + candle-glow 光暈 -->
                <div class="col-12 col-lg-5">
                    <div class="brand-img-wrap">
                        <img
                            src="https://images.unsplash.com/photo-1510626176961-4b57d4fbad03?w=800&q=80"
                            alt="義起吃品牌故事"
                            loading="lazy"
                        />
                        <div class="candle-glow brand-glow"></div>
                    </div>
                </div>

                <!-- 右欄：文字 -->
                <div class="col-12 col-lg-7">
                    <div class="brand-text-wrap">
                        <span class="since-label">Since 1924</span>
                        <h2 class="eat-h1 mb-0">
                            傳承百年的<br /><em>時光風味</em>
                        </h2>
                        <div class="feather-divider on-low"></div>
                        <p class="brand-body-text mb-3">
                            「義起吃」不僅僅是一家餐廳，它是義大利薩爾迪尼亞的延伸。每一道料理都承載著托斯卡尼燭光的溫暖，以及拿坡里海洋沁涼的氣息。
                        </p>
                        <p class="brand-body-text">
                            我們堅持使用義大利邊境進口的頂級麵粉與百年特種橄欖油，由廚店主廚每日手工製麵糰，在濃厚的燭光氛圍中，為您獻上最純粹的歌式美饌。
                        </p>
                        <RouterLink to="/about" class="brand-link">
                            探索我們的故事
                            <span class="brand-link-arrow">→</span>
                        </RouterLink>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ═══════════════════════════════════════════
       熱門餐點 — Dish Cards + Swiper 輪播
  ════════════════════════════════════════════ -->
    <section class="dishes-section" id="dishes">
        <div class="container">
            <!-- Section Header + Swiper 方向鍵 -->
            <div class="section-header">
                <div>
                    <span class="section-eyebrow">Signature Dishes</span>
                    <h2 class="eat-h2 mb-0">招牌料理預覽</h2>
                </div>
                <div class="swiper-nav-wrap">
                    <button
                        class="swiper-nav-btn"
                        id="dishesPrev"
                        aria-label="Previous"
                    >
                        <svg
                            viewBox="0 0 16 16"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            <path d="M10 3L5 8l5 5" />
                        </svg>
                    </button>
                    <button
                        class="swiper-nav-btn"
                        id="dishesNext"
                        aria-label="Next"
                    >
                        <svg
                            viewBox="0 0 16 16"
                            fill="none"
                            stroke="currentColor"
                            stroke-width="2"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                        >
                            <path d="M6 3l5 5-5 5" />
                        </svg>
                    </button>
                </div>
            </div>

            <!-- Swiper -->
            <div class="swiper swiper-dishes">
                <div class="swiper-wrapper">
                    <div
                        class="swiper-slide"
                        v-for="dish in dishes"
                        :key="dish.id"
                    >
                        <div class="card-eat">
                            <div class="card-img-wrap">
                                <img
                                    :src="dish.img"
                                    :alt="dish.name"
                                    loading="lazy"
                                />
                                <div class="card-img-overlay-gradient"></div>
                                <span
                                    class="card-badge"
                                    :style="dish.badgeStyle"
                                    >{{ dish.badge }}</span
                                >
                                <button
                                    class="card-favorite"
                                    aria-label="加入收藏"
                                >
                                    ♡
                                </button>
                            </div>
                            <div class="card-body">
                                <div
                                    class="d-flex justify-content-between align-items-start"
                                >
                                    <h3 class="card-title">{{ dish.name }}</h3>
                                    <span class="card-price"
                                        >NT$ {{ dish.price }}</span
                                    >
                                </div>
                                <p class="card-text">{{ dish.desc }}</p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="swiper-pagination"></div>
            </div>

            <!-- 瀏覽完整菜單 CTA -->
            <div class="text-center mt-5">
                <Button variant="tertiary" to="/menu">瀏覽完整菜單</Button>
            </div>
        </div>
    </section>

    <!-- ═══════════════════════════════════════════
       預約 Banner
  ════════════════════════════════════════════ -->
    <section class="reserve-section" id="reserve">
        <div class="reserve-bg"></div>
        <div class="reserve-overlay"></div>
        <div class="reserve-glow"></div>

        <div class="reserve-content">
            <span class="reserve-eyebrow">Reserve a Table</span>
            <h2 class="eat-h1 mb-3">為您的特別時刻，<br />預留一個燭光席位</h2>
            <p class="reserve-body">
                不論是浪漫約會或友好聚餐，我們都將為您呈現最難忘的歌式之夜。
            </p>
            <Button variant="primary" to="/reservation">
                <svg
                    width="14"
                    height="14"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    style="flex-shrink: 0"
                >
                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                    <line x1="16" y1="2" x2="16" y2="6" />
                    <line x1="8" y1="2" x2="8" y2="6" />
                    <line x1="3" y1="10" x2="21" y2="10" />
                </svg>
                線上預訂
            </Button>
        </div>
    </section>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { RouterLink } from 'vue-router'
import Swiper from 'swiper'
import { Navigation, Pagination } from 'swiper/modules'
import 'swiper/css'
import 'swiper/css/pagination'
import Button from '@/components/Button.vue'

// ── Hero 背景圖漸入 ──────────────────────────────────
const heroBgLoaded = ref(false)

// ── 動態資料與 API 串接 ──────────────────────────────
// 1. 準備一個空的陣列，等後台把資料倒進來
const dishes = ref([])

// 2. 去後台抓資料的函式
async function fetchDishes() {
    const API_URL = import.meta.env.VITE_API_BASE_URL || '/api'
    const endpoint = `${API_URL}/Dishes/GetAllJson`
    const ORIGIN = 'https://localhost:7119'
    
    try {
        const response = await fetch(endpoint)

        if (!response.ok) {
            throw new Error(`伺服器回應錯誤：${response.status} (路徑：${endpoint})`)
        }

        const data = await response.json()

        // 分類過濾邏輯：每個分類只取前 3 筆，並保持原始順序
        const filteredData = []
        const categoryCount = {}
        data.forEach(item => {
            const cid = item.categoryId
            if (!categoryCount[cid]) categoryCount[cid] = 0
            if (categoryCount[cid] < 3) {
                filteredData.push(item)
                categoryCount[cid]++
            }
        })

        // 對齊資料格式
        dishes.value = filteredData.map((item, index) => {
            if (index === 0) console.log('第一筆完整資料物件 (請檢查裡面的欄位名稱):', item);

            // 1. 優先從回傳資料抓路徑 (嘗試不同可能的大小寫)
            let rawPath = item.imageUrl || item.ImageUrl || item.imagePath || '';
            
            // 2. 如果都沒有路徑 (null)，嘗試用菜名匹配 (例如 images/菜名.jpg)
            if (!rawPath && item.dishName) {
                rawPath = `images/${item.dishName}.jpg`;
            }

            // 強化圖片路徑處理
            let imgPath = rawPath.replace(/\\/g, '/').replace(/^~\//, '');
            const wwwrootMatch = /\/wwwroot\/(.*)/i.exec('/' + imgPath);
            if (wwwrootMatch && wwwrootMatch[1]) {
                imgPath = wwwrootMatch[1];
            }
            imgPath = imgPath.replace(/^\//, '');
            
            const finalImg = imgPath 
                ? `/api/${imgPath}` 
                : 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=600&q=80';

            return {
                id: item.id,
                name: item.dishName,
                price: item.price.toLocaleString(),
                img: finalImg,
                desc: '道地義式風味，主廚嚴選推薦。',
                badge: item.price > 300 ? '熱銷' : '推薦'
            };
        })

        await nextTick()
        initSwiper()
    } catch (error) {
        console.error('抓取失敗：', error)
        // 防呆機制：如果連不到後台，塞一筆提示資料讓畫面不空
        dishes.value = [
            {
                id: 999,
                name: '後台連線失敗',
                price: '0',
                img: 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=600&q=80',
                desc: `請檢查 API 路徑是否正確：${endpoint}`,
                badge: '錯誤'
            }
        ]
        await nextTick()
        initSwiper()
    }
}
    

// ── Swiper 初始化 ─────────────────────────────────────
function initSwiper() {
    new Swiper('.swiper-dishes', {
        modules: [Navigation, Pagination],
        slidesPerView: 1.15,
        spaceBetween: 20,
        grabCursor: true,
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        navigation: {
            prevEl: '#dishesPrev',
            nextEl: '#dishesNext',
        },
        breakpoints: {
            576: { slidesPerView: 1.5, spaceBetween: 20 },
            768: { slidesPerView: 2, spaceBetween: 24 },
            992: { slidesPerView: 2.5, spaceBetween: 24 },
            1200: { slidesPerView: 3, spaceBetween: 28 },
        },
    })
}

// ── 生命週期：網頁載入時執行 ──────────────────────────
onMounted(() => {
    heroBgLoaded.value = true
    fetchDishes() // 網頁一開，就去抓資料
})
</script>

<style scoped>
/* ── Hero ─────────────────────────────────────────── */
.hero-section {
    position: relative;
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    background-color: var(--eat-surface-lowest);
}
.hero-bg {
    position: absolute;
    inset: 0;
    background-image: url('https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=1600&q=80');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    transform: scale(1.04);
    transition: transform 8s ease;
}
.hero-bg.loaded {
    transform: scale(1);
}
.hero-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
        to bottom,
        rgba(24, 11, 6, 0.35) 0%,
        rgba(24, 11, 6, 0.55) 50%,
        rgba(24, 11, 6, 0.82) 100%
    );
}
.hero-content {
    position: relative;
    z-index: 2;
    text-align: center;
    padding: 2rem 1rem;
}
.hero-eyebrow {
    font-family: var(--font-label);
    font-size: 0.72rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 1.25rem;
    display: block;
}
.hero-subtitle {
    font-family: var(--font-body);
    font-size: clamp(1rem, 2.5vw, 1.35rem);
    color: rgba(249, 221, 211, 0.75);
    font-style: italic;
    letter-spacing: 0.08em;
    margin-top: 1rem;
    margin-bottom: 2.5rem;
}
.hero-cta {
    display: flex;
    gap: 1rem;
    justify-content: center;
    flex-wrap: wrap;
}
.hero-scroll-hint {
    position: absolute;
    bottom: 2.5rem;
    left: 50%;
    transform: translateX(-50%);
    z-index: 2;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.4rem;
    color: rgba(208, 197, 181, 0.4);
    cursor: pointer;
    transition: color 0.3s;
    text-decoration: none;
}
.hero-scroll-hint:hover {
    color: var(--eat-secondary);
}
.hero-scroll-hint span {
    font-family: var(--font-label);
    font-size: 0.6rem;
    letter-spacing: 0.25em;
    text-transform: uppercase;
}
.scroll-chevron {
    width: 20px;
    height: 20px;
    border-right: 1px solid currentColor;
    border-bottom: 1px solid currentColor;
    transform: rotate(45deg);
    animation: bounce-down 1.8s ease-in-out infinite;
}
@keyframes bounce-down {
    0%,
    100% {
        transform: rotate(45deg) translateY(0);
        opacity: 0.4;
    }
    50% {
        transform: rotate(45deg) translateY(5px);
        opacity: 1;
    }
}

/* ── 品牌介紹 ─────────────────────────────────────── */
.brand-section {
    background-color: var(--eat-surface-low);
    padding: 6rem 0;
    overflow: hidden;
}
.brand-img-wrap {
    position: relative;
    border-radius: var(--eat-radius);
    overflow: hidden;
    height: 520px;
}
.brand-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
}
.brand-glow {
    position: absolute;
    inset: 0;
    border-radius: var(--eat-radius);
}
.since-label {
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 0.75rem;
    display: block;
}
.brand-text-wrap {
    display: flex;
    flex-direction: column;
    justify-content: center;
    height: 100%;
    padding-left: 1rem;
}
.brand-body-text {
    font-family: var(--font-body);
    font-size: 0.975rem;
    color: rgba(249, 221, 211, 0.72);
    font-style: italic;
    line-height: 1.9;
    margin-bottom: 0;
}
.brand-link {
    font-family: var(--font-label);
    font-size: 0.72rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--eat-primary);
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    transition:
        gap 0.35s ease,
        opacity 0.35s ease;
    margin-top: 2rem;
}
.brand-link:hover {
    gap: 0.85rem;
    opacity: 0.8;
    color: var(--eat-primary);
}
.brand-link-arrow {
    transition: transform 0.35s ease;
}
.brand-link:hover .brand-link-arrow {
    transform: translateX(4px);
}

/* ── 熱門餐點 ─────────────────────────────────────── */
.dishes-section {
    background-color: var(--eat-surface);
    padding: 5.5rem 0 6rem;
    overflow-x: hidden; /* 防止 swiper overflow:visible 撐出水平捲軸 */
}
.section-header {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    margin-bottom: 3rem;
    flex-wrap: wrap;
    gap: 1rem;
}
.section-eyebrow {
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 0.4rem;
    display: block;
}
.swiper-dishes {
    overflow: visible !important;
    padding-bottom: 3.5rem !important;
}
:deep(.swiper-slide) {
    height: auto;
}
:deep(.swiper-pagination-bullet) {
    background: rgba(208, 197, 181, 0.2);
    opacity: 1;
    width: 22px;
    height: 2px;
    border-radius: 0;
    transition: var(--eat-transition);
}
:deep(.swiper-pagination-bullet-active) {
    background: var(--eat-primary);
    width: 36px;
}
.swiper-nav-wrap {
    display: flex;
    gap: 0.5rem;
}
.swiper-nav-btn {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background: rgba(30, 16, 11, 0.65);
    backdrop-filter: blur(8px);
    border: 1px solid rgba(227, 199, 107, 0.22);
    color: var(--eat-on-surface-variant);
    display: inline-flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: var(--eat-transition);
    flex-shrink: 0;
}
.swiper-nav-btn:hover {
    background: rgba(227, 199, 107, 0.12);
    border-color: var(--eat-primary);
    color: var(--eat-primary);
}
.swiper-nav-btn svg {
    width: 14px;
    height: 14px;
}

/* Dish Card */
.card-eat {
    background: var(--eat-surface-high);
    border: none;
    border-radius: var(--eat-radius);
    overflow: hidden;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
    height: 100%;
}
.card-eat:hover {
    transform: translateY(-8px);
}
.card-img-wrap {
    position: relative;
    height: 280px;
    overflow: hidden;
}
.card-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 1s cubic-bezier(0.4, 0, 0.2, 1);
}
.card-eat:hover .card-img-wrap img {
    transform: scale(1.08);
}
.card-img-overlay-gradient {
    position: absolute;
    inset: 0;
    background: linear-gradient(
        to top,
        var(--eat-surface-high) 0%,
        transparent 50%
    );
    opacity: 0.85;
}
.card-badge {
    position: absolute;
    top: 1rem;
    left: 1rem;
    background: rgba(227, 199, 107, 0.18);
    backdrop-filter: blur(6px);
    border: 1px solid rgba(227, 199, 107, 0.45);
    padding: 0.15rem 0.55rem;
    font-family: var(--font-label);
    font-size: 0.65rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: var(--eat-primary);
    border-radius: var(--eat-radius-sm);
}
.card-favorite {
    position: absolute;
    top: 1rem;
    right: 1rem;
    background: none;
    border: none;
    color: rgba(249, 221, 211, 0.6);
    cursor: pointer;
    transition: color 0.3s ease;
    font-size: 1.25rem;
    line-height: 1;
}
.card-favorite:hover {
    color: var(--eat-primary);
}
.card-body {
    padding: 1.75rem 2rem;
    background: var(--eat-surface-high);
}
.card-title {
    font-family: var(--font-headline);
    font-size: 1.35rem;
    color: var(--eat-primary);
    font-style: italic;
    margin-bottom: 0.25rem;
}
.card-price {
    font-family: var(--font-label);
    font-size: 0.85rem;
    color: var(--eat-secondary);
    letter-spacing: 0.08em;
    white-space: nowrap;
}
.card-text {
    font-family: var(--font-body);
    font-size: 0.9rem;
    color: rgba(249, 221, 211, 0.65);
    font-style: italic;
    line-height: 1.7;
    margin-top: 0.75rem;
    margin-bottom: 0;
}
.card-footer-action {
    padding: 0 1.75rem 1.75rem;
    background: var(--eat-surface-high);
    display: flex;
    align-items: center;
}

/* ── 預約 Banner ──────────────────────────────────── */
.reserve-section {
    position: relative;
    overflow: hidden;
    background-color: var(--eat-surface-lowest);
}
.reserve-bg {
    position: absolute;
    inset: 0;
    background-image: url('https://images.unsplash.com/photo-1428515613728-6b4607e44363?w=1400&q=80');
    background-size: cover;
    background-position: center 30%;
    background-attachment: fixed;
}
.reserve-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
        135deg,
        rgba(24, 11, 6, 0.88) 0%,
        rgba(30, 16, 11, 0.72) 100%
    );
}
.reserve-glow {
    position: absolute;
    inset: 0;
    background: radial-gradient(
        ellipse at 50% 50%,
        rgba(227, 199, 107, 0.1) 0%,
        rgba(30, 16, 11, 0) 65%
    );
    pointer-events: none;
}
.reserve-content {
    position: relative;
    z-index: 2;
    padding: 7rem 1rem;
    text-align: center;
}
.reserve-eyebrow {
    font-family: var(--font-label);
    font-size: 0.68rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-secondary);
    margin-bottom: 1.25rem;
    display: block;
}
.reserve-body {
    font-family: var(--font-body);
    font-size: clamp(0.9rem, 2vw, 1.05rem);
    color: rgba(249, 221, 211, 0.65);
    font-style: italic;
    max-width: 520px;
    margin: 0 auto 2.75rem;
    line-height: 1.85;
    letter-spacing: 0.02em;
}

/* ── RWD ──────────────────────────────────────────── */
@media (max-width: 991px) {
    .brand-text-wrap {
        padding-left: 0;
        padding-top: 2rem;
    }
    .brand-img-wrap {
        height: 380px;
    }
    .reserve-bg {
        background-attachment: scroll;
    }
}
@media (max-width: 767px) {
    .hero-cta {
        flex-direction: column;
        align-items: center;
    }
    .section-header {
        flex-direction: column;
        align-items: flex-start;
    }
}
</style>
