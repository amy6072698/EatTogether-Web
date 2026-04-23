import { defineConfig, globalIgnores } from 'eslint/config'
import globals from 'globals'
import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'

export default defineConfig([
    // ── 適用範圍：src 內所有 JS / Vue 檔案 ───────────────────
    {
        name: 'app/files-to-lint',
        files: ['**/*.{vue,js,mjs}'],
    },

    // ── 排除不需要 lint 的目錄 ────────────────────────────────
    globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**']),

    // ── 瀏覽器全域變數（window、document、console 等）────────
    {
        languageOptions: {
            globals: {
                ...globals.browser,
            },
        },
    },

    // ── 基礎規則：ESLint 官方推薦設定 ────────────────────────
    js.configs.recommended,

    // ── Vue3 規則：eslint-plugin-vue 官方推薦設定 ─────────────
    ...pluginVue.configs['flat/essential'],

    // ── 專案自訂規則 ──────────────────────────────────────────
    {
        name: 'app/custom-rules',
        files: ['src/**/*.{js,vue}'],
        rules: {
            // ── 禁止裸 fetch()（核心規則）────────────────────
            // 所有 API 呼叫一律透過 src/utils/apiFetch.js
            // 直接使用 fetch() 會導致：
            //   1. Cookie 未帶上（credentials: 'include' 遺漏）→ JWT 驗證失敗
            //   2. 401 不會自動 refresh token
            //   3. 403 / 5xx 沒有統一 Toast 提示
            //   4. 網路錯誤無 try/catch → unhandled rejection → 頁面卡死
            'no-restricted-globals': [
                'error',
                {
                    name: 'fetch',
                    message:
                        '禁止直接使用 fetch()。\n' +
                        '請改用 src/utils/apiFetch.js：\n' +
                        "  import apiFetch from '@/utils/apiFetch.js'\n" +
                        "  const res = await apiFetch('/your-path', { method: 'POST', body: JSON.stringify(data) })",
                },
            ],

            // ── 禁止 console.log 殘留（warn 不擋 build）─────
            // 開發期可暫時使用，PR 前應移除
            // 若確實需要保留，在該行加上 // eslint-disable-next-line no-console
            'no-console': ['warn', { allow: ['warn', 'error'] }],

            // ── Vue 規則 ──────────────────────────────────────
            // 頁面元件（Home.vue、Login.vue 等）允許單字命名
            'vue/multi-word-component-names': 'off',

            // 禁止在 template 使用 v-html（XSS 風險）
            'vue/no-v-html': 'warn',
        },
    },

    // ── apiFetch.js 本身排除 fetch 限制 ──────────────────────
    // 這是唯一可以直接呼叫原生 fetch() 的檔案
    {
        name: 'app/apifetch-override',
        files: ['src/utils/apiFetch.js'],
        rules: {
            'no-restricted-globals': 'off',
        },
    },

    // ── 關閉與 Prettier 衝突的格式規則 ───────────────────────
    // 格式的職責完全交給 Prettier（.prettierrc.json）處理
    // 對應已安裝的套件：@vue/eslint-config-prettier
    skipFormatting,
])
