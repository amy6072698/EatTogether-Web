import { defineStore } from 'pinia';
import { ref, reactive, computed } from 'vue';

export const useOrderStore = defineStore('order', () => {
  const cart    = reactive({});   // { productId: qty }
  const notes   = reactive({});   // { productId: noteString }
  const pax     = ref(0);   // 0 = 尚未填寫（必填）
  const specialRequest = ref('');

  const cartItems = computed(() =>
    Object.entries(cart)
      .filter(([, qty]) => qty > 0)
      .map(([id, qty]) => ({ productId: Number(id), qty }))
  );

  const totalItems = computed(() =>
    Object.values(cart).reduce((s, v) => s + v, 0)
  );

  function addItem(productId) {
    cart[productId] = (cart[productId] || 0) + 1;
  }

  function removeItem(productId) {
    if (!cart[productId]) return;
    cart[productId]--;
    if (cart[productId] <= 0) {
      delete cart[productId];
      delete notes[productId];
    }
  }

  function setNote(productId, note) {
    if (note && note.trim()) {
      notes[productId] = note.trim();
    } else {
      delete notes[productId];
    }
  }

  function clearOrder() {
    Object.keys(cart).forEach(k => delete cart[k]);
    Object.keys(notes).forEach(k => delete notes[k]);
    pax.value = 0;
    specialRequest.value = '';
  }

  return { cart, notes, pax, specialRequest, cartItems, totalItems, addItem, removeItem, setNote, clearOrder };
});
