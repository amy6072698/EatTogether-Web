前台部分（EatTogether.API + Vue 3）

專案架構說明
  後台：EatTogether.MVC（ASP.NET Core MVC）
  前台 API：EatTogether.API（ASP.NET Core Web API）
  前台畫面：Vue 3（獨立專案，呼叫 API）

**注意：前台 API 只回傳 Status=1 且 PublishDate<=now 的文章，草稿/下架/排程未到一律不暴露


====================最新消息模組====================

[V]add 文章列表 API
  url: GET api/News?page=1&pageSize=10

  [V]add DTOs
    NewsListDto class
      Id, CategoryName, Title, Summary（截斷100字）, CoverImageUrl, PublishDate, IsPinned, ViewCount

    NewsDetailDto class
      Id, CategoryName, Title, Description（完整內文HTML）, CoverImageUrl, PublishDate, ViewCount

    NewsPagedResultDto<T> class
      Data, Page, PageSize, TotalCount, TotalPages

  [V]add NewsController
    ctor(EatTogetherDBContext context)
    **不建 Service 層，邏輯簡單直接操 DbContext

    [V]GET api/News（文章列表）
      條件：Status==1 && PublishDate<=now
      排序：IsPinned DESC → PublishDate DESC
      分頁：Skip/Take
      投影：NewsListDto，Summary 截斷 100 字

    [V]GET api/News/{id}（單篇文章）
      條件：Status==1 && PublishDate<=now && Id==id
      找不到回傳 404
      投影：NewsDetailDto，回傳完整 Description

  **summary 含 HTML tag（<p> 等），前台顯示需處理（v-html 或 strip tag）


====================Vue 3 前台====================

[]建立 Vue 專案基礎
  []專案建立（Vite + Vue 3）
  []安裝 vue-router
  []設定 router（兩個路由）
    /news              → NewsListView.vue
    /news/:id          → NewsDetailView.vue


[]add 最新消息列表頁
  []NewsListView.vue
    []onMounted 呼叫 GET api/News
    []ref() 存 articles 陣列、分頁資訊
    []v-for 渲染文章卡片
    []卡片顯示：標題、Summary、發布日期、分類、封面圖、點閱數
    []summary 含 HTML tag 需處理（strip tag 或 v-html）
    []分頁切換（點擊頁碼重新 fetch）
    []置頂文章（IsPinned）可加視覺標記
    []套入 Stitch 切版樣式


[]add 單篇文章詳細頁
  []NewsDetailView.vue
    []onMounted 呼叫 GET api/News/{id}
    []找不到（404）導回列表頁或顯示錯誤訊息
    []v-html 渲染 Description（Quill.js 產出的 HTML）
    []顯示點閱數
    []onMounted 同時呼叫 PATCH api/News/{id}/view 累加點閱數
    []套入 Stitch 切版樣式


====================點閱數功能====================

[]add 點閱數 API
  []modify Article entity
    ViewCount 欄位已存在，不需加欄位

  []add PATCH api/News/{id}/view（累加點閱數）
    找到文章 → ViewCount++  → SaveChanges
    找不到回傳 404
    回傳：{ viewCount: newCount }

  **用 PATCH 不用 GET，避免 GET 有 side effect

[]Vue 前台串接點閱數
  已記錄於 NewsDetailView.vue 的 onMounted 內
  呼叫 PATCH api/News/{id}/view，取回新數字後更新畫面顯示


====================小鈴鐺通知模組====================

（此模組暫緩，待最新消息完成後再開始）

[]規劃通知資料來源
  **來源1：已發布文章（對應 UserNotification 關聯表）
  **來源2：進行中活動

[]add 通知列表 API
  url: GET api/Notifications（或掛在會員身上）
  **需確認是否需要登入才能看通知

[]add Vue 鈴鐺元件
  []BellIcon.vue
  []點擊展開通知下拉清單
  []未讀數量 badge
  []點擊單筆通知標記已讀
