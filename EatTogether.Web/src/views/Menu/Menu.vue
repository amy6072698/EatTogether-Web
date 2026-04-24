<template>
  <div class="menu-page">
    <header class="menu-header">
      <div class="menu-header-bg"></div>
      <div class="menu-header-overlay"></div>
      <div class="container" style="position:relative;z-index:2;">
        <span class="menu-eyebrow">Signature Flavors</span>
        <h1 class="eat-h1">精選菜單</h1>

        <nav class="category-tabs" ref="tabsNav">
          <button
            v-for="cat in categories"
            :key="cat.id"
            :ref="el => { if (el) tabBtnRefs[cat.id] = el }"
            class="tab-btn"
            :class="{ active: currentCategory === cat.id }"
            @click="currentCategory = cat.id"
          >
            {{ cat.name }}
          </button>
          <span class="tab-indicator" :style="indicatorStyle"></span>
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
          <button v-if="searchQuery" class="search-clear" @click="searchQuery = ''">✕</button>
        </div>
        <div class="filter-chips-wrap">
          <span class="filter-label">篩選</span>
          <div class="filter-chips">
            <button
              class="filter-chip"
              :class="{ active: filterPop }"
              @click="filterPop = !filterPop"
            >👑 人氣餐點</button>
            <button
              class="filter-chip"
              :class="{ active: filterRec }"
              @click="filterRec = !filterRec"
            >⭐ 主廚推薦</button>
            <button
              class="filter-chip"
              :class="{ active: filterSpicy }"
              @click="filterSpicy = !filterSpicy"
            >🌶️ 有辣</button>
            <button
              class="filter-chip"
              :class="{ active: filterVeg }"
              @click="filterVeg = !filterVeg"
            >🥬 素食</button>
            <button
              class="filter-chip"
              :class="{ active: filterFav }"
              @click="filterFav = !filterFav"
            >❤️ 我的最愛</button>
          </div>
        </div>
        <div class="view-toggle">
          <button class="view-btn" :class="{ active: viewMode === 'grid' }" @click="viewMode = 'grid'" title="卡片模式">⊞</button>
          <button class="view-btn" :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'" title="列表模式">≡</button>
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
      <TransitionGroup v-else name="card" tag="div" :class="viewMode === 'grid' ? 'menu-grid' : 'menu-list'">
        <div
          v-for="(dish, index) in filteredDishes"
          :key="dish.id"
          v-reveal="index"
          class="dish-card"
          :class="{ 'is-soldout': dish.stockStatus === 2 }"
          @click="openModal(dish)"
          @mouseleave="activePreview = null"
        >
          <div
            class="dish-img-wrap"
            @mousemove="dish.stockStatus !== 2 && onSpotlight($event)"
            @mouseleave="dish.stockStatus !== 2 && offSpotlight($event)"
          >
            <img
              v-if="dish.imageUrl"
              :src="formatImageUrl(dish.imageUrl)"
              :alt="dish.dishName"
              loading="lazy"
            />
            <div v-else class="img-placeholder">
              <span>{{ dish.dishName.charAt(0) }}</span>
            </div>
            <div v-if="dish.stockStatus === 1" class="stock-bar" :style="{ width: stockWidth(dish.id) + '%' }"></div>
            <div v-if="dish.stockStatus === 2" class="soldout-overlay">
              <span class="soldout-text">已售完</span>
            </div>
            <button
              class="fav-btn"
              @click.stop="toggleFavorite(dish.id)"
              :aria-label="favorites.includes(dish.id) ? '取消收藏' : '加入收藏'"
            >{{ favorites.includes(dish.id) ? '❤️' : '🤍' }}</button>
            <div class="badge-group">
              <span v-if="dish.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="dish.isPopular" class="badge badge-pop">熱銷</span>
              <span v-if="dish.isVegetarian" class="badge badge-veg">素食</span>
              <span v-if="dish.stockStatus === 1" class="badge badge-low-stock">即將售完</span>
              <template v-if="dish.isLimited">
                <span v-if="getCountdown(dish.endDate, dish.startDate) === '尚未供應'"
                  class="badge badge-pending">尚未供應</span>
                <span v-else-if="getCountdown(dish.endDate, dish.startDate) === '即將開始'"
                  class="badge badge-soon">即將開始</span>
                <span v-else-if="getCountdown(dish.endDate, dish.startDate) === '已結束'"
                  class="badge badge-sold">停止供應</span>
                <span v-else
                  class="badge badge-lim fomo-timer"
                >{{ getCountdown(dish.endDate, dish.startDate) }}</span>
              </template>
            </div>

            <!-- 快速預覽按鈕（在圖片內，不超出 overflow:hidden 範圍） -->
            <button
              v-if="dish.stockStatus !== 2"
              class="preview-btn"
              @click.stop="activePreview = activePreview === dish.id ? null : dish.id"
              aria-label="快速預覽"
            >👁</button>
          </div>

          <!-- 快速預覽 Tooltip（移到 dish-img-wrap 外，相對 dish-card 定位） -->
          <Transition name="preview-tip">
            <div
              v-if="activePreview === dish.id"
              class="preview-tip"
              @click.stop
            >
              <p class="tip-desc">{{ dish.description || '精選新鮮食材，傳承義式經典風味，每一口都是主廚的心意。' }}</p>
              <div v-if="parseIngredients(dish.ingredientsJson).length" class="tip-ingredients">
                <span
                  v-for="ing in parseIngredients(dish.ingredientsJson).slice(0, 3)"
                  :key="ing.name"
                  class="tip-ing-chip"
                >{{ ing.name }}</span>
              </div>
              <button class="tip-more" @click.stop="openModal(dish)">查看完整資訊 →</button>
            </div>
          </Transition>

          <div class="dish-info">
            <div class="dish-title-row">
              <h3 class="dish-name">{{ dish.dishName }}</h3>
              <span class="dish-price">NT$ {{ dish.price.toLocaleString() }}</span>
            </div>
            <p class="dish-desc">{{ dish.description || '精選新鮮食材，傳承義式經典風味，每一口都是主廚的心意。' }}</p>
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
        :key="`empty-${searchQuery}-${filterVeg}-${filterSpicy}-${filterRec}-${filterFav}-${filterPop}-${currentCategory}`"
      >
        <svg class="empty-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 80" fill="none">
          <!-- 盤子陰影 -->
          <ellipse cx="40" cy="54" rx="27" ry="5" stroke="currentColor" stroke-width="1.2" stroke-dasharray="4 2.5" opacity="0.25"/>
          <!-- 盤子外圈 -->
          <ellipse cx="40" cy="40" rx="22" ry="22" stroke="currentColor" stroke-width="1.8"/>
          <!-- 盤子內圈 -->
          <ellipse cx="40" cy="40" rx="15" ry="15" stroke="currentColor" stroke-width="1" opacity="0.35"/>
          <!-- 叉子（左） -->
          <line x1="19" y1="14" x2="19" y2="66" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
          <line x1="16" y1="14" x2="16" y2="22" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/>
          <line x1="22" y1="14" x2="22" y2="22" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/>
          <path d="M16 22 Q19 27 22 22" stroke="currentColor" stroke-width="1.3" fill="none" stroke-linecap="round"/>
          <!-- 湯匙（右） -->
          <ellipse cx="61" cy="19" rx="4.5" ry="6.5" stroke="currentColor" stroke-width="1.8"/>
          <line x1="61" y1="25.5" x2="61" y2="66" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
        </svg>
        <p class="empty-title">找不到符合條件的餐點</p>
        <p class="empty-sub">換個關鍵字，或清除篩選條件試試看</p>
        <button class="clear-filter-btn" @click="clearFilters">清除篩選</button>
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
              ref="modalImgRef"
            />
            <div v-else class="modal-img-placeholder">
              <span>{{ selectedDish.dishName.charAt(0) }}</span>
            </div>
            <div class="modal-img-gradient"></div>
            <div class="share-wrap" ref="shareWrapRef">
              <button class="modal-share" @click.stop="shareMenuOpen = !shareMenuOpen" aria-label="分享">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"/>
                  <polyline points="16 6 12 2 8 6"/>
                  <line x1="12" y1="2" x2="12" y2="15"/>
                </svg>
              </button>
              <Transition name="share-menu">
                <div v-if="shareMenuOpen" class="share-menu">
                  <button class="share-item" @click="openShareItem('line')">
                    <span class="share-icon si-line">L</span>LINE
                  </button>
                  <button class="share-item" @click="openShareItem('facebook')">
                    <span class="share-icon si-fb">f</span>Facebook
                  </button>
                  <button class="share-item" @click="openShareItem('x')">
                    <span class="share-icon si-x">𝕏</span>X
                  </button>
                  <button class="share-item" @click="openShareItem('copy')">
                    <span class="share-icon si-copy">
                      <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg>
                    </span>複製連結
                  </button>
                </div>
              </Transition>
            </div>
            <button class="modal-close" @click="closeModal">✕</button>
            <div class="modal-badge-group">
              <span v-if="selectedDish.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="selectedDish.isPopular" class="badge badge-pop">熱銷</span>
              <span v-if="selectedDish.isVegetarian" class="badge badge-veg">素食</span>
            </div>
          </div>

          <!-- 資訊區 -->
          <div class="modal-body" ref="modalBodyRef">
            <div class="modal-header-row">
              <h2 class="modal-title">{{ selectedDish.dishName }}</h2>
              <div class="modal-price">NT$ {{ selectedDish.price.toLocaleString() }}</div>
            </div>

            <!-- 描述 -->
            <p class="text-on-surface-variant font-body text-sm italic mb-8">
              {{ selectedDish.description || '主廚精選地道食材，為您呈現純粹的義式饗宴。' }}
            </p>

            <!-- 精選食材 -->
            <div v-if="parseIngredients(selectedDish.ingredientsJson).length" class="modal-section">
              <h3 class="text-primary font-headline font-bold text-sm mb-4 tracking-widest">精選食材</h3>
              <div class="grid grid-cols-2 md:grid-cols-3 gap-3 mb-3">
                <div
                  v-for="item in parseIngredients(selectedDish.ingredientsJson)"
                  :key="item.name"
                  class="bg-surface-container-low p-4 rounded-xl transition-all ing-chip-clickable"
                  :class="{ 'ing-chip-active': activeIngredient === item.name }"
                  @click="activeIngredient = activeIngredient === item.name ? null : item.name"
                >
                  <span class="text-on-surface font-headline font-bold text-xs block mb-1">
                    {{ item.name }}<span class="ing-info-icon">🔍</span>
                  </span>
                  <span class="text-on-surface-variant font-body text-[10px] italic">{{ item.subDesc }}</span>
                </div>
              </div>
              <Transition name="ing-card-slide">
                <IngredientCard
                  v-if="activeIngredient"
                  :key="activeIngredient"
                  :ingredientName="activeIngredient"
                  @close="activeIngredient = null"
                  class="mb-8"
                />
              </Transition>
            </div>

            <!-- 屬性列 -->
            <div class="modal-section">
              <div class="modal-section-label">餐點屬性</div>
              <div class="attr-chips">
                <span class="attr-chip" v-if="selectedDish.categoryName">{{ selectedDish.categoryName }}</span>
                <span class="attr-chip veg" v-if="selectedDish.isVegetarian">🥬 素食</span>
                <span class="attr-chip spicy" v-if="selectedDish.spicyLevel > 0">
                  {{ '🌶️'.repeat(selectedDish.spicyLevel) }} {{ spicyLabel(selectedDish.spicyLevel) }}
                </span>
                <span class="attr-chip rec" v-if="selectedDish.isRecommended">⭐ 主廚推薦</span>
                <span class="attr-chip pop" v-if="selectedDish.isPopular">🔥 熱銷</span>
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
                >{{ (hoverStar || userRatings[selectedDish.id] || 0) >= star ? '★' : '☆' }}</button>
              </div>
              <p v-if="userRatings[selectedDish.id]" class="star-voted">
                您已評 {{ userRatings[selectedDish.id] }} 顆星
              </p>
            </div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- 返回套餐 -->
    <Transition name="back-top">
      <button
        v-if="returnTo"
        class="return-setmeal-btn"
        @click="router.push(returnTo)"
        aria-label="返回套餐"
      >← 返回套餐</button>
    </Transition>

    <!-- 回到頂端 -->
    <Transition name="back-top">
      <button v-if="showBackTop" class="back-top-btn" @click="scrollToTop" aria-label="回到頂端">
        ↑
      </button>
    </Transition>

    <ToastContainer />
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ToastContainer from '@/components/common/ToastContainer.vue';
import IngredientCard from '@/components/common/menu/IngredientCard.vue';
import { useToast } from '@/composables/useToast.js';
const { show } = useToast();

const route = useRoute();
const router = useRouter();
const returnTo = computed(() => route.query.returnTo || null);

// ── Intersection Observer 進場 ───────────────────────
const vReveal = {
  mounted(el, binding) {
    const delay = binding.value * 50
    el.style.opacity = '0'
    el.style.transform = 'translateY(40px)'
    el.style.transition = 'opacity 0.4s cubic-bezier(0.22, 1, 0.36, 1), transform 0.4s cubic-bezier(0.22, 1, 0.36, 1)'
    el.style.transitionDelay = `${delay}ms`
    const observer = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting) {
        el.style.opacity = '1'
        el.style.transform = 'translateY(0)'
        setTimeout(() => {
          el.style.opacity = ''
          el.style.transform = ''
          el.style.transition = ''
          el.style.transitionDelay = ''
        }, 400 + delay)
        observer.disconnect()
      }
    }, { threshold: 0.1 })
    observer.observe(el)
    el._revealObserver = observer
  },
  unmounted(el) {
    el._revealObserver?.disconnect()
  }
}

// ── Spotlight 光暈 ───────────────────────────────────
const onSpotlight = (e) => {
  const el = e.currentTarget
  const rect = el.getBoundingClientRect()
  el.style.setProperty('--sx', `${e.clientX - rect.left}px`)
  el.style.setProperty('--sy', `${e.clientY - rect.top}px`)
  el.style.setProperty('--spotlight-opacity', '1')
}
const offSpotlight = (e) => {
  e.currentTarget.style.setProperty('--spotlight-opacity', '0')
}

// ── 庫存進度條寬度（以 id 為 seed，穩定不亂跳）─────────
const stockWidth = (id) => {
  const n = typeof id === 'number' ? id : String(id).split('').reduce((a, c) => a + c.charCodeAt(0), 0)
  return 20 + (n % 16) // 20–35%
}

// ── 安全解析食材 JSON ────────────────────────────────
const parseIngredients = (jsonString) => {
  if (!jsonString) return [];
  try {
    const result = JSON.parse(jsonString);
    return Array.isArray(result) ? result : [];
  } catch {
    return [];
  }
};

// ── 3D Tilt ──────────────────────────────────────────
const tiltStyles = reactive({});

const handleMouseMove = (event, dishId) => {
  const card = event.currentTarget;
  const rect = card.getBoundingClientRect();
  const relX = (event.clientX - rect.left) / rect.width - 0.5;
  const relY = (event.clientY - rect.top) / rect.height - 0.5;
  // --glare-x/y 讓 ::after 偽元素能跟著游標移動
  card.style.setProperty('--glare-x', `${((relX + 0.5) * 100).toFixed(1)}%`);
  card.style.setProperty('--glare-y', `${((relY + 0.5) * 100).toFixed(1)}%`);
  tiltStyles[dishId] = {
    transform: `rotateX(${(-relY * 15).toFixed(2)}deg) rotateY(${(relX * 15).toFixed(2)}deg) translateY(-10px) scale(1.02)`,
    transition: 'transform 0.08s ease',
    boxShadow: `${(-relX * 20).toFixed(1)}px ${(relY * 20 + 16).toFixed(1)}px 40px rgba(0,0,0,0.55), 0 0 0 1px rgba(227,199,107,0.18)`,
  };
};

const handleMouseLeave = (dishId) => {
  tiltStyles[dishId] = {
    transform: 'rotateX(0deg) rotateY(0deg) translateY(0) scale(1)',
    transition: 'transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1), box-shadow 0.6s ease',
    boxShadow: '',
  };
};

// ── 倒數計時 ─────────────────────────────────────────
const currentTime = ref(new Date());
let _clockTimer = null;
onMounted(() => {
  _clockTimer = setInterval(() => { currentTime.value = new Date(); }, 1000);
});
onUnmounted(() => {
  clearInterval(_clockTimer);
});

// 回傳四種狀態：'尚未供應' | '即將開始' | 倒數字串 | '已結束'
const getCountdown = (endDateString, startDateString) => {
  if (!endDateString) return '';
  const now   = currentTime.value;    // 存取 .value 讓 Vue 追蹤響應性
  const end   = new Date(endDateString);
  const start = startDateString ? new Date(startDateString) : null;

  // 已超過結束時間
  if (end <= now) return '已結束';

  // 尚未開始
  if (start && start > now) {
    const msToStart = start - now;
    // 超過 24 小時才開始 → 尚未供應；24 小時內 → 即將開始
    return msToStart > 24 * 60 * 60 * 1000 ? '尚未供應' : '即將開始';
  }

  // 供應中 — 計算距離結束的倒數
  const totalSec = Math.floor((end - now) / 1000);
  const days     = Math.floor(totalSec / 86400);
  const hours    = Math.floor((totalSec % 86400) / 3600);
  const minutes  = Math.floor((totalSec % 3600) / 60);
  const seconds  = totalSec % 60;

  if (days >= 1) return `剩餘 ${days} 天 ${hours} 小時`;
  const hh = String(hours).padStart(2, '0');
  const mm = String(minutes).padStart(2, '0');
  const ss = String(seconds).padStart(2, '0');
  return `倒數 ${hh}:${mm}:${ss}`;
};

// ── Sliding Tab Indicator ─────────────────────────────
const tabsNav = ref(null);
const tabBtnRefs = reactive({});
const indicatorStyle = ref({ left: '0px', width: '0px', opacity: 0 });

const updateIndicator = async () => {
  await nextTick();
  const btn = tabBtnRefs[currentCategory.value];
  const nav = tabsNav.value;
  if (!btn || !nav) return;
  const btnRect = btn.getBoundingClientRect();
  const navRect = nav.getBoundingClientRect();
  indicatorStyle.value = {
    left: `${btnRect.left - navRect.left}px`,
    width: `${btnRect.width}px`,
    opacity: 1,
  };
};

// ── State ────────────────────────────────────────────
const dishes = ref([]);
const loading = ref(true);
const error = ref(null);
const currentCategory = ref(0);

watch(currentCategory, updateIndicator);
const searchQuery = ref('');
const filterVeg = ref(false);
const filterSpicy = ref(false);
const filterRec = ref(false);
const filterFav = ref(false);
const filterPop = ref(false);
const sortOrder = ref('default');
const viewMode = ref('grid');

// ── 評分 ──────────────────────────────────────────────
const userRatings = reactive(JSON.parse(localStorage.getItem('menu-ratings') || '{}'));
const hoverStar = ref(0);

const submitRating = (dishId, star) => {
  userRatings[dishId] = star;
  localStorage.setItem('menu-ratings', JSON.stringify(userRatings));
  show('⭐ 感謝您的評分！', 'success');
  const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';
  fetch(`${API_BASE}/Dishes/${dishId}/Rate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ score: star }),
  }).catch(() => {}); // fire-and-forget，忽略錯誤
};

// ── 收藏 ──────────────────────────────────────────────
const favorites = ref(JSON.parse(localStorage.getItem('menu-favorites') || '[]'));

const toggleFavorite = (dishId) => {
  const idx = favorites.value.indexOf(dishId);
  if (idx === -1) {
    favorites.value.push(dishId);
    show('❤️ 已加入收藏', 'success');
  } else {
    favorites.value.splice(idx, 1);
    show('🤍 已取消收藏', 'info');
  }
  localStorage.setItem('menu-favorites', JSON.stringify(favorites.value));
};

// Modal
const isModalOpen = ref(false);
const selectedDish = ref(null);
const activePreview = ref(null);
const activeIngredient = ref(null);
const modalBodyRef = ref(null);
const modalImgRef = ref(null);

watch(isModalOpen, async (open) => {
  if (!open) return;
  await nextTick();

  // B: Ken Burns — start zoomed-in, slowly ease back to natural
  const img = modalImgRef.value;
  if (img) {
    img.style.transition = 'none';
    img.style.transform = 'scale(1.18)';
    img.offsetHeight; // force reflow
    img.style.transition = 'transform 7s cubic-bezier(0.25, 0.46, 0.45, 0.94)';
    img.style.transform = 'scale(1.0)';
  }

  const body = modalBodyRef.value;
  if (!body) return;

  const sections = [...body.children];

  // D: pre-hide chips — they animate separately after their parent section reveals
  const chips = body.querySelectorAll('.attr-chip');
  chips.forEach(chip => {
    chip.style.transition = 'none';
    chip.style.opacity = '0';
    chip.style.transform = 'scale(0) translateY(6px)';
  });

  // A: reset all direct children — more travel distance + subtle scale for depth
  sections.forEach(el => {
    el.style.transition = 'none';
    el.style.opacity = '0';
    el.style.transform = 'translateY(30px) scale(0.96)';
  });

  body.offsetHeight; // force reflow before animating

  // A: stagger each section in with a springy easing
  sections.forEach((el, i) => {
    setTimeout(() => {
      el.style.transition = 'opacity 0.55s cubic-bezier(0.22, 1, 0.36, 1), transform 0.55s cubic-bezier(0.22, 1, 0.36, 1)';
      el.style.opacity = '1';
      el.style.transform = 'translateY(0) scale(1)';
    }, i * 90);
  });

  // D: chips pop in as A wraps up its last section (tighter timing = snappier feel)
  const chipsDelay = (sections.length - 1) * 90 + 180;
  chips.forEach((chip, i) => {
    setTimeout(() => {
      chip.style.transition = 'transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.25s ease';
      chip.style.opacity = '1';
      chip.style.transform = 'scale(1) translateY(0)';
    }, chipsDelay + i * 55);
  });
});

const categories = [
  { id: 0, name: '全部' },
  { id: 1, name: '主餐' },
  { id: 2, name: '飲料' },
  { id: 3, name: '甜點' },
  { id: 4, name: '湯品' },
  { id: 5, name: '附餐' }
];

// ── 清除所有篩選 ──────────────────────────────────────
const clearFilters = () => {
  searchQuery.value = ''
  filterVeg.value = false
  filterSpicy.value = false
  filterRec.value = false
  filterFav.value = false
  filterPop.value = false
  currentCategory.value = 0
  sortOrder.value = 'default'
}

// ── Modal ────────────────────────────────────────────
const shareMenuOpen = ref(false);
const shareWrapRef = ref(null);

const openShareItem = async (type) => {
  const dish = selectedDish.value;
  if (!dish) return;
  const dishUrl = `${window.location.origin}/menu?dish=${dish.id}`;
  const encodedUrl  = encodeURIComponent(dishUrl);
  const encodedText = encodeURIComponent(`${dish.dishName} NT$${dish.price.toLocaleString()}`);
  switch (type) {
    case 'line':     window.open(`https://social-plugins.line.me/lineit/share?url=${encodedUrl}`, '_blank'); break;
    case 'facebook': window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`, '_blank'); break;
    case 'x':        window.open(`https://twitter.com/intent/tweet?text=${encodedText}&url=${encodedUrl}`, '_blank'); break;
    case 'copy':
      try {
        await navigator.clipboard.writeText(dishUrl);
        show('🔗 連結已複製！', 'success');
      } catch {
        show('複製失敗，請手動複製', 'error');
      }
      break;
  }
  shareMenuOpen.value = false;
};

const handleShareClickOutside = (e) => {
  if (shareMenuOpen.value && shareWrapRef.value && !shareWrapRef.value.contains(e.target)) {
    shareMenuOpen.value = false;
  }
};

const openModal = (dish) => {
  selectedDish.value = dish;
  isModalOpen.value = true;
  document.body.style.overflow = 'hidden';
};

const closeModal = () => {
  isModalOpen.value = false;
  selectedDish.value = null;
  shareMenuOpen.value = false;
  activeIngredient.value = null;
  document.body.style.overflow = '';
};

const handleEsc = (e) => { if (e.key === 'Escape') closeModal(); };
onMounted(() => {
  window.addEventListener('keydown', handleEsc);
  document.addEventListener('click', handleShareClickOutside);
});
onUnmounted(() => {
  window.removeEventListener('keydown', handleEsc);
  document.removeEventListener('click', handleShareClickOutside);
});

// ── Utils ────────────────────────────────────────────
const spicyLabel = (level) => {
  return ['', '微辣', '中辣', '大辣', '極辣'][level] ?? '辣';
};

const formatImageUrl = (url) => {
  if (!url) return null;
  return url.startsWith('/images/') ? url : null;
};

// ── API ──────────────────────────────────────────────
let _refreshTimer = null;

const fetchMenu = async () => {
  // 首次載入時才顯示 loading，背景輪詢時保留現有資料避免閃白
  if (dishes.value.length === 0) {
    loading.value = true;
  }
  const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';
  try {
    const res = await fetch(`${API_BASE}/Dishes/active`);
    if (!res.ok) throw new Error(`抓取失敗 (${res.status})`);
    dishes.value = await res.json();
    error.value = null;
  } catch (err) {
    console.error('Menu Fetch Error:', err);
    // 背景輪詢失敗時不覆蓋現有資料，只在首次失敗時顯示錯誤
    if (dishes.value.length === 0) {
      error.value = '無法載入菜單資料，請確認 API 伺服器狀態。';
    }
  } finally {
    loading.value = false;
  }
};

// ── 回到頂端 ──────────────────────────────────────────
const showBackTop = ref(false);
const scrollToTop = () => window.scrollTo({ top: 0, behavior: 'smooth' });

const handleScroll = () => { showBackTop.value = window.scrollY > 400; };
onMounted(() => window.addEventListener('scroll', handleScroll, { passive: true }));
onUnmounted(() => window.removeEventListener('scroll', handleScroll));

// ── Computed ─────────────────────────────────────────
const hasActiveFilter = computed(() =>
  searchQuery.value !== '' ||
  filterVeg.value ||
  filterSpicy.value ||
  filterRec.value ||
  filterFav.value ||
  filterPop.value ||
  currentCategory.value !== 0 ||
  sortOrder.value !== 'default'
);
const filteredDishes = computed(() => {
  const result = dishes.value.filter(d => {
    if (currentCategory.value !== 0 && d.categoryId !== currentCategory.value) return false;
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase();
      const hit = d.dishName.toLowerCase().includes(q) ||
                  (d.description && d.description.toLowerCase().includes(q));
      if (!hit) return false;
    }
    if (filterVeg.value && !d.isVegetarian) return false;
    if (filterSpicy.value && !(d.spicyLevel > 0)) return false;
    if (filterRec.value && !d.isRecommended) return false;
    if (filterFav.value && !favorites.value.includes(d.id)) return false;
    if (filterPop.value && !d.isPopular) return false;
    return true;
  });

  switch (sortOrder.value) {
    case 'price-asc':  return [...result].sort((a, b) => a.price - b.price);
    case 'price-desc': return [...result].sort((a, b) => b.price - a.price);
    case 'recommended': return [...result].sort((a, b) => (b.isRecommended ? 1 : 0) - (a.isRecommended ? 1 : 0));
    default:           return [...result].sort((a, b) => (a.stockStatus === 2 ? 1 : 0) - (b.stockStatus === 2 ? 1 : 0));
  }
});


onMounted(async () => {
  await fetchMenu();
  _refreshTimer = setInterval(fetchMenu, 5_000);
  updateIndicator();

  // 深層連結：偵測 ?dish=id，自動打開對應 Modal
  const params = new URLSearchParams(window.location.search);
  const dishParam = params.get('dish');
  if (dishParam) {
    const dish = dishes.value.find(d => String(d.id) === dishParam);
    if (dish) {
      if (dish.categoryId) currentCategory.value = dish.categoryId;
      openModal(dish);
    }
  }
});
onUnmounted(() => {
  clearInterval(_refreshTimer);
});
</script>

<style scoped>
.menu-page {
  background-color: var(--eat-surface);
  min-height: 100vh;
  color: var(--eat-on-surface);
  font-family: var(--font-body);
}

.menu-header {
  padding: 8rem 0 2rem;
  text-align: center;
  position: relative;
  overflow: hidden;
}
.menu-header-bg {
  position: absolute;
  inset: 0;
  background-image: url('https://images.unsplash.com/photo-1473093226795-af9932fe5856?w=1400&q=80');
  background-size: cover;
  background-position: center 60%;
}
.menu-header-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    to bottom,
    rgba(24, 11, 6, 0.78) 0%,
    rgba(24, 11, 6, 0.68) 55%,
    var(--eat-surface) 100%
  );
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
  position: relative;
  padding-bottom: 6px;
}

.tab-indicator {
  position: absolute;
  bottom: 0;
  height: 2px;
  background: var(--eat-primary);
  border-radius: 1px;
  transition: left 0.35s cubic-bezier(0.4, 0, 0.2, 1),
              width 0.35s cubic-bezier(0.4, 0, 0.2, 1),
              opacity 0.2s;
  pointer-events: none;
  box-shadow: 0 0 8px rgba(227, 199, 107, 0.5);
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
.tab-btn:hover { color: var(--eat-secondary); }
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
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 30px;
  color: var(--eat-on-surface);
  font-family: var(--font-body);
  font-size: 0.9rem;
  outline: none;
  transition: border-color 0.3s;
}
.search-input:focus { border-color: rgba(227, 199, 107, 0.4); }
.search-input::placeholder { color: rgba(249, 221, 211, 0.3); }
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

.filter-chips-wrap {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.filter-label {
  font-family: var(--font-label);
  font-size: 0.75rem;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  color: rgba(249, 221, 211, 0.45);
  white-space: nowrap;
  align-self: center;
}
.filter-chips { display: flex; gap: 0.5rem; flex-wrap: wrap; }
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
.filter-chip:hover { border-color: rgba(227, 199, 107, 0.35); color: rgba(249, 221, 211, 0.8); }
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
.sort-select:hover  { border-color: rgba(227, 199, 107, 0.35); }
.sort-select:focus  { border-color: rgba(227, 199, 107, 0.4); }
.sort-select option { background: #1e100b; color: var(--eat-on-surface); }
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
.view-toggle { display: flex; gap: 0.25rem; }
.view-btn {
  background: none;
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 8px;
  color: rgba(249, 221, 211, 0.5);
  font-size: 1rem;
  width: 2rem; height: 2rem;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer;
  transition: all 0.3s;
}
.view-btn:hover { border-color: rgba(227, 199, 107, 0.35); color: rgba(249, 221, 211, 0.8); }
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
  transition: opacity 0.5s ease, transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.card-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.card-enter-from { opacity: 0; transform: translateY(24px) scale(0.97); }
.card-leave-to   { opacity: 0; transform: translateY(-8px) scale(0.97); }
.card-move       { transition: transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1); }

.dish-card {
  background-color: var(--eat-surface-high);
  border-radius: 16px;
  overflow: hidden;
  will-change: transform;
  transition: transform 0.3s ease,
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
  opacity: 0.75;
}
.dish-card.is-soldout::before {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(30, 28, 26, 0.52);
  border-radius: inherit;
  z-index: 4;
  pointer-events: none;
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

/* ── 庫存剩餘進度條 ── */
.stock-bar {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  background: linear-gradient(90deg, #f97316, #fb923c 80%, rgba(251,146,60,0));
  border-radius: 0 2px 0 0;
  z-index: 3;
  animation: stock-pulse 1.8s ease-in-out infinite;
}
@keyframes stock-pulse {
  0%, 100% { opacity: 1; }
  50%       { opacity: 0.6; }
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
  filter: drop-shadow(0 1px 3px rgba(0,0,0,0.6));
  transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.fav-btn:hover { transform: scale(1.25); }

/* ── 快速預覽按鈕 ── */
.preview-btn {
  position: absolute;
  bottom: 0.65rem;
  right: 0.65rem;
  z-index: 4;
  width: 30px; height: 30px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.55);
  border: 1px solid rgba(227, 199, 107, 0.3);
  color: rgba(255, 255, 255, 0.85);
  font-size: 0.85rem;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  opacity: 0;
  transform: scale(0.8);
  transition: opacity 0.22s ease, transform 0.22s ease, background 0.2s;
}
.dish-card:hover .preview-btn { opacity: 1; transform: scale(1); }
.preview-btn:hover { background: rgba(0, 0, 0, 0.85); border-color: var(--eat-primary); }

/* ── 快速預覽 Tooltip ── */
.preview-tip {
  position: absolute;
  /* 蓋在圖片上方：圖片高度 200px，tooltip 從頂部往下偏移讓它出現在圖片區域內 */
  top: 0.75rem;
  left: 0.75rem;
  right: 0.75rem;
  background: rgba(12, 5, 2, 0.95);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(227, 199, 107, 0.25);
  border-radius: 14px;
  padding: 1rem;
  z-index: 10;
  box-shadow: 0 16px 48px rgba(0, 0, 0, 0.6);
}
.tip-desc {
  font-family: var(--font-body);
  font-size: 0.82rem;
  color: rgba(249, 221, 211, 0.75);
  font-style: italic;
  line-height: 1.6;
  margin: 0 0 0.65rem;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.tip-ingredients {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  margin-bottom: 0.75rem;
}
.tip-ing-chip {
  font-family: var(--font-label);
  font-size: 0.68rem;
  padding: 0.15rem 0.55rem;
  background: rgba(227, 199, 107, 0.08);
  border: 1px solid rgba(227, 199, 107, 0.2);
  border-radius: 20px;
  color: var(--eat-secondary);
}
.tip-more {
  display: block;
  width: 100%;
  padding: 0.4rem 0;
  background: none;
  border: none;
  border-top: 1px solid rgba(227, 199, 107, 0.12);
  color: var(--eat-primary);
  font-family: var(--font-label);
  font-size: 0.75rem;
  letter-spacing: 0.08em;
  cursor: pointer;
  text-align: right;
  transition: opacity 0.15s;
  padding-top: 0.6rem;
  margin-top: 0.1rem;
}
.tip-more:hover { opacity: 0.75; }

/* Tooltip 進退場動畫 */
.preview-tip-enter-active { transition: opacity 0.18s ease, transform 0.18s cubic-bezier(0.22, 1, 0.36, 1); }
.preview-tip-leave-active { transition: opacity 0.12s ease, transform 0.12s ease; }
.preview-tip-enter-from   { opacity: 0; transform: translateY(6px) scale(0.97); }
.preview-tip-leave-to     { opacity: 0; transform: translateY(3px) scale(0.98); }

@media (max-width: 768px) {
  .preview-btn { display: none; }
}

.dish-img-wrap {
  position: relative;
  height: 200px;
  overflow: hidden;
  background-color: #251813;
  --sx: 50%;
  --sy: 50%;
  --spotlight-opacity: 0;
}
.dish-img-wrap img {
  width: 100%; height: 100%;
  object-fit: cover;
  transition: transform 1s ease;
}
.dish-card:hover .dish-img-wrap img { transform: scale(1.1); }

/* Spotlight 光暈層 */
.dish-img-wrap::after {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
  background: radial-gradient(
    circle 60px at var(--sx) var(--sy),
    rgba(255, 255, 255, 0.22) 0%,
    rgba(255, 255, 255, 0.06) 45%,
    transparent 100%
  );
  opacity: var(--spotlight-opacity);
  transition: opacity 0.3s ease;
  z-index: 4;
  mix-blend-mode: screen;
}
/* 售完卡片不顯示光暈 */
.is-soldout .dish-img-wrap::after { display: none; }

.img-placeholder {
  width: 100%; height: 100%;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
}
.img-placeholder span {
  font-family: var(--font-headline);
  font-size: 5rem;
  color: rgba(227, 199, 107, 0.1);
  font-style: italic;
}

.badge-group {
  position: absolute; top: 1rem; left: 1rem;
  display: flex; flex-direction: column; gap: 0.4rem; z-index: 2;
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
.badge-rec       { background-color: #e8a800; color: #1a0800; text-shadow: none; box-shadow: 0 1px 6px rgba(232,168,0,0.45); }
.badge-pop       { background-color: rgba(217, 83, 79, 0.9);  color: white; }
.badge-veg       { background-color: rgba(80, 160, 80, 0.85); color: white; }
.badge-low-stock { background-color: rgba(200, 100, 0, 0.9);  color: white; }
.badge-lim     { background-color: rgba(200, 40, 40, 0.92);  color: white; }
.badge-soon    { background-color: rgba(180, 120, 0, 0.9);   color: white; }           /* 即將開始：琥珀橘 */
.badge-pending { background-color: rgba(60, 100, 180, 0.88); color: white; }           /* 尚未供應：藍灰 */
.badge-sold    { background-color: rgba(80, 80, 80, 0.85);   color: rgba(255,255,255,0.55); }

/* ── 限定供應倒數跳動特效 ── */
@keyframes pulse-urgency {
  0%, 100% {
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

.dish-info { padding: 1.5rem; flex-grow: 1; display: flex; flex-direction: column; }
.dish-title-row {
  display: flex; justify-content: space-between;
  align-items: flex-start; margin-bottom: 0.75rem;
}
.dish-name {
  font-family: var(--font-headline);
  color: var(--eat-primary);
  font-size: 1.25rem;
  margin: 0; font-style: italic;
}
.dish-price {
  font-family: var(--font-label);
  font-size: 0.9rem;
  font-weight: 500; margin-top: 0.25rem;
  white-space: nowrap; margin-left: 0.5rem;
  /* 光澤掃過基礎設定 */
  background: linear-gradient(
    90deg,
    #c9a84c 0%,
    #f5e27a 35%,
    #ffffff 50%,
    #f5e27a 65%,
    #c9a84c 100%
  );
  background-size: 200% auto;
  background-position: 100% center;   /* 預設停在右側（看不到光澤）*/
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  color: transparent;
}

/* hover 時觸發一次掃光，結束後停在右側（回到金色） */
.dish-card:hover .dish-price {
  animation: price-shimmer 0.65s ease forwards;
  animation-iteration-count: 1;
}

@keyframes price-shimmer {
  0%   { background-position: 100% center; }
  100% { background-position: -100% center; }
}
.dish-desc {
  font-family: var(--font-body);
  font-size: 0.9rem; line-height: 1.7;
  color: rgba(249, 221, 211, 0.6);
  margin-bottom: 1.5rem; flex-grow: 1;
  font-style: italic;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.dish-footer {
  display: flex; justify-content: space-between; align-items: center;
  border-top: 1px solid rgba(227, 199, 107, 0.1); padding-top: 1rem;
}
.dish-tags { display: flex; gap: 0.75rem; }
.tag { font-size: 0.75rem; }
.category-name {
  font-family: var(--font-label);
  font-size: 0.7rem;
  color: rgba(227, 199, 107, 0.4);
  text-transform: uppercase; letter-spacing: 0.1em;
}

/* ── Modal ── */
.modal-enter-active { transition: opacity 0.35s ease; }
.modal-leave-active { transition: opacity 0.25s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }

.modal-overlay {
  position: fixed; inset: 0; z-index: 1000;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(16px);
  display: flex; align-items: center; justify-content: center;
  padding: 1.5rem;
}

.modal-box {
  width: 100%; max-width: 520px;
  background: #1e100b;
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 24px;
  overflow: hidden;
  animation: modalPop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1);
}
@keyframes modalPop {
  from { opacity: 0; transform: scale(0.92) translateY(20px); }
  to   { opacity: 1; transform: scale(1) translateY(0); }
}

.modal-img-wrap {
  position: relative;
  height: 240px;
  background: #251813;
  overflow: hidden;
}
.modal-img { width: 100%; height: 100%; object-fit: cover; }
.modal-img-placeholder {
  width: 100%; height: 100%;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
  font-family: var(--font-headline);
  font-size: 6rem; color: rgba(227, 199, 107, 0.1); font-style: italic;
}
.modal-img-gradient {
  position: absolute; bottom: 0; left: 0; right: 0; height: 50%;
  background: linear-gradient(to top, #1e100b, transparent);
}
.modal-close {
  position: absolute; top: 1rem; right: 1rem;
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(0,0,0,0.5); border: none;
  color: rgba(255,255,255,0.8); font-size: 0.9rem;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.2s;
}
.modal-close:hover { background: rgba(0,0,0,0.8); }
.share-wrap { position: absolute; top: 1rem; right: 3.5rem; z-index: 3; }
.modal-share {
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(0,0,0,0.5); border: none; color: rgba(255,255,255,0.8);
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.2s, color 0.2s;
}
.modal-share:hover { background: rgba(0,0,0,0.8); color: var(--eat-primary); }
.share-menu {
  position: absolute; top: calc(100% + 0.45rem); right: 0;
  background: rgba(18, 8, 4, 0.96); backdrop-filter: blur(16px);
  border: 1px solid rgba(227, 199, 107, 0.22); border-radius: 12px;
  padding: 0.35rem; display: flex; flex-direction: column; gap: 0.15rem;
  min-width: 148px; box-shadow: 0 12px 40px rgba(0,0,0,0.55);
}
.share-item {
  display: flex; align-items: center; gap: 0.6rem;
  padding: 0.48rem 0.7rem; border: none; background: none; border-radius: 8px;
  color: rgba(249, 221, 211, 0.82); font-family: var(--font-label);
  font-size: 0.78rem; letter-spacing: 0.04em; cursor: pointer;
  transition: background 0.15s; white-space: nowrap; width: 100%; text-align: left;
}
.share-item:hover { background: rgba(255,255,255,0.06); }
.share-icon {
  width: 22px; height: 22px; border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.72rem; font-weight: 700; flex-shrink: 0; line-height: 1;
}
.si-line { background: #06C755; color: white; border-radius: 6px; font-size: 0.62rem; }
.si-fb   { background: #1877F2; color: white; border-radius: 50%; font-size: 0.88rem; }
.si-x    { background: #0f0f0f; color: white; border-radius: 50%; border: 1px solid rgba(255,255,255,0.2); font-size: 0.75rem; }
.si-copy { background: rgba(227,199,107,0.12); color: var(--eat-primary); border: 1px solid rgba(227,199,107,0.3); border-radius: 6px; }
.share-menu-enter-active { transition: opacity 0.18s ease, transform 0.18s cubic-bezier(0.22, 1, 0.36, 1); }
.share-menu-leave-active { transition: opacity 0.12s ease, transform 0.12s ease; }
.share-menu-enter-from   { opacity: 0; transform: scale(0.88) translateY(-8px); transform-origin: top right; }
.share-menu-leave-to     { opacity: 0; transform: scale(0.92) translateY(-4px); transform-origin: top right; }
.modal-badge-group {
  position: absolute; bottom: 1rem; left: 1.25rem;
  display: flex; gap: 0.4rem; z-index: 2;
}

.modal-body {
  padding: 1.75rem;
  max-height: 55vh;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(227, 199, 107, 0.2) transparent;
}
.modal-header-row {
  display: flex; justify-content: space-between;
  align-items: flex-start; margin-bottom: 1rem; gap: 1rem;
}
.modal-title {
  font-family: var(--font-headline);
  color: var(--eat-primary);
  font-size: 1.8rem; font-style: italic;
  line-height: 1.2; margin: 0;
}
.modal-price {
  font-family: var(--font-label);
  color: var(--eat-secondary);
  font-size: 1.1rem; font-weight: 600;
  white-space: nowrap; padding-top: 0.35rem;
}
/* ── Modal 語意 Utility（對應 --eat-* 設計代號）── */
/* 色彩 Token */
.text-on-surface-variant   { color: var(--eat-on-surface-variant); }
.text-on-surface           { color: var(--eat-on-surface); }
.text-primary              { color: var(--eat-primary); }
/* 字體大小 */
.text-sm                   { font-size: 0.875rem; line-height: 1.5; }
.text-xs                   { font-size: 0.75rem; line-height: 1.4; }
.text-\[10px\]             { font-size: 10px; line-height: 1.4; }
/* 字體 Token */
.font-headline             { font-family: var(--font-headline); }
.font-body                 { font-family: var(--font-body); }
/* 字重 / 樣式 */
.font-bold                 { font-weight: 700; }
.italic                    { font-style: italic; }
/* 間距 */
.tracking-widest           { letter-spacing: 0.15em; }
.mb-8                      { margin-bottom: 2rem; }
.mb-4                      { margin-bottom: 1rem; }
.mb-1                      { margin-bottom: 0.25rem; }
/* 顯示 */
.block                     { display: block; }
/* Grid */
.grid                      { display: grid; }
.grid-cols-2               { grid-template-columns: repeat(2, 1fr); }
.gap-3                     { gap: 0.75rem; }
/* 背景 / 形狀 */
.bg-surface-container-low  { background-color: var(--eat-surface-container); }
.p-4                       { padding: 1rem; }
.rounded-xl                { border-radius: 0.75rem; }
.transition-all            { transition: background-color 0.3s ease, transform 0.3s ease; }
.bg-surface-container-low:hover { background-color: var(--eat-surface-high); }

.ing-chip-clickable {
  cursor: pointer;
}
.ing-chip-clickable:hover {
  border: 1px solid rgba(227, 199, 107, 0.5);
  background-color: var(--eat-surface-high);
}
.ing-chip-active {
  border: 1px solid rgba(227, 199, 107, 0.6) !important;
  background-color: rgba(227, 199, 107, 0.07) !important;
}
.ing-info-icon {
  font-size: 0.6rem;
  margin-left: 0.3rem;
  opacity: 0;
  transition: opacity 0.2s;
  vertical-align: middle;
}
.ing-chip-clickable:hover .ing-info-icon,
.ing-chip-active .ing-info-icon {
  opacity: 0.75;
}

/* IngredientCard slide transition */
.ing-card-slide-enter-active {
  transition: opacity 0.22s ease, transform 0.22s cubic-bezier(0.22, 1, 0.36, 1);
}
.ing-card-slide-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.ing-card-slide-enter-from {
  opacity: 0;
  transform: translateY(-8px);
}
.ing-card-slide-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

.modal-section { margin-bottom: 1.25rem; }
.modal-section-label {
  font-family: var(--font-label);
  font-size: 0.7rem; text-transform: uppercase;
  letter-spacing: 0.15em; opacity: 0.4;
  margin-bottom: 0.65rem;
}
.attr-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.attr-chip {
  font-size: 0.8rem;
  padding: 0.3rem 0.9rem;
  border-radius: 20px;
  border: 1px solid rgba(227, 199, 107, 0.2);
  color: rgba(249, 221, 211, 0.65);
  font-family: var(--font-label);
}
/* ── 評分星星 ── */
.star-row { display: flex; gap: 0.25rem; margin-bottom: 0.5rem; }
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
.star-btn:hover  { transform: scale(1.2); }
.star-btn:not(:hover) { /* 讓未 hover 的保持原色 */ }
/* ☆ 未達評分 → 降低不透明度 */
.star-btn:has(+ .star-btn) { /* 非最後一顆，靠 v-bind 控制 */ }

.star-voted {
  font-family: var(--font-label);
  font-size: 0.7rem;
  color: rgba(249, 221, 211, 0.45);
  letter-spacing: 0.05em;
  margin: 0;
}

.attr-chip.veg   { border-color: rgba(80,160,80,0.4); color: rgba(144,238,144,0.8); }
.attr-chip.spicy { border-color: rgba(217,83,79,0.4); color: rgba(255,140,100,0.9); }
.attr-chip.rec   { border-color: rgba(227,199,107,0.4); color: var(--eat-primary); }
.attr-chip.pop   { border-color: rgba(217,83,79,0.3); color: rgba(255,140,100,0.8); }

/* ── Empty State ── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.6rem;
}

.empty-icon {
  width: 88px;
  height: 88px;
  color: var(--eat-primary);
  opacity: 0.65;
  margin-bottom: 0.75rem;
  animation: shake 0.75s cubic-bezier(0.36, 0.07, 0.19, 0.97) both;
  animation-delay: 0.08s;
}

@keyframes shake {
  0%, 100% { transform: translateX(0) rotate(0deg); }
  15%       { transform: translateX(-9px) rotate(-5deg); }
  30%       { transform: translateX(8px)  rotate(4deg); }
  45%       { transform: translateX(-6px) rotate(-2.5deg); }
  60%       { transform: translateX(5px)  rotate(1.5deg); }
  75%       { transform: translateX(-3px) rotate(-1deg); }
  90%       { transform: translateX(2px)  rotate(0.5deg); }
}

.empty-title {
  font-family: var(--font-headline);
  font-size: 1.25rem;
  color: var(--eat-primary);
  font-style: italic;
  margin: 0;
}

.empty-sub {
  font-family: var(--font-body);
  font-size: 0.9rem;
  color: rgba(249, 221, 211, 0.4);
  font-style: italic;
  margin: 0 0 0.75rem;
}

.clear-filter-btn {
  background: none;
  border: 1px solid rgba(227, 199, 107, 0.4);
  color: var(--eat-primary);
  padding: 0.55rem 2.25rem;
  font-family: var(--font-label);
  font-size: 0.78rem;
  cursor: pointer;
  border-radius: 30px;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  transition: background 0.3s, border-color 0.3s, transform 0.2s;
}
.clear-filter-btn:hover {
  background: rgba(227, 199, 107, 0.1);
  border-color: var(--eat-primary);
  transform: translateY(-2px);
}

/* States */
.state-container { padding: 8rem 0; text-align: center; }
.spinner {
  width: 48px; height: 48px;
  border: 2px solid rgba(227, 199, 107, 0.1);
  border-top-color: var(--eat-primary);
  border-radius: 50%;
  animation: spin 1s cubic-bezier(0.4, 0, 0.2, 1) infinite;
  margin: 0 auto 2rem;
}
.loading-text {
  font-family: var(--font-label);
  letter-spacing: 0.2em;
  color: var(--eat-secondary); font-size: 0.9rem;
}
.retry-btn {
  margin-top: 2rem; background: none;
  border: 1px solid var(--eat-primary);
  color: var(--eat-primary);
  padding: 0.6rem 2.5rem;
  font-family: var(--font-label); font-size: 0.8rem;
  cursor: pointer; transition: all 0.3s;
  text-transform: uppercase; letter-spacing: 0.2em;
}
.retry-btn:hover { background-color: var(--eat-primary); color: var(--eat-surface); }
@keyframes spin { to { transform: rotate(360deg); } }

/* ── 回到頂端按鈕 ── */
.return-setmeal-btn {
  position: fixed;
  bottom: 2.5rem;
  left: 2rem;
  padding: 0.55rem 1.2rem;
  border-radius: 24px;
  background: rgba(24, 11, 6, 0.88);
  border: 1px solid rgba(227, 199, 107, 0.4);
  color: var(--eat-primary);
  font-family: var(--font-label);
  font-size: 0.8rem;
  letter-spacing: 0.08em;
  cursor: pointer;
  backdrop-filter: blur(10px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
  transition: border-color 0.2s, transform 0.2s, background 0.2s;
  z-index: 1100;
  white-space: nowrap;
}
.return-setmeal-btn:hover {
  border-color: var(--eat-primary);
  background: rgba(24, 11, 6, 0.97);
  transform: translateY(-2px);
}

.back-top-btn {
  position: fixed;
  bottom: 2.5rem;
  right: 2rem;
  width: 2.8rem;
  height: 2.8rem;
  border-radius: 50%;
  background: rgba(24, 11, 6, 0.85);
  border: 1px solid rgba(227, 199, 107, 0.35);
  color: var(--eat-primary);
  font-size: 1.1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(8px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
  transition: border-color 0.2s, transform 0.2s, box-shadow 0.2s;
  z-index: 200;
}
.back-top-btn:hover {
  border-color: var(--eat-primary);
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.5);
}
.back-top-enter-active, .back-top-leave-active { transition: opacity 0.3s, transform 0.3s; }
.back-top-enter-from, .back-top-leave-to { opacity: 0; transform: translateY(12px); }

/* Utils */
.container { max-width: 1200px; margin: 0 auto; padding: 0 2rem; }
.py-5 { padding-top: 2rem; padding-bottom: 6rem; }

/* md:grid-cols-3 — 768px 以上升為三欄 */
@media (min-width: 768px) {
  .md\:grid-cols-3 { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 768px) {
  .menu-header { padding-top: 6rem; }
  .eat-h1 { font-size: 2.2rem; }
  .menu-grid { grid-template-columns: 1fr; }
  .filter-bar { flex-direction: column; align-items: stretch; }
  .search-wrap { max-width: 100%; }
  .modal-title { font-size: 1.5rem; }
  .modal-body { max-height: 60vh; }
}
</style>