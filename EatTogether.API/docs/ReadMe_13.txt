前台部分（EatTogether.API + Vue 3）

專案架構說明
  後台：EatTogether.MVC（ASP.NET Core MVC）
  前台 API：EatTogether.API（ASP.NET Core Web API）
  前台畫面：Vue 3（獨立專案，呼叫 API）

**負責模組：線上訂位系統 × 優惠券系統
**分支：feature/web-booking（訂位）、feature/web-23（優惠券）

====================進度符號說明====================

[V]        已完成
[working]  進行中
[]         待開始

====================已完成基礎建設（dev 已合併）====================

[V] EF Model 更新（PR #20）
  [V] Reservation.cs 新增 MemberId (int?)、CancelledAt (DateTime?)、Member navigation
  [V] Member.cs 新增 ICollection<Reservation> Reservations
  [V] EatTogetherDBContext.cs 補上 FK_Reservations_Members 關聯設定

[V] Program.cs 已註冊（無需重複新增）
  [V] ICouponRepository, CouponRepository
  [V] IMemberCouponRepository, MemberCouponRepository
  [V] ITableRepository, TableRepository
  [V] IEmailService, EmailService
  [V] JwtHelper（Singleton）

[V] 已存在 DTOs（dev 已合併）
  [V] CouponDto（含 DiscountTypeText、DiscountDescription、IsExpired 等計算屬性）
  [V] CouponValidateDto（IsValid, CouponId?, CouponName, Discount, Message）
  [V] MemberCouponDto（含 StatusText、StatusBadgeClass 等計算屬性）
  [V] TableDto


====================訂位模組（Reservation）====================

**Status 定義：0=訂位中 / 1=已報到 / 2=已取消 / 3=NoShow
**訪客訂位（MemberId=null）憑 BookingNumber + Email 查詢；會員訂位綁定 MemberId


──── 後端 API ────

[]add DTOs
  路徑：EatTogether.API/Models/DTOs/

  []ReservationCreateDto class
    Name (string), Phone (string), Email (string)
    ReservationDate (DateTime), AdultsCount (int), ChildrenCount (int)
    Remark (string?)
    **不含 BookingNumber（後端自動產生）、Status（固定 0）

  []ReservationListDto class
    Id, BookingNumber, Name, ReservationDate, AdultsCount, ChildrenCount
    Status (int), StatusText (string), StatusBadgeClass (string)
    CancelledAt (DateTime?)

  []ReservationDetailDto class
    Id, BookingNumber, Name, Phone, Email
    ReservationDate, AdultsCount, ChildrenCount, Status, StatusText
    Remark, ReservedAt, CancelledAt

  []AvailabilityDto class
    IsAvailable (bool), RemainingCapacity (int), Message (string)
    TableTypeAvailability List<TableTypeSlotDto>
      TableType (string), SeatCount (int), Total (int), Available (int)

  []ReservationGuestQueryDto class（訪客查詢用）
    BookingNumber (string), Email (string)


[]add Extensions
  路徑：EatTogether.API/Models/Extensions/ReservationDtoExtensions.cs

  []static ReservationListDto ToListDto(this Reservation r)
  []static ReservationDetailDto ToDetailDto(this Reservation r)
  []static StatusText 對照（0=訂位中 / 1=已報到 / 2=已取消 / 3=NoShow）
  []static StatusBadgeClass 對照（bg-primary / bg-success / bg-danger / bg-secondary）


[]add Repository
  路徑：EatTogether.API/Models/Repositories/

  []IReservationRepository interface
    GetByMemberIdAsync(int memberId)               // 會員歷史訂位
    GetByBookingNumberAndEmailAsync(string bookingNumber, string email)  // 訪客查詢
    GetByIdAsync(int id)
    CreateAsync(ReservationCreateDto dto, int? memberId)
    CancelAsync(int id)                            // Status=2, CancelledAt=now
    GetMaxSeqOfMonthAsync(int year, int month)     // BookingNumber 流水號用
    GetSessionBookedCountAsync(DateTime start, DateTime end) // 時段已訂人數
    GetTableAvailabilityAsync(DateTime date, TimeSpan time)  // 各桌型剩餘數
    IsConflictAsync(DateTime reservationDate, int? memberId) // 防重複訂位

  []ReservationRepository class（實作 IReservationRepository）
    **IsConflictAsync：±90 分鐘內同 MemberId（非 null）有 Status IN (0,1) → 衝突
    **GetSessionBookedCountAsync：Status IN (0,1)，±90 分鐘內加總 AdultsCount+ChildrenCount


[]add Service
  路徑：EatTogether.API/Models/Services/ReservationService.cs

  []CreateAsync 七道建立防線（同後台）
    防線① 訂位時間必須在現在起 30 分鐘後
    防線② 營業時間 11:00~19:45
    防線③ 分鐘只能選 00/15/30/45
    防線④ 總人數對應桌型（≤2=雙人桌 / ≤4=四人桌 / ≤6=六人桌 / ≤10=大桌）
    防線⑤ ±90 分鐘內同桌型組數不得超過該桌型總數
    防線⑥ ±90 分鐘內總人數不得超過總容量 70%
    防線⑦ 同一會員（MemberId!=null）±90 分鐘內不得重複訂位
    成功後自動產生 BookingNumber（R + 年後2碼 + 月2碼 + 流水號3碼，e.g. R260111006）
    成功後呼叫 IEmailService 寄送 HTML 確認信

  []CancelAsync
    檢查：訂位時間前 1 小時不可取消（可調整）
    檢查：僅 Status==0（訂位中）或 Status==1 可取消（已取消/NoShow 不可）
    若為會員訂位（MemberId!=null），驗證操作者身份
    若為訪客訂位（MemberId==null），驗證 BookingNumber + Email 匹配
    成功後寄送取消確認 Email

  []GetAvailabilityAsync(DateTime date, TimeSpan time, int adults, int children)
    回傳 AvailabilityDto（IsAvailable, RemainingCapacity, Message）

  []GetTableAvailabilityAsync(DateTime date, TimeSpan time)
    回傳各桌型剩餘數 List<TableTypeSlotDto>

  **BookingNumber 格式：R + yy + MM + seq(3碼)，seq 每月從 001 起算


[]add Program.cs 新增 DI 注冊
  []builder.Services.AddScoped<IReservationRepository, ReservationRepository>()
  []builder.Services.AddScoped<ReservationService>()
  []builder.Services.AddHostedService<ReservationReminderBackgroundService>()
  []builder.Services.AddHostedService<NoShowMarkingBackgroundService>()


[]add ReservationsController
  路徑：EatTogether.API/Controllers/ReservationsController.cs
  [Route("api/[controller]")] [ApiController]

  []GET  api/Reservations/Availability
    QueryString：date, time, adults, children
    回傳：AvailabilityDto
    **不需登入，公開 API

  []GET  api/Reservations/TableAvailability
    QueryString：date, time
    回傳：List<TableTypeSlotDto>（各桌型名稱、總數、可用數）
    **不需登入，公開 API

  []POST api/Reservations
    Body：ReservationCreateDto
    從 JWT Cookie 取 MemberId（未登入為 null）
    成功：回傳 { bookingNumber, message }
    失敗：回傳 400 + 錯誤訊息（對應七道防線）

  []GET  api/Reservations/My    [Authorize]
    從 JWT 取 MemberId
    回傳 List<ReservationListDto>，依 ReservationDate DESC 排序
    前端自行分成「即將到來」（Status IN (0,1)）與「歷史紀錄」（Status IN (2,3)）

  []GET  api/Reservations/Query（訪客查詢）
    QueryString：bookingNumber, email
    回傳 ReservationDetailDto 或 404
    **不需登入

  []PUT  api/Reservations/{id}/Cancel
    Body：{ bookingNumber?, email? }（訪客需帶；會員從 JWT 驗證）
    成功：回傳 { message }
    失敗：回傳 400 + 錯誤訊息

  []GET  api/Tables/{id}/QRCode
    回傳 PNG（QR Code 內容為前台訂位頁 URL + ?tableId={id}）
    使用 QRCoder 套件（NuGet: QRCoder）


[]add BackgroundService：訂位前 24hr 提醒 Email
  路徑：EatTogether.API/Models/Services/ReservationReminderBackgroundService.cs
  繼承 BackgroundService，每小時執行一次
  查詢 ReservationDate 在 23~25 小時後、Status IN (0,1) 的訂位
  透過 IEmailService 寄送提醒信（含訂位單號、日期時段、人數）
  **避免重複寄送：可加 ReminderSentAt 欄位或用快取記錄已寄 Id（選其一）

[]add BackgroundService：No-Show 自動標記
  路徑：EatTogether.API/Models/Services/NoShowMarkingBackgroundService.cs
  繼承 BackgroundService，每 5 分鐘執行一次
  查詢 ReservationDate < now - 10分鐘、Status==0（訂位中）的訂位 → Status=3（NoShow）
  **累積 NoShow 次數達 3 次：更新 Member.IsBlacklisted=true（限制訂位）
  **次數計算：COUNT WHERE MemberId=X AND Status=3


──── 前端 Vue ────

[]add 路由（router/index.js）
  []{ path: '/reservation',         name: 'Reservation',      component: BookingView.vue }
  []{ path: '/reservation/query',   name: 'ReservationQuery', component: ReservationQueryView.vue }
  []{ path: '/member/reservations', name: 'MyReservations',   component: MyReservationsView.vue, meta: { requiresAuth: true } }


[]add 線上訂位頁
  路徑：EatTogether.Web/src/views/Reservation/BookingView.vue

  []日期選擇（vue-datepicker，限今日起 + 90 天內）
  []時段選擇：小時 11~19 下拉 + 分鐘 00/15/30/45 下拉
  []人數選擇：大人（1~10）+ 小孩（0~10）數字輸入
  []姓名、電話、Email、備註欄位

  []即時可用性回饋（人數或時段變動時觸發）
    watch：adults + children + date + hour + minute
    debounce 500ms 後呼叫 GET api/Reservations/Availability
    顯示：有空位（綠色提示）/ 無空位（紅色提示）+ 剩餘容量文字

  []各桌型剩餘空位數顯示
    日期時段確定後呼叫 GET api/Reservations/TableAvailability
    以卡片或表格顯示各桌型（雙人桌 x2 剩 / 四人桌 x3 剩...）

  []表單送出前七道驗證（前端攔截，顯示友善錯誤訊息）
    ① 訂位時間必須在 30 分鐘後
    ② 營業時間 11:00~19:45
    ③ 分鐘只能選 00/15/30/45（下拉限制已做到）
    ④ 人數需 1~10 人
    ⑤ 姓名必填（2~50字）
    ⑥ 手機格式（09xxxxxxxx）
    ⑦ Email 格式驗證（選填但填了要格式正確）

  []送出成功：顯示成功 Modal
    顯示訂位單號（BookingNumber）
    提示確認信已寄至 Email
    提供「查詢訂位」按鈕（帶入 bookingNumber query）

  []QR Code 功能
    若 URL 帶有 ?tableId=xxx，自動帶入桌號顯示於表單上方
    安裝套件：npm install qrcode.vue（後台 API 也生成 QR Code 圖片供列印）


[]add 訪客查詢訂位頁
  路徑：EatTogether.Web/src/views/Reservation/ReservationQueryView.vue

  []輸入 BookingNumber + Email 查詢
  []顯示訂位明細（單號、日期時段、人數、狀態）
  []狀態=訂位中 且 距訂位時間 > 1 小時 → 顯示「取消訂位」按鈕
  []取消確認 Dialog → 送出 PUT api/Reservations/{id}/Cancel
  []取消成功顯示提示，並說明取消確認信已寄出


[]add 我的訂位頁（需登入）
  路徑：EatTogether.Web/src/views/Reservation/MyReservationsView.vue

  []onMounted 呼叫 GET api/Reservations/My
  []分兩區塊：
    即將到來（Status 0 或 1，ReservationDate >= today）
      顯示：訂位單號、日期時段、人數、狀態 Badge
      Status==0 且距訂位 > 1 小時 → 顯示「取消訂位」按鈕
    歷史紀錄（Status 2 或 3，或 ReservationDate < today）
      顯示：訂位單號、日期時段、人數、狀態（已取消/已報到/No-Show）
  []取消流程同訪客查詢頁（Dialog 確認 → API → 重新 fetch）
  []空資料時顯示友善提示（「尚無訂位紀錄，立即訂位！」）


====================優惠券模組（Coupon）====================

**DiscountType：0=折金額 / 1=打折%
**MemberCoupon.IsUsed：false=可使用 / true=已使用（過期由前端判斷 EndDate）


──── 後端 API ────

[]add DTOs（確認現有 DTOs 是否符合需求，必要時補充）

  **以下 DTOs 已存在（dev 合併），確認欄位後決定是否需要擴充：
  [V] CouponDto（Id, Name, Code, DiscountType, DiscountValue, MinSpend,
                 StartDate, EndDate?, LimitCount?, ReceivedCount?, IsDisabled
                 + DiscountTypeText, DiscountDescription, IsExpired, StatusText, StatusBadgeClass）
  [V] CouponValidateDto（IsValid, CouponId?, CouponName, Discount, Message）
  [V] MemberCouponDto（含 IsUsed, UsedDate?, ClaimedAt?, DiscountDescription,
                       IsExpired, StatusText, StatusBadgeClass）

  []CouponClaimResultDto class（若現有 CouponDto 不足則補充）
    Success (bool), Message (string), CouponId (int?)

  []BirthdayCheckDto class
    HasBirthdayCoupon (bool)
    CouponName (string?)
    Code (string?)
    DiscountDescription (string?)
    EndDate (DateTime?)
    DaysUntilExpiry (int?)


[]add Service：CouponService（前台版）
  路徑：EatTogether.API/Models/Services/CouponService.cs
  **若 dev 上已有 CouponService，確認後補充前台需要的方法

  []GetAvailableCouponsAsync(int? memberId)
    查詢 IsDisabled==false, Now >= StartDate, EndDate==null||Now<=EndDate
    若 memberId 有值，標記 IsClaimed（該會員是否已領過）
    **IsClaimed 計算：MemberCoupons 中存在 MemberId==memberId && CouponId==c.Id

  []ClaimCouponAsync(int couponId, int memberId)
    防線① 優惠券不存在或已停用 → Fail
    防線② 活動尚未開始或已過期 → Fail
    防線③ 已達限量（ReceivedCount >= LimitCount）→ Fail
    防線④ 此會員已領過 → Fail（GetByMemberAndCouponAsync 檢查）
    成功後 MemberCoupons.Add + IncrementReceivedCountAsync

  []GetMyCouponsAsync(int memberId)
    回傳 List<MemberCouponDto>，前端依 IsUsed + IsExpired 分三狀態：
      可使用：IsUsed==false && !IsExpired
      已使用：IsUsed==true
      已過期：IsUsed==false && IsExpired

  []ValidateCouponAsync(string code, int? memberId, decimal orderAmount)
    複用後台 RedeemCoupon 六道防線邏輯
    **不核銷，只驗證 + 回傳折扣金額（核銷在結帳完成後才觸發）

  []GetBirthdayCheckAsync(int memberId)
    查詢 Member.BirthDate 月份 == 今日月份
    若是生日月，查詢 Code 前綴 BDAY + 本月的優惠券
    檢查該會員是否已領取（MemberCoupons）
    回傳 BirthdayCheckDto


[]add Program.cs 新增 DI 注冊
  []builder.Services.AddScoped<CouponService>()（若尚未註冊）
  []builder.Services.AddHostedService<CouponExpiryNotifyBackgroundService>()


[]add CouponsController
  路徑：EatTogether.API/Controllers/CouponsController.cs
  [Route("api/[controller]")] [ApiController]

  []GET  api/Coupons
    QueryString：discountType? (0/1), birthdayOnly? (bool)
    從 JWT Cookie 取 memberId（未登入為 null）
    回傳 List<CouponDto>（含 IsClaimed 標記）
    **不需登入，公開 API；但有 memberId 才能標記 IsClaimed

  []POST api/Coupons/{id}/Claim    [Authorize]
    從 JWT 取 memberId
    呼叫 CouponService.ClaimCouponAsync
    成功：回傳 { success: true, message: "領取成功" }
    失敗：回傳 400 + 錯誤訊息（對應防線）

  []GET  api/Coupons/My    [Authorize]
    從 JWT 取 memberId
    回傳 List<MemberCouponDto>

  []POST api/Coupons/Validate
    Body：{ code, orderAmount }
    從 JWT Cookie 取 memberId（未登入為 null）
    呼叫 CouponService.ValidateCouponAsync
    回傳 CouponValidateDto

  []GET  api/Coupons/BirthdayCheck    [Authorize]
    從 JWT 取 memberId
    回傳 BirthdayCheckDto
    **前端 App.vue onMounted 登入後呼叫，決定是否顯示 Banner

  []GET  api/Coupons/My/UsageHistory    [Authorize]
    從 JWT 取 memberId
    回傳 List<MemberCouponDto>，僅 IsUsed==true
    含 UsedDate、折抵金額（DiscountDescription）、對應訂單資訊


[]add BackgroundService：優惠券到期前 3 天提醒 Email
  路徑：EatTogether.API/Models/Services/CouponExpiryNotifyBackgroundService.cs
  繼承 BackgroundService，每天固定時間（e.g. 早上 9:00）執行
  查詢 EndDate 在 3 天內、IsDisabled==false 的優惠券
  查詢各優惠券中 IsUsed==false 且未過期的 MemberCoupon
  透過 IEmailService 寄送到期提醒信給對應會員
  **避免重複寄送：記錄已寄的 CouponId + MemberId 組合（可用快取或 DB flag）


──── 前端 Vue ────

[]add 路由（router/index.js）
  []{ path: '/coupons',             name: 'CouponList',  component: CouponListView.vue }
  []{ path: '/member/coupons',      name: 'MyCoupons',   component: MyCouponsView.vue,   meta: { requiresAuth: true } }
  []{ path: '/member/coupon-usage', name: 'CouponUsage', component: CouponUsageView.vue, meta: { requiresAuth: true } }


[]add 優惠券列表頁
  路徑：EatTogether.Web/src/views/Coupon/CouponListView.vue

  []onMounted 呼叫 GET api/Coupons（帶 memberId，由 authStore 取得）
  []分類篩選：
    折扣類型：全部 / 折金額 / 折百分比（DiscountType 0/1）
    生日專屬：勾選後只顯示 Code 前綴 BDAY 的券
  []v-for 渲染優惠券卡片（CouponCard.vue）
    券名、折扣內容（DiscountDescription）、有效期限、剩餘數量、最低消費
    已登入且 IsClaimed==true → 顯示「已領取」Badge，按鈕 disabled
    已登入且 IsClaimed==false → 「立即領取」按鈕
    未登入 → 「登入後領取」按鈕（點擊觸發 AuthModal）

  []一鍵領取流程
    POST api/Coupons/{id}/Claim
    成功 → 按鈕變「已領取」disabled（樂觀更新 + 後端確認）
    失敗 → Toast 顯示錯誤訊息（已領取 / 已達限量 / 已過期等）
    **使用 Toast 元件（components/common/Toast.vue）


[]add 我的優惠券頁（需登入）
  路徑：EatTogether.Web/src/views/Coupon/MyCouponsView.vue

  []onMounted 呼叫 GET api/Coupons/My
  []三狀態分頁（Tab 切換）
    可使用：IsUsed==false && !IsExpired
    已使用：IsUsed==true
    已過期：IsUsed==false && IsExpired
  []每張券顯示：券名、折扣碼（Code）、折扣內容、有效期限
  []可使用狀態顯示到期倒數（剩 N 天）
  []空資料友善提示


[]add 折扣碼驗證框元件
  路徑：EatTogether.Web/src/components/Coupon/CouponRedeemInput.vue
  **供結帳流程（Order/In.vue 等）引入使用

  []輸入折扣碼欄位 + 「套用」按鈕
  []點擊套用 → POST api/Coupons/Validate { code, orderAmount }
  []IsValid==true → 顯示折扣金額（綠色提示：「已折扣 NT$xxx」）
  []IsValid==false → 顯示錯誤（紅色提示：Message 內容）
  []確認結帳後帶入 couponCode 至訂單 API，後端完成核銷
  []emit('coupon-applied', { couponId, discount }) 供父元件監聽


[]add 生日 Banner 元件
  路徑：EatTogether.Web/src/components/Coupon/BirthdayBanner.vue

  []App.vue 或 Navbar.vue 中，登入成功後呼叫 GET api/Coupons/BirthdayCheck
  []HasBirthdayCoupon==true → 渲染生日 Banner（醒目樣式）
    顯示：券名、折扣碼、折扣內容、到期倒數（DaysUntilExpiry 天後到期）
    提供「前往領取」快捷按鈕（導至 /coupons 並帶 ?birthday=true 篩選）
  []Banner 可手動關閉（sessionStorage 記錄，同次訪問不重複顯示）


[]add 優惠券使用明細頁（需登入）
  路徑：EatTogether.Web/src/views/Coupon/CouponUsageView.vue

  []onMounted 呼叫 GET api/Coupons/My/UsageHistory
  []列表顯示：券名、使用時間（UsedDate）、折抵金額（DiscountDescription）、對應訂單
  []空資料友善提示


====================跨模組整合====================

與 Auth 模組（authStore）
  訂位：未登入也可訂位，登入後查詢 My 紀錄
  優惠券：未登入可瀏覽，登入後才能領取
  生日 Banner：登入後 App.vue onMounted 觸發 BirthdayCheck

與 Order 模組（Order/In.vue）
  CouponRedeemInput.vue 在結帳時引入
  emit coupon-applied 事件 → 父元件帶入訂單送出

與 Navbar.vue
  新增「訂位」連結（已在 Navbar 定義，路由補上即可）
  登入後「會員」下拉選單加入「我的訂位」、「我的優惠券」連結

與 QR Code
  每桌 QR Code 掃描後帶入 ?tableId → BookingView.vue 顯示桌號提示
  後端 GET api/Tables/{id}/QRCode 回傳 PNG 供後台列印


====================NuGet / npm 套件需求====================

後端需新增：
  []QRCoder（NuGet）— QR Code 圖片生成

前端需新增：
  **vue-datepicker 已安裝（訂位日期選擇用）
  **無須另安裝 QR Code 套件（由後端 API 生成圖片，前端 <img> 顯示）


====================新增檔案總覽====================

後端（EatTogether.API）：
  Models/DTOs/
    ReservationCreateDto.cs
    ReservationListDto.cs
    ReservationDetailDto.cs
    AvailabilityDto.cs（含 TableTypeSlotDto）
    ReservationGuestQueryDto.cs
    CouponClaimResultDto.cs
    BirthdayCheckDto.cs

  Models/Extensions/
    ReservationDtoExtensions.cs

  Models/Repositories/
    IReservationRepository.cs
    ReservationRepository.cs

  Models/Services/
    ReservationService.cs
    ReservationReminderBackgroundService.cs
    NoShowMarkingBackgroundService.cs
    CouponService.cs（補充前台方法）
    CouponExpiryNotifyBackgroundService.cs

  Controllers/
    ReservationsController.cs
    CouponsController.cs


前端（EatTogether.Web/src）：
  views/Reservation/
    BookingView.vue
    ReservationQueryView.vue
    MyReservationsView.vue

  views/Coupon/
    CouponListView.vue
    MyCouponsView.vue
    CouponUsageView.vue

  components/Coupon/
    CouponCard.vue
    CouponRedeemInput.vue
    BirthdayBanner.vue

  router/index.js（補充 5 條路由）
  Program.cs（補充 DI 注冊）
