import { defineStore } from 'pinia';
import { ref, reactive, computed } from 'vue';

export const useOrderStore = defineStore('order', () => {
  const cart = reactive({});   // { productId: qty }
  const pax = ref(2);
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
    if (cart[productId] <= 0) delete cart[productId];
  }

  function clearOrder() {
    Object.keys(cart).forEach(k => delete cart[k]);
  }

  return { cart, pax, specialRequest, cartItems, totalItems, addItem, removeItem, clearOrder };
});
