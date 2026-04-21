<template>
    <footer class="footer-eat">
        <!-- Logo -->
        <div class="footer-logo">
            <svg
                height="64"
                viewBox="0 0 400 120"
                xmlns="http://www.w3.org/2000/svg"
            >
                <circle cx="60" cy="60" fill="#1A0D08" r="56" />
                <circle
                    cx="60"
                    cy="60"
                    fill="none"
                    opacity="0.5"
                    r="52"
                    stroke="#F5D87A"
                    stroke-width="1"
                />
                <line
                    stroke="#F5D87A"
                    stroke-linecap="round"
                    stroke-width="3"
                    x1="49"
                    x2="49"
                    y1="28"
                    y2="92"
                />
                <line
                    stroke="#F5D87A"
                    stroke-linecap="round"
                    stroke-width="2.2"
                    x1="43"
                    x2="43"
                    y1="28"
                    y2="48"
                />
                <line
                    stroke="#F5D87A"
                    stroke-linecap="round"
                    stroke-width="2.2"
                    x1="49"
                    x2="49"
                    y1="28"
                    y2="48"
                />
                <line
                    stroke="#F5D87A"
                    stroke-linecap="round"
                    stroke-width="2.2"
                    x1="55"
                    x2="55"
                    y1="28"
                    y2="48"
                />
                <path
                    d="M43 48 Q49 58 55 48"
                    fill="none"
                    stroke="#F5D87A"
                    stroke-width="2.2"
                />
                <line
                    stroke="#F5D87A"
                    stroke-linecap="round"
                    stroke-width="3"
                    x1="71"
                    x2="71"
                    y1="28"
                    y2="92"
                />
                <path
                    d="M71 28 Q83 37 83 52 Q83 61 71 64"
                    fill="#F5D87A"
                    opacity="0.18"
                />
                <path
                    d="M71 28 Q83 37 83 52 Q83 61 71 64"
                    fill="none"
                    stroke="#F5D87A"
                    stroke-width="2.2"
                />
                <circle cx="60" cy="93" fill="#C9A96E" r="2" />
                <line
                    opacity="0.35"
                    stroke="#F5D87A"
                    stroke-width="1"
                    x1="130"
                    x2="130"
                    y1="28"
                    y2="92"
                />
                <text
                    fill="#F5D87A"
                    font-family="'Noto Serif TC','Songti SC',serif"
                    font-size="44"
                    font-weight="700"
                    letter-spacing="8"
                    x="158"
                    y="74"
                >
                    義起吃
                </text>
                <text
                    fill="#C9A96E"
                    font-family="'Cormorant Garamond','Georgia',serif"
                    font-size="14"
                    font-style="italic"
                    letter-spacing="5"
                    opacity="0.85"
                    x="162"
                    y="97"
                >
                    Eat Together
                </text>
            </svg>
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
        <p class="footer-copy">© 義起吃 EatTogether. All Rights Reserved.</p>
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
    margin-top: 0.5rem;
    margin-bottom: 0;
}
</style>
