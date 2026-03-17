##### <u>QuotationSystem - 報價管理系統
</u>這是一個基於 ASP.NET Core 8.0 Web API 開發的報價管理後端系統。本專案展示如何處理複雜的主附表（Master-Detail）關聯、自動化編號生成，以及符合 RESTful 規範的 API 設計。

###### 使用技術
- 系統架構：採用 Service Pattern 實現開發關注點分離（SoC），確保 Controller 簡潔且商業邏輯可重複使用。
- 依賴注入 (DI)：透過 Interface 層（IQuotationService）實作依賴反轉，提升系統的可測試性與擴充性。
- 精確金額計算：
1. 使用 decimal 型別避免浮點數誤差。
2. 後端自動計算 5% 營業稅（四捨五入）與各項小計，確保帳務準確。
3. 自動編號邏輯：實作 QTN-yyyyMMxx 格式的自動流水號生成算法。
4. API 文檔化：完整整合 Swagger (OpenAPI) 並包含詳細的 XML 中文註解，方便前後端對接。

###### 技術棧
- Framework: .NET 8.0 (ASP.NET Core Web API)
- ORM: Entity Framework Core
- Database: SQLite / SQL Server (支援多種提供者)
- Documentation: Swagger (Swashbuckle)
- Validation: Data Annotations & Model Validation

##### API 接口說明

| Action |Route |Description |
| - | - | - |
|POST |/api/Quotations |建立報價單 |
|GET |/api/Quotations |取得所有報價單清單或依單號取得特定報價單詳細明細 |
|PUT |/api/Quotations |修改報價單內容 |
|DELETE |/api/Quotations | 刪除報價單|
