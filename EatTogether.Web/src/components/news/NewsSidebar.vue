<template>
    <aside class="news-sidebar">
        <div class="news-sidebar-inner">
            <div class="news-sidebar-heading">
                <h2 class="news-sidebar-title">Il Manoscritto</h2>
                <p class="news-sidebar-sub">News &amp; Stories</p>
            </div>
            <nav class="news-sidebar-nav">
                <a
                    v-for="cat in categories"
                    :key="cat.id ?? 'all'"
                    class="news-sidebar-link"
                    :class="{ active: selectedCategory === cat.id }"
                    href="#"
                    @click.prevent="$emit('select', cat.id)"
                >
                    <span class="news-sidebar-label">{{ cat.label }}</span>
                    <span class="news-sidebar-en">{{ cat.en }}</span>
                </a>
            </nav>
        </div>
    </aside>
</template>

<script setup>
defineProps({
    categories: {
        type: Array,
        required: true,
    },
    selectedCategory: {
        type: String,
        default: null,
    },
})

defineEmits(['select'])
</script>

<style scoped>
/* ── Sidebar ──────────────────────────────────────── */
.news-sidebar {
    width: 100%;
    flex-shrink: 0;
}
@media (min-width: 768px) {
    .news-sidebar {
        width: 15rem;
    }
}
.news-sidebar-inner {
    position: sticky;
    top: 6rem;
    display: flex;
    flex-direction: column;
    gap: 2rem;
}
.news-sidebar-title {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 1.8rem;
    color: var(--eat-primary);
    margin-bottom: 0;
}
.news-sidebar-sub {
    font-family: var(--font-label);
    font-size: 0.62rem;
    letter-spacing: 0.35em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.4;
    margin-top: 0.3rem;
}
.news-sidebar-nav {
    display: flex;
    flex-direction: column;
    gap: 1.25rem;
}
.news-sidebar-link {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    font-family: var(--font-label);
    font-size: 1rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--eat-on-surface-variant);
    opacity: 0.4;
    text-decoration: none;
    transition: all 0.25s ease;
}
.news-sidebar-link:hover,
.news-sidebar-link.active {
    color: var(--eat-primary);
    opacity: 1;
    transform: translateX(6px);
}
.news-sidebar-link.active {
    font-weight: 600;
}

.news-sidebar-label {
    font-style: italic;
}

.news-sidebar-en {
    font-family: var(--font-headline);
    font-style: italic;
    font-size: 0.8rem;
    opacity: 0.8;
    letter-spacing: 0.1em;
    min-width: 3.5rem;
    margin-left: 0.5rem;
}
</style>
