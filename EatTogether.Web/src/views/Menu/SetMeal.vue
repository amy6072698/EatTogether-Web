<template>
  <div class="setmeal-page">
    <header class="setmeal-header">
      <div class="setmeal-header-bg"></div>
      <div class="setmeal-header-overlay"></div>
      <div class="container" style="position:relative;z-index:2;">
        <span class="setmeal-eyebrow">Set Menus</span>
        <h1 class="eat-h1">精選套餐</h1>
      </div>
    </header>

    <main class="container py-5">

      <!-- Loading -->
      <div v-if="loading" class="state-container">
        <div class="spinner"></div>
        <p class="loading-text">正在為您準備套餐資訊...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="state-container error-state">
        <div class="error-icon">!</div>
        <p>{{ error }}</p>
        <button @click="fetchSetMeals" class="retry-btn">重新嘗試</button>
      </div>

      <!-- 套餐卡片列表 -->
      <TransitionGroup v-else name="card" tag="div" class="setmeal-grid">
        <div
          v-for="(meal, index) in setMeals"
          :key="meal.id"
          v-reveal="index"
          class="meal-card"
          @click="openModal(meal)"
        >
          <div
            class="meal-img-wrap"
            @mousemove="onSpotlight($event)"
            @mouseleave="offSpotlight($event)"
          >
            <img
              v-if="meal.imageUrl"
              :src="formatImageUrl(meal.imageUrl)"
              :alt="meal.setMealName"
              loading="lazy"
            />
            <div v-else class="img-placeholder">
              <span>{{ meal.setMealName.charAt(0) }}</span>
            </div>
            <div v-if="mealIsLowStock(meal)" class="stock-bar" :style="{ width: stockWidth(meal.id) + '%' }"></div>
            <div class="badge-group">
              <span v-if="meal.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="meal.isPopular" class="badge badge-pop">熱銷</span>
            </div>
          </div>

          <div v-if="!isAvailable(meal)" class="time-overlay">
            <span class="time-overlay-text">{{ startLabel(meal) }} 開始供應</span>
          </div>

          <div class="meal-info">
            <div class="meal-title-row">
              <h3 class="meal-name">{{ meal.setMealName }}</h3>
              <span class="meal-price">NT$ {{ meal.setPrice.toLocaleString() }}</span>
            </div>

            <div class="meal-tags">
              <span v-if="getTimeRange(meal)" class="tag time-tag">
                🕐 {{ getTimeRange(meal) }}
              </span>
              <span v-if="getSavings(meal) > 0" class="tag save-tag">
                省下 NT$ {{ getSavings(meal).toLocaleString() }}
              </span>
            </div>

            <p class="meal-desc">{{ meal.description || '精選多道義式料理，超值組合任君享用。' }}</p>

            <div class="meal-items-preview">
              <span
                v-for="item in meal.items.filter(i => !i.isOptional).slice(0, 3)"
                :key="item.dishId"
                class="item-chip"
              >{{ item.dishName }}</span>
              <span v-if="meal.items.filter(i => !i.isOptional).length > 3" class="item-chip more">
                +{{ meal.items.filter(i => !i.isOptional).length - 3 }} 更多
              </span>
            </div>
          </div>
        </div>
      </TransitionGroup>

      <!-- Empty -->
      <div v-if="!loading && !error && setMeals.length === 0" class="state-container empty-state">
        <p>目前沒有供應中的套餐，請稍後再來。</p>
      </div>
    </main>

    <ToastContainer />

    <!-- ── Modal ── -->
    <Transition name="modal">
      <div v-if="isModalOpen && selectedMeal" class="modal-overlay" @click.self="closeModal">
        <div class="modal-box">

          <!-- 圖片區 -->
          <div class="modal-img-wrap">
            <img
              v-if="selectedMeal.imageUrl"
              :src="formatImageUrl(selectedMeal.imageUrl)"
              :alt="selectedMeal.setMealName"
              class="modal-img"
              ref="modalImgRef"
            />
            <div v-else class="modal-img-placeholder">
              <span>{{ selectedMeal.setMealName.charAt(0) }}</span>
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
              <span v-if="selectedMeal.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="selectedMeal.isPopular" class="badge badge-pop">熱銷</span>
            </div>
          </div>

          <!-- 資訊區 -->
          <div class="modal-body" ref="modalBodyRef">
            <div class="modal-header-row">
              <h2 class="modal-title">{{ selectedMeal.setMealName }}</h2>
            </div>

            <p v-if="selectedMeal.description" class="modal-desc">{{ selectedMeal.description }}</p>

            <div v-if="getTimeRange(selectedMeal)" class="modal-time">
              🕐 供應時段：{{ getTimeRange(selectedMeal) }}
            </div>

            <!-- 固定餐點 -->
            <div v-if="fixedItems(selectedMeal).length" class="modal-section">
              <div class="modal-section-label">套餐內容</div>
              <div class="fixed-items">
                <div v-for="item in fixedItems(selectedMeal)" :key="item.dishId" class="fixed-item" :class="{ 'is-soldout': dishStock(item.dishId) === 2 }">
                  <span class="item-name">{{ item.dishName }}</span>
                  <span class="item-qty">× {{ item.quantity }}</span>
                  <span v-if="dishStock(item.dishId) === 2" class="item-soldout-tag">已售完</span>
                  <span v-else class="item-price">NT$ {{ item.dishPrice.toLocaleString() }}</span>
                </div>
              </div>
            </div>

            <!-- 選擇性群組 -->
            <div v-for="(group, groupNo) in optionGroups(selectedMeal)" :key="groupNo" class="modal-section">
              <div class="modal-section-label">
                請選 {{ group.pickLimit }} 項（{{ group.categoryName }}）
                <span class="group-count" :class="{ full: groupTotal(selectedMeal.id, groupNo) >= group.pickLimit }">
                  已選 {{ groupTotal(selectedMeal.id, groupNo) }} / {{ group.pickLimit }}
                </span>
              </div>
              <div class="option-items">
                <div v-for="item in group.items" :key="item.dishId" class="option-item" :class="{ 'is-soldout': dishStock(item.dishId) === 2 }">
                  <span class="option-name">{{ item.dishName }}</span>
                  <span v-if="dishStock(item.dishId) === 2" class="option-soldout-tag">已售完</span>
                  <span v-else class="option-price">NT$ {{ item.dishPrice.toLocaleString() }}</span>
                  <div class="qty-control">
                    <button
                      class="qty-btn"
                      @click="changeQty(selectedMeal.id, groupNo, item.dishId, -1)"
                      :disabled="getQty(selectedMeal.id, groupNo, item.dishId) === 0 || dishStock(item.dishId) === 2"
                    >−</button>
                    <span class="qty-num">{{ getQty(selectedMeal.id, groupNo, item.dishId) }}</span>
                    <button
                      class="qty-btn"
                      @click="changeQty(selectedMeal.id, groupNo, item.dishId, 1)"
                      :disabled="groupTotal(selectedMeal.id, groupNo) >= group.pickLimit || dishStock(item.dishId) === 2"
                    >+</button>
                  </div>
                </div>
              </div>
            </div>

            <!-- 價格計算區 -->
            <div class="price-summary">
              <div class="price-row">
                <span>單點合計</span>
                <span>NT$ {{ animatedSingleTotal.toLocaleString() }}</span>
              </div>
              <div class="price-row set-price-row">
                <span>套餐價格</span>
                <span>NT$ {{ selectedMeal.setPrice.toLocaleString() }}</span>
              </div>
              <div class="price-row save-row" v-if="modalSingleTotal - selectedMeal.setPrice > 0">
                <span>省下</span>
                <span class="save-amount">NT$ {{ animatedSavings.toLocaleString() }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import ToastContainer from '@/components/common/ToastContainer.vue';
import { useToast } from '@/composables/useToast.js';
const { show } = useToast();

// ── Intersection Observer 進場 ──────────────────────
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

// ── 庫存進度條 ──────────────────────────────────────
const stockWidth = (id) => {
  const n = typeof id === 'number' ? id : String(id).split('').reduce((a, c) => a + c.charCodeAt(0), 0)
  return 20 + (n % 16)
}

// 以 dishId 查詢 stockStatus（SetMeal items 不帶此欄位，需從 dishes API 取得）
const dishes = ref([]);
const dishStockMap = computed(() =>
  Object.fromEntries(dishes.value.map(d => [d.id, d.stockStatus]))
);
const dishStock = (dishId) => dishStockMap.value[dishId] ?? 0;

const mealIsLowStock = (meal) =>
  meal.stockStatus === 1 ||
  (meal.items && meal.items.some(i => dishStock(i.dishId) === 1))

// ── 時段判斷 ────────────────────────────────────────
const currentTime = ref(new Date());

const parseTimeToday = (timeStr) => {
  if (!timeStr) return null;
  const [h, m, s = 0] = timeStr.split(':').map(Number);
  const d = new Date(currentTime.value);
  d.setHours(h, m, s, 0);
  return d;
};

const isAvailable = (meal) => {
  if (!meal.startTime && !meal.endTime) return true;
  const now = currentTime.value;
  const start = parseTimeToday(meal.startTime);
  const end   = parseTimeToday(meal.endTime);
  if (start && end) return now >= start && now <= end;
  if (start) return now >= start;
  if (end)   return now <= end;
  return true;
};

const startLabel = (meal) => meal.startTime ? meal.startTime.substring(0, 5) : '';

// ── State ──────────────────────────────────────────
const setMeals = ref([]);
const loading = ref(true);
const error = ref(null);
const isModalOpen = ref(false);
const selectedMeal = ref(null);
const modalBodyRef = ref(null);
const modalImgRef = ref(null);

watch(isModalOpen, async (open) => {
  if (!open) return;
  await nextTick();

  // B: Ken Burns
  const img = modalImgRef.value;
  if (img) {
    img.style.transition = 'none';
    img.style.transform = 'scale(1.18)';
    img.offsetHeight;
    img.style.transition = 'transform 7s cubic-bezier(0.25, 0.46, 0.45, 0.94)';
    img.style.transform = 'scale(1.0)';
  }

  const body = modalBodyRef.value;
  if (!body) return;

  const sections = [...body.children];

  // D: pre-hide chips (SetMeal 無 .attr-chip，此段安全空跑)
  const chips = body.querySelectorAll('.attr-chip');
  chips.forEach(chip => {
    chip.style.transition = 'none';
    chip.style.opacity = '0';
    chip.style.transform = 'scale(0) translateY(6px)';
  });

  // A: reset sections
  sections.forEach(el => {
    el.style.transition = 'none';
    el.style.opacity = '0';
    el.style.transform = 'translateY(30px) scale(0.96)';
  });

  body.offsetHeight;

  // A: stagger in
  sections.forEach((el, i) => {
    setTimeout(() => {
      el.style.transition = 'opacity 0.55s cubic-bezier(0.22, 1, 0.36, 1), transform 0.55s cubic-bezier(0.22, 1, 0.36, 1)';
      el.style.opacity = '1';
      el.style.transform = 'translateY(0) scale(1)';
    }, i * 90);
  });

  // D: chips bounce in
  const chipsDelay = (sections.length - 1) * 90 + 180;
  chips.forEach((chip, i) => {
    setTimeout(() => {
      chip.style.transition = 'transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.25s ease';
      chip.style.opacity = '1';
      chip.style.transform = 'scale(1) translateY(0)';
    }, chipsDelay + i * 55);
  });
});
// quantities[mealId_groupNo_dishId] = number
const quantities = reactive({});

// ── Utils ──────────────────────────────────────────
const formatImageUrl = (url) => {
  if (!url) return null;
  return url.startsWith('/images/') ? url : null;
};

const getTimeRange = (meal) => {
  if (!meal.startTime && !meal.endTime) return '';
  const s = meal.startTime ? meal.startTime.substring(0, 5) : '';
  const e = meal.endTime   ? meal.endTime.substring(0, 5)   : '';
  if (s && e) return `${s}–${e}`;
  if (s) return `${s} 起`;
  return `至 ${e}`;
};

const getSavings = (meal) => {
  const original = meal.items.reduce((sum, i) => sum + i.dishPrice * i.quantity, 0);
  return Math.max(0, original - meal.setPrice);
};

const fixedItems = (meal) => meal.items.filter(i => !i.isOptional);

const optionGroups = (meal) => {
  const groups = {};
  meal.items.filter(i => i.isOptional).forEach(item => {
    const key = item.optionGroupNo;
    if (!groups[key]) {
      groups[key] = { pickLimit: item.pickLimit, categoryName: item.categoryName, items: [] };
    }
    groups[key].items.push(item);
  });
  return groups;
};

// 取得某道餐點的數量
const getQty = (mealId, groupNo, dishId) => {
  return quantities[`${mealId}_${groupNo}_${dishId}`] ?? 0;
};

// 該群組目前總數量
const groupTotal = (mealId, groupNo) => {
  const meal = setMeals.value.find(m => m.id === mealId);
  if (!meal) return 0;
  const group = optionGroups(meal)[groupNo];
  if (!group) return 0;
  return group.items.reduce((sum, item) => sum + getQty(mealId, groupNo, item.dishId), 0);
};

// +1 / -1
const changeQty = (mealId, groupNo, dishId, delta) => {
  const key = `${mealId}_${groupNo}_${dishId}`;
  const current = quantities[key] ?? 0;
  quantities[key] = Math.max(0, current + delta);

  if (delta > 0) {
    const meal = setMeals.value.find(m => m.id === mealId);
    if (meal) {
      const group = optionGroups(meal)[groupNo];
      if (group && groupTotal(mealId, groupNo) >= group.pickLimit) {
        show('✓ 已完成選擇', 'success');
      }
    }
  }
};

// 初始化：所有選項數量歸零
const initQuantities = (meal) => {
  const groups = optionGroups(meal);
  Object.entries(groups).forEach(([groupNo, group]) => {
    group.items.forEach(item => {
      const key = `${meal.id}_${groupNo}_${item.dishId}`;
      if (!(key in quantities)) quantities[key] = 0;
    });
  });
};

// ── Modal 價格計算 ──────────────────────────────────
const modalSingleTotal = computed(() => {
  if (!selectedMeal.value) return 0;
  const meal = selectedMeal.value;
  const fixedTotal = fixedItems(meal).reduce((sum, i) => sum + i.dishPrice * i.quantity, 0);
  const groups = optionGroups(meal);
  let optionalTotal = 0;
  Object.entries(groups).forEach(([groupNo, group]) => {
    group.items.forEach(item => {
      optionalTotal += item.dishPrice * getQty(meal.id, groupNo, item.dishId);
    });
  });
  return fixedTotal + optionalTotal;
});

// ── 數字滾動動畫 ───────────────────────────────────
const animatedSingleTotal = ref(0);
const animatedSavings = ref(0);
let _syncWatcher = null;

const easeOutQuart = (t) => 1 - Math.pow(1 - t, 4);

const animateValue = (target, to, duration = 800) => {
  const start = performance.now();
  const tick = (now) => {
    const p = Math.min((now - start) / duration, 1);
    target.value = Math.round(easeOutQuart(p) * to);
    if (p < 1) requestAnimationFrame(tick);
  };
  requestAnimationFrame(tick);
};

// ── Modal ──────────────────────────────────────────
const shareMenuOpen = ref(false);
const shareWrapRef = ref(null);

const openShareItem = async (type) => {
  const meal = selectedMeal.value;
  if (!meal) return;
  const encodedUrl = encodeURIComponent(window.location.href);
  const encodedText = encodeURIComponent(`${meal.setMealName} NT$${meal.setPrice.toLocaleString()}`);
  switch (type) {
    case 'line':     window.open(`https://social-plugins.line.me/lineit/share?url=${encodedUrl}`, '_blank'); break;
    case 'facebook': window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`, '_blank'); break;
    case 'x':        window.open(`https://twitter.com/intent/tweet?text=${encodedText}&url=${encodedUrl}`, '_blank'); break;
    case 'copy':
      try {
        await navigator.clipboard.writeText(`${meal.setMealName} NT$${meal.setPrice.toLocaleString()} | 義起吃`);
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

const openModal = (meal) => {
  selectedMeal.value = meal;
  initQuantities(meal);
  isModalOpen.value = true;
  document.body.style.overflow = 'hidden';

  animatedSingleTotal.value = 0;
  animatedSavings.value = 0;
  const targetSingle = modalSingleTotal.value;
  const targetSavings = Math.max(0, targetSingle - meal.setPrice);
  animateValue(animatedSingleTotal, targetSingle);
  if (targetSavings > 0) animateValue(animatedSavings, targetSavings);

  // 動畫結束後同步 live 數值（使用者調整選項時即時反映）
  if (_syncWatcher) _syncWatcher();
  _syncWatcher = watch(modalSingleTotal, (val) => {
    animatedSingleTotal.value = val;
    animatedSavings.value = Math.max(0, val - (selectedMeal.value?.setPrice ?? 0));
  });
};

const closeModal = () => {
  if (_syncWatcher) { _syncWatcher(); _syncWatcher = null; }
  isModalOpen.value = false;
  selectedMeal.value = null;
  shareMenuOpen.value = false;
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

// ── API ────────────────────────────────────────────
let _refreshTimer = null;

const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';

const fetchDishes = async () => {
  try {
    const res = await fetch(`${API_BASE}/Dishes/active`);
    if (res.ok) dishes.value = await res.json();
  } catch {}
};

const fetchSetMeals = async () => {
  // 首次載入時才顯示 loading，背景輪詢時保留現有資料避免閃白
  if (setMeals.value.length === 0) {
    loading.value = true;
  }
  try {
    const res = await fetch(`${API_BASE}/SetMeals/active`);
    if (!res.ok) throw new Error(`抓取失敗 (${res.status})`);
    setMeals.value = await res.json();
    error.value = null;
  } catch (err) {
    console.error('SetMeal Fetch Error:', err);
    // 背景輪詢失敗時不覆蓋現有資料，只在首次失敗時顯示錯誤
    if (setMeals.value.length === 0) {
      error.value = '無法載入套餐資料，請確認 API 伺服器狀態。';
    }
  } finally {
    loading.value = false;
  }
};

let _clockTimer = null;
onMounted(() => {
  fetchDishes();
  fetchSetMeals();
  _refreshTimer = setInterval(() => { fetchSetMeals(); fetchDishes(); }, 5_000);
  _clockTimer = setInterval(() => { currentTime.value = new Date(); }, 1000);
});
onUnmounted(() => {
  clearInterval(_refreshTimer);
  clearInterval(_clockTimer);
});
</script>

<style scoped>
.setmeal-page {
  background-color: var(--eat-surface);
  min-height: 100vh;
  color: var(--eat-on-surface);
  font-family: var(--font-body);
}

.setmeal-header {
  padding: 8rem 0 4rem;
  text-align: center;
  position: relative;
  overflow: hidden;
}
.setmeal-header-bg {
  position: absolute;
  inset: 0;
  background-image: url('https://images.unsplash.com/photo-1544025162-d76694265947?w=1400&q=80');
  background-size: cover;
  background-position: center 40%;
}
.setmeal-header-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    to bottom,
    rgba(24, 11, 6, 0.80) 0%,
    rgba(24, 11, 6, 0.70) 55%,
    var(--eat-surface) 100%
  );
}

.setmeal-eyebrow {
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
  margin-bottom: 0;
  font-style: italic;
}

/* ── Grid ── */
.setmeal-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2.5rem;
}

/* ── Card ── */
.meal-card {
  background-color: var(--eat-surface-high);
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(227, 199, 107, 0.05);
  cursor: pointer;
  position: relative;
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1),
              border-color 0.3s,
              box-shadow 0.3s;
  display: flex;
  flex-direction: column;
}

/* ── 時段外遮罩 ── */
.time-overlay {
  position: absolute;
  inset: 0;
  background: rgba(10, 5, 2, 0.68);
  backdrop-filter: blur(3px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 5;
  border-radius: inherit;
  pointer-events: none;
}
.time-overlay-text {
  font-family: var(--font-label);
  font-size: 0.95rem;
  letter-spacing: 0.18em;
  color: var(--eat-primary);
  border: 1px solid rgba(227, 199, 107, 0.35);
  padding: 0.5rem 1.4rem;
  border-radius: 4px;
  background: rgba(227, 199, 107, 0.06);
}
.meal-card:hover {
  transform: translateY(-6px);
  border-color: rgba(227, 199, 107, 0.2);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.4);
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

.meal-img-wrap {
  position: relative;
  height: 220px;
  overflow: hidden;
  background-color: #251813;
  --sx: 50%;
  --sy: 50%;
  --spotlight-opacity: 0;
}
.meal-img-wrap img {
  width: 100%; height: 100%;
  object-fit: cover;
  transition: transform 1s ease;
}
.meal-card:hover .meal-img-wrap img { transform: scale(1.06); }

/* Spotlight 光暈層 */
.meal-img-wrap::after {
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
  display: flex; gap: 0.4rem; z-index: 2;
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
.badge-rec { background-color: rgba(227, 199, 107, 0.9); color: var(--eat-surface); }
.badge-pop { background-color: rgba(217, 83, 79, 0.9);  color: white; }

.meal-info { padding: 1.5rem; flex-grow: 1; display: flex; flex-direction: column; gap: 0.75rem; }

.meal-title-row {
  display: flex; justify-content: space-between;
  align-items: flex-start; gap: 0.5rem;
}
.meal-name {
  font-family: var(--font-headline);
  color: var(--eat-primary);
  font-size: 1.3rem;
  margin: 0; font-style: italic;
}
.meal-price {
  font-family: var(--font-label);
  color: var(--eat-secondary);
  font-size: 0.95rem;
  white-space: nowrap; padding-top: 0.2rem;
}

.meal-tags { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.tag {
  font-family: var(--font-label);
  font-size: 0.72rem;
  padding: 0.2rem 0.65rem;
  border-radius: 20px;
}
.time-tag {
  background: rgba(227, 199, 107, 0.1);
  border: 1px solid rgba(227, 199, 107, 0.25);
  color: var(--eat-secondary);
}
.save-tag {
  background: rgba(80, 160, 80, 0.12);
  border: 1px solid rgba(80, 160, 80, 0.35);
  color: #7ecf7e;
}

.meal-desc {
  font-family: var(--font-body);
  font-size: 0.88rem;
  color: rgba(249, 221, 211, 0.6);
  font-style: italic;
  line-height: 1.6;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.meal-items-preview { display: flex; flex-wrap: wrap; gap: 0.4rem; margin-top: auto; padding-top: 0.5rem; border-top: 1px solid rgba(227, 199, 107, 0.1); }
.item-chip {
  font-family: var(--font-label);
  font-size: 0.68rem;
  padding: 0.15rem 0.55rem;
  background: rgba(227, 199, 107, 0.07);
  border: 1px solid rgba(227, 199, 107, 0.18);
  border-radius: 4px;
  color: rgba(249, 221, 211, 0.7);
}
.item-chip.more { color: var(--eat-secondary); border-color: rgba(227, 199, 107, 0.3); }

/* ── Card 進場動畫 ── */
.card-enter-active { transition: opacity 0.5s ease, transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1); }
.card-leave-active { transition: opacity 0.3s ease, transform 0.3s ease; }
.card-enter-from { opacity: 0; transform: translateY(24px) scale(0.97); }
.card-leave-to   { opacity: 0; transform: translateY(-8px) scale(0.97); }
.card-move       { transition: transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1); }

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
  width: 100%; max-width: 540px;
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
  position: relative; height: 240px;
  background: #251813; overflow: hidden;
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
  max-height: 60vh;
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
  font-size: 1.7rem; font-style: italic;
  line-height: 1.2; margin: 0;
}
.modal-price-wrap { text-align: right; flex-shrink: 0; }
.modal-price {
  font-family: var(--font-label);
  color: var(--eat-secondary);
  font-size: 1.1rem; font-weight: 600;
}
.modal-save {
  font-family: var(--font-label);
  font-size: 0.75rem;
  color: #7ecf7e;
  margin-top: 0.2rem;
}
.modal-desc {
  font-family: var(--font-body);
  font-size: 0.9rem;
  color: rgba(249, 221, 211, 0.65);
  font-style: italic;
  line-height: 1.7;
  margin-bottom: 1rem;
}
.modal-time {
  font-family: var(--font-label);
  font-size: 0.78rem;
  color: var(--eat-secondary);
  margin-bottom: 1.25rem;
  letter-spacing: 0.05em;
}

.modal-section { margin-bottom: 1.5rem; }
.modal-section-label {
  font-family: var(--font-label);
  font-size: 0.7rem; text-transform: uppercase;
  letter-spacing: 0.15em; opacity: 0.4;
  margin-bottom: 0.65rem;
}

/* 固定餐點列表 */
.fixed-items { display: flex; flex-direction: column; gap: 0.5rem; }
.fixed-item {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.6rem 0.9rem;
  background: rgba(255,255,255,0.03);
  border-radius: 8px;
  border: 1px solid rgba(227, 199, 107, 0.08);
}
.item-name { flex-grow: 1; font-family: var(--font-body); font-size: 0.9rem; }
.item-qty { font-family: var(--font-label); font-size: 0.78rem; color: rgba(249,221,211,0.45); }
.item-price { font-family: var(--font-label); font-size: 0.82rem; color: var(--eat-secondary); white-space: nowrap; }

/* 選擇性群組 */
.modal-section-label { display: flex; align-items: center; gap: 0.5rem; }
.group-count {
  font-family: var(--font-label);
  font-size: 0.7rem;
  color: rgba(249, 221, 211, 0.45);
  margin-left: auto;
}
.group-count.full { color: #7ecf7e; }

.option-items { display: flex; flex-direction: column; gap: 0.4rem; }
.option-item {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.6rem 0.9rem;
  background: rgba(255,255,255,0.03);
  border-radius: 8px;
  border: 1px solid rgba(227, 199, 107, 0.08);
}
.option-name { flex-grow: 1; font-family: var(--font-body); font-size: 0.9rem; }
.option-price { font-family: var(--font-label); font-size: 0.82rem; color: var(--eat-secondary); white-space: nowrap; }

.fixed-item.is-soldout,
.option-item.is-soldout { opacity: 0.45; }

.item-soldout-tag,
.option-soldout-tag {
  font-family: var(--font-label);
  font-size: 0.68rem;
  letter-spacing: 0.08em;
  color: rgba(217, 83, 79, 0.85);
  border: 1px solid rgba(217, 83, 79, 0.35);
  padding: 0.1rem 0.5rem;
  border-radius: 4px;
  white-space: nowrap;
}

/* 數量加減控制器 */
.qty-control {
  display: flex; align-items: center; gap: 0.5rem;
  flex-shrink: 0;
}
.qty-btn {
  width: 28px; height: 28px;
  border-radius: 50%;
  border: 1px solid rgba(227, 199, 107, 0.35);
  background: transparent;
  color: var(--eat-primary);
  font-size: 1rem; line-height: 1;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.2s, border-color 0.2s;
}
.qty-btn:hover:not(:disabled) {
  background: rgba(227, 199, 107, 0.12);
  border-color: var(--eat-primary);
}
.qty-btn:disabled { opacity: 0.3; cursor: not-allowed; }
.qty-num {
  font-family: var(--font-label);
  font-size: 0.95rem;
  color: var(--eat-on-surface);
  min-width: 16px; text-align: center;
}

/* ── 價格計算區 ── */
.price-summary {
  margin-top: 1.5rem;
  padding: 1.25rem;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(227, 199, 107, 0.12);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
.price-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-family: var(--font-label);
  font-size: 0.88rem;
  color: rgba(249, 221, 211, 0.7);
}
.set-price-row {
  padding-top: 0.6rem;
  border-top: 1px solid rgba(227, 199, 107, 0.1);
  color: var(--eat-secondary);
  font-size: 1rem;
}
.save-row {
  font-size: 0.88rem;
}
.save-amount {
  color: #7ecf7e;
  font-weight: 600;
}

/* ── States ── */
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
.error-icon {
  width: 48px; height: 48px; border-radius: 50%;
  background: rgba(217, 83, 79, 0.15);
  border: 1px solid rgba(217, 83, 79, 0.4);
  color: rgba(217, 83, 79, 0.8);
  font-size: 1.4rem; font-weight: 700;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 1.5rem;
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

/* ── Utils ── */
.container { max-width: 1200px; margin: 0 auto; padding: 0 2rem; }
.py-5 { padding-top: 4rem; padding-bottom: 6rem; }

@media (max-width: 768px) {
  .setmeal-header { padding-top: 6rem; }
  .eat-h1 { font-size: 2.2rem; }
  .setmeal-grid { grid-template-columns: 1fr; }
  .modal-title { font-size: 1.4rem; }
  .modal-body { max-height: 65vh; }
}
</style>
