<template>
    <footer class="footer-eat bg-eat-surface-lowest text-center py-5 px-3 px-md-4">
        <div class="d-flex justify-content-center mb-4">
            <img
                src="@/assets/images/logo.svg"
                alt="義起吃 Eat Together"
                height="64"
                class="d-inline-block align-middle"
            />
        </div>

        <div class="row justify-content-center g-0 footer-info-row mx-auto my-5">
            <div class="col-12 col-md-6 mb-4 mb-md-0">
                <p class="text-eat-primary fw-medium mb-3 letter-spacing-1">聯絡資訊</p>
                <p class="eat-body text-eat-muted m-0 lh-lg">
                    台北市大安區慢食街 88 號<br />
                    Tel: (02) 2345-6789
                </p>
            </div>
            <div class="col-12 col-md-6">
                <p class="text-eat-primary fw-medium mb-3 letter-spacing-1">營業時間</p>
                <p class="eat-body text-eat-muted m-0 lh-lg">
                    週一至週五：17:30 – 23:00<br />
                    週末及例假日：11:30 – 23:30
                </p>
            </div>
        </div>

        <div class="footer-social d-flex justify-content-center gap-4 mb-4">
            <a
                v-for="social in socialLinks"
                :key="social.label"
                :href="social.href"
                target="_blank"
                rel="noopener noreferrer"
                class="social-icon-link text-eat-muted"
                :aria-label="social.label"
            >
                <i :class="['bi', social.icon]"></i>
            </a>
        </div>

        <div class="d-flex justify-content-center gap-4 flex-column flex-lg-row mt-lg-5">
            <p class="eat-label text-eat-muted mb-0">
                義起吃 Eat Together &copy;2026 Copyrights All Rights Reserved
            </p>
            <p class="eat-label text-eat-muted mb-0">本專題僅為學習用途，不做任何商業使用。</p>
        </div>
    </footer>
</template>

<script setup>
const socialLinks = [
    { label: 'Facebook', href: '#', icon: 'bi-facebook' },
    { label: 'Instagram', href: '#', icon: 'bi-instagram' },
    { label: 'Line', href: '#', icon: 'bi-line' },
]
</script>

<style scoped>
/* 僅保留高度客製化、無法直接用全域類別取代的樣式 */
.footer-eat {
    /* 保留客製化的邊框顏色（因為 utilities 中沒有剛好 rgba 15% 的設定） */
    border-top: 1px solid rgba(180, 120, 30, 0.15);
}

.footer-info-row {
    width: 100%;
    max-width: 640px;
}

.letter-spacing-1 {
    letter-spacing: 0.04em;
}

/* 複寫 base.css 預設的 <a> 顏色與 hover 效果 */
.footer-social a {
    transition: color 0.35s ease;
}
.footer-social a:hover {
    color: var(--eat-primary) !important;
}

/* 針對社群 Icon 的客製化樣式 */
.social-icon-link {
    font-size: 1.5rem; /* 調整 Icon 大小，約 24px */
    transition:
        color 0.35s ease,
        transform 0.35s ease;
    display: inline-block; /* 讓 transform 生效 */
}

.social-icon-link:hover {
    color: var(--eat-primary) !important; /* 滑過時變成主色（金色） */
    transform: translateY(-3px); /* 滑過時微微向上浮動，增加互動質感 */
}
</style>
