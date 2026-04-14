<template>
  <div class="menu-page">
    <!-- Header Section -->
    <header class="menu-header">
      <div class="container">
        <span class="menu-eyebrow">Signature Flavors</span>
        <h1 class="eat-h1">精選菜單</h1>
        
        <!-- Category Tabs -->
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
      <!-- Loading State -->
      <div v-if="loading" class="state-container">
        <div class="spinner"></div>
        <p class="loading-text">正在為您準備美味佳餚...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="state-container error-state">
        <div class="error-icon">!</div>
        <p>{{ error }}</p>
        <button @click="fetchMenu" class="retry-btn">重新嘗試</button>
      </div>

      <!-- Dish Grid -->
      <div v-else class="menu-grid">
        <div 
          v-for="dish in filteredDishes" 
          :key="dish.id" 
          class="dish-card"
        >
          <div class="dish-img-wrap">
            <!-- Image Logic -->
            <img 
              v-if="dish.imageUrl" 
              :src="formatImageUrl(dish.imageUrl)" 
              :alt="dish.dishName"
              loading="lazy"
            />
            <div v-else class="img-placeholder">
              <span>{{ dish.dishName.charAt(0) }}</span>
            </div>
            
            <!-- Badges -->
            <div class="badge-group">
              <span v-if="dish.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="dish.isPopular" class="badge badge-pop">熱銷</span>
            </div>
          </div>

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
                  🌶️ {{ '🌶️'.repeat(dish.spicyLevel - 1) }}
                </span>
              </div>
              <div class="category-name">{{ dish.categoryName }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && !error && filteredDishes.length === 0" class="state-container empty-state">
        <p>目前此分類下暫無餐點供選擇。</p>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';

// ── State ────────────────────────────────────────────
const dishes = ref([]);
const loading = ref(true);
const error = ref(null);
const currentCategory = ref(0); // Default: All

const categories = [
  { id: 0, name: '全部' },
  { id: 1, name: '主餐' },
  { id: 2, name: '飲料' },
  { id: 3, name: '甜點' },
  { id: 4, name: '湯品' },
  { id: 5, name: '附餐' }
];

// ── API Fetching ─────────────────────────────────────
const fetchMenu = async () => {
  loading.value = true;
  error.value = null;
  
  const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';
  const endpoint = `${API_BASE}/Dishes/GetActiveJson`;

  try {
    const response = await fetch(endpoint);
    if (!response.ok) throw new Error(`抓取失敗 (${response.status})`);
    
    const data = await response.json();
    dishes.value = data;
  } catch (err) {
    console.error('Menu Fetch Error:', err);
    error.value = '無法載入菜單資料，請確認 API 伺服器狀態。';
  } finally {
    loading.value = false;
  }
};

// ── Utils ────────────────────────────────────────────
const formatImageUrl = (url) => {
  if (!url) return null;
  // Clean path formatting (same logic as Home.vue)
  let path = url.replace(/\\/g, '/');
  path = path.replace(/^~\//, '');
  const wwwrootMatch = /\/wwwroot\/(.*)/i.exec('/' + path);
  if (wwwrootMatch && wwwrootMatch[1]) path = wwwrootMatch[1];
  path = path.replace(/^\//, '');
  
  return `/api/${path}`;
};

// ── Computed ─────────────────────────────────────────
const filteredDishes = computed(() => {
  if (currentCategory.value === 0) return dishes.value;
  return dishes.value.filter(d => d.categoryId === currentCategory.value);
});

onMounted(fetchMenu);
</script>

<style scoped>
/* Design Tokens / Colors */
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

/* Grid Layout */
.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2.5rem;
}

.dish-card {
  background-color: var(--eat-surface-high);
  border-radius: 16px;
  overflow: hidden;
  transition: transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1);
  display: flex;
  flex-direction: column;
  border: 1px solid rgba(227, 199, 107, 0.05);
}

.dish-card:hover {
  transform: translateY(-10px);
  border-color: rgba(227, 199, 107, 0.2);
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

/* Badges */
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

/* Dish Info */
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
}

.dish-desc {
  font-family: var(--font-body);
  font-size: 0.9rem;
  line-height: 1.7;
  color: rgba(249, 221, 211, 0.6);
  margin-bottom: 1.5rem;
  flex-grow: 1;
  font-style: italic;
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

/* Loading/Error States */
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
  to { transform: rotate(360deg); }
}

/* Utils */
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
}

.py-5 { padding-top: 4rem; padding-bottom: 6rem; }

@media (max-width: 768px) {
  .menu-header { padding-top: 6rem; }
  .eat-h1 { font-size: 2.2rem; }
  .menu-grid { grid-template-columns: 1fr; }
}
</style>
