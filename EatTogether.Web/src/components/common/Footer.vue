<template>
    <footer class="footer-eat">
        <!-- Logo -->
        <div class="footer-logo">
            <img
                src="@/assets/images/logo.svg"
                alt="義起吃 Eat Together"
                height="64"
                class="d-inline-block align-middle"
            />
        </div>

        <!-- Info -->
        <div class="row justify-content-center g-0 footer-info-row">
            <div class="col-12 col-md-6 mb-4 mb-md-0">
                <p class="footer-info-title">聯絡資訊</p>
                <p class="footer-info-text">
                    台北市大安區慢食街 88 號<br />
                    Tel: (02) 2345-6789
                </p>
            </div>
            <div class="col-12 col-md-6">
                <p class="footer-info-title">營業時間</p>
                <p class="footer-info-text">
                    週一至週五：17:30 – 23:00<br />
                    週末及例假日：11:30 – 23:30
                </p>
            </div>
        </div>

        <!-- Social Links -->
        <div class="footer-social d-flex justify-content-center gap-5 mb-4">
            <a
                v-for="social in socialLinks"
                :key="social.label"
                :href="social.href"
                target="_blank"
                rel="noopener noreferrer"
            >
                {{ social.label }}
            </a>
        </div>

        <!-- Copyright -->
        <div class="d-flex justify-content-center gap-4 flex-column flex-lg-row mt-lg-5">
            <p class="footer-copy mb-0">
                義起吃 Eat Together &copy;2026 Copyrights All Rights Reserved
            </p>
            <p class="footer-copy mb-0">本專題僅為學習用途，不做任何商業使用。</p>
        </div>
    </footer>
</template>

<script setup>
const socialLinks = [
    { label: 'Facebook', href: '#' },
    { label: 'Instagram', href: '#' },
    { label: 'Line', href: '#' },
]
</script>

<style scoped>
.footer-eat {
    background-color: var(--eat-surface-lowest);
    border-top: 1px solid rgba(180, 120, 30, 0.15);
    padding: 4rem 2rem;
    text-align: center;
}

.footer-logo {
    display: flex;
    justify-content: center;
    margin-bottom: 1rem;
}

.footer-info-row {
    width: 100%;
    max-width: 640px;
    margin: 2.5rem auto;
}

.footer-info-title {
    font-family: var(--font-body);
    font-size: 0.95rem;
    color: var(--eat-primary);
    font-weight: 500;
    margin-bottom: 1rem;
    letter-spacing: 0.04em;
}

.footer-info-text {
    font-family: var(--font-body);
    font-size: 0.9rem;
    color: rgba(200, 185, 165, 0.65);
    line-height: 1.9;
    letter-spacing: 0.02em;
    margin: 0;
}

.footer-social a {
    font-family: var(--font-body);
    font-size: 0.9rem;
    color: rgba(200, 185, 165, 0.6);
    text-decoration: none;
    letter-spacing: 0.04em;
    transition: color 0.35s ease;
}
.footer-social a:hover {
    color: var(--eat-primary);
}

.footer-copy {
    font-family: var(--font-label);
    font-size: 0.72rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: rgba(152, 143, 129, 0.5);
}
</style>
