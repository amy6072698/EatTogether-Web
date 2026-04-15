<template>
  <div class="menu-page">
    <header class="menu-header">
      <div class="container">
        <span class="menu-eyebrow">Signature Flavors</span>
        <h1 class="eat-h1">精選菜單</h1>

        <nav class="category-tabs">
          <button
            v-for="cat in categories"
            :key="cat.id"
            class="tab-btn"
            :class="{ active: currentCategory === cat.id }"
            @click="currentCategory = cat.id"
          >
            {{ cat.name }}
          </button>
        </nav>
      </div>
    </header>

    <main class="container py-5">

      <!-- 搜尋 + 過濾列 -->
      <div class="filter-bar">
        <div class="search-wrap">
          <span class="search-icon">🔍</span>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="搜尋餐點名稱或描述..."
            class="search-input"
          />
          <button v-if="searchQuery" class="search-clear" @click="searchQuery = ''">✕</button>
        </div>
        <div class="filter-chips">
          <button
            class="filter-chip"
            :class="{ active: filterVeg }"
            @click="filterVeg = !filterVeg"
          >🥬 素食</button>
          <button
            class="filter-chip"
            :class="{ active: filterSpicy }"
            @click="filterSpicy = !filterSpicy"
          >🌶️ 有辣</button>
          <button
            class="filter-chip"
            :class="{ active: filterRec }"
            @click="filterRec = !filterRec"
          >⭐ 主廚推薦</button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="state-container">
        <div class="spinner"></div>
        <p class="loading-text">正在為您準備美味佳餚...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="state-container error-state">
        <div class="error-icon">!</div>
        <p>{{ error }}</p>
        <button @click="fetchMenu" class="retry-btn">重新嘗試</button>
      </div>

      <!-- 餐點卡片 -->
      <TransitionGroup v-else name="card" tag="div" class="menu-grid">
        <div
          v-for="dish in filteredDishes"
          :key="dish.id"
          class="dish-card"
          @click="openModal(dish)"
        >
          <div class="dish-img-wrap">
            <img
              v-if="dish.imageUrl"
              :src="formatImageUrl(dish.imageUrl)"
              :alt="dish.dishName"
              loading="lazy"
            />
            <div v-else class="img-placeholder">
              <span>{{ dish.dishName.charAt(0) }}</span>
            </div>
            <div class="badge-group">
              <span v-if="dish.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="dish.isPopular" class="badge badge-pop">熱銷</span>
              <span v-if="dish.isVegetarian" class="badge badge-veg">素食</span>
            </div>
          </div>

          <div class="dish-info">
            <div class="dish-title-row">
              <h3 class="dish-name">{{ dish.dishName }}</h3>
              <span class="dish-price">NT$ {{ dish.price.toLocaleString() }}</span>
            </div>
            <p class="dish-desc">{{ mealExtraInfo[dish.dishName]?.desc || dish.description || '精選新鮮食材，傳承義式經典風味，每一口都是主廚的心意。' }}</p>
            <div class="dish-footer">
              <div class="dish-tags">
                <span class="tag veg-tag" v-if="dish.isVegetarian">🥬 素食</span>
                <span class="tag spicy-tag" v-if="dish.spicyLevel > 0">
                  {{ '🌶️'.repeat(dish.spicyLevel) }}
                </span>
              </div>
              <div class="category-name">{{ dish.categoryName }}</div>
            </div>
          </div>
        </div>
      </TransitionGroup>

      <!-- Empty -->
      <div v-if="!loading && !error && filteredDishes.length === 0" class="state-container empty-state">
        <p>找不到符合條件的餐點，換個關鍵字或分類試試看。</p>
      </div>
    </main>

    <!-- ── Modal ── -->
    <Transition name="modal">
      <div v-if="isModalOpen && selectedDish" class="modal-overlay" @click.self="closeModal">
        <div class="modal-box">

          <!-- 圖片區 -->
          <div class="modal-img-wrap">
            <img
              v-if="selectedDish.imageUrl"
              :src="formatImageUrl(selectedDish.imageUrl)"
              :alt="selectedDish.dishName"
              class="modal-img"
            />
            <div v-else class="modal-img-placeholder">
              <span>{{ selectedDish.dishName.charAt(0) }}</span>
            </div>
            <div class="modal-img-gradient"></div>
            <button class="modal-close" @click="closeModal">✕</button>
            <div class="modal-badge-group">
              <span v-if="selectedDish.isRecommended" class="badge badge-rec">推薦</span>
              <span v-if="selectedDish.isPopular" class="badge badge-pop">熱銷</span>
              <span v-if="selectedDish.isVegetarian" class="badge badge-veg">素食</span>
            </div>
          </div>

          <!-- 資訊區 -->
          <div class="modal-body">
            <div class="modal-header-row">
              <h2 class="modal-title">{{ selectedDish.dishName }}</h2>
              <div class="modal-price">NT$ {{ selectedDish.price.toLocaleString() }}</div>
            </div>

            <p class="text-on-surface-variant text-sm leading-relaxed mt-4 mb-6">
              {{ mealExtraInfo[selectedDish.dishName]?.desc || selectedDish.description || '主廚精選地道食材，為您呈現純粹的義式饗宴。' }}
            </p>

            <!-- 食材列表 -->
            <div v-if="mealExtraInfo[selectedDish.dishName]?.ingredients" class="modal-section">
              <div class="text-xs font-bold uppercase tracking-widest text-primary mb-3">精選食材</div>
              <div class="ingredient-grid">
                <div
                  v-for="item in mealExtraInfo[selectedDish.dishName].ingredients"
                  :key="item.name"
                  class="ingr-card"
                >
                  <span class="ingr-name">{{ item.name }}</span>
                  <span class="ingr-desc">{{ item.desc }}</span>
                </div>
              </div>
            </div>

            <!-- 屬性列 -->
            <div class="modal-section">
              <div class="modal-section-label">餐點屬性</div>
              <div class="attr-chips">
                <span class="attr-chip" v-if="selectedDish.categoryName">{{ selectedDish.categoryName }}</span>
                <span class="attr-chip veg" v-if="selectedDish.isVegetarian">🥬 素食</span>
                <span class="attr-chip spicy" v-if="selectedDish.spicyLevel > 0">
                  {{ '🌶️'.repeat(selectedDish.spicyLevel) }} {{ spicyLabel(selectedDish.spicyLevel) }}
                </span>
                <span class="attr-chip rec" v-if="selectedDish.isRecommended">⭐ 主廚推薦</span>
                <span class="attr-chip pop" v-if="selectedDish.isPopular">🔥 熱銷</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

// ── 菜品補充資訊對照表 ────────────────────────────────
const mealExtraInfo = {
  // ── 主餐 ──
  '松露奶油燉飯': {
    desc: '濃郁黑松露奶油包裹每粒米心，入口即是奢華的土地芬芳。',
    ingredients: [
      { name: '義大利卡納羅利米', desc: '頂級短粒米，澱粉豐富綿密' },
      { name: '黑松露醬',         desc: '法國進口，濃郁土地香氣' },
      { name: '帕瑪森起司',       desc: '24個月熟成 DOP 認證' },
      { name: '白葡萄酒',         desc: '皮埃蒙特白酒提鮮' },
      { name: '法式奶油',         desc: 'AOP 認證諾曼第無鹽奶油' },
      { name: '紫洋蔥',           desc: '慢炒化為甜味基底' },
    ]
  },
  '瑪格麗特披薩': {
    desc: '那不勒斯窯烤正宗手法，番茄甜酸與莫扎瑞拉融合成義式靈魂。',
    ingredients: [
      { name: '00麵粉薄底',     desc: '拿坡里72小時冷藏發酵' },
      { name: '聖馬扎諾番茄醬', desc: '義大利 DOP 認證罐裝' },
      { name: '水牛莫扎瑞拉',   desc: '坎帕尼亞當日新鮮進口' },
      { name: '新鮮羅勒',       desc: '溫室現摘熱那亞甜羅勒' },
      { name: '冷壓橄欖油',     desc: '西西里特級初榨' },
      { name: '海鹽',           desc: '西西里天然岩鹽提味' },
    ]
  },
  '波隆那肉醬寬麵': {
    desc: '慢燉四小時的波隆那肉醬，層層包裹手擀新鮮寬麵，濃而不膩。',
    ingredients: [
      { name: '手擀雞蛋寬麵',   desc: '每日現製新鮮 Pappardelle' },
      { name: '牛豬混合絞肉',   desc: '黃金 7:3 比例慢炒' },
      { name: '番茄糊',         desc: '濃縮聖馬扎諾番茄' },
      { name: '紅酒',           desc: '基安蒂 DOCG 去腥提香' },
      { name: '迷迭香',         desc: '普羅旺斯品種現摘' },
      { name: '帕瑪森起司',     desc: '36個月熟成現磨' },
    ]
  },
  '烤海鮮拼盤': {
    desc: '當日鮮撈海鮮以香草橄欖油炙烤，鎖住大海的鮮甜原味。',
    ingredients: [
      { name: '虎蝦',       desc: '當日鮮撈活體即炙' },
      { name: '透抽',       desc: '澎湖現撈切圈嫩烤' },
      { name: '淡菜',       desc: '紐西蘭綠唇青口貝' },
      { name: '干貝',       desc: '北海道 S 級生食等級' },
      { name: '蒜頭',       desc: '西西里品種香草醃製' },
      { name: '檸檬香草油', desc: '自製百里香橄欖油醃漬' },
    ]
  },
  '小牛排佐迷迭香醬汁': {
    desc: '嫩煎至玫瑰粉心的小牛排，淋上迷迭香紅酒醬汁，細緻而深邃。',
    ingredients: [
      { name: '澳洲小牛里肌', desc: '穀飼180天嫩肉排' },
      { name: '迷迭香',       desc: '普羅旺斯新鮮香草' },
      { name: '紅酒',         desc: '巴羅洛 DOCG 降鍋收汁' },
      { name: '牛高湯',       desc: '熬製8小時濃縮精華' },
      { name: '法式奶油',     desc: '諾曼第 AOP 掛鍋增亮' },
      { name: '蒜頭',         desc: '現壓慢炒至焦糖化' },
    ]
  },
  '白酒蛤蜊義大利麵': {
    desc: '鮮蛤以白酒燜開，湯汁化作海味精華拌入細麵，清爽鮮活。',
    ingredients: [
      { name: '義大利細扁麵', desc: 'Linguine De Cecco 銅模壓製' },
      { name: '台灣文蛤',     desc: '台南鹿耳門當日新鮮' },
      { name: '白葡萄酒',     desc: '維納西亞 DOC 提鮮' },
      { name: '蒜片',         desc: '薄切慢炒至金黃' },
      { name: '辣椒',         desc: '卡拉布里亞乾辣椒微辣' },
      { name: '平葉巴西里',   desc: '現摘義大利巴西里' },
    ]
  },
  '蒜香橄欖油細麵': {
    desc: '以特級橄欖油與金黃蒜片詮釋義式極簡，簡單卻令人難忘。',
    ingredients: [
      { name: '義大利天使細麵', desc: '銅模擠壓粗糙表面吸醬' },
      { name: '西西里橄欖油',   desc: '特級冷壓初榨 DOP' },
      { name: '蒜頭',           desc: '薄片慢炸至焦糖邊緣' },
      { name: '乾辣椒',         desc: '義大利南部品種增香' },
      { name: '帕瑪森起司',     desc: '現磨趁熱覆蓋' },
      { name: '黑胡椒',         desc: '現磨馬達加斯加粗粒' },
    ]
  },
  '奶油培根義大利麵': {
    desc: '羅馬傳統配方，蛋黃與起司乳化成絲滑醬汁，煙燻培根添香。',
    ingredients: [
      { name: '義大利管麵',   desc: 'Rigatoni Rummo 銅模粗紋' },
      { name: '義式培根',     desc: 'Guanciale 豬臉頰煙燻' },
      { name: '雞蛋黃',       desc: '牧場放養雞濃郁蛋黃' },
      { name: '佩科里諾起司', desc: '羅馬 PDO 羊奶起司' },
      { name: '黑胡椒',       desc: '現磨粗粒靈魂調味' },
      { name: '帕瑪森起司',   desc: '24個月熟成增鮮' },
    ]
  },
  '嫩煎鮭魚佐酸豆醬': {
    desc: '挪威鮭魚煎至外酥內嫩，酸豆奶油醬提出清爽的地中海氣息。',
    ingredients: [
      { name: '挪威鮭魚排', desc: '大西洋有機養殖魚排' },
      { name: '酸豆',       desc: '義大利潘泰萊里亞鹽漬' },
      { name: '法式奶油',   desc: '諾曼第無鹽奶油化醬' },
      { name: '白酒',       desc: '皮諾格里吉歐去腥提味' },
      { name: '時蘿',       desc: '北歐風味新鮮香草' },
      { name: '檸檬汁',     desc: '義大利阿瑪菲檸檬現榨' },
    ]
  },
  // ── 飲料 ──
  '義式濃縮咖啡': {
    desc: '義大利原豆高壓萃取，厚實克麗瑪承載焦糖與黑巧克力香氣。',
    ingredients: [
      { name: '阿拉比卡豆', desc: '衣索比亞＋巴西混合配方' },
      { name: '軟水',       desc: '礦物質精確調配萃取水' },
      { name: '9 Bar 氣壓', desc: '義式標準萃取壓力' },
      { name: '25秒萃取',   desc: '黃金時間窗精準控制' },
    ]
  },
  '卡布奇諾': {
    desc: '濃縮咖啡與綿密奶泡的完美一比一，溫度恰好喚醒每個清晨。',
    ingredients: [
      { name: '義式濃縮咖啡', desc: '雙份 Ristretto 濃萃' },
      { name: '全脂牛奶',     desc: '義大利 Parmalat 牛奶' },
      { name: '手打奶泡',     desc: '65°C 絲絨奶泡質地' },
      { name: '肉桂粉',       desc: '錫蘭真正肉桂（可選）' },
    ]
  },
  '義式氣泡水': {
    desc: '阿爾卑斯山泉天然碳酸，清冽氣泡洗滌味蕾，為下一口做準備。',
    ingredients: [
      { name: '天然礦泉水', desc: '阿爾卑斯山脈源頭汲取' },
      { name: '天然碳酸氣', desc: '原生碳酸礦泉未額外充氣' },
      { name: '檸檬片',     desc: '阿瑪菲有機檸檬（可選）' },
    ]
  },
  '紅酒（杯）': {
    desc: '托斯卡尼精選單杯紅酒，單寧柔順，搭佐主餐相得益彰。',
    ingredients: [
      { name: '桑嬌維塞葡萄', desc: 'Chianti Classico 品種' },
      { name: '橡木桶陳釀',   desc: '法式新桶 30%，熟成18個月' },
      { name: 'DOCG 認證',    desc: '義大利法定最高產區認證' },
    ]
  },
  '白葡萄酒（杯）': {
    desc: '皮埃蒙特白酒清新果香，以礦石感與花香完美搭配海鮮料理。',
    ingredients: [
      { name: '嘉維 Cortese 葡萄', desc: '皮埃蒙特原生白葡萄' },
      { name: '低溫不鏽鋼發酵',    desc: '保留原始花果香氣' },
      { name: 'DOC 認證',          desc: '義大利法定產區認證' },
    ]
  },
  '水蜜桃冰茶': {
    desc: '手工熬煮蜜桃糖漿注入錫蘭紅茶，酸甜滋味如夏日微風。',
    ingredients: [
      { name: '錫蘭紅茶',   desc: '斯里蘭卡 BOP 等級茶葉' },
      { name: '新鮮水蜜桃', desc: '台灣拉拉山當季鮮摘' },
      { name: '龍眼蜂蜜',   desc: '台灣純釀自然甜味' },
      { name: '新鮮檸檬汁', desc: '現榨平衡甜酸層次' },
      { name: '薄荷葉',     desc: '有機水耕薄荷增涼' },
    ]
  },
  // ── 甜點 ──
  '提拉米蘇': {
    desc: '馬斯卡彭奶酪與濃縮咖啡交織，每一匙都是義大利甜蜜的告白。',
    ingredients: [
      { name: '馬斯卡彭起司',    desc: '義大利 Galbani 原裝進口' },
      { name: '義式濃縮咖啡',    desc: '現萃冷卻後浸潤餅乾' },
      { name: '薩芙瓦蒂手指餅乾', desc: '義大利進口正宗原裝' },
      { name: '牧場放養蛋黃',    desc: '日本特選雞蛋濃郁蛋黃' },
      { name: '比利時可可粉',    desc: '無糖天然可可表層' },
      { name: '馬沙拉酒',        desc: '西西里 Marsala Fine 烈酒' },
    ]
  },
  '義式奶酪': {
    desc: '絲滑鮮奶凍配上草莓果醬，輕盈口感中藏著無盡的奶香柔情。',
    ingredients: [
      { name: '動物性鮮奶油', desc: '36% 乳脂豐潤奶香' },
      { name: '有機全脂牛奶', desc: '牧場直送當日鮮乳' },
      { name: '魚明膠金級',   desc: '細緻口感關鍵凝固劑' },
      { name: '大溪地香草莢', desc: '波本品種飽滿香氣' },
      { name: '有機蔗糖',     desc: '適量甜度不搶戲' },
      { name: '草莓庫利',     desc: '新鮮草莓慢熬果醬' },
    ]
  },
  '焦糖布丁': {
    desc: '主廚以法式手法燒製金黃焦糖殼，敲破瞬間香氣四溢。',
    ingredients: [
      { name: '牧場放養蛋黃', desc: '濃郁蛋香奠定基底' },
      { name: '動物性鮮奶油', desc: '36% 乳脂絲滑底料' },
      { name: '香草莢',       desc: '大溪地波本香草' },
      { name: '細磨白砂糖',   desc: '均勻焦化不苦澀' },
      { name: '焦糖脆殼',     desc: '噴槍現燒金黃薄層' },
    ]
  },
  '草莓聖代': {
    desc: '季節草莓與手工香草冰淇淋疊成高塔，清甜酸香層次分明。',
    ingredients: [
      { name: '當季新鮮草莓',  desc: '苗栗大湖鮮摘整顆' },
      { name: '手工香草冰淇淋', desc: '每日現製無人工添加' },
      { name: '輕盈香緹奶油',  desc: '現打鮮奶油疊層' },
      { name: '草莓醬',        desc: '新鮮草莓熬製果醬' },
      { name: '有機薄荷葉',    desc: '水耕種植點綴提香' },
    ]
  },
  // ── 湯品 ──
  '番茄羅勒濃湯': {
    desc: '慢燉聖馬扎諾番茄融入新鮮羅勒，酸甜層次療癒每個疲憊的靈魂。',
    ingredients: [
      { name: '聖馬扎諾番茄', desc: 'DOP 認證全熟罐裝' },
      { name: '新鮮羅勒',     desc: '熱那亞甜羅勒現摘' },
      { name: '西西里蒜頭',   desc: '慢炒釋放甜香底味' },
      { name: '特級橄欖油',   desc: '冷壓初榨增香收尾' },
      { name: '動物性鮮奶油', desc: '少許添加增滑順感' },
      { name: '法棍脆片',     desc: '蒜香烤脆搭配湯底' },
    ]
  },
  '義式蔬菜湯': {
    desc: '十種時蔬以雞高湯慢燉，撒上帕瑪森起司，溫暖質樸的家鄉味。',
    ingredients: [
      { name: '十種有機時蔬',  desc: '台灣有機農場當季採購' },
      { name: '義大利白腰豆',  desc: '進口罐裝豆增飽足感' },
      { name: 'Ditalini 管麵', desc: '小管形麵碎增湯稠度' },
      { name: '雞高湯',        desc: '熬製6小時清澈金湯' },
      { name: '帕瑪森起司皮',  desc: '增鮮提味的秘密武器' },
      { name: '特級橄欖油',    desc: '上桌前淋入提升香氣' },
    ]
  },
  '海鮮巧達濃湯': {
    desc: '新鮮蛤蜊與蝦仁燉入奶油白醬，濃郁鮮美，每一匙皆是海洋的禮物。',
    ingredients: [
      { name: '台南文蛤',     desc: '鹿耳門養殖當日現撈' },
      { name: '台灣白蝦',     desc: '本地養殖鮮甜去殼' },
      { name: '馬鈴薯',       desc: '天然澱粉增加濃稠度' },
      { name: '美式燻烤培根', desc: '煙燻香氣底味點綴' },
      { name: '動物性鮮奶油', desc: '36% 乳脂奶香濃湯' },
      { name: '白葡萄酒',     desc: '提鮮去腥海鮮必備' },
    ]
  },
  // ── 附餐 ──
  '蒜香佛卡夏麵包': {
    desc: '烤得酥脆的義式佛卡夏浸透橄欖油與海鹽，是最誠摯的開胃歡迎。',
    ingredients: [
      { name: '義大利 00 麵粉', desc: '精磨高筋麵粉發酵體' },
      { name: '新鮮迷迭香',     desc: '地中海品種現摘壓入' },
      { name: '焦糖化蒜頭',     desc: '慢煎至焦糖色澤入味' },
      { name: '西西里橄欖油',   desc: '大量冷壓初榨浸潤' },
      { name: '義大利岩鹽',     desc: '海鹽粒表層點綴提鮮' },
      { name: '油封番茄乾',     desc: '慢曬濃縮酸甜（可選）' },
    ]
  },
  '凱薩沙拉': {
    desc: '羅馬生菜佐自製凱薩醬，帕瑪森起司削片與香脆麵包丁錦上添花。',
    ingredients: [
      { name: '羅馬生菜',     desc: '脆嫩當日進貨冷藏保鮮' },
      { name: '主廚凱薩醬',   desc: '秘製鯷魚蒜香自製醬料' },
      { name: '帕瑪森起司',   desc: '現刨大薄片覆蓋其上' },
      { name: '蒜香麵包丁',   desc: '奶油烤製酥脆口感' },
      { name: '油漬鯷魚片',   desc: '義大利 IGP 認證鯷魚' },
      { name: '現磨黑胡椒',   desc: '粗粒增香最後調味' },
    ]
  },
  '炸魷魚圈': {
    desc: '現炸至金黃的魷魚圈外酥內彈，佐以蒜香美乃滋令人一口接一口。',
    ingredients: [
      { name: '澎湖新鮮魷魚', desc: '每日直送現切魷魚圈' },
      { name: '義大利麵包粉', desc: 'Panko 粗粒脆衣' },
      { name: '阿瑪菲檸檬汁', desc: '現榨提鮮去腥' },
      { name: '自製蒜香美乃滋', desc: '烤大蒜調製沾醬' },
      { name: '精鹽',          desc: '起鍋前精準撒量' },
      { name: '煙燻紅椒粉',    desc: '西班牙品種增色提香' },
    ]
  },
  '帕瑪火腿拼盤': {
    desc: '36個月熟成帕瑪火腿薄如絲紙，搭哈密瓜與無花果，鹹甜絕妙平衡。',
    ingredients: [
      { name: '帕瑪火腿',     desc: '36個月 Prosciutto di Parma' },
      { name: '美濃哈密瓜',   desc: '台灣有機農場鮮甜品種' },
      { name: '新鮮無花果',   desc: '當季鮮摘或義式蜜漬' },
      { name: '芝麻葉',       desc: '微苦辛香平衡油脂' },
      { name: '帕瑪森起司片', desc: '現刨頂級熟成碎片' },
      { name: '特級橄欖油',   desc: '最後淋入收尾提香' },
    ]
  }
};

// ── State ────────────────────────────────────────────
const dishes = ref([]);
const loading = ref(true);
const error = ref(null);
const currentCategory = ref(0);
const searchQuery = ref('');
const filterVeg = ref(false);
const filterSpicy = ref(false);
const filterRec = ref(false);

// Modal
const isModalOpen = ref(false);
const selectedDish = ref(null);

const categories = [
  { id: 0, name: '全部' },
  { id: 1, name: '主餐' },
  { id: 2, name: '飲料' },
  { id: 3, name: '甜點' },
  { id: 4, name: '湯品' },
  { id: 5, name: '附餐' }
];

// ── Modal ────────────────────────────────────────────
const openModal = (dish) => {
  selectedDish.value = dish;
  isModalOpen.value = true;
  document.body.style.overflow = 'hidden';
};

const closeModal = () => {
  isModalOpen.value = false;
  selectedDish.value = null;
  document.body.style.overflow = '';
};

const handleEsc = (e) => { if (e.key === 'Escape') closeModal(); };
onMounted(() => window.addEventListener('keydown', handleEsc));
onUnmounted(() => window.removeEventListener('keydown', handleEsc));

// ── Utils ────────────────────────────────────────────
const spicyLabel = (level) => {
  return ['', '微辣', '中辣', '大辣', '極辣'][level] ?? '辣';
};

const formatImageUrl = (url) => {
  if (!url) return null;
  let path = url.replace(/\\/g, '/').replace(/^~\//, '');
  const match = /\/wwwroot\/(.*)/i.exec('/' + path);
  if (match?.[1]) path = match[1];
  path = path.replace(/^\//, '');
  return `/api/${path}`;
};

// ── API ──────────────────────────────────────────────
const fetchMenu = async () => {
  loading.value = true;
  error.value = null;
  const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';
  try {
    const res = await fetch(`${API_BASE}/Dishes/GetActiveJson`);
    if (!res.ok) throw new Error(`抓取失敗 (${res.status})`);
    dishes.value = await res.json();
  } catch (err) {
    console.error('Menu Fetch Error:', err);
    error.value = '無法載入菜單資料，請確認 API 伺服器狀態。';
  } finally {
    loading.value = false;
  }
};

// ── Computed ─────────────────────────────────────────
const filteredDishes = computed(() => {
  return dishes.value.filter(d => {
    if (currentCategory.value !== 0 && d.categoryId !== currentCategory.value) return false;
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase();
      const hit = d.dishName.toLowerCase().includes(q) ||
                  (d.description && d.description.toLowerCase().includes(q));
      if (!hit) return false;
    }
    if (filterVeg.value && !d.isVegetarian) return false;
    if (filterSpicy.value && !(d.spicyLevel > 0)) return false;
    if (filterRec.value && !d.isRecommended) return false;
    return true;
  });
});

onMounted(fetchMenu);
</script>

<style scoped>
.menu-page {
  background-color: var(--eat-surface);
  min-height: 100vh;
  color: var(--eat-on-surface);
  font-family: var(--font-body);
}

.menu-header {
  padding: 8rem 0 4rem;
  text-align: center;
  background: linear-gradient(to bottom, rgba(24, 11, 6, 0.95), var(--eat-surface));
}

.menu-eyebrow {
  display: block;
  font-family: var(--font-label);
  color: var(--eat-secondary);
  letter-spacing: 0.4em;
  text-transform: uppercase;
  font-size: 0.75rem;
  margin-bottom: 1rem;
}

.eat-h1 {
  font-family: var(--font-headline);
  font-size: clamp(2.5rem, 5vw, 3.5rem);
  color: var(--eat-primary);
  margin-bottom: 3rem;
  font-style: italic;
}

/* Category Tabs */
.category-tabs {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  padding: 0 1rem;
}

.tab-btn {
  background: none;
  border: none;
  color: rgba(249, 221, 211, 0.5);
  font-family: var(--font-label);
  font-size: 0.9rem;
  padding: 0.6rem 1.2rem;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  letter-spacing: 0.1em;
  border-radius: 20px;
}
.tab-btn:hover { color: var(--eat-secondary); }
.tab-btn.active {
  color: var(--eat-primary);
  background-color: rgba(227, 199, 107, 0.1);
  box-shadow: inset 0 0 0 1px rgba(227, 199, 107, 0.3);
}

/* ── 搜尋 + 過濾列 ── */
.filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
  margin-bottom: 3rem;
}

.search-wrap {
  position: relative;
  flex: 1;
  min-width: 220px;
  max-width: 420px;
}
.search-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.85rem;
  opacity: 0.5;
}
.search-input {
  width: 100%;
  padding: 0.65rem 2.5rem 0.65rem 2.2rem;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 30px;
  color: var(--eat-on-surface);
  font-family: var(--font-body);
  font-size: 0.9rem;
  outline: none;
  transition: border-color 0.3s;
}
.search-input:focus { border-color: rgba(227, 199, 107, 0.4); }
.search-input::placeholder { color: rgba(249, 221, 211, 0.3); }
.search-clear {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: rgba(249, 221, 211, 0.4);
  cursor: pointer;
  font-size: 0.75rem;
  padding: 0;
}

.filter-chips { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.filter-chip {
  background: none;
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 20px;
  color: rgba(249, 221, 211, 0.5);
  font-family: var(--font-label);
  font-size: 0.8rem;
  padding: 0.45rem 1rem;
  cursor: pointer;
  transition: all 0.3s;
}
.filter-chip:hover { border-color: rgba(227, 199, 107, 0.35); color: rgba(249, 221, 211, 0.8); }
.filter-chip.active {
  background: rgba(227, 199, 107, 0.12);
  border-color: rgba(227, 199, 107, 0.5);
  color: var(--eat-primary);
}

/* ── Grid ── */
.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2.5rem;
}

/* ── Card 進場動畫 ── */
.card-enter-active {
  transition: opacity 0.5s ease, transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.card-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.card-enter-from { opacity: 0; transform: translateY(24px) scale(0.97); }
.card-leave-to   { opacity: 0; transform: translateY(-8px) scale(0.97); }
.card-move       { transition: transform 0.5s cubic-bezier(0.34, 1.56, 0.64, 1); }

.dish-card {
  background-color: var(--eat-surface-high);
  border-radius: 16px;
  overflow: hidden;
  transition: transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1), border-color 0.3s;
  display: flex;
  flex-direction: column;
  border: 1px solid rgba(227, 199, 107, 0.05);
  cursor: pointer;
}
.dish-card:hover {
  transform: translateY(-10px);
  border-color: rgba(227, 199, 107, 0.2);
}

.dish-img-wrap {
  position: relative;
  height: 200px;
  overflow: hidden;
  background-color: #251813;
}
.dish-img-wrap img {
  width: 100%; height: 100%;
  object-fit: cover;
  transition: transform 1s ease;
}
.dish-card:hover .dish-img-wrap img { transform: scale(1.1); }

.img-placeholder {
  width: 100%; height: 100%;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
}
.img-placeholder span {
  font-family: var(--font-headline);
  font-size: 5rem;
  color: rgba(227, 199, 107, 0.1);
  font-style: italic;
}

.badge-group {
  position: absolute; top: 1rem; left: 1rem;
  display: flex; flex-direction: column; gap: 0.4rem; z-index: 2;
}
.badge {
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
  font-size: 0.65rem;
  font-family: var(--font-label);
  font-weight: 600;
  letter-spacing: 0.1em;
  backdrop-filter: blur(4px);
}
.badge-rec { background-color: rgba(227, 199, 107, 0.9); color: var(--eat-surface); }
.badge-pop { background-color: rgba(217, 83, 79, 0.9); color: white; }
.badge-veg { background-color: rgba(80, 160, 80, 0.85); color: white; }

.dish-info { padding: 1.5rem; flex-grow: 1; display: flex; flex-direction: column; }
.dish-title-row {
  display: flex; justify-content: space-between;
  align-items: flex-start; margin-bottom: 0.75rem;
}
.dish-name {
  font-family: var(--font-headline);
  color: var(--eat-primary);
  font-size: 1.25rem;
  margin: 0; font-style: italic;
}
.dish-price {
  font-family: var(--font-label);
  color: var(--eat-secondary);
  font-size: 0.9rem;
  font-weight: 500; margin-top: 0.25rem;
  white-space: nowrap; margin-left: 0.5rem;
}
.dish-desc {
  font-family: var(--font-body);
  font-size: 0.9rem; line-height: 1.7;
  color: rgba(249, 221, 211, 0.6);
  margin-bottom: 1.5rem; flex-grow: 1;
  font-style: italic;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.dish-footer {
  display: flex; justify-content: space-between; align-items: center;
  border-top: 1px solid rgba(227, 199, 107, 0.1); padding-top: 1rem;
}
.dish-tags { display: flex; gap: 0.75rem; }
.tag { font-size: 0.75rem; }
.category-name {
  font-family: var(--font-label);
  font-size: 0.7rem;
  color: rgba(227, 199, 107, 0.4);
  text-transform: uppercase; letter-spacing: 0.1em;
}

/* ── Modal ── */
.modal-enter-active { transition: opacity 0.35s ease; }
.modal-leave-active { transition: opacity 0.25s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }

.modal-overlay {
  position: fixed; inset: 0; z-index: 1000;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(16px);
  display: flex; align-items: center; justify-content: center;
  padding: 1.5rem;
}

.modal-box {
  width: 100%; max-width: 520px;
  background: #1e100b;
  border: 1px solid rgba(227, 199, 107, 0.15);
  border-radius: 24px;
  overflow: hidden;
  animation: modalPop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1);
}
@keyframes modalPop {
  from { opacity: 0; transform: scale(0.92) translateY(20px); }
  to   { opacity: 1; transform: scale(1) translateY(0); }
}

.modal-img-wrap {
  position: relative;
  height: 240px;
  background: #251813;
  overflow: hidden;
}
.modal-img { width: 100%; height: 100%; object-fit: cover; }
.modal-img-placeholder {
  width: 100%; height: 100%;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, #362620 0%, #1e100b 100%);
  font-family: var(--font-headline);
  font-size: 6rem; color: rgba(227, 199, 107, 0.1); font-style: italic;
}
.modal-img-gradient {
  position: absolute; bottom: 0; left: 0; right: 0; height: 50%;
  background: linear-gradient(to top, #1e100b, transparent);
}
.modal-close {
  position: absolute; top: 1rem; right: 1rem;
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(0,0,0,0.5); border: none;
  color: rgba(255,255,255,0.8); font-size: 0.9rem;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.2s;
}
.modal-close:hover { background: rgba(0,0,0,0.8); }
.modal-badge-group {
  position: absolute; bottom: 1rem; left: 1.25rem;
  display: flex; gap: 0.4rem; z-index: 2;
}

.modal-body {
  padding: 1.75rem;
  max-height: 55vh;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(227, 199, 107, 0.2) transparent;
}
.modal-header-row {
  display: flex; justify-content: space-between;
  align-items: flex-start; margin-bottom: 1rem; gap: 1rem;
}
.modal-title {
  font-family: var(--font-headline);
  color: var(--eat-primary);
  font-size: 1.8rem; font-style: italic;
  line-height: 1.2; margin: 0;
}
.modal-price {
  font-family: var(--font-label);
  color: var(--eat-secondary);
  font-size: 1.1rem; font-weight: 600;
  white-space: nowrap; padding-top: 0.35rem;
}
/* ── Modal 語意 Utility（對應 --eat-* 設計代號）── */
.text-on-surface-variant   { color: var(--eat-on-surface-variant); font-family: var(--font-body); font-style: italic; }
.text-on-surface           { color: var(--eat-on-surface); }
.text-primary              { color: var(--eat-primary); font-family: var(--font-label); }
.text-sm                   { font-size: 0.875rem; }
.text-xs                   { font-size: 0.75rem; }
.text-\[10px\]             { font-size: 10px; }
.leading-relaxed           { line-height: 1.625; }
.font-bold                 { font-weight: 700; }
.font-semibold             { font-weight: 600; }
.uppercase                 { text-transform: uppercase; }
.tracking-widest           { letter-spacing: 0.15em; }
.mt-4                      { margin-top: 1rem; }
.mb-6                      { margin-bottom: 1.5rem; }
.mb-3                      { margin-bottom: 0.75rem; }
.mb-2                      { margin-bottom: 0.5rem; }
.mr-2                      { margin-right: 0.5rem; }
.inline-flex               { display: inline-flex; }
.items-center              { align-items: center; }
.px-3                      { padding-left: 0.75rem; padding-right: 0.75rem; }
.py-1                      { padding-top: 0.25rem; padding-bottom: 0.25rem; }
.rounded-full              { border-radius: 9999px; }
.bg-surface-container-high { background-color: var(--eat-surface-high); }
.border                    { border-width: 1px; border-style: solid; }
.border-outline-variant\/15 { border-color: rgba(77, 70, 58, 0.15); }

.modal-section { margin-bottom: 1.25rem; }
.modal-section-label {
  font-family: var(--font-label);
  font-size: 0.7rem; text-transform: uppercase;
  letter-spacing: 0.15em; opacity: 0.4;
  margin-bottom: 0.65rem;
}
.attr-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.attr-chip {
  font-size: 0.8rem;
  padding: 0.3rem 0.9rem;
  border-radius: 20px;
  border: 1px solid rgba(227, 199, 107, 0.2);
  color: rgba(249, 221, 211, 0.65);
  font-family: var(--font-label);
}
.attr-chip.veg   { border-color: rgba(80,160,80,0.4); color: rgba(144,238,144,0.8); }
.attr-chip.spicy { border-color: rgba(217,83,79,0.4); color: rgba(255,140,100,0.9); }
.attr-chip.rec   { border-color: rgba(227,199,107,0.4); color: var(--eat-primary); }
.attr-chip.pop   { border-color: rgba(217,83,79,0.3); color: rgba(255,140,100,0.8); }

.ingredient-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
}
.ingr-card {
  background-color: var(--eat-surface-high);
  border: 1px solid rgba(227, 199, 107, 0.08);
  border-radius: 8px;
  padding: 0.55rem 0.7rem;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}
.ingr-name {
  font-family: var(--font-label);
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--eat-on-surface);
  line-height: 1.3;
}
.ingr-desc {
  font-family: var(--font-body);
  font-size: 0.6rem;
  color: var(--eat-on-surface-variant);
  line-height: 1.4;
  font-style: italic;
}

/* States */
.state-container { padding: 8rem 0; text-align: center; }
.spinner {
  width: 48px; height: 48px;
  border: 2px solid rgba(227, 199, 107, 0.1);
  border-top-color: var(--eat-primary);
  border-radius: 50%;
  animation: spin 1s cubic-bezier(0.4, 0, 0.2, 1) infinite;
  margin: 0 auto 2rem;
}
.loading-text {
  font-family: var(--font-label);
  letter-spacing: 0.2em;
  color: var(--eat-secondary); font-size: 0.9rem;
}
.retry-btn {
  margin-top: 2rem; background: none;
  border: 1px solid var(--eat-primary);
  color: var(--eat-primary);
  padding: 0.6rem 2.5rem;
  font-family: var(--font-label); font-size: 0.8rem;
  cursor: pointer; transition: all 0.3s;
  text-transform: uppercase; letter-spacing: 0.2em;
}
.retry-btn:hover { background-color: var(--eat-primary); color: var(--eat-surface); }
@keyframes spin { to { transform: rotate(360deg); } }

/* Utils */
.container { max-width: 1200px; margin: 0 auto; padding: 0 2rem; }
.py-5 { padding-top: 4rem; padding-bottom: 6rem; }

@media (max-width: 768px) {
  .menu-header { padding-top: 6rem; }
  .eat-h1 { font-size: 2.2rem; }
  .menu-grid { grid-template-columns: 1fr; }
  .filter-bar { flex-direction: column; align-items: stretch; }
  .search-wrap { max-width: 100%; }
  .modal-title { font-size: 1.5rem; }
  .ingredient-grid { grid-template-columns: repeat(2, 1fr); }
  .modal-body { max-height: 60vh; }
}
</style>