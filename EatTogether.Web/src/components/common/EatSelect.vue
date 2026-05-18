<template>
    <Listbox v-model="selectedValue">
        <div class="eat-select-wrap">
            <ListboxButton class="eat-select-btn" v-slot="{ open }">
                <span :class="{ 'eat-select-placeholder': !selectedValue }">
                    {{ selectedValue?.label ?? placeholder }}
                </span>
                <span class="eat-select-arrow" :class="{ 'is-open': open }">▾</span>
            </ListboxButton>

            <Transition name="eat-select-dropdown">
                <ListboxOptions class="eat-select-dropdown">
                    <ListboxOption
                        v-for="option in options"
                        :key="option.value"
                        :value="option"
                        v-slot="{ active, selected }"
                    >
                        <li
                            class="eat-select-option"
                            :class="{
                                'is-active': active,
                                'is-selected': selected,
                            }"
                        >
                            {{ option.label }}
                        </li>
                    </ListboxOption>
                </ListboxOptions>
            </Transition>
        </div>
    </Listbox>
</template>

<!--
  EatSelect.vue — 全站統一下拉選單元件（規範 E）
  原生 <select> 跨瀏覽器樣式不一致，全站一律使用此元件取代。
  以 Headless UI Listbox 實作，樣式完全可控。

  Props:
    modelValue (Object|null, default null)  — 目前選取項目，格式 { value, label }
    options    (Array, required)            — 選項陣列，每項格式 { value, label }
    placeholder (String, default '請選擇') — 未選取時的提示文字

  options 格式範例:
    const options = [
      { value: 'dine-in', label: '內用' },
      { value: 'takeout', label: '外帶' },
    ]

  v-model 使用範例:
    <EatSelect v-model="selected" :options="options" />
    <EatSelect v-model="selected" :options="options" placeholder="請選擇用餐方式" />

  取值方式:
    selected.value       // 整個物件 { value, label }
    selected.value?.value  // 只取 value（例如 'dine-in'）
    selected.value?.label  // 只取 label（例如 '內用'）
-->
<script setup>
import { computed } from 'vue'
import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from '@headlessui/vue'

const props = defineProps({
    modelValue: { type: Object, default: null },
    options: { type: Array, required: true },
    placeholder: { type: String, default: '請選擇' },
})

const emit = defineEmits(['update:modelValue'])

const selectedValue = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val),
})
</script>

<style scoped>
.eat-select-wrap {
    position: relative;
}

.eat-select-btn {
    width: 100%;
    background: var(--eat-surface-high);
    border: 1px solid var(--eat-outline-variant);
    border-radius: var(--eat-radius-sm);
    color: var(--eat-on-surface);
    font-family: var(--font-body);
    font-size: 0.95rem;
    padding: 0.375rem 0.75rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
    cursor: pointer;
    transition: border-color 0.25s;
    line-height: 1.5;
    text-align: left;
}

.eat-select-btn:focus-visible {
    border-color: var(--eat-primary);
    outline: none;
    box-shadow: 0 4px 12px -4px rgba(227, 199, 107, 0.3);
}

.eat-select-placeholder {
    color: rgba(249, 221, 211, 0.4);
}

.eat-select-arrow {
    font-size: 0.8rem;
    opacity: 0.7;
    transition: transform 0.2s ease;
    flex-shrink: 0;
    margin-left: 0.5rem;
}

.eat-select-arrow.is-open {
    transform: rotate(180deg);
}

.eat-select-dropdown {
    position: absolute;
    top: calc(100% + 0.25rem);
    left: 0;
    width: 100%;
    background: rgba(30, 16, 10, 0.96);
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
    border: 1px solid rgba(180, 120, 30, 0.2);
    border-radius: var(--eat-radius);
    padding: 0.3rem;
    max-height: 240px;
    overflow-y: auto;
    z-index: 200;
    list-style: none;
    margin: 0;
}

.eat-select-option {
    font-family: var(--font-body);
    font-size: 0.95rem;
    color: var(--eat-on-surface);
    padding: 0.7rem 1rem;
    border-radius: var(--eat-radius-sm);
    cursor: pointer;
    transition:
        background 0.15s,
        color 0.15s;
}

.eat-select-option.is-active {
    background: rgba(227, 199, 107, 0.08);
    color: rgba(254, 225, 130, 0.9);
}

.eat-select-option.is-selected {
    color: var(--eat-primary);
    font-weight: 600;
}

.eat-select-option.is-active.is-selected {
    background: rgba(227, 199, 107, 0.08);
    color: var(--eat-primary);
}

/* Transition */
.eat-select-dropdown-enter-active {
    transition:
        opacity 0.15s ease,
        transform 0.15s ease;
}

.eat-select-dropdown-leave-active {
    transition:
        opacity 0.1s ease,
        transform 0.1s ease;
}

.eat-select-dropdown-enter-from,
.eat-select-dropdown-leave-to {
    opacity: 0;
    transform: translateY(-4px);
}
</style>
