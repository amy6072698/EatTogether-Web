<template>
    <!-- ══ 桌位驗證中 ══ -->
    <div v-if="tableStatus === 'checking'" class="gate-wrap">
        <div class="gate-spinner"></div>
        <p class="gate-hint font-label">驗證桌位中…</p>
    </div>

    <!-- ══ 禁止進入：無桌號 ══ -->
    <div v-else-if="tableStatus === 'no-table'" class="gate-wrap">
        <div class="gate-icon">🍽️</div>
        <h2 class="gate-title font-headline">歡迎光臨</h2>
        <p class="gate-body font-body">如需內用服務，請洽現場服務人員。</p>
    </div>

    <!-- ══ 禁止進入：空桌 / 找不到 ══ -->
    <div v-else-if="tableStatus === 'empty' || tableStatus === 'not-found'" class="gate-wrap">
        <div class="gate-icon">🪑</div>
        <h2 class="gate-title font-headline">此桌位目前為空桌</h2>
        <p class="gate-body font-body">如需服務請洽現場服務人員。</p>
        <p class="gate-sub font-label">桌號：{{ tableNameFromUrl }}</p>
    </div>

    <!-- ══ 禁止進入：保留中 ══ -->
    <div v-else-if="tableStatus === 'reserved'" class="gate-wrap">
        <div class="gate-icon">🔒</div>
        <h2 class="gate-title font-headline">此桌位已保留</h2>
        <p class="gate-body font-body">此桌位目前為訂位保留桌，如需服務請洽現場服務人員。</p>
        <p class="gate-sub font-label">桌號：{{ tableNameFromUrl }}</p>
    </div>

    <!-- ══ 正常點餐頁面 ══ -->
    <div v-else class="root-wrap">
        <!-- ══ 全寬頂部 Header ══ -->
        <header class="dine-header">
            <!-- 左：今日菜單 -->
            <div class="dh-brand">
                <h1 class="dh-title font-headline">今日菜單</h1>
            </div>

            <!-- 中：桌號 ＋ 用餐人數（同一行） -->
            <div class="dh-center">
                <span class="dh-label font-label">桌號</span>
                <span class="dh-table-name font-headline">{{
                    tables.find((t) => t.id === tableId)?.tableName || tableNameFromUrl || '—'
                }}</span>

                <div class="dh-sep"></div>

                <span :class="['dh-label', 'font-label', { 'pax-label-error': paxError }]">
                    用餐人數
                </span>
                <button
                    :class="['dh-pax-btn', 'font-label', { 'pax-ctrl-error': paxError }]"
                    :disabled="store.pax <= 1"
                    @click="store.pax = Math.max(1, store.pax - 1)"
                >
                    −
                </button>
                <input
                    type="number"
                    min="1"
                    max="99"
                    :class="[
                        'dh-pax-input',
                        'font-label',
                        { 'pax-empty': store.pax === 0, 'pax-input-error': paxError },
                    ]"
                    :value="store.pax || ''"
                    placeholder="—"
                    @change="
                        store.pax = Math.max(0, Math.min(99, parseInt($event.target.value) || 0))
                    "
                />
                <button
                    :class="['dh-pax-btn', 'font-label', { 'pax-ctrl-error': paxError }]"
                    @click="store.pax = (store.pax || 0) + 1"
                >
                    +
                </button>
            </div>

            <!-- 右：會員（可點擊展開下拉） -->
            <div
                class="dh-member dh-member-btn"
                @click.stop="memberDropdownOpen = !memberDropdownOpen"
            >
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="16"
                    height="16"
                    fill="currentColor"
                    style="color: rgba(208, 197, 181, 0.45); flex-shrink: 0"
                    viewBox="0 0 16 16"
                >
                    <path
                        d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0zm4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4zm-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.029 10 8 10c-2.03 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h10z"
                    />
                </svg>
                <span class="dh-member-label font-label">{{ memberName }}</span>
                <svg
                    class="dh-chevron"
                    :class="{ open: memberDropdownOpen }"
                    xmlns="http://www.w3.org/2000/svg"
                    width="10"
                    height="10"
                    fill="currentColor"
                    viewBox="0 0 16 16"
                >
                    <path
                        d="M7.247 11.14L2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z"
                    />
                </svg>
                <!-- 下拉選單 -->
                <div v-if="memberDropdownOpen" class="member-dropdown" @click.stop="">
                    <button class="member-dropdown-item font-label" @click="openSwitchAccount">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="13"
                            height="13"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                fill-rule="evenodd"
                                d="M1 8a7 7 0 1 0 14 0A7 7 0 0 0 1 8zm15 0A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-4.5-.5a.5.5 0 0 1 0 1H5.707l2.147 2.146a.5.5 0 0 1-.708.708l-3-3a.5.5 0 0 1 0-.708l3-3a.5.5 0 1 1 .708.708L5.707 7.5H11.5z"
                            />
                        </svg>
                        切換帳號
                    </button>
                </div>
            </div>
            <!-- 點擊背景關閉下拉 -->
            <div
                v-if="memberDropdownOpen"
                class="member-backdrop"
                @click="memberDropdownOpen = false"
            ></div>
        </header>

        <!-- ══ 手機分類捲動列 ══ -->
        <header class="mobile-header">
            <nav class="mobile-cat-tabs">
                <button
                    v-for="cat in sidebarCategories"
                    :key="cat.key"
                    @click="scrollToSection(cat.key)"
                    :class="['mobile-cat-btn', { active: activeSidebarCat === cat.key }]"
                >
                    {{
                        cat.key === '今日推薦'
                            ? '今日推薦'
                            : cat.key === '主廚特選'
                              ? '主廚特選'
                              : cat.label
                    }}<span class="cat-badge">{{ cat.count }}</span>
                </button>
            </nav>
        </header>

        <div class="page-layout">
            <!-- ── LEFT Sidebar（桌機） ── -->
            <aside class="sidebar-left flex flex-col">
                <div class="px-4 py-4" style="border-bottom: 1px solid rgba(77, 70, 58, 0.2)">
                    <div class="relative w-full gap-1" style="display: flex; align-items: center">
                        <svg
                            class="absolute left-4 top-1/2 -translate-y-1/2"
                            style="color: rgba(208, 197, 181, 0.4)"
                            xmlns="http://www.w3.org/2000/svg"
                            width="13"
                            height="13"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                            />
                        </svg>
                        <input
                            v-model="searchQuery"
                            type="text"
                            class="input-line w-full"
                            style="padding-left: 0.8rem; font-size: 0.85rem"
                            placeholder="搜尋料理名稱…"
                        />
                    </div>
                </div>
                <nav class="flex-1 py-3 overflow-y-auto">
                    <template v-for="(cat, idx) in sidebarCategories" :key="cat.key">
                        <!-- 特殊分類(今日推薦/主廚特選)與一般分類(全部/套餐…)之間的分隔線 -->
                        <div
                            v-if="
                                idx > 0 &&
                                !['今日推薦', '主廚特選', '我的收藏', '歷史訂單'].includes(
                                    cat.key
                                ) &&
                                ['今日推薦', '主廚特選', '我的收藏', '歷史訂單'].includes(
                                    sidebarCategories[idx - 1].key
                                )
                            "
                            class="cat-divider"
                        ></div>
                        <button
                            @click="scrollToSection(cat.key)"
                            :class="[
                                'cat-link w-full text-left',
                                { active: activeSidebarCat === cat.key },
                                {
                                    'cat-special': [
                                        '今日推薦',
                                        '主廚特選',
                                        '我的收藏',
                                        '歷史訂單',
                                    ].includes(cat.key),
                                },
                            ]"
                        >
                            <span>{{
                                cat.key === '今日推薦'
                                    ? '今日推薦'
                                    : cat.key === '主廚特選'
                                      ? '主廚特選'
                                      : cat.key === '我的收藏'
                                        ? '我的收藏'
                                        : cat.key === '歷史訂單'
                                          ? '歷史訂單'
                                          : cat.label
                            }}</span>
                            <span class="cat-count">{{ cat.count }}</span>
                        </button>
                    </template>
                </nav>
            </aside>

            <!-- ── CENTRE: Menu ── -->
            <main class="menu-main min-w-0" id="menuMain">
                <!-- Toolbar -->
                <div class="toolbar">
                    <div class="mobile-search-wrap">
                        <svg
                            style="color: rgba(208, 197, 181, 0.4); flex-shrink: 0"
                            xmlns="http://www.w3.org/2000/svg"
                            width="13"
                            height="13"
                            fill="currentColor"
                            viewBox="0 0 16 16"
                        >
                            <path
                                d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
                            />
                        </svg>
                        <input
                            v-model="searchQuery"
                            type="text"
                            class="input-line"
                            style="font-size: 0.85rem"
                            placeholder="搜尋料理名稱…"
                        />
                    </div>
                    <div class="toolbar-row">
                        <div class="chips-wrap">
                            <span class="chip-label">篩選條件：</span>
                            <button
                                @click="toggleChip('veg')"
                                :class="['chip-btn', { active: activeChip === 'veg' }]"
                            >
                                素食
                            </button>
                            <button
                                @click="toggleChip('spicy')"
                                :class="['chip-btn', { active: activeChip === 'spicy' }]"
                            >
                                辣味
                            </button>
                        </div>
                        <div class="view-toggle">
                            <button
                                @click="curView = 'list'"
                                :class="['view-btn', { active: curView === 'list' }]"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="13"
                                    height="13"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        fill-rule="evenodd"
                                        d="M2.5 12a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5z"
                                    />
                                </svg>
                            </button>
                            <button
                                @click="curView = 'grid'"
                                :class="['view-btn', { active: curView === 'grid' }]"
                            >
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="13"
                                    height="13"
                                    fill="currentColor"
                                    viewBox="0 0 16 16"
                                >
                                    <path
                                        d="M1 2.5A1.5 1.5 0 0 1 2.5 1h3A1.5 1.5 0 0 1 7 2.5v3A1.5 1.5 0 0 1 5.5 7h-3A1.5 1.5 0 0 1 1 5.5v-3zm8 0A1.5 1.5 0 0 1 10.5 1h3A1.5 1.5 0 0 1 15 2.5v3A1.5 1.5 0 0 1 13.5 7h-3A1.5 1.5 0 0 1 9 5.5v-3zm-8 8A1.5 1.5 0 0 1 2.5 9h3A1.5 1.5 0 0 1 7 10.5v3A1.5 1.5 0 0 1 5.5 15h-3A1.5 1.5 0 0 1 1 13.5v-3zm8 0A1.5 1.5 0 0 1 10.5 9h3A1.5 1.5 0 0 1 15 10.5v3A1.5 1.5 0 0 1 13.5 15h-3A1.5 1.5 0 0 1 9 13.5v-3z"
                                    />
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>

                <div v-if="loading" class="status-msg">菜單載入中…</div>
                <div v-else-if="loadError" class="status-msg" style="color: #ffb4ab">
                    {{ loadError }}
                </div>

                <div v-else class="menu-sections">
                    <template v-for="section in displaySections" :key="section.key">
                        <div v-show="section.dishes.length > 0" :id="'sec-' + section.key">
                            <h2 class="section-title">{{ section.label }}</h2>
                            <div class="dishes-wrap" :class="{ 'grid-view': curView === 'grid' }">
                                <div
                                    v-for="dish in section.dishes"
                                    :key="dish.productId"
                                    class="dish-row mb-3"
                                    :class="{ 'dish-row-grid': curView === 'grid' }"
                                    @click="openDetail(dish)"
                                >
                                    <img
                                        v-if="dish.imageUrl && !imgErrors.has(dish.productId)"
                                        class="dish-img"
                                        :src="
                                            imgFallback.get(dish.productId) ||
                                            resolveImage(dish.imageUrl)
                                        "
                                        :alt="dish.productName"
                                        @error="handleImgError(dish)"
                                    />
                                    <div v-else class="dish-img dish-img-placeholder">🍽️</div>
                                    <div
                                        class="dish-content min-w-0"
                                        :class="{ 'dish-content-grid': curView === 'grid' }"
                                    >
                                        <!-- 餐點名稱 -->
                                        <h3
                                            class="font-headline dish-name italic"
                                            style="color: #e3c76b"
                                        >
                                            {{ dish.productName }}
                                        </h3>
                                        <!-- 描述 -->
                                        <p
                                            class="font-body text-xs italic leading-relaxed"
                                            style="color: rgba(249, 221, 211, 0.5); margin: 0"
                                        >
                                            {{ dish.description }}
                                        </p>
                                        <!-- 列表視圖：badge + 價格 & 按鈕並排 -->
                                        <template v-if="curView !== 'grid'">
                                            <div class="dish-badges">
                                                <span
                                                    v-if="dish.isRecommended"
                                                    class="badge badge-new"
                                                    >推薦</span
                                                >
                                                <span v-if="dish.isPopular" class="badge badge-chef"
                                                    >主廚特選</span
                                                >
                                                <span
                                                    v-if="dish.isVegetarian"
                                                    class="badge badge-veg"
                                                    >素</span
                                                >
                                                <span
                                                    v-if="dish.spicyLevel > 0"
                                                    class="badge badge-spicy"
                                                    >辣</span
                                                >
                                            </div>
                                            <!-- 價格 ＋ 數量按鈕同一列 -->
                                            <div class="list-footer" @click.stop="">
                                                <p
                                                    class="font-label text-xs tracking-wider"
                                                    style="color: #d5b478"
                                                >
                                                    NT$ {{ dish.unitPrice?.toLocaleString() }}
                                                </p>
                                                <div class="qty-col">
                                                    <button
                                                        class="qty-btn"
                                                        @click.stop="handleRemove(dish)"
                                                    >
                                                        −
                                                    </button>
                                                    <span
                                                        class="font-label text-xs qty-num"
                                                        :class="{
                                                            active:
                                                                (store.cart[dish.productId] || 0) >
                                                                0,
                                                        }"
                                                        style="font-size: 20px"
                                                        >{{ store.cart[dish.productId] || 0 }}</span
                                                    >
                                                    <button
                                                        class="qty-btn"
                                                        @click.stop="openDetail(dish)"
                                                    >
                                                        +
                                                    </button>
                                                </div>
                                            </div>
                                        </template>
                                    </div>
                                    <!-- 網格視圖：badge → 價格 → 按鈕，全部固定在底部置中 -->
                                    <div
                                        v-if="curView === 'grid'"
                                        class="grid-footer"
                                        @click.stop=""
                                    >
                                        <div class="dish-badges" style="justify-content: center">
                                            <span v-if="dish.isRecommended" class="badge badge-new"
                                                >推薦</span
                                            >
                                            <span v-if="dish.isPopular" class="badge badge-chef"
                                                >主廚特選</span
                                            >
                                            <span v-if="dish.isVegetarian" class="badge badge-veg"
                                                >素</span
                                            >
                                            <span
                                                v-if="dish.spicyLevel > 0"
                                                class="badge badge-spicy"
                                                >辣</span
                                            >
                                        </div>
                                        <p class="font-label grid-price mb-2">
                                            NT$ {{ dish.unitPrice?.toLocaleString() }}
                                        </p>
                                        <div class="qty-row">
                                            <button
                                                class="qty-btn"
                                                @click.stop="handleRemove(dish)"
                                            >
                                                −
                                            </button>
                                            <span
                                                class="font-label text-xs qty-num"
                                                :class="{
                                                    active: (store.cart[dish.productId] || 0) > 0,
                                                }"
                                                style="font-size: 20px"
                                                >{{ store.cart[dish.productId] || 0 }}</span
                                            >
                                            <button class="qty-btn" @click.stop="openDetail(dish)">
                                                +
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="feather-divider my-5"></div>
                        </div>
                    </template>

                    <!-- 歷史訂單 -->
                    <template v-if="activeSidebarCat === '歷史訂單'">
                        <h2 class="section-title">歷史訂單</h2>
                        <div
                            v-for="order in orderHistory"
                            :key="order.orderNumber"
                            class="history-card"
                        >
                            <div class="history-meta">
                                <span
                                    class="font-label"
                                    style="color: rgba(208, 197, 181, 0.5); font-size: 0.7rem"
                                    >訂單編號：{{ order.orderNumber }}</span
                                >
                                <span
                                    class="font-label"
                                    style="color: rgba(208, 197, 181, 0.5); font-size: 0.7rem"
                                >
                                    下單時間：{{
                                        new Date(order.orderAt).toLocaleString('zh-TW', {
                                            year: 'numeric',
                                            month: '2-digit',
                                            day: '2-digit',
                                            hour: '2-digit',
                                            minute: '2-digit',
                                        })
                                    }}
                                </span>
                            </div>
                            <div class="history-items">
                                <div
                                    v-for="item in order.items?.filter((i) => i)"
                                    :key="item.productName"
                                    class="font-body"
                                    style="color: #f9ddd3; font-size: 0.9rem"
                                >
                                    {{ item.qty }} x {{ item.productName }}
                                    <span
                                        v-if="item.note"
                                        style="color: rgba(208, 197, 181, 0.5); font-size: 0.8rem"
                                        >（{{ item.note }}）</span
                                    >
                                </div>
                                <!-- 整單備註 -->
                                <div
                                    v-if="order.orderNote"
                                    class="font-body"
                                    style="
                                        color: rgba(208, 197, 181, 0.5);
                                        font-size: 0.8rem;
                                        margin-top: 0.25rem;
                                    "
                                >
                                    備註：{{ order.orderNote }}
                                </div>
                            </div>
                            <button class="history-reorder-btn font-label" @click="reorder(order)">
                                再點一次
                            </button>
                        </div>
                    </template>
                </div>
            </main>

            <!-- ── RIGHT: Order Panel ── -->
            <aside class="order-panel" :class="{ 'panel-open': orderPanelOpen }">
                <!-- 標題列 -->
                <div class="panel-header candle-glow">
                    <span class="font-headline" style="color: #e3c76b; font-size: 1.1rem"
                        >我的訂單</span
                    >
                    <div style="display: flex; align-items: center; gap: 0.75rem">
                        <span
                            class="font-label"
                            style="
                                color: rgba(208, 197, 181, 0.5);
                                font-size: 0.75rem;
                                letter-spacing: 0.12em;
                            "
                            >共 {{ store.totalItems }} 項</span
                        >
                        <button @click="orderPanelOpen = false" class="panel-close-btn">✕</button>
                    </div>
                </div>

                <!-- 餐點清單（可捲動） -->
                <div class="panel-items">
                    <div
                        v-if="store.totalItems === 0"
                        style="text-align: center; padding: 3rem 1rem"
                    >
                        <p style="font-size: 2.5rem; opacity: 0.15; margin-bottom: 0.75rem">🍽️</p>
                        <p class="font-body" style="color: rgba(249, 221, 211, 0.4)">
                            尚未點選任何餐點
                        </p>
                        <p
                            class="font-label"
                            style="
                                color: rgba(208, 197, 181, 0.35);
                                font-size: 0.7rem;
                                letter-spacing: 0.18em;
                                text-transform: uppercase;
                                margin-top: 0.5rem;
                            "
                        >
                            點擊料理即可加入
                        </p>
                    </div>
                    <div
                        v-for="item in cartItemsWithDetails"
                        :key="item.productId"
                        class="order-item"
                    >
                        <div class="order-item-top">
                            <p class="font-headline order-item-name" style="color: #e3c76b">
                                {{ item.productName }}
                            </p>
                            <div class="order-item-right">
                                <span class="font-label order-item-price"
                                    >NT$ {{ (item.unitPrice * item.qty).toLocaleString() }}</span
                                >
                                <button
                                    class="qty-btn-order"
                                    @click="store.removeItem(item.productId)"
                                >
                                    −
                                </button>
                                <span class="font-label order-item-qty" style="color: #f9ddd3">{{
                                    item.qty
                                }}</span>
                                <button
                                    class="qty-btn-order"
                                    @click="store.addItem(item.productId)"
                                >
                                    +
                                </button>
                            </div>
                        </div>
                        <p v-if="store.notes[item.productId]" class="font-body order-item-note">
                            {{ store.notes[item.productId] }}
                        </p>
                    </div>
                </div>

                <!-- 固定底部：備註 + 合計 + 按鈕 -->
                <div class="panel-footer">
                    <div
                        style="
                            padding: 0.75rem 1rem;
                            border-bottom: 1px solid rgba(77, 70, 58, 0.2);
                        "
                    >
                        <textarea
                            v-model="store.specialRequest"
                            class="note-textarea font-body resize-none"
                            rows="2"
                            placeholder="備註：過敏食材、特殊需求…"
                        ></textarea>

                        <!-- 訪客：已符合門檻的自動活動 → 登入即享提示 -->
                        <div
                            v-if="total > 0 && !isLoggedIn && autoEvents.length"
                            class="notify-events"
                        >
                            <div
                                v-for="ev in autoEvents"
                                :key="'guest-auto-' + ev.id"
                                class="notify-event-card eligible guest-login-hint"
                            >
                                <div class="notify-event-top">
                                    <span class="notify-event-icon">⭐</span>
                                    <span class="font-label notify-event-title">{{
                                        ev.title
                                    }}</span>
                                    <span class="notify-event-badge font-label eligible-badge"
                                        >已符合門檻</span
                                    >
                                </div>
                                <p class="font-body notify-event-desc">
                                    登入即享限時優惠：{{ ev.discountDescription }}
                                </p>
                            </div>
                        </div>

                        <!-- 快到門檻提示（差額 ≤ 100 的自動活動） -->
                        <div v-if="total > 0 && nearAutoEvents.length" class="notify-events">
                            <div
                                v-for="ev in nearAutoEvents"
                                :key="'near-' + ev.id"
                                class="notify-event-card near-threshold"
                            >
                                <div class="notify-event-top">
                                    <span class="notify-event-icon">🔥</span>
                                    <span class="font-label notify-event-title">{{
                                        ev.title
                                    }}</span>
                                    <span class="notify-event-badge font-label near-badge">
                                        差 NT${{ ev.minSpend - total }}
                                    </span>
                                </div>
                                <p class="font-body notify-event-desc">
                                    再消費 NT${{ ev.minSpend - total }} 即可享{{
                                        isLoggedIn ? '' : '（登入後）'
                                    }}限時優惠：{{ ev.discountDescription }}
                                </p>
                            </div>
                        </div>

                        <!-- 通知型活動（IsAutoDiscount=0）→ 改用 Modal 通知，見下方 Teleport -->

                        <!-- 優惠券輸入 -->
                        <div style="display: flex; gap: 0.5rem; margin-top: 0.5rem">
                            <input
                                v-model="couponCode"
                                type="text"
                                class="input-line font-body"
                                style="flex: 1; font-size: 0.85rem"
                                placeholder="輸入優惠券代碼"
                            />
                            <button
                                @click="applyCoupon"
                                class="font-label"
                                style="
                                    padding: 0.4rem 0.75rem;
                                    background: transparent;
                                    border: 1px solid rgba(227, 199, 107, 0.5);
                                    color: #e3c76b;
                                    border-radius: 0.25rem;
                                    font-size: 0.75rem;
                                    letter-spacing: 0.1em;
                                    cursor: pointer;
                                    white-space: nowrap;
                                "
                            >
                                套用
                            </button>
                        </div>
                        <p
                            v-if="couponMsg"
                            class="font-label mb-1"
                            style="font-size: 0.7rem; margin-top: 0.25rem"
                            :style="{ color: couponOk ? '#a3d977' : '#ffb4ab' }"
                        >
                            {{ couponMsg }}
                        </p>
                    </div>
                    <div
                        style="
                            padding: 0.75rem 1rem;
                            display: flex;
                            flex-direction: column;
                            gap: 0.5rem;
                        "
                    >
                        <div style="display: flex; flex-direction: column; gap: 0.3rem">
                            <!-- 自動活動折扣列（已登入會員才顯示） -->
                            <template v-if="isLoggedIn">
                                <div
                                    v-for="ev in autoEvents"
                                    :key="ev.id"
                                    style="
                                        display: flex;
                                        justify-content: space-between;
                                        align-items: center;
                                    "
                                >
                                    <span
                                        class="font-label"
                                        style="
                                            font-size: 0.8rem;
                                            color: rgba(227, 199, 107, 0.75);
                                            letter-spacing: 0.05em;
                                        "
                                    >
                                        🎉 {{ ev.title }}
                                    </span>
                                    <span
                                        class="font-label"
                                        style="
                                            font-size: 0.8rem;
                                            color: #e3c76b;
                                            letter-spacing: 0.06em;
                                        "
                                    >
                                        {{ ev.discountDescription }}
                                    </span>
                                </div>
                            </template>
                            <!-- 優惠券折扣列 -->
                            <div
                                v-if="couponOk && couponDiscount > 0"
                                style="
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                "
                            >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: rgba(163, 217, 119, 0.8);
                                        letter-spacing: 0.06em;
                                    "
                                    >折扣</span
                                >
                                <span
                                    class="font-label"
                                    style="
                                        font-size: 0.8rem;
                                        color: #a3d977;
                                        letter-spacing: 0.06em;
                                    "
                                    >－ NT$ {{ couponDiscount.toLocaleString() }}</span
                                >
                            </div>
                            <!-- 合計列 -->
                            <div
                                style="
                                    display: flex;
                                    justify-content: space-between;
                                    align-items: center;
                                "
                            >
                                <span
                                    class="font-headline"
                                    style="color: #e3c76b; font-size: 1.1rem"
                                    >合計</span
                                >
                                <span
                                    class="font-label"
                                    style="
                                        color: #e3c76b;
                                        font-size: 1.1rem;
                                        letter-spacing: 0.08em;
                                    "
                                    >NT$ {{ finalTotal.toLocaleString() }}</span
                                >
                            </div>
                        </div>
                        <button
                            @click="submitOrder"
                            :disabled="store.totalItems === 0 || submitting"
                            class="submit-btn font-label"
                            style="
                                display: block;
                                width: 100%;
                                padding: 0.875rem 0;
                                font-size: 1rem;
                                letter-spacing: 0.28em;
                                text-transform: uppercase;
                            "
                        >
                            {{ submitting ? '送出中…' : '送出點餐' }}
                        </button>
                        <button
                            @click="clearAll"
                            class="clear-btn font-label"
                            style="
                                display: block;
                                width: 100%;
                                padding: 0.625rem 0;
                                font-size: 1rem;
                                letter-spacing: 0.1em;
                                text-transform: uppercase;
                            "
                        >
                            清空訂單
                        </button>
                    </div>
                </div>
            </aside>
        </div>

        <!-- ── 手機底部列 ── -->
        <div class="mobile-bottom-bar" @click="orderPanelOpen = true">
            <div class="flex items-center gap-3">
                <span class="bottom-badge">{{ store.totalItems }}</span>
                <span
                    class="font-label text-xs tracking-widest uppercase"
                    style="color: rgba(208, 197, 181, 0.7)"
                    >查看訂單</span
                >
            </div>
            <div class="flex items-center gap-3">
                <span class="font-label text-sm tracking-wide" style="color: #e3c76b"
                    >NT$ {{ finalTotal.toLocaleString() }}</span
                >
                <span class="bottom-cta font-label text-xs tracking-[0.2em] uppercase"
                    >送出點餐</span
                >
            </div>
        </div>

        <!-- 手機 overlay -->
        <div v-if="orderPanelOpen" @click="orderPanelOpen = false" class="mobile-overlay"></div>

        <!-- Notify Events Toast（IsAutoDiscount=0 活動，右下角通知） -->
        <Teleport to="body">
            <div class="notify-toast-stack">
                <TransitionGroup name="notify-toast">
                    <div
                        v-for="toast in notifyToasts"
                        :key="toast.key"
                        :class="['notify-toast-card', toast.ev.isEligible ? 'eligible' : 'near']"
                    >
                        <button class="notify-toast-close" @click="dismissToast(toast.key)">✕</button>
                        <div class="notify-toast-icon">{{ toast.ev.isEligible ? '🎁' : '🔥' }}</div>
                        <p class="font-body notify-toast-msg">
                            {{
                                toast.ev.isEligible
                                    ? `金額已達門檻，可參加「${toast.ev.title}」活動！`
                                    : `再消費 NT$${toast.ev.minSpend - total} 即可參加「${toast.ev.title}」活動！`
                            }}
                        </p>
                    </div>
                </TransitionGroup>
            </div>
        </Teleport>

        <!-- Auth Modal -->
        <AuthModal
            v-if="authModalVisible"
            :initial-step="authModalInitStep"
            @guest="onGuestOrder"
            @logged-in="onLoggedIn"
        />

        <!-- Detail Modal -->
        <DishDetailModal
            :dish="activeDetail"
            @close="activeDetail = null"
            @confirm="onDetailConfirm"
        />

        <!-- Success Modal -->
        <OrderSuccessModal
            :visible="successModalOpen"
            :order-number="orderNumber"
            :table-label="tables.find((t) => t.id === tableId)?.tableName || String(tableId)"
            @close="onSuccessClose"
        />

        <!-- Pax Error Alert -->
        <Teleport to="body">
            <Transition name="pax-alert">
                <div v-if="paxError" class="pax-alert-wrap">
                    <div class="pax-alert-box">
                        <span class="pax-alert-icon">⚠</span>
                        <div class="pax-alert-text">
                            <p class="pax-alert-title">請填寫用餐人數</p>
                            <p class="pax-alert-sub">點頂部「＋」設定人數後再送出</p>
                        </div>
                    </div>
                </div>
            </Transition>
        </Teleport>

        <!-- Toast -->
        <div
            class="toast-wrap"
            :style="
                toastVisible
                    ? 'opacity:1;transform:translateX(-50%) translateY(0)'
                    : 'opacity:0;pointer-events:none;transform:translateX(-50%) translateY(10px)'
            "
        >
            <div
                class="px-5 py-3 shadow-xl flex items-center gap-3"
                style="background: #362620; border: 1px solid rgba(77, 70, 58, 0.4)"
            >
                <span style="color: #e3c76b">+</span>
                <span class="font-label text-xs tracking-widest uppercase" style="color: #f9ddd3">{{
                    toastMsg
                }}</span>
            </div>
        </div>
    </div>
    <!-- end root-wrap -->
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useOrderStore } from '@/stores/order'
import apiFetch from '@/utils/apiFetch'
import DishDetailModal from '@/components/Order/DishDetailModal.vue'
import OrderSuccessModal from '@/components/Order/OrderSuccessModal.vue'
import AuthModal from '@/components/Order/OrderAuthModal.vue'

const route = useRoute()
const store = useOrderStore()
const tableId = ref('')
const memberName = ref('訪客') // TODO: 串接登入資訊後替換

const imgErrors = reactive(new Set()) // 圖片完全載入失敗的 productId
const imgFallback = reactive(new Map()) // productId → fallback URL（jpg→png）
const products = ref([])
const tables = ref([]) // 桌位清單
const loading = ref(true)
const loadError = ref('')
const searchQuery = ref('')
const activeChip = ref('all')
const activeSidebarCat = ref('')
const curView = ref('list')
const orderPanelOpen = ref(false)
const activeDetail = ref(null)
const successModalOpen = ref(false)
const orderNumber = ref('')
const submitting = ref(false)
const toastVisible = ref(false)
const toastMsg = ref('')
let toastTimer = null
const paxError = ref(false)
let paxErrorTimer = null

// ── 桌位狀態：'checking' | 'occupied' | 'empty' | 'reserved' | 'not-found'
// 若 URL 沒有 ?table= 參數則跳過驗證（讓服務人員可直接開頁面選桌）
const tableStatus = ref(route.query.table ? 'checking' : 'no-table')
const tableNameFromUrl = route.query.table || '' // e.g. "B3"

// ── 身份驗證
const authModalVisible = ref(false)
const authModalInitStep = ref('choice') // 'choice' | 'login'
const memberDropdownOpen = ref(false) // Header 會員下拉選單
const isLoggedIn = ref(false) // -----前台點餐頁用----- 是否已登入會員
const currentMemberId = ref(null) // -----前台點餐頁用----- 當前會員 ID（訪客為 null）

// ── 會員收藏
const favoriteProducts = ref([])

// async function loadFavorites() {
//   try {
//     const res = await apiFetch('/Orders/Favorites');
//     if (res.ok) favoriteProducts.value = await res.json();
//   } catch {}
// }
// 先這樣
async function loadFavorites(memberId = null) {
    try {
        const url = memberId ? `/Orders/Favorites?memberId=${memberId}` : '/Orders/Favorites'
        const res = await apiFetch(url)
        if (res.ok) favoriteProducts.value = await res.json()
    } catch {}
}

// 歷史訂單
const orderHistory = ref([])

async function loadOrderHistory(memberId = null) {
    try {
        const url = memberId
            ? `/Orders/MemberOrderHistory?memberId=${memberId}`
            : '/Orders/MemberOrderHistory'
        const res = await apiFetch(url)
        if (res.ok) orderHistory.value = await res.json()
    } catch {}
}

const couponCode = ref('')
const couponMsg = ref('')
const couponOk = ref(false)
const couponId = ref(null)
const couponDiscount = ref(0) // 實際折扣金額

function resetCoupon() {
    couponCode.value = ''
    couponMsg.value = ''
    couponOk.value = false
    couponId.value = null
    couponDiscount.value = 0
}

function clearAll() {
    store.clearOrder()
    resetCoupon()
}

async function applyCoupon() {
    if (!couponCode.value.trim()) return
    couponOk.value = false
    couponId.value = null
    couponDiscount.value = 0
    couponMsg.value = ''
    try {
        const res = await apiFetch('/Orders/ValidateCoupon', {
            method: 'POST',
            body: JSON.stringify({
                code: couponCode.value.trim().toUpperCase(),
                orderAmount: total.value,
            }),
        })
        const data = await res.json()
        if (data.isValid) {
            couponMsg.value = data.message || `折抵 NT$ ${data.discount}`
            couponOk.value = true
            couponId.value = data.couponId
            couponDiscount.value = data.discount ?? 0
        } else {
            couponMsg.value = data.message || '優惠券無效或不符條件'
        }
    } catch {
        couponMsg.value = '驗證失敗，請稍後再試'
    }
}

// ── 身份驗證：嘗試用現有 cookie token 驗證，失敗則顯示 Modal ──
// TODO: 之後接組員的會員登入頁，改回真實 JWT 驗證
async function checkAuth() {
    try {
        const res = await apiFetch('/Auth/Me')
        if (res.ok) {
            const data = await res.json()
            memberName.value = data.name || '會員'
            loadFavorites()
            loadOrderHistory()
        } else {
            // 401 → token 無效，顯示選擇 Modal
            authModalInitStep.value = 'choice'
            authModalVisible.value = true
        }
    } catch {
        // 網路異常也顯示 Modal（讓使用者選訪客）
        authModalInitStep.value = 'choice'
        authModalVisible.value = true
    }
}

function onGuestOrder() {
    authModalVisible.value = false
    memberName.value = '訪客'
    isLoggedIn.value = false
    currentMemberId.value = null
}

function onLoggedIn(data) {
    authModalVisible.value = false
    memberName.value = data.name || data.memberName || data.email || '會員'
    isLoggedIn.value = true
    currentMemberId.value = data.memberId ?? null
    loadFavorites(data.memberId)
    loadOrderHistory(data.memberId)
}

function openSwitchAccount() {
    memberDropdownOpen.value = false
    authModalInitStep.value = 'login' // 直接跳登入表單
    authModalVisible.value = true
}

// ── 此頁為全螢幕版面，移除全域 body padding（Navbar 已隱藏）──
onMounted(async () => {
    document.body.style.paddingTop = '0'

    // 先驗證身份（非同步，不阻塞菜單載入）
    checkAuth()

    try {
        const [menuRes, tablesRes] = await Promise.all([
            apiFetch('/Orders/MenuItems'),
            apiFetch('/Orders/Tables'),
        ])
        if (!menuRes.ok) throw new Error(`HTTP ${menuRes.status}`)
        products.value = await menuRes.json()
        if (tablesRes.ok) tables.value = await tablesRes.json()

        // ── 桌位狀態驗證（只在有 ?table= 時執行）──
        if (tableNameFromUrl) {
            const matched = tables.value.find(
                (t) => t.tableName?.toLowerCase() === tableNameFromUrl.toLowerCase()
            )
            if (!matched) {
                tableStatus.value = 'not-found'
            } else if (matched.status === 1) {
                tableStatus.value = 'occupied'
                tableId.value = matched.id // 自動帶入桌號
            } else if (matched.status === 2) {
                tableStatus.value = 'reserved'
            } else {
                tableStatus.value = 'empty' // 0 或其他皆視為空桌
            }
        }

        // 預設選第一個 sidebar 分類（優先今日推薦，否則第一個一般分類）
        activeSidebarCat.value = sidebarCategories.value[0]?.key ?? ''
    } catch (err) {
        loadError.value = '菜單載入失敗，請稍後再試。'
        console.error(err)
    } finally {
        loading.value = false
    }
})

onUnmounted(() => {
    document.body.style.paddingTop = '' // 離開頁面時還原
})

function handleImgError(dish) {
    const current = imgFallback.get(dish.productId) || resolveImage(dish.imageUrl)
    if (/\.jpg$/i.test(current)) {
        // jpg 失敗 → 先試 png
        imgFallback.set(dish.productId, current.replace(/\.jpg$/i, '.png'))
    } else {
        // png 也失敗（或本來就不是 jpg）→ 顯示 placeholder
        imgErrors.add(dish.productId)
    }
}

function resolveImage(url) {
    if (!url) return ''
    let path = url.replace(/\\/g, '/').replace(/^~\//, '')
    const match = /\/wwwroot\/(.*)/i.exec('/' + path)
    if (match?.[1]) path = match[1]
    return `/${path.replace(/^\//, '')}`
}

// 一般分類排序（特殊的今日推薦/主廚特選不在此列）
const CATEGORY_ORDER = ['套餐', '主餐', '湯品', '甜點', '附餐', '飲料']

const sidebarCategories = computed(() => {
    // ── 特殊分類（依資料有無顯示）──
    const specials = [
        {
            key: '今日推薦',
            label: '今日推薦',
            count: products.value.filter((p) => p.isRecommended).length,
        },
        {
            key: '主廚特選',
            label: '主廚特選',
            count: products.value.filter((p) => p.isPopular).length,
        },
        { key: '我的收藏', label: '我的收藏', count: favoriteProducts.value.length },
        { key: '歷史訂單', label: '歷史訂單', count: orderHistory.value.length },
    ].filter((s) => s.count > 0)

    // ── 一般分類 ──
    const map = new Map()
    products.value.forEach((p) => {
        const cat = p.categoryName
        if (!cat) return
        if (!map.has(cat)) map.set(cat, { key: cat, label: cat, count: 0 })
        map.get(cat).count++
    })
    const cats = Array.from(map.values()).sort((a, b) => {
        const ai = CATEGORY_ORDER.indexOf(a.key),
            bi = CATEGORY_ORDER.indexOf(b.key)
        return (ai < 0 ? 99 : ai) - (bi < 0 ? 99 : bi)
    })

    // 全部：放在分隔線後、一般分類前
    const allEntry = { key: '全部', label: '全部', count: products.value.length }

    return [...specials, allEntry, ...cats]
})

const filteredProducts = computed(() => {
    let r = products.value
    if (searchQuery.value) {
        const t = searchQuery.value.toLowerCase()
        r = r.filter((p) => p.productName?.toLowerCase().includes(t))
    }
    // 只剩素食/辣味 chip 篩選
    if (activeChip.value === 'veg') r = r.filter((p) => p.isVegetarian)
    if (activeChip.value === 'spicy') r = r.filter((p) => p.spicyLevel > 0)
    return r
})

const displaySections = computed(() => {
    // 收藏分類
    if (activeSidebarCat.value === '我的收藏') {
        if (!favoriteProducts.value.length) return []
        return [{ key: '我的收藏', label: '我的收藏', dishes: favoriteProducts.value }]
    }
    // 再點一次
    if (activeSidebarCat.value === '歷史訂單') {
        if (!orderHistory.value.length) return []
        return [{ key: '歷史訂單', label: '歷史訂單', dishes: [], orders: orderHistory.value }]
    }
    // 搜尋模式：跨所有分類
    if (searchQuery.value.trim()) {
        const dishes = filteredProducts.value
        if (!dishes.length) return []
        return [{ key: 'search', label: `搜尋：${searchQuery.value}`, dishes }]
    }
    // 特殊分類
    if (activeSidebarCat.value === '今日推薦') {
        const dishes = filteredProducts.value.filter((p) => p.isRecommended)
        if (!dishes.length) return []
        return [{ key: '今日推薦', label: '今日推薦', dishes }]
    }
    if (activeSidebarCat.value === '主廚特選') {
        const dishes = filteredProducts.value.filter((p) => p.isPopular)
        if (!dishes.length) return []
        return [{ key: '主廚特選', label: '主廚特選', dishes }]
    }
    // 全部：按分類順序分組顯示
    if (activeSidebarCat.value === '全部') {
        return CATEGORY_ORDER.map((cat) => ({
            key: cat,
            label: cat,
            dishes: filteredProducts.value.filter((p) => p.categoryName === cat),
        })).filter((s) => s.dishes.length > 0)
    }
    // 一般分類
    const dishes = filteredProducts.value.filter((p) => p.categoryName === activeSidebarCat.value)
    if (!dishes.length) return []
    return [{ key: activeSidebarCat.value, label: activeSidebarCat.value, dishes }]
})

const cartItemsWithDetails = computed(() =>
    store.cartItems
        .map(({ productId, qty }) => {
            const d = products.value.find((p) => p.productId === productId)
            return d ? { ...d, qty } : null
        })
        .filter(Boolean)
)

const total = computed(() =>
    cartItemsWithDetails.value.reduce((s, i) => s + i.unitPrice * i.qty, 0)
)

// ── 活動 ──────────────────────────────────────────────────────────
const autoEvents     = ref([]) // IsAutoDiscount=1，已達門檻，自動折扣
const notifyEvents   = ref([]) // IsAutoDiscount=0，通知型活動
const nearAutoEvents = ref([]) // IsAutoDiscount=1，差額 ≤ 100，快到門檻提示

// ── Notify Toast（IsAutoDiscount=0 活動，右下角通知）──
const notifyToasts       = ref([])       // [{ key, ev }]
const _shownNotifyStates = new Map()     // id → 'near' | 'eligible'，狀態改變才再跳
let   _toastKeySeq       = 0

function dismissToast(key) {
    notifyToasts.value = notifyToasts.value.filter(t => t.key !== key)
}

const autoEventDiscount = computed(() =>
    isLoggedIn.value ? autoEvents.value.reduce((s, e) => s + (e.calculatedDiscount ?? 0), 0) : 0
)

async function fetchActiveEvents() {
    try {
        const res = await apiFetch(`/Orders/ActiveEvents?amount=${total.value}`)
        if (!res.ok) return
        const data = await res.json()
        autoEvents.value     = data.autoEvents     ?? []
        notifyEvents.value   = data.notifyEvents   ?? []
        nearAutoEvents.value = data.nearAutoEvents ?? []

        // 找出符合條件 or 差額≤100，且狀態與上次不同的 notify 活動才彈
        const newNotify = notifyEvents.value.filter(ev => {
            const isRelevant   = ev.isEligible || (ev.minSpend - total.value) <= 100
            if (!isRelevant) return false
            const currentState = ev.isEligible ? 'eligible' : 'near'
            return _shownNotifyStates.get(ev.id) !== currentState // 狀態改變才彈
        })
        if (newNotify.length) {
            newNotify.forEach(ev => {
                _shownNotifyStates.set(ev.id, ev.isEligible ? 'eligible' : 'near')
                const key = ++_toastKeySeq
                notifyToasts.value.push({ key, ev: { ...ev } })
                // 8 秒後自動消失
                setTimeout(() => dismissToast(key), 8000)
            })
        }
    } catch {
        /* 靜默失敗 */
    }
}

watch(total, (val) => {
    if (val > 0) {
        fetchActiveEvents()
    } else {
        autoEvents.value     = []
        notifyEvents.value   = []
        nearAutoEvents.value = []
    }
})

const finalTotal = computed(() =>
    Math.max(0, total.value - couponDiscount.value - autoEventDiscount.value)
)

function handleAdd(dish) {
    store.addItem(dish.productId)
    showToast(`「${dish.productName}」已加入訂單`)
}
function handleRemove(dish) {
    store.removeItem(dish.productId)
}
// 素食/辣味 chip 點同一個再點可取消
function toggleChip(c) {
    activeChip.value = activeChip.value === c ? 'all' : c
}

function scrollToSection(cat) {
    activeSidebarCat.value = cat
    searchQuery.value = ''
    // 切換分類後捲回頂端
    document.getElementById('menuMain')?.scrollTo({ top: 0, behavior: 'smooth' })
}

function openDetail(dish) {
    activeDetail.value = dish
}
function onDetailConfirm(dish, qty, note) {
    for (let i = 0; i < qty; i++) store.addItem(dish.productId)
    store.setNote(dish.productId, note)
    showToast(`「${dish.productName}」× ${qty} 已加入訂單`)
    activeDetail.value = null
}

async function submitOrder() {
    if (store.totalItems === 0 || submitting.value) return
    if (!tableId.value) {
        showToast('請先輸入桌號')
        return
    }
    if (!store.pax || store.pax < 1) {
        showPaxError()
        return
    }
    submitting.value = true
    try {
        const body = {
            tableId: Number(tableId.value),
            inOrOut: true,
            peopleNum: store.pax,
            isAddOrder: false,
            payMethod: 'Cash',
            note: store.specialRequest || null,
            couponId: couponId.value,
            discountAmount: couponOk.value ? couponDiscount.value : 0,
            items: cartItemsWithDetails.value.map((i) => ({
                productId: i.productId,
                productName: i.productName,
                qty: i.qty,
                unitPrice: i.unitPrice,
                isSetMeal: i.isSetMeal ?? false,
                note: store.notes[i.productId] || null,
            })),
        }
        const res = await apiFetch('/Orders/CreatePreOrder', {
            method: 'POST',
            body: JSON.stringify(body),
        })
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        const data = await res.json()
        orderNumber.value = data.orderNumber
        store.clearOrder() // 送出成功立即清空購物車與備註
        resetCoupon()
        successModalOpen.value = true
        orderPanelOpen.value = false
    } catch (err) {
        console.error(err)
        showToast('送出失敗，請稍後再試')
    } finally {
        submitting.value = false
    }
}

function onSuccessClose() {
    successModalOpen.value = false
}

function showToast(msg) {
    toastMsg.value = msg
    toastVisible.value = true
    clearTimeout(toastTimer)
    toastTimer = setTimeout(() => {
        toastVisible.value = false
    }, 2500)
}

function showPaxError() {
    paxError.value = true
    clearTimeout(paxErrorTimer)
    paxErrorTimer = setTimeout(() => {
        paxError.value = false
    }, 2800)
}

function reorder(order) {
    order.items.forEach((item) => {
        const matched = products.value.find((p) => p.productName === item.productName)
        if (matched) {
            store.cart[matched.productId] = (store.cart[matched.productId] || 0) + item.qty
        }
    })
    activeSidebarCat.value = '全部'
    showToast('已加入購物車')
}
</script>

<style>
/* 內用點餐頁面：清除全域 padding，鎖死頁面高度（不讓 body 出捲軸）*/
html:has(.root-wrap),
body:has(.root-wrap) {
    padding-top: 0 !important;
    overflow: hidden !important;
    height: 100% !important;
}
/* 內用頁面隱藏全站 Navbar / Footer（掃碼點餐不需要） */
html:has(.root-wrap) nav.navbar,
html:has(.root-wrap) footer,
html:has(.gate-wrap) nav.navbar,
html:has(.gate-wrap) footer {
    display: none !important;
}

/* ═══ Pax Error Alert（Teleport to body，必須全域） ═══ */
.pax-alert-wrap {
    position: fixed;
    top: 5rem;
    left: 50%;
    transform: translateX(-50%);
    z-index: 9999;
    pointer-events: none;
}
.pax-alert-box {
    display: flex;
    align-items: center;
    gap: 0.9rem;
    padding: 0.95rem 1.5rem;
    background: #1f1007;
    border: 2px solid #ffb347;
    border-radius: 0.65rem;
    box-shadow:
        0 0 0 4px rgba(255, 179, 71, 0.12),
        0 12px 40px rgba(0, 0, 0, 0.7);
    white-space: nowrap;
}
.pax-alert-icon {
    font-size: 1.75rem;
    line-height: 1;
    color: #ffb347;
    flex-shrink: 0;
    animation: pax-icon-pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}
@keyframes pax-icon-pop {
    0% {
        transform: scale(0.4) rotate(-15deg);
        opacity: 0;
    }
    100% {
        transform: scale(1) rotate(0deg);
        opacity: 1;
    }
}
.pax-alert-text {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
}
.pax-alert-title {
    font-size: 1rem;
    font-style: italic;
    font-weight: 600;
    color: #ffb347;
    margin: 0;
    letter-spacing: 0.05em;
}
.pax-alert-sub {
    font-size: 0.7rem;
    letter-spacing: 0.1em;
    color: rgba(249, 221, 211, 0.5);
    margin: 0;
}

/* Transition */
.pax-alert-enter-active {
    transition: all 0.28s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.pax-alert-leave-active {
    transition: all 0.35s ease;
}
.pax-alert-enter-from {
    opacity: 0;
    transform: translateX(-50%) translateY(-16px) scale(0.88);
}
.pax-alert-leave-to {
    opacity: 0;
    transform: translateX(-50%) translateY(-8px) scale(0.95);
}

/* ═══ Notify Toast（右下角堆疊通知，Teleport to body） ═══ */
.notify-toast-stack {
    position: fixed;
    bottom: 1.5rem;
    right: 1.25rem;
    z-index: 8500;
    display: flex;
    flex-direction: column-reverse;
    gap: 0.6rem;
    max-width: 300px;
    pointer-events: none;
}
.notify-toast-card {
    position: relative;
    border-radius: 0.6rem;
    padding: 0.75rem 2.2rem 0.75rem 1rem;
    display: flex;
    align-items: flex-start;
    gap: 0.55rem;
    box-shadow: 0 6px 24px rgba(0, 0, 0, 0.45);
    pointer-events: all;
}
.notify-toast-card.eligible {
    background: #1e2d1a;
    border: 1px solid rgba(163, 217, 119, 0.45);
    border-left: 3px solid #a3d977;
}
.notify-toast-card.near {
    background: #2a1e10;
    border: 1px solid rgba(255, 160, 80, 0.45);
    border-left: 3px solid #ff9f4a;
}
.notify-toast-icon { font-size: 1.1rem; flex-shrink: 0; margin-top: 0.05rem; }
.notify-toast-msg {
    font-size: 0.78rem;
    line-height: 1.5;
    margin: 0;
    color: #f9ddd3;
}
.notify-toast-card.eligible .notify-toast-msg { color: #c4eda0; }
.notify-toast-card.near     .notify-toast-msg { color: #ffd0a0; }
.notify-toast-close {
    position: absolute;
    top: 0.4rem;
    right: 0.5rem;
    background: none;
    border: none;
    color: rgba(208, 197, 181, 0.35);
    font-size: 0.7rem;
    cursor: pointer;
    padding: 0.15rem 0.3rem;
    transition: color 0.2s;
    line-height: 1;
}
.notify-toast-close:hover { color: rgba(208, 197, 181, 0.8); }

/* TransitionGroup */
.notify-toast-enter-active { transition: opacity 0.3s, transform 0.3s; }
.notify-toast-leave-active  { transition: opacity 0.25s, transform 0.25s; }
.notify-toast-enter-from    { opacity: 0; transform: translateX(40px); }
.notify-toast-leave-to      { opacity: 0; transform: translateX(40px); }
.notify-toast-leave-active  { position: absolute; }
</style>

<style scoped>
/* ════════════════════════════════════════════
   桌位狀態封鎖畫面
   ════════════════════════════════════════════ */
.gate-wrap {
    min-height: 100vh;
    background: #1e100b;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 1rem;
    padding: 2rem;
    text-align: center;
}
.gate-icon {
    font-size: 3.5rem;
    opacity: 0.55;
    margin-bottom: 0.5rem;
}
.gate-title {
    font-size: 1.6rem;
    font-style: italic;
    color: #e3c76b;
    margin: 0;
}
.gate-body {
    font-size: 1rem;
    font-style: italic;
    color: rgba(249, 221, 211, 0.6);
    margin: 0;
    max-width: 26rem;
    line-height: 1.7;
}
.gate-sub {
    font-size: 0.75rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.35);
    margin-top: 0.5rem;
}
/* loading spinner */
.gate-spinner {
    width: 36px;
    height: 36px;
    border: 3px solid rgba(227, 199, 107, 0.15);
    border-top-color: #e3c76b;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
    margin-bottom: 0.5rem;
}
@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}
.gate-hint {
    font-size: 0.8rem;
    letter-spacing: 0.18em;
    color: rgba(208, 197, 181, 0.4);
    margin: 0;
}
@import url('https://fonts.googleapis.com/css2?family=Noto+Serif+TC:wght@400;700&family=Newsreader:ital,wght@0,400;0,600;1,400&family=Work+Sans:wght@300;400&family=Cormorant+Garamond:ital,wght@1,400;1,600&display=swap');

.root-wrap {
    background: #1e100b;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
    /* 鎖死整頁：flex 直欄，不讓 body 出捲軸 */
    height: 100vh;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

/* ════════════════════════════════════════════
   全寬頂部 Header
   ════════════════════════════════════════════ */
.dine-header {
    flex-shrink: 0;
    z-index: 60;
    height: 4rem;
    background: #180b06;
    border-bottom: 1px solid rgba(77, 70, 58, 0.4);
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.35);
    display: flex;
    align-items: center;
    padding: 1rem;
    gap: 1rem;
}

/* 左：今日菜單 */
.dh-brand {
    flex-shrink: 0;
}
.dh-title {
    font-size: 2rem;
    color: #e3c76b;
    font-style: italic;
    margin: 0;
    line-height: 1;
}

/* 中：桌號 + 用餐人數，同一行 */
.dh-center {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.6rem;
}
.dh-label {
    font-size: 1rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.5);
    white-space: nowrap;
}
.dh-table-name {
    color: #e3c76b;
    font-size: 0.95rem;
    font-style: italic;
}

/* 分隔線 */
.dh-sep {
    width: 1px;
    height: 1.5rem;
    background: rgba(77, 70, 58, 0.5);
    flex-shrink: 0;
    margin: 0 0.4rem;
}

/* 人數按鈕 */
.dh-pax-btn {
    width: 26px;
    height: 26px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.2rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
}
.dh-pax-btn:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}

/* 人數輸入框 */
.dh-pax-input {
    width: 2.2rem;
    text-align: center;
    background: transparent;
    border: none;
    border-bottom: 1px solid rgba(227, 199, 107, 0.4);
    color: #f9ddd3;
    font-size: 0.95rem;
    outline: none;
    padding: 0;
    -moz-appearance: textfield;
    transition: border-color 0.3s;
}
.dh-pax-input::-webkit-outer-spin-button,
.dh-pax-input::-webkit-inner-spin-button {
    -webkit-appearance: none;
}
.dh-pax-input:focus {
    border-bottom-color: #e3c76b;
}
.dh-pax-input::placeholder {
    color: rgba(208, 197, 181, 0.4);
}

/* 人數未填警示（閒置閃爍） */
.pax-empty {
    border-bottom-color: rgba(227, 199, 107, 0.7);
    animation: pax-pulse 1.6s ease-in-out infinite;
}
@keyframes pax-pulse {
    0%,
    100% {
        border-bottom-color: rgba(227, 199, 107, 0.35);
    }
    50% {
        border-bottom-color: rgba(227, 199, 107, 0.9);
    }
}

/* 送出驗證失敗：搖晃 + 橘色 */
@keyframes pax-shake {
    0%,
    100% {
        transform: translateX(0);
    }
    20% {
        transform: translateX(-6px);
    }
    40% {
        transform: translateX(6px);
    }
    60% {
        transform: translateX(-4px);
    }
    80% {
        transform: translateX(4px);
    }
}
.pax-label-error {
    color: #ffb347 !important;
    animation: pax-shake 0.45s ease;
}
.pax-input-error {
    border-bottom-color: #ffb347 !important;
    color: #ffb347 !important;
    animation: pax-shake 0.45s ease;
}
.pax-ctrl-error {
    border-color: #ffb347 !important;
    color: #ffb347 !important;
    animation: pax-shake 0.45s ease;
}

/* 右：會員 */
.dh-member {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    gap: 0.4rem;
    min-width: 9rem;
    white-space: nowrap;
    position: relative;
}
.dh-member-btn {
    cursor: pointer;
    padding: 0.25rem 0.5rem;
    border-radius: 0.375rem;
    transition: background 0.2s;
}
.dh-member-btn:hover {
    background: rgba(227, 199, 107, 0.07);
}
.dh-member-label {
    font-size: 0.9rem;
    letter-spacing: 0.08em;
    color: rgba(208, 197, 181, 0.75);
}
.dh-chevron {
    color: rgba(208, 197, 181, 0.4);
    flex-shrink: 0;
    transition: transform 0.2s;
}
.dh-chevron.open {
    transform: rotate(180deg);
}

/* 下拉選單 */
.member-dropdown {
    position: absolute;
    top: calc(100% + 0.5rem);
    right: 0;
    min-width: 9rem;
    background: #2b1c16;
    border: 1px solid rgba(77, 70, 58, 0.45);
    border-radius: 0.5rem;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
    overflow: hidden;
    z-index: 100;
}
.member-dropdown-item {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.65rem 1rem;
    font-size: 0.8rem;
    letter-spacing: 0.1em;
    color: rgba(208, 197, 181, 0.7);
    background: none;
    border: none;
    cursor: pointer;
    transition:
        background 0.15s,
        color 0.15s;
    text-align: left;
}
.member-dropdown-item:hover {
    background: rgba(227, 199, 107, 0.07);
    color: #e3c76b;
}
.member-backdrop {
    position: fixed;
    inset: 0;
    z-index: 99;
}

/* ═══ 手機分類捲動列 ═══ */
.mobile-header {
    display: none;
    position: sticky;
    top: var(--dine-header-h);
    z-index: 50;
    background: #180b06;
    border-bottom: 1px solid rgba(77, 70, 58, 0.3);
}
.mobile-cat-tabs {
    display: flex;
    overflow-x: auto;
    padding: 0.4rem 0.75rem 0.55rem;
    scrollbar-width: none;
    gap: 0;
}
.mobile-cat-tabs::-webkit-scrollbar {
    display: none;
}
.mobile-cat-btn {
    white-space: nowrap;
    padding: 0.3rem 0.85rem;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.68rem;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.5);
    border: none;
    border-bottom: 2px solid transparent;
    background: transparent;
    cursor: pointer;
    transition: all 0.25s;
    display: flex;
    align-items: center;
    gap: 0.3rem;
}
.mobile-cat-btn.active {
    color: #e3c76b;
    border-bottom-color: #e3c76b;
}
.cat-badge {
    font-size: 0.55rem;
    background: rgba(77, 70, 58, 0.5);
    color: rgba(208, 197, 181, 0.5);
    border-radius: 99px;
    padding: 0.05rem 0.35rem;
}

/* ═══ 三欄佈局 ═══ */
.page-layout {
    display: grid;
    grid-template-columns: 210px 1fr 360px;
    flex: 1; /* 填滿 root-wrap 剩餘高度 */
    min-height: 0; /* flex 子項必須，否則 grid 不縮小 */
    overflow: hidden; /* 整個版面不捲動 */
}

/* ═══ Sidebar ═══ */
.sidebar-left {
    background: #180b06;
    border-right: 1px solid rgba(77, 70, 58, 0.2);
}
@media (min-width: 1101px) {
    .sidebar-left {
        height: 100%; /* 撐滿 grid 格 */
        overflow-y: auto;
    }
}
.cat-link {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.7rem 1.5rem;
    font-family: 'Work Sans', sans-serif;
    font-size: 1.2rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.6);
    border-left: 2px solid transparent;
    border-right: none;
    border-top: none;
    border-bottom: none;
    background: transparent;
    cursor: pointer;
    transition: all 0.3s;
    width: 100%;
}
.cat-link:hover {
    color: #e3c76b;
    background: rgba(227, 199, 107, 0.04);
}
.cat-link.active {
    color: #e3c76b;
    border-left-color: #e3c76b;
    background: rgba(227, 199, 107, 0.06);
}
.cat-count {
    margin-left: auto;
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.35);
}
/* 今日推薦 / 主廚特選 特殊分類的分隔線 */
.cat-link.cat-special {
    color: rgba(227, 199, 107, 0.75);
    font-size: 1rem;
}
.cat-link.cat-special.active {
    color: #e3c76b;
}
.cat-link.cat-special .cat-count {
    color: rgba(227, 199, 107, 0.4);
}
.cat-divider {
    height: 1px;
    background: linear-gradient(90deg, rgba(77, 70, 58, 0.5), transparent);
    margin: 0.35rem 1.5rem;
}

/* ═══ 歷史訂單 ═══ */
.history-card {
    background: rgba(43, 28, 22, 0.6);
    border: 1px solid rgba(77, 70, 58, 0.4);
    border-radius: 0.5rem;
    padding: 1rem 1.25rem;
    margin-bottom: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}
.history-meta {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
}
.history-items {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.history-reorder-btn {
    align-self: flex-end;
    padding: 0.5rem 1.25rem;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    border: none;
    border-radius: 0.375rem;
    font-size: 0.8rem;
    letter-spacing: 0.15em;
    cursor: pointer;
    transition: filter 0.2s;
}
.history-reorder-btn:hover {
    filter: brightness(1.08);
}

/* ═══ Menu main ═══ */
.menu-main {
    background: #1e100b;
}
@media (min-width: 1101px) {
    .menu-main {
        height: 100%; /* 撐滿 grid 格，只有這欄捲動 */
        overflow-y: auto;
    }
}
.menu-sections {
    padding: 0 1.25rem 7rem;
}
@media (min-width: 1101px) {
    .menu-sections {
        padding: 0 1.5rem 2.5rem;
    }
}

/* ═══ Toolbar ═══ */
.toolbar {
    position: sticky;
    top: 0;
    z-index: 20;
    background: #1e100b;
    border-bottom: 1px solid rgba(77, 70, 58, 0.25);
    padding: 1rem 1rem;
}
.mobile-search-wrap {
    display: none;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
}
.toolbar-row {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    flex-wrap: nowrap;
}
.chips-wrap {
    display: flex;
    gap: 0.4rem;
    overflow-x: auto;
    flex: 1 1 0;
    min-width: 0;
    scrollbar-width: none;
}
.chips-wrap::-webkit-scrollbar {
    display: none;
}
.view-toggle {
    display: flex;
    flex-shrink: 0;
}

/* ═══ Order Panel ═══ */
.order-panel {
    pointer-events: auto !important;
    z-index: 50 !important;
    background: #180b06;
    border-left: 1px solid rgba(77, 70, 58, 0.2);
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
}
.panel-header {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(77, 70, 58, 0.2);
}
/* 餐點清單：可捲動，撐滿剩餘空間 */
.panel-items {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    padding: 0.25rem 1rem;
}
/* 底部：備註 + 合計 + 按鈕，固定不捲 */
.panel-footer {
    flex-shrink: 0;
    border-top: 1px solid rgba(77, 70, 58, 0.2);
}
.panel-close-btn {
    display: none;
    background: none;
    border: none;
    cursor: pointer;
    color: rgba(208, 197, 181, 0.7);
    font-size: 1.5rem;
    line-height: 1;
    padding: 0;
}

/* ═══ 手機底部列 ═══ */
.mobile-bottom-bar {
    display: none;
}
.mobile-overlay {
    display: none;
}

/* ═══ Mobile ≤ 1100px ═══ */
@media (max-width: 1100px) {
    /* 手機版解除整頁高度鎖定，讓內容自然流動 */
    html:has(.root-wrap),
    body:has(.root-wrap) {
        overflow: auto !important;
        height: auto !important;
    }
    .root-wrap {
        height: auto;
        overflow: visible;
    }

    /* dine-header 手機版：隱藏品牌區 */
    .dh-brand {
        display: none;
    }
    .dh-member {
        display: none;
    }
    .dh-center {
        justify-content: flex-start;
        gap: 0.75rem;
    }

    .mobile-header {
        display: block;
    }
    .mobile-search-wrap {
        display: flex;
    }
    .page-layout {
        grid-template-columns: 1fr;
        flex: none;
        height: auto;
        overflow: visible;
    }
    .sidebar-left {
        display: none;
    }

    /* Order panel → full-height bottom sheet */
    .order-panel {
        position: fixed;
        inset: 0;
        z-index: 200;
        transform: translateY(100%);
        transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
        border-left: none;
        border-top: 1px solid rgba(77, 70, 58, 0.4);
        pointer-events: auto;
    }
    .order-panel.panel-open {
        transform: translateY(0);
    }
    .order-panel.panel-open * {
        pointer-events: auto;
    }
    .panel-close-btn {
        display: block;
    }

    .mobile-bottom-bar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        position: fixed;
        bottom: 0;
        left: 0;
        right: 0;
        z-index: 80;
        background: linear-gradient(135deg, #2b1c16 0%, #1e100b 100%);
        border-top: 1px solid rgba(77, 70, 58, 0.4);
        padding: 0.85rem 1.25rem;
        cursor: pointer;
        box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.4);
    }
    /* backdrop-filter 會在 Chromium 造成 z-index 失效，改用純背景色 */
    .mobile-overlay {
        display: block;
        position: fixed;
        inset: 0;
        z-index: 100;
        background: rgba(24, 11, 6, 0.78);
        pointer-events: auto;
    }
}

.bottom-badge {
    min-width: 1.5rem;
    height: 1.5rem;
    border-radius: 99px;
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    font-weight: 700;
    display: flex;
    align-items: center;
    justify-content: center;
}
.bottom-cta {
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    padding: 0.45rem 1rem;
    border-radius: 0.125rem;
}

/* ═══ Dish rows ═══ */
.dish-row {
    display: grid;
    grid-template-columns: 160px 1fr; /* 列表視圖：圖片 + 內容（按鈕已併入內容欄） */
    align-items: stretch;
    background: #362620;
    border-radius: 0.75rem;
    cursor: pointer;
    transition:
        transform 0.45s cubic-bezier(0.4, 0, 0.2, 1),
        box-shadow 0.45s ease;
}
.dish-row:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.35);
}
.dish-row.dish-row-grid {
    grid-template-columns: 1fr;
    display: flex;
    flex-direction: column;
    overflow: hidden; /* 讓圓角裁切圖片頂部 */
}
.dish-img {
    width: 160px;
    height: 120px;
    object-fit: cover;
    align-self: center;
    display: block;
    flex-shrink: 0;
    transition: transform 0.7s cubic-bezier(0.4, 0, 0.2, 1);
}
.dish-row:hover .dish-img {
    transform: scale(1.05);
}
.dish-row-grid .dish-img {
    width: 100%;
    height: 140px;
}
.dish-img-placeholder {
    width: 160px;
    height: 120px;
    align-self: center;
    background: #2b1c16;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    color: rgba(208, 197, 181, 0.15);
    font-size: 1.8rem;
}
.dish-row-grid .dish-img-placeholder {
    width: 100%;
    height: 140px;
}
/* 列表視圖：內容欄垂直置中，確保上下間距一致 */
.dish-content {
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding: 0.75rem 0.75rem;
}
/* 網格卡片：內容區填滿，靠上對齊 */
.dish-content-grid {
    flex: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start; /* 靠上，不留空白 */
    text-align: center;
}
/* Badge 列 */
.dish-badges {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    margin: 0.4rem 0;
}
/* 網格視圖：badge 置中 */
.dish-content-grid .dish-badges {
    justify-content: center;
}
/* 網格底部：價格 + 按鈕，固定在卡片底部，置中 */
.grid-footer {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0 0.75rem;
    border-top: 1px solid rgba(77, 70, 58, 0.25);
}
.grid-price {
    color: #d5b478;
    font-size: 1rem;
    letter-spacing: 0.08em;
    text-align: center;
}
/* 列表視圖底列：價格左、按鈕右，同一行 */
.list-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: auto;
    padding-top: 0.4rem;
}
.qty-col {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0.4rem;
}
.qty-row {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 0.4rem;
    margin-bottom: 0.8rem;
    margin-top: auto;
}
.qty-num {
    color: #f9ddd3;
    width: 40px;
    text-align: center;
}
.qty-num.active {
    color: #e3c76b;
}
.dishes-wrap.grid-view {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
    gap: 0.75rem;
}

/* ═══ Misc ═══ */
.candle-glow {
    background: radial-gradient(
        ellipse 80% 60% at 50% 0%,
        rgba(227, 199, 107, 0.1) 0%,
        transparent 70%
    );
}
.feather-divider {
    height: 1px;
    background: linear-gradient(90deg, transparent, #e4c285 50%, transparent);
    position: relative;
}
.feather-divider::after {
    content: '◈';
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
    color: #e4c285;
    font-size: 0.7rem;
    background: #1e100b;
    padding: 0 0.65rem;
}
/* 餐點名稱字體大小 — 在這裡調整 */
.dish-name {
    /* 列表視圖 */
    font-size: 1.4rem;
}
.dish-row-grid .dish-name {
    /* 網格視圖 */
    font-size: 1.285rem;
    text-align: center;
    width: 100%;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.section-title {
    font-family: 'Noto Serif TC', serif;
    font-style: italic;
    font-size: 2rem;
    color: #e3c76b;
    padding-top: 1.5rem;
    margin-bottom: 0.85rem;
}
.status-msg {
    text-align: center;
    padding: 5rem 1rem;
    font-family: 'Newsreader', serif;
    font-style: italic;
    color: rgba(249, 221, 211, 0.4);
}
.order-item {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
    padding: 0.5rem 0;
    border-bottom: 1px solid rgba(77, 70, 58, 0.25);
}
.order-item:last-child {
    border-bottom: none;
}
.order-item-top {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}
.order-item-note {
    font-size: 1rem;
    color: rgba(208, 197, 181, 0.45);
    margin: 0;
    padding-left: 0.1rem;
    white-space: pre-wrap;
    word-break: break-all;
}
.order-item-name {
    margin: 0.2rem 0;
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-size: 1rem;
}
.order-item-right {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    flex-shrink: 0;
}
.order-item-price {
    padding-right: 0.6rem;
    font-size: 1rem;
    letter-spacing: 0.08em;
    color: #d5b478;
    white-space: nowrap;
}
.order-item-qty {
    font-size: 1rem;
    width: 1.5rem;
    text-align: center;
}

/* ═══ Buttons ═══ */
.qty-btn {
    pointer-events: auto !important;
    cursor: pointer !important;
    width: 30px;
    height: 30px;
    border: 1px solid rgba(77, 70, 58, 0.7);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
}
.qty-btn:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
/* 購物車專用 +/- 按鈕（較小） */
.qty-btn-order {
    width: 24px;
    height: 24px;
    border: 1px solid rgba(77, 70, 58, 0.6);
    background: #2b1c16;
    color: #f9ddd3;
    border-radius: 0.125rem;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
    line-height: 1;
    transition:
        border-color 0.3s,
        color 0.3s;
    flex-shrink: 0;
}
.qty-btn-order:hover {
    border-color: #e3c76b;
    color: #e3c76b;
}
.chip-label {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: rgba(208, 197, 181, 0.45);
    white-space: nowrap;
    flex-shrink: 0;
    align-self: center;
}
.chip-btn {
    font-family: 'Work Sans', sans-serif;
    font-size: 1rem;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    padding: 0.28rem 0.8rem;
    border-radius: 0.75rem;
    background: #5d4514;
    color: #e4c285;
    border: 1px solid transparent;
    cursor: pointer;
    transition: all 0.3s;
    user-select: none;
    white-space: nowrap;
}
.chip-btn:hover {
    border-color: rgba(228, 194, 133, 0.5);
}
.chip-btn.active {
    border-color: #e4c285;
    box-shadow:
        0 0 16px rgba(228, 194, 133, 0.35),
        inset 0 0 8px rgba(228, 194, 133, 0.07);
    color: #fee182;
}
.view-btn {
    padding: 0.35rem 0.65rem;
    font-family: 'Work Sans', sans-serif;
    font-size: 0.7rem;
    color: rgba(208, 197, 181, 0.5);
    border: 1px solid rgba(77, 70, 58, 0.5);
    cursor: pointer;
    transition: all 0.3s;
    background: transparent;
}
.view-btn.active {
    color: #e3c76b;
    border-color: #e3c76b;
    background: rgba(227, 199, 107, 0.06);
}
.submit-btn {
    background: linear-gradient(to right, #e3c76b, #c6ab53);
    color: #3b2f00;
    display: block;
    font-family: 'Work Sans', sans-serif;
    cursor: pointer;
    border: none;
}
.submit-btn:hover:not(:disabled) {
    filter: brightness(1.1);
}
.submit-btn:active:not(:disabled) {
    transform: scale(0.97);
}
.submit-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}
.clear-btn {
    border: 1px solid rgba(77, 70, 58, 0.5);
    color: rgba(208, 197, 181, 0.7);
    background: none;
    cursor: pointer;
    font-family: 'Work Sans', sans-serif;
}
.clear-btn:hover {
    border-color: rgba(255, 100, 80, 0.5);
    color: #ffb4ab;
}

/* ═══ 備註文字框 ═══ */
.note-textarea {
    width: 100%;
    background: rgba(43, 28, 22, 0.6);
    border: 1px solid rgba(227, 199, 107, 0.35);
    border-radius: 0.25rem;
    color: #f9ddd3;
    font-size: 0.85rem;
    padding: 0.6rem 0.75rem;
    outline: none;
    transition:
        border-color 0.3s,
        box-shadow 0.3s;
    line-height: 1.5;
}
.note-textarea::placeholder {
    color: rgba(208, 197, 181, 0.3);
}
.note-textarea:focus {
    border-color: rgba(227, 199, 107, 0.7);
    box-shadow: 0 0 0 2px rgba(227, 199, 107, 0.08);
}

/* ═══ Form ═══ */
.input-line {
    background: transparent;
    border: none;
    border-bottom: 1px solid rgba(77, 70, 58, 0.65);
    border-radius: 0;
    color: #f9ddd3;
    font-family: 'Newsreader', serif;
    font-size: 0.95rem;
    padding: 0.5rem 0;
    outline: none;
    width: 100%;
    transition:
        border-color 0.4s,
        box-shadow 0.4s;
}
.input-line:focus {
    border-bottom-color: #e3c76b;
    box-shadow: 0 4px 12px -4px rgba(227, 199, 107, 0.22);
}
.input-line::placeholder {
    color: rgba(208, 197, 181, 0.3);
}
.table-id-input {
    background: transparent;
    border: none;
    border-bottom: 1px solid rgba(227, 199, 107, 0.35);
    color: #e3c76b;
    width: 8rem;
    outline: none;
    padding-left: 5px;
    transition: border-color 0.3s;
    -moz-appearance: textfield;
    font-family: 'Noto Serif TC', serif;
    font-style: italic;
}
.table-id-input::-webkit-outer-spin-button,
.table-id-input::-webkit-inner-spin-button {
    -webkit-appearance: none;
}
.table-id-input:focus {
    border-bottom-color: #e3c76b;
}
.table-id-input::placeholder {
    color: rgba(227, 199, 107, 0.3);
    font-size: 0.85rem;
}
.table-id-select {
    pointer-events: auto !important;
    cursor: pointer !important;
    background: #2b1c16;
    border: none;
    border-bottom: 1px solid rgba(227, 199, 107, 0.45);
    color: #e3c76b;
    font-family: 'Noto Serif TC', serif;
    font-style: italic;
    font-size: 1rem;
    padding: 0.1rem 0.25rem;
    outline: none;
    cursor: pointer;
    min-width: 6rem;
    transition: border-color 0.3s;
}
.table-id-select:focus {
    border-bottom-color: #e3c76b;
}
.table-id-select option {
    background: #2b1c16;
    color: #f9ddd3;
    font-style: normal;
}

/* ═══ Badges ═══ */
.badge-new {
    background: rgba(227, 199, 107, 0.18);
    border: 1px solid rgba(227, 199, 107, 0.45);
    color: #e3c76b;
}
.badge-veg {
    background: rgba(66, 49, 43, 0.5);
    border: 1px solid rgba(120, 180, 80, 0.4);
    color: #a3d977;
}
.badge-spicy {
    background: rgba(147, 0, 10, 0.25);
    border: 1px solid rgba(255, 100, 80, 0.4);
    color: #ffb4ab;
}
.badge-chef {
    background: rgba(93, 69, 20, 0.4);
    border: 1px solid rgba(228, 194, 133, 0.35);
    color: #e4c285;
}
.badge {
    font-family: 'Work Sans', sans-serif;
    font-size: 0.55rem;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    padding: 0.15rem 0.45rem;
}

/* ═══ 通知型活動 ═══ */
.notify-events {
    display: flex;
    flex-direction: column;
    gap: 0.45rem;
    margin-top: 0.5rem;
}
.notify-event-card {
    border-radius: 0.4rem;
    padding: 0.55rem 0.75rem;
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
}
.notify-event-card.eligible {
    background: rgba(227, 199, 107, 0.08);
    border: 1px solid rgba(227, 199, 107, 0.35);
}
.notify-event-card.not-eligible {
    background: rgba(77, 70, 58, 0.2);
    border: 1px solid rgba(77, 70, 58, 0.35);
}
.notify-event-top {
    display: flex;
    align-items: center;
    gap: 0.4rem;
}
.notify-event-icon {
    font-size: 0.85rem;
    flex-shrink: 0;
}
.notify-event-title {
    font-size: 0.78rem;
    color: #e3c76b;
    letter-spacing: 0.06em;
    flex: 1;
}
.notify-event-badge {
    font-size: 0.65rem;
    letter-spacing: 0.06em;
    padding: 0.1rem 0.45rem;
    border-radius: 99px;
}
.eligible .notify-event-badge {
    background: rgba(163, 217, 119, 0.15);
    color: #a3d977;
    border: 1px solid rgba(163, 217, 119, 0.4);
}
.not-eligible .notify-event-badge {
    background: rgba(208, 197, 181, 0.08);
    color: rgba(208, 197, 181, 0.5);
    border: 1px solid rgba(208, 197, 181, 0.2);
}
.notify-event-desc {
    font-size: 0.72rem;
    color: rgba(208, 197, 181, 0.55);
    margin: 0;
    padding-left: 1.3rem;
}

/* 訪客登入即享提示卡 */
.notify-event-card.guest-login-hint {
    background: rgba(227, 199, 107, 0.06);
    border: 1px solid rgba(227, 199, 107, 0.4);
    border-left: 3px solid #e3c76b;
}
.notify-event-card.guest-login-hint .notify-event-desc {
    color: rgba(227, 199, 107, 0.6);
}
.eligible-badge {
    background: rgba(163, 217, 119, 0.15);
    color: #a3d977;
    border: 1px solid rgba(163, 217, 119, 0.4);
}
/* 快到門檻提示卡 */
.notify-event-card.near-threshold {
    background: rgba(255, 160, 80, 0.07);
    border: 1px solid rgba(255, 160, 80, 0.35);
    border-left: 3px solid #ff9f4a;
}
.notify-event-card.near-threshold .notify-event-title {
    color: #ffb870;
}
.notify-event-card.near-threshold .notify-event-desc {
    color: rgba(255, 184, 112, 0.7);
}
.near-badge {
    background: rgba(255, 160, 80, 0.15);
    color: #ff9f4a;
    border: 1px solid rgba(255, 160, 80, 0.4);
}

/* ═══ Toast ═══ */
.toast-wrap {
    position: fixed;
    bottom: 5rem;
    left: 50%;
    z-index: 300;
    transition: all 0.35s;
}
@media (min-width: 1101px) {
    .toast-wrap {
        bottom: 2rem;
    }
}

/* ═══ Scrollbar ═══ */
::-webkit-scrollbar {
    width: 4px;
    height: 4px;
}
::-webkit-scrollbar-track {
    background: transparent;
}
::-webkit-scrollbar-thumb {
    background: #4d463a;
    border-radius: 3px;
}

/* ═══ Font helpers ═══ */
.font-body {
    font-family: 'Newsreader', serif;
}
.font-headline {
    font-family: 'Noto Serif TC', serif;
}
.font-label {
    font-family: 'Work Sans', sans-serif;
}

/* 覆蓋 bootstrap-icons 造成的樣式污染 */
.btn-success,
.btn-danger,
.btn-info {
    display: none !important;
}
</style>
