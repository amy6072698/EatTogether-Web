<template>
    <p
        v-if="show && message"
        class="form-error-msg"
    >
        {{ message }}
    </p>
</template>

<!--
  FormErrorMessage.vue — 表單欄位錯誤訊息元件
  顯示單一欄位的驗證錯誤文字，show 與 message 同時為真才渲染。

  Props:
    message (String, required) — 錯誤訊息文字
    show    (Boolean, default false) — 控制顯示/隱藏

  範例:
    <FormErrorMessage :message="emailError" :show="!!emailError" />

    搭配 ref 驗證的寫法:
      const emailError = ref('')
      // 驗證後設定 emailError.value = '請輸入有效的 Email'
      <FormErrorMessage :message="emailError" :show="!!emailError" />
-->
<script setup>
defineProps({
    message: { type: String, required: true },
    show: { type: Boolean, default: false },
})
</script>

<style scoped>
.form-error-msg {
    font-family: var(--font-label);
    font-size: 0.75rem;
    color: var(--eat-error);
    margin-top: 0.35rem;
    margin-bottom: 0;
}
</style>
