<template>
    <div class="menu-page">
        <header class="menu-header">
            <div class="container">
                <span class="menu-eyebrow">Signature Flavors</span>
                <h1 class="eat-h1">精選菜單</h1>

                <nav class="category-tabs">
                    <button
                        v-for="cat in categories"
                        :key="cat.id"
                        class="tab-btn"
                        :class="{ active: currentCategory === cat.id }"
                        @click="currentCategory = cat.id"
                    >
                        {{ cat.name }}
                    </button>
                </nav>
            </div>
        </header>

        <main class="container py-5">
            <!-- 搜尋 + 過濾列 -->
            <div class="filter-bar">
                <div class="search-wrap">
                    <span class="search-icon">🔍</span>
                    <input
                        v-model="searchQuery"
                        type="text"
                        placeholder="搜尋餐點名稱或描述..."
                        class="search-input"
                    />
                    <button v-if="searchQuery" class="search-clear" @click="searchQuery = ''">
                        ✕
                    </button>
                </div>
                <div class="filter-chips">
                    <button
                        class="filter-chip"
                        :class="{ active: filterVeg }"
                        @click="filterVeg = !filterVeg"
                    >
                        🥬 素食
                    </button>
                    <button
                        class="filter-chip"
                        :class="{ active: filterSpicy }"
                        @click="filterSpicy = !filterSpicy"
                    >
                        🌶️ 有辣
                    </button>
                    <button
                        class="filter-chip"
                        :class="{ active: filterRec }"
                        @click="filterRec = !filterRec"
                    >
                        ⭐ 主廚推薦
                    </button>
                    <button
                        class="filter-chip"
                        :class="{ active: filterFav }"
                        @click="filterFav = !filterFav"
                    >
                        ❤️ 我的最愛
                    </button>
                    <button
                        class="filter-chip"
                        :class="{ active: filterAvailable }"
                        @click="filterAvailable = !filterAvailable"
                    >
                        ✅ 供應中
                    </button>
                </div>
                <div class="view-toggle">
                    <button
                        class="view-btn"
                        :class="{ active: viewMode === 'grid' }"
                        @click="viewMode = 'grid'"
                        title="卡片模式"
                    >
                        ⊞
                    </button>
                    <button
                        class="view-btn"
                        :class="{ active: viewMode === 'list' }"
                        @click="viewMode = 'list'"
                        title="列表模式"
                    >
                        ≡
                    </button>
                </div>
                <select v-model="sortOrder" class="sort-select">
                    <option value="default">預設排序</option>
                    <option value="price-asc">價格低到高</option>
                    <option value="price-desc">價格高到低</option>
                    <option value="recommended">推薦優先</option>
                </select>
            </div>

            <!-- 篩選統計 -->
            <p class="filter-stats">
                <span v-if="!hasActiveFilter">共 {{ filteredDishes.length }} 道餐點</span>
                <span v-else>找到 {{ filteredDishes.length }} 道符合條件的餐點</span>
            </p>

            <!-- Loading -->
            <div v-if="loading" class="state-container">
                <div class="spinner"></div>
                <p class="loading-text">正在為您準備美味佳餚...</p>
            </div>

            <!-- Error -->
            <div v-else-if="error" class="state-container error-state">
                <div class="error-icon">!</div>
                <p>{{ error }}</p>
                <button @click="fetchMenu" class="retry-btn">重新嘗試</button>
            </div>

            <!-- 餐點卡片 -->
            <TransitionGroup
                v-else
                name="card"
                tag="div"
                :class="viewMode === 'grid' ? 'menu-grid' : 'menu-list'"
            >
                <div
                    v-for="dish in filteredDishes"
                    :key="dish.id"
                    class="dish-card"
                    :class="{ 'is-soldout': dish.stockStatus === 2 }"
                    @click="openModal(dish)"
                    @mousemove="handleMouseMove($event, dish.id)"
                    @mouseleave="handleMouseLeave(dish.id)"
                    :style="tiltStyles[dish.id]"
                >
                    <div class="dish-img-wrap">
                        <img
                            v-if="dish.imageUrl"
                            :src="formatImageUrl(dish.imageUrl)"
                            :alt="dish.dishName"
                            loading="lazy"
                        />
                        <div v-else class="img-placeholder">
                            <span>{{ dish.dishName.charAt(0) }}</span>
                        </div>
                        <div v-if="dish.stockStatus === 2" class="soldout-overlay">
                            <span class="soldout-text">售完</span>
                        </div>
                        <button
                            class="fav-btn"
                            @click.stop="toggleFavorite(dish.id)"
                            :aria-label="favorites.includes(dish.id) ? '取消收藏' : '加入收藏'"
                        >
                            {{ favorites.includes(dish.id) ? '❤️' : '🤍' }}
                        </button>
                        <div class="badge-group">
                            <span v-if="dish.isRecommended" class="badge badge-rec">推薦</span>
                            <span v-if="dish.isPopular" class="badge badge-pop">熱銷</span>
                            <span v-if="dish.isVegetarian" class="badge badge-veg">素食</span>
                            <span v-if="dish.stockStatus === 1" class="badge badge-low-stock"
                                >剩餘不多</span
                            >
                            <template v-if="dish.isLimited">
                                <span
                                    v-if="getCountdown(dish.endDate, dish.startDate) === '尚未供應'"
                                    class="badge badge-pending"
                                    >尚未供應</span
                                >
                                <span
                                    v-else-if="
                                        getCountdown(dish.endDate, dish.startDate) === '即將開始'
                                    "
                                    class="badge badge-soon"
                                    >即將開始</span
                                >
                                <span
                                    v-else-if="
                                        getCountdown(dish.endDate, dish.startDate) === '已結束'
                                    "
                                    class="badge badge-sold"
                                    >停止供應</span
                                >
                                <span v-else class="badge badge-lim fomo-timer">{{
                                    getCountdown(dish.endDate, dish.startDate)
                                }}</span>
                            </template>
                        </div>
                    </div>

                    <div class="dish-info">
                        <div class="dish-title-row">
                            <h3 class="dish-name">{{ dish.dishName }}</h3>
                            <span class="dish-price">NT$ {{ dish.price.toLocaleString() }}</span>
                        </div>
                        <p class="dish-desc">
                            {{
                                dish.description ||
                                '精選新鮮食材，傳承義式經典風味，每一口都是主廚的心意。'
                            }}
                        </p>
                        <div class="dish-footer">
                            <div class="dish-tags">
                                <span class="tag veg-tag" v-if="dish.isVegetarian">🥬 素食</span>
                                <span class="tag spicy-tag" v-if="dish.spicyLevel > 0">
                                    {{ '🌶️'.repeat(dish.spicyLevel) }}
                                </span>
                            </div>
                            <div class="category-name">{{ dish.categoryName }}</div>
                        </div>
                    </div>
                </div>
            </TransitionGroup>

            <!-- Empty -->
            <div
                v-if="!loading && !error && filteredDishes.length === 0"
                class="state-container empty-state"
            >
                <p>找不到符合條件的餐點，換個關鍵字或分類試試看。</p>
            </div>
        </main>

        <!-- ── Modal ── -->
        <Transition name="modal">
            <div v-if="isModalOpen && selectedDish" class="modal-overlay" @click.self="closeModal">
                <div class="modal-box">
                    <!-- 圖片區 -->
                    <div class="modal-img-wrap">
                        <img
                            v-if="selectedDish.imageUrl"
                            :src="formatImageUrl(selectedDish.imageUrl)"
                            :alt="selectedDish.dishName"
                            class="modal-img"
                        />
                        <div v-else class="modal-img-placeholder">
                            <span>{{ selectedDish.dishName.charAt(0) }}</span>
                        </div>
                        <div class="modal-img-gradient"></div>
                        <button class="modal-close" @click="closeModal">✕</button>
                        <div class="modal-badge-group">
                            <span v-if="selectedDish.isRecommended" class="badge badge-rec"
                                >推薦</span
                            >
                            <span v-if="selectedDish.isPopular" class="badge badge-pop">熱銷</span>
                            <span v-if="selectedDish.isVegetarian" class="badge badge-veg"
                                >素食</span
                            >
                        </div>
                    </div>

                    <!-- 資訊區 -->
                    <div class="modal-body">
                        <div class="modal-header-row">
                            <h2 class="modal-title">{{ selectedDish.dishName }}</h2>
                            <div class="modal-price">
                                NT$ {{ selectedDish.price.toLocaleString() }}
                            </div>
                        </div>

                        <!-- 描述 -->
                        <p class="text-on-surface-variant font-body text-sm italic mb-8">
                            {{
                                selectedDish.description ||
                                '主廚精選地道食材，為您呈現純粹的義式饗宴。'
                            }}
                        </p>

                        <!-- 精選食材 -->
                        <div
                            v-if="parseIngredients(selectedDish.ingredientsJson).length"
                            class="modal-section"
                        >
                            <h3
                                class="text-primary font-headline font-bold text-sm mb-4 tracking-widest"
                            >
                                精選食材
                            </h3>
                            <div class="grid grid-cols-2 md:grid-cols-3 gap-3 mb-8">
                                <div
                                    v-for="item in parseIngredients(selectedDish.ingredientsJson)"
                                    :key="item.name"
                                    class="bg-surface-container-low p-4 rounded-xl transition-all"
                                >
                                    <span
                                        class="text-on-surface font-headline font-bold text-xs block mb-1"
                                        >{{ item.name }}</span
                                    >
                                    <span
                                        class="text-on-surface-variant font-body text-[10px] italic"
                                        >{{ item.subDesc }}</span
                                    >
                                </div>
                            </div>
                        </div>

                        <!-- 屬性列 -->
                        <div class="modal-section">
                            <div class="modal-section-label">餐點屬性</div>
                            <div class="attr-chips">
                                <span class="attr-chip" v-if="selectedDish.categoryName">{{
                                    selectedDish.categoryName
                                }}</span>
                                <span class="attr-chip veg" v-if="selectedDish.isVegetarian"
                                    >🥬 素食</span
                                >
                                <span class="attr-chip spicy" v-if="selectedDish.spicyLevel > 0">
                                    {{ '🌶️'.repeat(selectedDish.spicyLevel) }}
                                    {{ spicyLabel(selectedDish.spicyLevel) }}
                                </span>
                                <span class="attr-chip rec" v-if="selectedDish.isRecommended"
                                    >⭐ 主廚推薦</span
                                >
                                <span class="attr-chip pop" v-if="selectedDish.isPopular"
                                    >🔥 熱銷</span
                                >
                            </div>
                        </div>

                        <!-- 評分 -->
                        <div class="modal-section">
                            <div class="modal-section-label">為這道餐點評分</div>
                            <div class="star-row">
                                <button
                                    v-for="star in 5"
                                    :key="star"
                                    class="star-btn"
                                    @click="submitRating(selectedDish.id, star)"
                                    @mouseenter="hoverStar = star"
                                    @mouseleave="hoverStar = 0"
                                    :aria-label="`${star} 顆星`"
                                >
                                    {{
                                        (hoverStar || userRatings[selectedDish.id] || 0) >= star
                                            ? '★'
                                            : '☆'
                                    }}
                                </button>
                            </div>
                            <p v-if="userRatings[selectedDish.id]" class="star-voted">
                                您已評 {{ userRatings[selectedDish.id] }} 顆星
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </Transition>
    </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'

// ── 安全解析食材 JSON ────────────────────────────────
const parseIngredients = (jsonString) => {
    if (!jsonString) return []
    try {
        const result = JSON.parse(jsonString)
        return Array.isArray(result) ? result : []
    } catch {
        return []
    }
}

// ── 3D Tilt ──────────────────────────────────────────
const tiltStyles = reactive({})

const handleMouseMove = (event, dishId) => {
    const card = event.currentTarget
    const rect = card.getBoundingClientRect()
    const relX = (event.clientX - rect.left) / rect.width - 0.5
    const relY = (event.clientY - rect.top) / rect.height - 0.5
    // --glare-x/y 讓 ::after 偽元素能跟著游標移動
    card.style.setProperty('--glare-x', `${((relX + 0.5) * 100).toFixed(1)}%`)
    card.style.setProperty('--glare-y', `${((relY + 0.5) * 100).toFixed(1)}%`)
    tiltStyles[dishId] = {
        transform: `rotateX(${(-relY * 15).toFixed(2)}deg) rotateY(${(relX * 15).toFixed(2)}deg) translateY(-10px) scale(1.02)`,
        transition: 'transform 0.08s ease',
        boxShadow: `${(-relX * 20).toFixed(1)}px ${(relY * 20 + 16).toFixed(1)}px 40px rgba(0,0,0,0.55), 0 0 0 1px rgba(227,199,107,0.18)`,
    }
}

const handleMouseLeave = (dishId) => {
    tiltStyles[dishId] = {
        transform: 'rotateX(0deg) rotateY(0deg) translateY(0) scale(1)',
        transition: 'transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1), box-shadow 0.6s ease',
        boxShadow: '',
    }
}

// ── 倒數計時 ─────────────────────────────────────────
const currentTime = ref(new Date())
let _clockTimer = null
onMounted(() => {
    _clockTimer = setInterval(() => {
        currentTime.value = new Date()
    }, 1000)
})
onUnmounted(() => {
    clearInterval(_clockTimer)
})

// 回傳四種狀態：'尚未供應' | '即將開始' | 倒數字串 | '已結束'
const getCountdown = (endDateString, startDateString) => {
    if (!endDateString) return ''
    const now = currentTime.value // 存取 .value 讓 Vue 追蹤響應性
    const end = new Date(endDateString)
    const start = startDateString ? new Date(startDateString) : null

    // 已超過結束時間
    if (end <= now) return '已結束'

    // 尚未開始
    if (start && start > now) {
        const msToStart = start - now
        // 超過 24 小時才開始 → 尚未供應；24 小時內 → 即將開始
        return msToStart > 24 * 60 * 60 * 1000 ? '尚未供應' : '即將開始'
    }

    // 供應中 — 計算距離結束的倒數
    const totalSec = Math.floor((end - now) / 1000)
    const days = Math.floor(totalSec / 86400)
    const hours = Math.floor((totalSec % 86400) / 3600)
    const minutes = Math.floor((totalSec % 3600) / 60)
    const seconds = totalSec % 60

    if (days >= 1) return `剩餘 ${days} 天 ${hours} 小時`
    const hh = String(hours).padStart(2, '0')
    const mm = String(minutes).padStart(2, '0')
    const ss = String(seconds).padStart(2, '0')
    return `倒數 ${hh}:${mm}:${ss}`
}

// ── State ────────────────────────────────────────────
const dishes = ref([])
const loading = ref(true)
const error = ref(null)
const currentCategory = ref(0)
const searchQuery = ref('')
const filterVeg = ref(false)
const filterSpicy = ref(false)
const filterRec = ref(false)
const filterFav = ref(false)
const filterAvailable = ref(false)
const sortOrder = ref('default')
const viewMode = ref('grid')

// ── 評分 ──────────────────────────────────────────────
const userRatings = reactive(JSON.parse(localStorage.getItem('menu-ratings') || '{}'))
const hoverStar = ref(0)

const submitRating = (dishId, star) => {
    userRatings[dishId] = star
    localStorage.setItem('menu-ratings', JSON.stringify(userRatings))
    const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'
    fetch(`${API_BASE}/Dishes/${dishId}/Rate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ score: star }),
    }).catch(() => {}) // fire-and-forget，忽略錯誤
}

// ── 收藏 ──────────────────────────────────────────────
const favorites = ref(JSON.parse(localStorage.getItem('menu-favorites') || '[]'))

const toggleFavorite = (dishId) => {
    const idx = favorites.value.indexOf(dishId)
    if (idx === -1) {
        favorites.value.push(dishId)
    } else {
        favorites.value.splice(idx, 1)
    }
    localStorage.setItem('menu-favorites', JSON.stringify(favorites.value))
}

// Modal
const isModalOpen = ref(false)
const selectedDish = ref(null)

const categories = [
    { id: 0, name: '全部' },
    { id: 1, name: '主餐' },
    { id: 2, name: '飲料' },
    { id: 3, name: '甜點' },
    { id: 4, name: '湯品' },
    { id: 5, name: '附餐' },
]

// ── Modal ────────────────────────────────────────────
const openModal = (dish) => {
    selectedDish.value = dish
    isModalOpen.value = true
    document.body.style.overflow = 'hidden'
}

const closeModal = () => {
    isModalOpen.value = false
    selectedDish.value = null
    document.body.style.overflow = ''
}

const handleEsc = (e) => {
    if (e.key === 'Escape') closeModal()
}
onMounted(() => window.addEventListener('keydown', handleEsc))
onUnmounted(() => window.removeEventListener('keydown', handleEsc))

// ── Utils ────────────────────────────────────────────
const spicyLabel = (level) => {
    return ['', '微辣', '中辣', '大辣', '極辣'][level] ?? '辣'
}

const formatImageUrl = (url) => {
    if (!url) return null
    return url.startsWith('/images/') ? url : null
}

// ── API ──────────────────────────────────────────────
const fetchMenu = async () => {
    loading.value = true
    error.value = null
    const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'
    try {
        const res = await fetch(`${API_BASE}/Dishes/GetActiveJson`)
        if (!res.ok) throw new Error(`抓取失敗 (${res.status})`)
        dishes.value = await res.json()
    } catch (err) {
        console.error('Menu Fetch Error:', err)
        error.value = '無法載入菜單資料，請確認 API 伺服器狀態。'
    } finally {
        loading.value = false
    }
}

// ── Computed ─────────────────────────────────────────
const hasActiveFilter = computed(
    () =>
        searchQuery.value !== '' ||
        filterVeg.value ||
        filterSpicy.value ||
        filterRec.value ||
        filterFav.value ||
        filterAvailable.value ||
        currentCategory.value !== 0 ||
        sortOrder.value !== 'default'
)
const filteredDishes = computed(() => {
    const result = dishes.value.filter((d) => {
        if (currentCategory.value !== 0 && d.categoryId !== currentCategory.value) return false
        if (searchQuery.value) {
            const q = searchQuery.value.toLowerCase()
            const hit =
                d.dishName.toLowerCase().includes(q) ||
                (d.description && d.description.toLowerCase().includes(q))
            if (!hit) return false
        }
        if (filterVeg.value && !d.isVegetarian) return false
        if (filterSpicy.value && !(d.spicyLevel > 0)) return false
        if (filterRec.value && !d.isRecommended) return false
        if (filterFav.value && !favorites.value.includes(d.id)) return false
        if (filterAvailable.value && d.stockStatus === 2) return false
        return true
    })

    switch (sortOrder.value) {
        case 'price-asc':
            return [...result].sort((a, b) => a.price - b.price)
        case 'price-desc':
            return [...result].sort((a, b) => b.price - a.price)
        case 'recommended':
            return [...result].sort((a, b) => (b.isRecommended ? 1 : 0) - (a.isRecommended ? 1 : 0))
        default:
            return [...result].sort(
                (a, b) => (a.stockStatus === 2 ? 1 : 0) - (b.stockStatus === 2 ? 1 : 0)
            )
    }
})

onMounted(fetchMenu)
</script>

<style scoped>
.menu-page {
    background-color: var(--eat-surface);
    min-height: 100vh;
    color: var(--eat-on-surface);
    font-family: var(--font-body);
}

.menu-header {
    padding: 8rem 0 4rem;
    text-align: center;
    background: linear-gradient(to bottom, rgba(24, 11, 6, 0.95), var(--eat-surface));
}

.menu-eyebrow {
    display: block;
    font-family: var(--font-label);
    color: var(--eat-secondary);
    letter-spacing: 0.4em;
    text-transform: uppercase;
    font-size: 0.75rem;
    margin-bottom: 1rem;
}

.eat-h1 {
    font-family: var(--font-headline);
    font-size: clamp(2.5rem, 5vw, 3.5rem);
    color: var(--eat-primary);
    margin-bottom: 3rem;
    font-style: italic;
}

/* Category Tabs */
.category-tabs {
    display: flex;
    justify-content: center;
    gap: 0.5rem;
    flex-wrap: wrap;
    padding: 0 1rem;
}

.tab-btn {
    background: none;
    border: none;
    color: rgba(249, 221, 211, 0.5);
    font-family: var(--font-label);
    font-size: 0.9rem;
    padding: 0.6rem 1.2rem;
    cursor: pointer;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    letter-spacing: 0.1em;
    border-radius: 20px;
}
.tab-btn:hover {
    color: var(--eat-secondary);
}
.tab-btn.active {
    color: var(--eat-primary);
    background-color: rgba(227, 199, 107, 0.1);
    box-shadow: inset 0 0 0 1px rgba(227, 199, 107, 0.3);
}

/* ── 搜尋 + 過濾列 ── */
.filter-bar {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    align-items: center;
    margin-bottom: 3rem;
}

.search-wrap {
    position: relative;
    flex: 1;
    min-width: 220px;
    max-width: 420px;
}
.search-icon {
    position: absolute;
    left: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    font-size: 0.85rem;
    opacity: 0.5;
}
.search-input {
    width: 100%;
    padding: 0.65rem 2.5rem 0.65rem 2.2rem;
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 30px;
    color: var(--eat-on-surface);
    font-family: var(--font-body);
    font-size: 0.9rem;
    outline: none;
    transition: border-color 0.3s;
}
.search-input:focus {
    border-color: rgba(227, 199, 107, 0.4);
}
.search-input::placeholder {
    color: rgba(249, 221, 211, 0.3);
}
.search-clear {
    position: absolute;
    right: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: rgba(249, 221, 211, 0.4);
    cursor: pointer;
    font-size: 0.75rem;
    padding: 0;
}

.filter-chips {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
}
.filter-chip {
    background: none;
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 20px;
    color: rgba(249, 221, 211, 0.5);
    font-family: var(--font-label);
    font-size: 0.8rem;
    padding: 0.45rem 1rem;
    cursor: pointer;
    transition: all 0.3s;
}
.filter-chip:hover {
    border-color: rgba(227, 199, 107, 0.35);
    color: rgba(249, 221, 211, 0.8);
}
.filter-chip.active {
    background: rgba(227, 199, 107, 0.12);
    border-color: rgba(227, 199, 107, 0.5);
    color: var(--eat-primary);
}

.sort-select {
    background: rgba(255, 255, 255, 0.04);
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 20px;
    color: var(--eat-on-surface);
    font-family: var(--font-label);
    font-size: 0.8rem;
    padding: 0.45rem 2rem 0.45rem 1rem;
    cursor: pointer;
    outline: none;
    transition: border-color 0.3s;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' viewBox='0 0 10 6'%3E%3Cpath d='M0 0l5 6 5-6z' fill='rgba(227,199,107,0.5)'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 0.75rem center;
}
.sort-select:hover {
    border-color: rgba(227, 199, 107, 0.35);
}
.sort-select:focus {
    border-color: rgba(227, 199, 107, 0.4);
}
.sort-select option {
    background: #1e100b;
    color: var(--eat-on-surface);
}
.filter-chip.active {
    background: rgba(227, 199, 107, 0.12);
    border-color: rgba(227, 199, 107, 0.5);
    color: var(--eat-primary);
}

/* ── 篩選統計列 ── */
.filter-stats {
    font-family: var(--font-label);
    font-size: 0.75rem;
    color: rgba(249, 221, 211, 0.4);
    letter-spacing: 0.15em;
    margin-bottom: 1.5rem;
    text-align: left;
}

/* ── 檢視模式切換按鈕 ── */
.view-toggle {
    display: flex;
    gap: 0.25rem;
}
.view-btn {
    background: none;
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 8px;
    color: rgba(249, 221, 211, 0.5);
    font-size: 1rem;
    width: 2rem;
    height: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s;
}
.view-btn:hover {
    border-color: rgba(227, 199, 107, 0.35);
    color: rgba(249, 221, 211, 0.8);
}
.view-btn.active {
    background: rgba(227, 199, 107, 0.12);
    border-color: rgba(227, 199, 107, 0.5);
    color: var(--eat-primary);
}

/* ── Grid ── */
.menu-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 2.5rem;
    perspective: 1000px;
}

/* ── List ── */
.menu-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    perspective: 1000px;
}
.menu-list .dish-card {
    flex-direction: row;
}
.menu-list .dish-img-wrap {
    width: 160px;
    min-width: 160px;
    height: 120px;
}
.menu-list .dish-info {
    padding: 1rem 1.25rem;
}
.menu-list .dish-desc {
    -webkit-line-clamp: unset;
    display: block;
    overflow: visible;
}

/* ── Card 進場動畫 ── */
.card-enter-active {
    transition:
        opacity 0.5s ease,
        transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.card-leave-active {
    transition:
        opacity 0.3s ease,
        transform 0.3s ease;
}
.card-enter-from {
    opacity: 0;
    transform: translateY(24px) scale(0.97);
}
.card-leave-to {
    opacity: 0;
    transform: translateY(-8px) scale(0.97);
}
.card-move {
    transition: transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.dish-card {
    background-color: var(--eat-surface-high);
    border-radius: 16px;
    overflow: hidden;
    /* 3D 傾斜基礎；overflow:hidden 在部分瀏覽器會拍平 preserve-3d，
     但卡片自身的 rotateX/Y 不受影響，::after glare 以純 2D overlay 實現 */
    transform-style: preserve-3d;
    will-change: transform;
    transition:
        transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1),
        box-shadow 0.3s ease,
        border-color 0.3s;
    display: flex;
    flex-direction: column;
    position: relative;
    border: 1px solid rgba(227, 199, 107, 0.05);
    cursor: pointer;
}
.dish-card:hover {
    border-color: rgba(227, 199, 107, 0.2);
}
.dish-card.is-soldout {
    opacity: 0.6;
}

/* ── 售完遮罩 ── */
.soldout-overlay {
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.55);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2;
}
.soldout-text {
    color: white;
    font-size: 1.1rem;
    font-family: var(--font-label);
    letter-spacing: 0.2em;
}

/* ── Glare 反光塗層 ── */
.dish-card::after {
    content: '';
    position: absolute;
    inset: 0;
    border-radius: inherit;
    /* 以 CSS 變數跟著游標移動，預設置中 */
    background: radial-gradient(
        circle at var(--glare-x, 50%) var(--glare-y, 50%),
        rgba(255, 255, 255, 0.12) 0%,
        rgba(255, 255, 255, 0.04) 40%,
        transparent 70%
    );
    opacity: 0;
    transition: opacity 0.3s ease;
    pointer-events: none;
    z-index: 3;
}
.dish-card:hover::after {
    opacity: 1;
}

/* ── 愛心收藏按鈕 ── */
.fav-btn {
    position: absolute;
    top: 0.75rem;
    right: 0.75rem;
    z-index: 3;
    background: transparent;
    border: none;
    cursor: pointer;
    font-size: 1.2rem;
    padding: 0;
    line-height: 1;
    filter: drop-shadow(0 1px 3px rgba(0, 0, 0, 0.6));
    transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.fav-btn:hover {
    transform: scale(1.25);
}

.dish-img-wrap {
    position: relative;
    height: 200px;
    overflow: hidden;
    background-color: #251813;
}
.dish-img-wrap img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 1s ease;
}
.dish-card:hover .dish-img-wrap img {
    transform: scale(1.1);
}

.img-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
}
.img-placeholder span {
    font-family: var(--font-headline);
    font-size: 5rem;
    color: rgba(227, 199, 107, 0.1);
    font-style: italic;
}

.badge-group {
    position: absolute;
    top: 1rem;
    left: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
    z-index: 2;
}
.badge {
    padding: 0.2rem 0.6rem;
    border-radius: 4px;
    font-size: 0.65rem;
    font-family: var(--font-label);
    font-weight: 600;
    letter-spacing: 0.1em;
    backdrop-filter: blur(4px);
}
.badge-rec {
    background-color: rgba(227, 199, 107, 0.9);
    color: var(--eat-surface);
}
.badge-pop {
    background-color: rgba(217, 83, 79, 0.9);
    color: white;
}
.badge-veg {
    background-color: rgba(80, 160, 80, 0.85);
    color: white;
}
.badge-low-stock {
    background-color: rgba(200, 100, 0, 0.9);
    color: white;
}
.badge-lim {
    background-color: rgba(200, 40, 40, 0.92);
    color: white;
}
.badge-soon {
    background-color: rgba(180, 120, 0, 0.9);
    color: white;
} /* 即將開始：琥珀橘 */
.badge-pending {
    background-color: rgba(60, 100, 180, 0.88);
    color: white;
} /* 尚未供應：藍灰 */
.badge-sold {
    background-color: rgba(80, 80, 80, 0.85);
    color: rgba(255, 255, 255, 0.55);
}

/* ── 限定供應倒數跳動特效 ── */
@keyframes pulse-urgency {
    0%,
    100% {
        transform: scale(1);
        box-shadow: 0 0 0 0 rgba(200, 40, 40, 0);
    }
    50% {
        transform: scale(1.08);
        box-shadow: 0 0 0 6px rgba(200, 40, 40, 0);
    }
    25% {
        box-shadow: 0 0 8px 3px rgba(200, 40, 40, 0.55);
    }
}

.fomo-timer {
    font-family: 'Courier New', Courier, monospace; /* 等寬字體：防止秒數跳動時文字位移 */
    letter-spacing: 0.05em;
    animation: pulse-urgency 2s ease-in-out infinite;
    /* 確保 transform-origin 在 badge 中心 */
    transform-origin: center center;
    display: inline-block;
}

.dish-info {
    padding: 1.5rem;
    flex-grow: 1;
    display: flex;
    flex-direction: column;
}
.dish-title-row {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 0.75rem;
}
.dish-name {
    font-family: var(--font-headline);
    color: var(--eat-primary);
    font-size: 1.25rem;
    margin: 0;
    font-style: italic;
}
.dish-price {
    font-family: var(--font-label);
    color: var(--eat-secondary);
    font-size: 0.9rem;
    font-weight: 500;
    margin-top: 0.25rem;
    white-space: nowrap;
    margin-left: 0.5rem;
}
.dish-desc {
    font-family: var(--font-body);
    font-size: 0.9rem;
    line-height: 1.7;
    color: rgba(249, 221, 211, 0.6);
    margin-bottom: 1.5rem;
    flex-grow: 1;
    font-style: italic;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
.dish-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-top: 1px solid rgba(227, 199, 107, 0.1);
    padding-top: 1rem;
}
.dish-tags {
    display: flex;
    gap: 0.75rem;
}
.tag {
    font-size: 0.75rem;
}
.category-name {
    font-family: var(--font-label);
    font-size: 0.7rem;
    color: rgba(227, 199, 107, 0.4);
    text-transform: uppercase;
    letter-spacing: 0.1em;
}

/* ── Modal ── */
.modal-enter-active {
    transition: opacity 0.35s ease;
}
.modal-leave-active {
    transition: opacity 0.25s ease;
}
.modal-enter-from,
.modal-leave-to {
    opacity: 0;
}

.modal-overlay {
    position: fixed;
    inset: 0;
    z-index: 1000;
    background: rgba(0, 0, 0, 0.75);
    backdrop-filter: blur(16px);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1.5rem;
}

.modal-box {
    width: 100%;
    max-width: 520px;
    background: #1e100b;
    border: 1px solid rgba(227, 199, 107, 0.15);
    border-radius: 24px;
    overflow: hidden;
    animation: modalPop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1);
}
@keyframes modalPop {
    from {
        opacity: 0;
        transform: scale(0.92) translateY(20px);
    }
    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}

.modal-img-wrap {
    position: relative;
    height: 240px;
    background: #251813;
    overflow: hidden;
}
.modal-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}
.modal-img-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
    font-family: var(--font-headline);
    font-size: 6rem;
    color: rgba(227, 199, 107, 0.1);
    font-style: italic;
}
.modal-img-gradient {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 50%;
    background: linear-gradient(to top, #1e100b, transparent);
}
.modal-close {
    position: absolute;
    top: 1rem;
    right: 1rem;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: rgba(0, 0, 0, 0.5);
    border: none;
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.9rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.2s;
}
.modal-close:hover {
    background: rgba(0, 0, 0, 0.8);
}
.modal-badge-group {
    position: absolute;
    bottom: 1rem;
    left: 1.25rem;
    display: flex;
    gap: 0.4rem;
    z-index: 2;
}

.modal-body {
    padding: 1.75rem;
    max-height: 55vh;
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: rgba(227, 199, 107, 0.2) transparent;
}
.modal-header-row {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 1rem;
    gap: 1rem;
}
.modal-title {
    font-family: var(--font-headline);
    color: var(--eat-primary);
    font-size: 1.8rem;
    font-style: italic;
    line-height: 1.2;
    margin: 0;
}
.modal-price {
    font-family: var(--font-label);
    color: var(--eat-secondary);
    font-size: 1.1rem;
    font-weight: 600;
    white-space: nowrap;
    padding-top: 0.35rem;
}
/* ── Modal 語意 Utility（對應 --eat-* 設計代號）── */
/* 色彩 Token */
.text-on-surface-variant {
    color: var(--eat-on-surface-variant);
}
.text-on-surface {
    color: var(--eat-on-surface);
}
.text-primary {
    color: var(--eat-primary);
}
/* 字體大小 */
.text-sm {
    font-size: 0.875rem;
    line-height: 1.5;
}
.text-xs {
    font-size: 0.75rem;
    line-height: 1.4;
}
.text-\[10px\] {
    font-size: 10px;
    line-height: 1.4;
}
/* 字體 Token */
.font-headline {
    font-family: var(--font-headline);
}
.font-body {
    font-family: var(--font-body);
}
/* 字重 / 樣式 */
.font-bold {
    font-weight: 700;
}
.italic {
    font-style: italic;
}
/* 間距 */
.tracking-widest {
    letter-spacing: 0.15em;
}
.mb-8 {
    margin-bottom: 2rem;
}
.mb-4 {
    margin-bottom: 1rem;
}
.mb-1 {
    margin-bottom: 0.25rem;
}
/* 顯示 */
.block {
    display: block;
}
/* Grid */
.grid {
    display: grid;
}
.grid-cols-2 {
    grid-template-columns: repeat(2, 1fr);
}
.gap-3 {
    gap: 0.75rem;
}
/* 背景 / 形狀 */
.bg-surface-container-low {
    background-color: var(--eat-surface-container);
}
.p-4 {
    padding: 1rem;
}
.rounded-xl {
    border-radius: 0.75rem;
}
.transition-all {
    transition:
        background-color 0.3s ease,
        transform 0.3s ease;
}
.bg-surface-container-low:hover {
    background-color: var(--eat-surface-high);
}

.modal-section {
    margin-bottom: 1.25rem;
}
.modal-section-label {
    font-family: var(--font-label);
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.15em;
    opacity: 0.4;
    margin-bottom: 0.65rem;
}
.attr-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
}
.attr-chip {
    font-size: 0.8rem;
    padding: 0.3rem 0.9rem;
    border-radius: 20px;
    border: 1px solid rgba(227, 199, 107, 0.2);
    color: rgba(249, 221, 211, 0.65);
    font-family: var(--font-label);
}
/* ── 評分星星 ── */
.star-row {
    display: flex;
    gap: 0.25rem;
    margin-bottom: 0.5rem;
}
.star-btn {
    font-size: 1.5rem;
    cursor: pointer;
    background: none;
    border: none;
    padding: 0;
    line-height: 1;
    color: var(--eat-primary);
    transition: transform 0.15s;
}
.star-btn:hover {
    transform: scale(1.2);
}
.star-btn:not(:hover) {
    /* 讓未 hover 的保持原色 */
}
/* ☆ 未達評分 → 降低不透明度 */
.star-btn:has(+ .star-btn) {
    /* 非最後一顆，靠 v-bind 控制 */
}

.star-voted {
    font-family: var(--font-label);
    font-size: 0.7rem;
    color: rgba(249, 221, 211, 0.45);
    letter-spacing: 0.05em;
    margin: 0;
}

.attr-chip.veg {
    border-color: rgba(80, 160, 80, 0.4);
    color: rgba(144, 238, 144, 0.8);
}
.attr-chip.spicy {
    border-color: rgba(217, 83, 79, 0.4);
    color: rgba(255, 140, 100, 0.9);
}
.attr-chip.rec {
    border-color: rgba(227, 199, 107, 0.4);
    color: var(--eat-primary);
}
.attr-chip.pop {
    border-color: rgba(217, 83, 79, 0.3);
    color: rgba(255, 140, 100, 0.8);
}

/* States */
.state-container {
    padding: 8rem 0;
    text-align: center;
}
.spinner {
    width: 48px;
    height: 48px;
    border: 2px solid rgba(227, 199, 107, 0.1);
    border-top-color: var(--eat-primary);
    border-radius: 50%;
    animation: spin 1s cubic-bezier(0.4, 0, 0.2, 1) infinite;
    margin: 0 auto 2rem;
}
.loading-text {
    font-family: var(--font-label);
    letter-spacing: 0.2em;
    color: var(--eat-secondary);
    font-size: 0.9rem;
}
.retry-btn {
    margin-top: 2rem;
    background: none;
    border: 1px solid var(--eat-primary);
    color: var(--eat-primary);
    padding: 0.6rem 2.5rem;
    font-family: var(--font-label);
    font-size: 0.8rem;
    cursor: pointer;
    transition: all 0.3s;
    text-transform: uppercase;
    letter-spacing: 0.2em;
}
.retry-btn:hover {
    background-color: var(--eat-primary);
    color: var(--eat-surface);
}
@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

/* Utils */
.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 2rem;
}
.py-5 {
    padding-top: 4rem;
    padding-bottom: 6rem;
}

/* md:grid-cols-3 — 768px 以上升為三欄 */
@media (min-width: 768px) {
    .md\:grid-cols-3 {
        grid-template-columns: repeat(3, 1fr);
    }
}

@media (max-width: 768px) {
    .menu-header {
        padding-top: 6rem;
    }
    .eat-h1 {
        font-size: 2.2rem;
    }
    .menu-grid {
        grid-template-columns: 1fr;
    }
    .filter-bar {
        flex-direction: column;
        align-items: stretch;
    }
    .search-wrap {
        max-width: 100%;
    }
    .modal-title {
        font-size: 1.5rem;
    }
    .modal-body {
        max-height: 60vh;
    }
}
</style>
