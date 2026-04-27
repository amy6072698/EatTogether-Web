import { defineStore } from 'pinia';
import { ref, reactive, computed } from 'vue';

export const useOrderStore = defineStore('order', () => {
  // 每條 line：{ lineId, productId, qty, note }
  // 相同 productId + 相同 note → 合併成同一條
  const lines = reactive([]);
  let _seq = 0;

  const pax = ref(0);
  const specialRequest = ref('');

  // ── computed ──────────────────────────────────────────────
  const totalItems = computed(() => lines.reduce((s, l) => s + l.qty, 0));

  // 回傳 { productId: totalQty }，供菜單卡片上的數量顯示
  const cart = computed(() => {
    const c = {};
    lines.forEach(l => { c[l.productId] = (c[l.productId] || 0) + l.qty; });
    return c;
  });

  // 回傳 lines 的淺拷貝，供購物車列表渲染
  const cartItems = computed(() => lines.map(l => ({ ...l })));

  // ── actions ───────────────────────────────────────────────

  /**
   * 加入餐點。相同 productId + 相同 note 合併在同一 line，
   * 否則新增獨立 line（實現「同餐點不同備註分開顯示」）。
   */
  function addItem(productId, note = '') {
    const n = note?.trim() ?? '';
    const existing = lines.find(l => l.productId === productId && l.note === n);
    if (existing) {
      existing.qty++;
    } else {
      lines.push({ lineId: ++_seq, productId, qty: 1, note: n });
    }
  }

  /**
   * 菜單卡片「−」鍵：依 productId 移除（優先移除無備註那條，
   * 若全部都有備註則移除最新加入的那條）。
   */
  function removeItem(productId) {
    const noNote = lines.find(l => l.productId === productId && !l.note);
    const any    = [...lines].reverse().find(l => l.productId === productId);
    const target = noNote ?? any;
    if (!target) return;
    target.qty--;
    if (target.qty <= 0) {
      const idx = lines.indexOf(target);
      lines.splice(idx, 1);
    }
  }

  /**
   * 購物車列表「−」鍵：依 lineId 精確移除。
   */
  function removeLineItem(lineId) {
    const idx = lines.findIndex(l => l.lineId === lineId);
    if (idx === -1) return;
    if (lines[idx].isSetMeal) {
      // 套餐直接整條移除
      lines.splice(idx, 1);
    } else {
      lines[idx].qty--;
      if (lines[idx].qty <= 0) lines.splice(idx, 1);
    }
  }

  /**
   * 加入套餐（每筆套餐為獨立一條 line，qty 固定=1）。
   * @param {number}   productId        Product 表 ID
   * @param {number}   unitPrice        套餐售價
   * @param {string}   note             備註
   * @param {object}   setMealData      { id, name, fixedItems, selectedOptions }
   */
  function addSetMeal(productId, unitPrice, note, setMealData) {
    lines.push({
      lineId:      ++_seq,
      productId,
      qty:         1,
      note:        note?.trim() ?? '',
      isSetMeal:   true,
      unitPrice,
      setMealData: { ...setMealData },
    });
  }

  /**
   * 更新套餐 line（編輯用）。
   */
  function updateSetMealLine(lineId, unitPrice, note, setMealData) {
    const line = lines.find(l => l.lineId === lineId);
    if (!line || !line.isSetMeal) return;
    line.unitPrice   = unitPrice;
    line.note        = note?.trim() ?? '';
    line.setMealData = { ...setMealData };
  }

  function clearOrder() {
    lines.splice(0, lines.length);
    pax.value = 0;
    specialRequest.value = '';
  }

  return {
    lines, pax, specialRequest,
    cart, cartItems, totalItems,
    addItem, removeItem, removeLineItem,
    addSetMeal, updateSetMealLine,
    clearOrder,
  };
});
