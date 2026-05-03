# Smart Manufacturing Logic Validation & Process Integration Demo (智慧製造邏輯驗證與進程整合演示系統)

本專案實作了一套基於 **Sidecar (側掛式)** 架構的工業應用演示系統。前端採用 **C# WinForms** 負責人機介面 (HMI) 與進程管理，後端採用 **Python FastAPI** 負責核心業務邏輯驗證。

---

## 核心技術點

### 1. 進程自動化生命週期管理 (Process Lifecycle Management)
系統實作了父子進程的強綁定控制，確保軟體運行環境的整潔：
* **動態相對路徑定標**：利用 `AppDomain.CurrentDomain.BaseDirectory` 自動偵測當前目錄下的 Python 執行檔，實作零設定 (Zero-config) 部署。
* **進程樹遞迴清理 (Recursive Process Tree Termination)**：在 C# 端關閉時，呼叫 `pyService.Kill(true)`，確保強制終止包含 Uvicorn 核心在內的所有子進程，防止埠口 (Port) 殘留。
* **禁用 Shell 代理 (Direct Process Attachment)**：將 `UseShellExecute` 設為 `false`，建立主程序對副程序的直接核心控制柄 (Handle) 映射，提高控制精度。

### 2. 異步握手與防禦性介面 (Async Handshake & Defensive UI)
為了解決後端模型或伺服器啟動耗時導致的時序衝突問題，實作了非同步初始化機制：
* **健康檢查輪詢 (Health Check Polling)**：透過 `CheckHealthAsync` 持續探測後端 API 埠口狀態。
* **介面狀態鎖定 (UI State Interlocking)**：在連線成功建立前，鎖定所有輸入控制項 (Enabled = false)，從物理層面杜絕操作員在系統未就緒時進行無效輸入。
* **非同步通訊 (Asynchronous Communication)**：採用 `HttpClient` 配合 `Task<string>`，確保大規模矩陣運算或網路延遲期間 UI 介面不產生凍結 (Freezing)。

### 3. 業務邏輯外包與數據規格定義 (Logic Outsourcing & Schema Validation)
捨棄在 C# 端寫死判斷邏輯的傳統做法，改為單一事實來源 (Single Source of Truth) 架構：
* **規格導向校驗 (Pydantic Schema)**：利用 `BaseModel` 定義數據合約，將 Sensor ID 格式與 RPM 物理閾值判定全數收回到 Python 端。
* **異常元素提取 (Precise JSON Extraction)**：利用 `JsonDocument` 定位 FastAPI 回傳的 `detail` 陣列，精確提取 `msg` 報錯資訊，實作乾淨的錯誤提示。

---

## 組件職責定義

| 組件名稱 | 技術棧 | 實質行為職責 |
| :--- | :--- | :--- |
| **HMI 介面層 (Form1.cs)** | C# WinForms | 負責進程啟停、UI 狀態機管理、使用者數據採集與結果呈現。 |
| **通訊代理層 (Class1.cs)** | C# HttpClient | 負責封裝 JSON 序列化、HTTP POST 請求發送及連線異常攔截。 |
| **運算邏輯層 (fastapiDEMO.py)** | FastAPI / Pydantic | 負責數據合法性判定 (Validator) 與虛擬推理邏輯執行。 |

---

## 數據校驗規範

系統後端對傳入數據執行以下攔截：
1.  **Sensor ID**：必須以 `CNC` 為字首開頭，否則回傳驗證錯誤。
2.  **RPM (轉速)**：數值必須介於 `0` 至 `10000` 之間，並過濾 NaN 或非整數格式。
3.  **JSON 完整性**：攔截無效的字串結構 (json_invalid)，並回傳解析失敗訊息。
