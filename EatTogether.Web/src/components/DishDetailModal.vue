<template>
  <div v-if="dish" @click.self="$emit('close')" class="modal-wrap open">
    <div class="modal-box">
      <div class="detail-img-wrap">
        <img :src="dish.imageUrl" :alt="dish.productName" />
        <button @click="$emit('close')" class="close-btn absolute top-4 right-4 z-10 w-8 h-8 rounded-full backdrop-blur-sm flex items-center justify-center text-lg transition-colors">✕</button>
      </div>
      <div class="px-8 py-6 space-y-4">
        <div class="flex items-start justify-between gap-4">
          <h2 class="font-headline text-2xl italic" style="color:#e3c76b">{{ dish.productName }}</h2>
          <span class="font-label text-lg tracking-wide shrink-0" style="color:#d5b478">NT$ {{ dish.unitPrice?.toLocaleString() }}</span>
        </div>
        <div class="flex gap-2 flex-wrap">
          <span v-for="tag in tags" :key="tag.key" :class="['badge', tag.cls]">{{ tag.label }}</span>
        </div>
        <p class="font-body italic leading-relaxed" style="color:rgba(249,221,211,0.7)">{{ dish.description }}</p>
        <div class="feather-divider" style="--bg:#2b1c16;margin:1rem 0;"></div>
        <div class="flex items-center justify-between">
          <span class="font-label text-xs tracking-widest uppercase" style="color:#d0c5b5">加入數量</span>
          <div class="flex items-center gap-3">
            <button class="qty-btn" @click="$emit('remove', dish)">−</button>
            <span class="font-label text-base w-6 text-center" style="color:#f9ddd3">{{ qty }}</span>
            <button class="qty-btn" @click="$emit('add', dish)">+</button>
          </div>
        </div>
        <button @click="onConfirm" class="submit-btn w-full py-4 font-label text-xs tracking-[0.28em] uppercase transition-all duration-300 mt-2">加入本桌訂單</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  dish: { type: Object, default: null },
  qty:  { type: Number, default: 0 },
});
const emit = defineEmits(['close', 'add', 'remove', 'confirm']);

const tags = computed(() => {
  if (!props.dish) return [];
  const t = [];
  if (props.dish.isRecommended) t.push({ key: 'rec',   cls: 'badge-new',   label: '推薦' });
  if (props.dish.isPopular)     t.push({ key: 'pop',   cls: 'badge-chef',  label: '主廚特選' });
  if (props.dish.isVegetarian)  t.push({ key: 'veg',   cls: 'badge-veg',   label: '素食' });
  if (props.dish.spicyLevel > 0) t.push({ key: 'spicy', cls: 'badge-spicy', label: '辣味' });
  return t;
});

function onConfirm() {
  emit('confirm', props.dish);
}
</script>

<style scoped>
.modal-wrap { display: none; position: fixed; inset: 0; background: rgba(24,11,6,0.87); backdrop-filter: blur(6px); z-index: 200; align-items: center; justify-content: center; }
.modal-wrap.open { display: flex; }
.modal-box { background: #2b1c16; width: min(580px, 95vw); max-height: 90vh; overflow-y: auto; box-shadow: 0 24px 60px rgba(0,0,0,0.6); }
.detail-img-wrap { position: relative; height: 240px; overflow: hidden; }
.detail-img-wrap img { width: 100%; height: 100%; object-fit: cover; }
.detail-img-wrap::after { content:''; position:absolute; inset:0; background: linear-gradient(to top, #2b1c16 0%, transparent 55%); }
.close-btn { background: rgba(24,11,6,0.7); border: 1px solid rgba(77,70,58,0.4); color: rgba(208,197,181,0.8); }
.close-btn:hover { color: #e3c76b; }
.qty-btn { width:28px; height:28px; border:1px solid rgba(77,70,58,0.7); background:#2b1c16; color:#f9ddd3; border-radius:0.125rem; cursor:pointer; display:flex; align-items:center; justify-content:center; font-size:1rem; line-height:1; transition:border-color 0.3s,color 0.3s; }
.qty-btn:hover { border-color:#e3c76b; color:#e3c76b; }
.submit-btn { background: linear-gradient(to right, #e3c76b, #c6ab53); color: #3b2f00; display: block; }
.submit-btn:hover { filter: brightness(1.1); }
.submit-btn:active { transform: scale(0.97); }
.feather-divider { height:1px; background:linear-gradient(90deg,transparent 0%,#e4c285 50%,transparent 100%); position:relative; }
.feather-divider::after { content:'◈'; position:absolute; left:50%; top:50%; transform:translate(-50%,-50%); color:#e4c285; font-size:0.7rem; background:var(--bg,#2b1c16); padding:0 0.65rem; }
.badge-new  { background:rgba(227,199,107,0.18); border:1px solid rgba(227,199,107,0.45); color:#e3c76b; }
.badge-veg  { background:rgba(66,49,43,0.5); border:1px solid rgba(120,180,80,0.4); color:#a3d977; }
.badge-spicy{ background:rgba(147,0,10,0.25); border:1px solid rgba(255,100,80,0.4); color:#ffb4ab; }
.badge-chef { background:rgba(93,69,20,0.4); border:1px solid rgba(228,194,133,0.35); color:#e4c285; }
.badge { font-family:'Work Sans',sans-serif; font-size:0.58rem; letter-spacing:0.18em; text-transform:uppercase; padding:0.18rem 0.5rem; }
.font-headline { font-family:'Noto Serif TC',serif; }
.font-label    { font-family:'Work Sans',sans-serif; }
.font-body     { font-family:'Newsreader',serif; }
</style>
