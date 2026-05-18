<template>
  <div class="ingredient-card">
    <button class="ing-close-btn" @click.stop="$emit('close')" aria-label="關閉">✕</button>

    <p class="ing-card-title">{{ ingredientName }}</p>

    <!-- Loading -->
    <div v-if="loading" class="ing-loading">
      <span class="ing-spinner"></span>
      <span class="ing-loading-text">正在查詢食材資訊...</span>
    </div>

    <!-- Error -->
    <p v-else-if="error" class="ing-error">無法取得資訊，請稍後再試。</p>

    <!-- Content -->
    <div v-else class="ing-sections">
      <div v-for="sec in sections" :key="sec.key" class="ing-section">
        <span class="ing-sec-icon">{{ sec.icon }}</span>
        <div class="ing-sec-body">
          <div class="ing-sec-label">{{ sec.label }}</div>
          <div class="ing-sec-text">{{ sec.text }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const props = defineProps({
  ingredientName: { type: String, required: true }
})
defineEmits(['close'])

const loading = ref(true)
const error = ref(false)
const sections = ref([])

const SECTIONS = [
  { key: '來源產地', icon: '🌍', label: '來源產地' },
  { key: '烹煮方式', icon: '🍳', label: '烹煮方式' },
  { key: '過敏原提示', icon: '⚠️', label: '過敏原提示' },
  { key: '營養價值', icon: '💪', label: '營養價值' },
]

const parseText = (text) =>
  SECTIONS.map(({ key, icon, label }) => {
    const match = text.match(new RegExp(`【${key}】([^【]*)`))
    return { key, icon, label, text: match ? match[1].trim() : '—' }
  })

const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'

// 頁面層級快取，同一個食材只打一次 API
const cache = new Map()

onMounted(async () => {
  if (cache.has(props.ingredientName)) {
    sections.value = cache.get(props.ingredientName)
    loading.value = false
    return
  }
  try {
    const res = await fetch(`${API_BASE}/Ingredients/info`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ingredientName: props.ingredientName })
    })
    if (!res.ok) throw new Error(res.status)
    const data = await res.json()
    const parsed = parseText(data.text)
    cache.set(props.ingredientName, parsed)
    sections.value = parsed
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.ingredient-card {
  position: relative;
  margin-top: 0.75rem;
  padding: 1rem 1rem 0.85rem;
  background: rgba(10, 4, 2, 0.92);
  backdrop-filter: blur(18px);
  border: 1px solid rgba(227, 199, 107, 0.32);
  border-radius: 14px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.45);
}

.ing-card-title {
  font-family: var(--font-headline);
  font-style: italic;
  font-size: 0.95rem;
  color: var(--eat-primary);
  margin: 0 0 0.75rem;
  padding-right: 1.5rem;
}

.ing-close-btn {
  position: absolute;
  top: 0.6rem;
  right: 0.6rem;
  background: none;
  border: none;
  color: rgba(249, 221, 211, 0.35);
  font-size: 0.68rem;
  cursor: pointer;
  padding: 0.2rem 0.35rem;
  border-radius: 4px;
  line-height: 1;
  transition: color 0.2s;
}
.ing-close-btn:hover { color: var(--eat-primary); }

/* Loading */
.ing-loading {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  padding: 0.25rem 0 0.15rem;
}
.ing-spinner {
  width: 14px;
  height: 14px;
  flex-shrink: 0;
  border: 2px solid rgba(227, 199, 107, 0.15);
  border-top-color: var(--eat-primary);
  border-radius: 50%;
  animation: ing-spin 0.75s linear infinite;
}
@keyframes ing-spin { to { transform: rotate(360deg); } }
.ing-loading-text {
  font-family: var(--font-label);
  font-size: 0.7rem;
  color: rgba(249, 221, 211, 0.38);
  letter-spacing: 0.08em;
}

/* Error */
.ing-error {
  font-family: var(--font-label);
  font-size: 0.72rem;
  color: rgba(249, 115, 22, 0.7);
  margin: 0;
  padding: 0.15rem 0;
}

/* Sections */
.ing-sections {
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
}
.ing-section {
  display: flex;
  gap: 0.5rem;
  align-items: flex-start;
}
.ing-sec-icon {
  font-size: 0.82rem;
  line-height: 1.45;
  flex-shrink: 0;
}
.ing-sec-label {
  font-family: var(--font-label);
  font-size: 0.6rem;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: rgba(227, 199, 107, 0.5);
  margin-bottom: 0.1rem;
}
.ing-sec-text {
  font-family: var(--font-body);
  font-size: 0.78rem;
  line-height: 1.55;
  color: rgba(249, 221, 211, 0.72);
}
</style>
