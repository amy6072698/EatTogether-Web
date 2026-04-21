<template>
  <div v-if="visible" @click.self="$emit('close')" class="modal-wrap open">
    <div class="modal-box text-center">
      <div class="px-10 py-14 space-y-6">
        <div class="text-5xl mb-2">🕯️</div>
        <h2 class="font-headline text-3xl italic" style="color:#e3c76b">訂單已送出</h2>
        <p class="font-body italic leading-relaxed" style="color:rgba(249,221,211,0.65)">
          您的點餐已傳至廚房，<br/>請稍候，服務員將為您送上。
        </p>
        <div class="px-6 py-4 text-left" style="background:rgba(93,69,20,0.5);border:1px solid rgba(228,194,133,0.2)">
          <p class="font-label text-xs tracking-widest uppercase" style="color:#e4c285">
            訂單編號 &nbsp;·&nbsp; <span style="color:#e3c76b">{{ orderNumber }}</span>
          </p>
          <p class="font-label text-xs tracking-widest uppercase mt-1" style="color:rgba(208,197,181,0.5)">
            桌號 · <span>{{ tableLabel }}</span>
          </p>
        </div>
        <button @click="$emit('close')" class="submit-btn px-14 py-4 font-label text-xs tracking-[0.25em] uppercase transition-all duration-300">繼續加點</button>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  visible:     { type: Boolean, default: false },
  orderNumber: { type: String,  default: '' },
  tableLabel:  { type: String,  default: '' },
});
defineEmits(['close']);
</script>

<style scoped>
.modal-wrap { display: none; position: fixed; inset: 0; background: rgba(24,11,6,0.87); backdrop-filter: blur(6px); z-index: 200; align-items: center; justify-content: center; }
.modal-wrap.open { display: flex; }
.modal-box { background: #2b1c16; width: min(580px, 95vw); max-height: 90vh; overflow-y: auto; box-shadow: 0 24px 60px rgba(0,0,0,0.6); }
.submit-btn { background: linear-gradient(to right, #e3c76b, #c6ab53); color: #3b2f00; display: inline-block; }
.submit-btn:hover { filter: brightness(1.1); }
.submit-btn:active { transform: scale(0.95); }
.font-headline { font-family:'Noto Serif TC',serif; }
.font-label    { font-family:'Work Sans',sans-serif; }
.font-body     { font-family:'Newsreader',serif; }
</style>
