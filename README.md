# .NET 10 同步 vs 非同步教學示範

## 📖 簡介

這是一份完整的 C# async/await 教學，包含 10 個可執行的範例，從基礎到進階。所有代碼均可在 .NET 10 Console 應用中直接運行，無需外部依賴。

## 📂 項目結構

```
AsyncAwaitTutorial/
├── Program.cs          # 主程式（包含所有 10 個範例）
├── README.md           # 本文件
├── GUIDE.md            # 完整教學指南
└── AsyncAwaitTutorial.csproj
```

## 🚀 快速開始

### 環境要求

- .NET 10.0 SDK 或更高版本
- 任何文字編輯器或 Visual Studio Code

### 運行程式

```bash
# 進入專案目錄
cd AsyncAwaitTutorial

# 編譯並運行
dotnet run

# 或先編譯後運行
dotnet build
dotnet ./bin/Debug/net10.0/AsyncAwaitTutorial.dll
```

### 互動菜單

程式會顯示菜單，選擇 1-10 執行對應的範例，或選擇 0 退出：

```
【選擇要執行的範例】
簡單範例:
  1. 同步 vs 非同步檔案讀寫 (I/O Bound)
  2. 使用 Task.Delay 模擬 I/O 操作
  3. 避免使用 .Result / .Wait() 導致死鎖
  4. async void vs async Task 的差異
  5. 多任務並行：Task.WhenAll vs 順序等待
中階範例:
  6. 多任務並行 + 限流 + CancellationToken
  7. 非同步例外處理與多任務
  8. Producer-Consumer 並行度控制
進階範例:
  9. 管線化處理 + 背壓控制 (Pipeline)
 10. I/O Bound vs CPU Bound 效能對比
  0. 結束程式
```

## 📚 10 個範例詳解

### 簡單範例

#### 1. 同步 vs 非同步檔案讀寫
- **主題**：I/O Bound 操作的同步和非同步對比
- **關鍵概念**：
  - File.ReadAllText（同步，阻塞）
  - File.ReadAllTextAsync（非同步，不阻塞）
  - 多個檔案並行讀取的優勢
- **預期輸出**：展示非同步多檔案讀取比同步快
- **實用性**：高（實際 Web 應用中常見）

#### 2. 使用 Task.Delay 模擬 I/O 操作
- **主題**：Task.Delay vs Thread.Sleep
- **關鍵概念**：
  - Thread.Sleep 阻塞線程
  - Task.Delay 不阻塞線程
  - 順序執行 vs 並行執行的耗時差異
- **預期輸出**：順序 2000ms，並行 1000ms
- **實用性**：高（測試非同步代碼的常見方式）

#### 3. 避免使用 .Result / .Wait() 導致死鎖
- **主題**：死鎖陷阱和正確做法
- **關鍵概念**：
  - `.Result` / `.Wait()` 阻塞線程
  - async 方法需要回到原線程上下文
  - 線程阻塞 → async 無法執行 → 死鎖
- **預期輸出**：演示 await 的正確做法
- **實用性**：非常高（最常見的陷阱）

#### 4. async void vs async Task
- **主題**：返回類型選擇的重要性
- **關鍵概念**：
  - async void：例外無法被捕捉
  - async Task：例外可被捕捉
  - async void 只應用於事件處理器
- **預期輸出**：展示例外處理的差異
- **實用性**：非常高（防止應用崩潰）

#### 5. 多任務並行：Task.WhenAll vs 順序等待
- **主題**：並行化加速
- **關鍵概念**：
  - 順序：await 多次
  - 並行：Task.WhenAll
  - 效能對比（3x 加速）
- **預期輸出**：順序 1500ms，並行 500ms
- **實用性**：高（基本優化技巧）

### 中階範例

#### 6. 多任務並行 + 限流 + CancellationToken
- **主題**：實際應用中的並行控制
- **關鍵概念**：
  - SemaphoreSlim 限制併行度
  - CancellationToken 支持取消
  - 避免資源耗盡
- **預期輸出**：展示每次最多 2 個並行任務
- **實用性**：非常高（生產環境必需）

#### 7. 非同步例外處理與多任務
- **主題**：多任務執行時的例外策略
- **關鍵概念**：
  - Task.WhenAll 遇到例外時的行為
  - 區分成功和失敗任務
  - 靈活的例外處理
- **預期輸出**：展示 3 種例外處理方式
- **實用性**：高（API 聚合、批量操作）

#### 8. Producer-Consumer 並行度控制
- **主題**：生產者/消費者模式
- **關鍵概念**：
  - Channel<T> 用於線程安全通信
  - 背壓（緩衝滿時自動等待）
  - 解耦生產和消費速度
- **預期輸出**：展示生產者和消費者的並行執行
- **實用性**：非常高（消息隊列、流處理）

### 進階範例

#### 9. 管線化處理 + 背壓控制
- **主題**：多階段數據處理
- **關鍵概念**：
  - 3 個階段（生產、轉換、消費）
  - 有限大小的 Channel 實現背壓
  - 防止記憶體溢出
- **預期輸出**：展示各階段並行執行
- **實用性**：非常高（ETL、數據管道）

#### 10. I/O Bound vs CPU Bound 效能對比
- **主題**：何時用 async，何時用 Task.Run
- **關鍵概念**：
  - I/O Bound：async/await 有幫助
  - CPU Bound：async 無幫助，需 Task.Run
  - 效能測量和決策
- **預期輸出**：I/O 並行 3x 加速，CPU 多線程 3~4x 加速
- **實用性**：非常高（架構決策）

## 📋 核心概念速查表

| 概念 | 說明 | 使用場景 |
|------|------|---------|
| `async Task<T>` | 返回 Task 的非同步方法 | I/O 操作、Web API |
| `await` | 等待 Task 完成，不阻塞線程 | async 方法內部 |
| `Task.WhenAll` | 等待所有 Task 完成 | 並行多個操作 |
| `Task.Delay` | 非阻塞延遲 | 模擬 I/O，超時控制 |
| `SemaphoreSlim` | 限制併行度 | 限流、限制連接數 |
| `CancellationToken` | 取消信號 | 支持優雅取消 |
| `Channel<T>` | 線程安全隊列 | Producer-Consumer |
| `Task.Run` | 在線程池執行同步代碼 | CPU Bound 操作 |

## ⚠️ 常見陷阱和解決方案

### 陷阱 1：使用 `.Result` 導致死鎖

```csharp
// ❌ 死鎖
var data = FetchDataAsync().Result;

// ✓ 正確
var data = await FetchDataAsync();
```

### 陷阱 2：async void 無法捕捉例外

```csharp
// ❌ 例外會應用崩潰
async void OnClick() => throw new Exception();

// ✓ 正確
async Task OnClick() => throw new Exception();
try { await OnClick(); } catch { }
```

### 陷阱 3：遺漏 await

```csharp
// ❌ 任務不執行
FetchDataAsync();
ProcessData();

// ✓ 正確
await FetchDataAsync();
ProcessData();
```

### 陷阱 4：CPU Bound 用 async 無幫助

```csharp
// ❌ 無並行，無性能提升
public async Task<long> Compute() => Fibonacci(30);

// ✓ 正確
public Task<long> Compute() => Task.Run(() => Fibonacci(30));
```

## 🎯 學習路徑建議

1. **第一周**：執行並理解範例 1-3
   - 理解 async/await 語法
   - 避免常見陷阱

2. **第二周**：執行範例 4-5
   - async Task vs async void
   - 並行化基礎

3. **第三周**：執行範例 6-8
   - 限流、CancellationToken
   - 異常處理

4. **第四周**：執行範例 9-10
   - 進階模式
   - 效能優化決策

## 🔗 深入學習

完整的教學指南請看 [GUIDE.md](GUIDE.md)，包含：

- 為什麼用同步/非同步（各 3 點）
- 適合用非同步的情境（6+ 種）
- 不適合用非同步的情況（4+ 種）
- 不適合用同步的情況（4+ 種）
- 架構決策指南
- 效能數據示例

## 🐛 故障排除

### Q: 編譯錯誤 "類型推斷失敗"

**A:** 確保使用 .NET 10 SDK
```bash
dotnet --version  # 應顯示 10.0.x
```

### Q: 程式執行後沒有輸出

**A:** 可能是輸入沒有正確讀取，確保選擇 0-10 之間的數字

### Q: 某個範例運行很慢

**A:** 這是正常的，某些範例有意延遲（如 Task.Delay），以展示並行效果

## 📝 修改和擴展

您可以修改程式碼進行實驗：

1. **修改延遲時間**：改變 Task.Delay 的毫秒數
2. **修改並行度**：改變 SemaphoreSlim 的初始值
3. **添加新例子**：在 Program.cs 中添加新的 async Task 方法

## 📄 開源許可

本教程代碼免費提供，可自由使用和修改。

## 👨‍🏫 關於本教程

- **難度**：初級到中級
- **先決知識**：C# 基礎（類、方法、Lambda）
- **預期學習時間**：4-6 周
- **實踐重要性**：非常高（必須動手執行代碼）

---

**祝您學習愉快！**

如有問題或建議，歡迎反饋。
