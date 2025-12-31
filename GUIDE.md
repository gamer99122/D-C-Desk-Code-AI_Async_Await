# .NET 10 同步 vs 非同步教學完整指南

## 📚 概述

本教程涵蓋 10 個循序漸進的範例，講解 C# 中的 async/await 同步和非同步編程。

### 範例分類

- **簡單範例（5個）**：基礎概念和常見陷阱
- **中階範例（3個）**：實際應用中的複雜場景
- **進階範例（2個）**：效能優化和高級模式

---

## 🎯 為什麼要用同步 vs 非同步？

### 為什麼要用非同步？

1. **不阻塞線程，提高資源利用率**
   - 同步操作會讓線程陷入等待
   - 非同步操作讓線程在等待期間處理其他工作
   - 例如：Web 伺服器用 1 個線程可服務數千個 I/O 操作

2. **提高吞吐量和響應能力**
   - I/O Bound 操作（網路、檔案、資料庫）可以並行化
   - 多個 I/O 操作的等待時間可以重疊
   - 例如：3 個 500ms 的 API 呼叫，順序執行 1500ms，並行執行 500ms

3. **支持可擴展性（Scalability）**
   - 同步方式：N 個並發請求需要 N 個線程（每個線程消耗記憶體和 OS 資源）
   - 非同步方式：N 個並發請求用少量線程處理（線程池會自動管理）
   - 現代伺服器可輕鬆處理數萬個並發連接

### 為什麼要用同步？

1. **簡化邏輯和代碼可讀性**
   - 同步代碼執行順序明確，易於理解和調試
   - 非同步需要理解 async/await、Task 等概念
   - 例如：簡單的命令行工具，同步就足夠

2. **避免複雜性和上下文切換開銷**
   - 非同步涉及狀態機、上下文捕捉、線程切換
   - 對於小規模應用，這些開銷可能超過收益
   - CPU Bound 操作（計算密集）用非同步無幫助

3. **某些 API 只有同步版本或不支持非同步**
   - 某些舊庫只提供同步方法
   - 某些場景（如某些編譯器、編譯任務）不支持非同步
   - 此時只能用同步或用 Task.Run 包裝

---

## ✅ 什麼情境適合用非同步？

### 1. 網路通信

- **HTTP API 請求**：用 HttpClient 發送請求，等待響應
  ```csharp
  var response = await httpClient.GetAsync(url);
  ```
  - 為什麼：網路延遲通常 100ms～1s，應用線程不應被阻塞
  - 效果：多個 API 呼叫可並行

- **資料庫查詢**：連接、查詢、讀取數據都是 I/O
  ```csharp
  var data = await connection.QueryAsync(sql);
  ```
  - 為什麼：資料庫延遲，線程應釋放給其他請求
  - 效果：同一數據庫連接可服務多個併發查詢

- **WebSocket / SignalR 實時通信**
  ```csharp
  await connection.SendAsync("method", arg);
  ```

- **gRPC 呼叫**：基於 HTTP/2 的服務間通信

### 2. 檔案 I/O

- **讀寫大檔案**
  ```csharp
  var content = await File.ReadAllTextAsync(path);
  ```
  - 為什麼：磁盤 I/O 不確定，等待期間線程應自由
  - 場景：Web 上傳檔案、日誌系統、資料匯出

- **流式處理檔案**
  ```csharp
  using (var stream = File.OpenRead(path))
  {
      var buffer = new byte[4096];
      int read = await stream.ReadAsync(buffer, 0, buffer.Length);
  }
  ```
  - 為什麼：大檔案不一次讀完，分批讀取避免記憶體溢出

### 3. 並發操作控制

- **多個獨立任務並行執行**
  ```csharp
  var results = await Task.WhenAll(task1, task2, task3);
  ```
  - 為什麼：加快總耗時
  - 場景：聚合多個 API、併行下載、批量處理

- **限流（Rate Limiting）**
  ```csharp
  await semaphore.WaitAsync();
  try { ... } finally { semaphore.Release(); }
  ```
  - 為什麼：控制並行度避免資源耗盡

- **超時控制**
  ```csharp
  using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  await operation.WithCancellation(cts.Token);
  ```

### 4. UI / 前端主線程

- **WPF / WinForms 更新 UI**
  ```csharp
  private async void OnButtonClick(object sender, EventArgs e)
  {
      var data = await FetchDataAsync();
      textBox.Text = data;  // UI 線程更新 UI
  }
  ```
  - 為什麼：UI 線程被 I/O 阻塞會卡頓
  - 效果：UI 響應流暢，使用者不感到卡

- **Blazor / Web 前端非同步**
  ```csharp
  var data = await HttpClient.GetJsonAsync<Data>(url);
  ```

### 5. 事件驅動和響應式編程

- **事件處理中的非同步操作**
  ```csharp
  button.Click += async (s, e) =>
  {
      await ProcessAsync();
  };
  ```
  - 為什麼：事件可能頻繁觸發，非同步避免線程飢餓

- **Reactive Extensions (Rx)**
  ```csharp
  observable.SelectMany(x => FetchAsync(x));
  ```

### 6. Web API / ASP.NET

- **處理 HTTP 請求**
  ```csharp
  [HttpGet]
  public async Task<IActionResult> Get(int id)
  {
      var data = await _db.GetAsync(id);
      return Ok(data);
  }
  ```
  - 為什麼：ASP.NET 線程池有限，每個請求一個線程，非同步讓線程回到池中
  - 效果：1000 個併發請求用少量線程處理，提高伺服器吞吐量

---

## ❌ 什麼情況不適合/不能用非同步？

### 1. CPU Bound 操作（計算密集）

- **問題**：非同步無幫助，因為操作不涉及等待
  ```csharp
  // ❌ 不適合用 async
  public async Task<long> ComputeAsync(int n)
  {
      return Fibonacci(n);  // 純計算，沒有等待
  }
  ```

- **正確做法**：用 `Task.Run` 多線程
  ```csharp
  // ✓ 正確
  public Task<long> ComputeAsync(int n)
  {
      return Task.Run(() => Fibonacci(n));
  }
  ```

- **場景**：
  - 複雜數學計算
  - 圖片處理、視頻編碼
  - JSON 序列化大量數據
  - 數據彙總、統計分析

- **原因**：
  - 計算期間線程不會釋放，不阻塞只是浪費
  - 多線程才能充分利用 CPU 核心
  - async 的狀態機開銷沒有回報

### 2. 必須同步的上下文

- **某些同步上下文**
  ```csharp
  // ❌ 不能用 async
  public string GetData()
  {
      return FetchDataAsync().Result;  // 死鎖風險！
  }
  ```

- **為什麼不能用**：
  - async 方法可能期望回到原線程上下文（如 UI 線程）
  - `.Result` / `.Wait()` 阻塞線程，原線程無法執行 async 方法
  - 結果：死鎖

- **必須同步的場景**：
  - Console 應用的 Main 方法（.NET Framework）
  - 某些庫強制要求同步 API
  - 某些同步上下文（如 Entity Framework 的舊版本）

- **解決方案**：
  - .NET 5+ 的 Main 可以是 async
  - 用 `await` 而不是 `.Result`
  - 用 `Task.Run` 在背景線程執行再取 `.Result`（不常見）

### 3. 需要確定性執行順序且無併行

- **情況**：操作之間有依賴，必須順序執行
  ```csharp
  // ✓ 無須優化為非同步
  var user = await GetUserAsync(id);
  var posts = await GetPostsAsync(user.Id);  // 依賴 user
  var comments = await GetCommentsAsync(posts[0].Id);  // 依賴 posts
  ```

- **為什麼**：
  - 無法並行，非同步沒有加速效果
  - 代碼複雜性增加，收益零
  - 同步寫法反而更清楚

- **例外**：如果多個操作無依賴，應並行
  ```csharp
  // ✓ 應用並行
  var userTask = GetUserAsync(id);
  var settingsTask = GetSettingsAsync(id);
  await Task.WhenAll(userTask, settingsTask);
  ```

### 4. 簡單同步任務，不涉及等待

- **場景**：
  - 資料驗證、格式化
  - 本地計算、字符串處理
  - 簡單的業務邏輯判斷

  ```csharp
  // ❌ 不需要 async
  public async Task<bool> ValidateEmailAsync(string email)
  {
      return email.Contains("@");  // 沒有等待，不應 async
  }

  // ✓ 正確
  public bool ValidateEmail(string email)
  {
      return email.Contains("@");
  }
  ```

- **為什麼**：
  - 無 I/O，無需非同步
  - 增加複雜性，無收益
  - 返回類型應是 `bool` 而不是 `Task<bool>`

---

## ❌ 什麼情況不適合/不能用同步？

### 1. 高並發 Web 伺服器環境

- **問題**：每個請求一個同步線程，耗盡線程池
  ```csharp
  // ❌ 在 Web 伺服器中是災難
  [HttpGet]
  public IActionResult GetUser(int id)
  {
      var data = _db.Get(id);  // 阻塞線程
      var orders = _api.GetOrders(id);  // 再次阻塞
      return Ok(new { data, orders });
  }
  ```

- **後果**：
  - 1000 併發請求需要 1000 個線程
  - 記憶體爆炸（每個線程 1MB～2MB）
  - 線程上下文切換導致 CPU 低效
  - 應用崩潰或無響應

- **場景**：
  - ASP.NET / ASP.NET Core API
  - 高流量 Web 服務
  - 微服務架構
  - 實時應用（WebSocket）

### 2. UI 應用的長時間 I/O 操作

- **問題**：UI 線程被阻塞，介面卡頓
  ```csharp
  // ❌ UI 會卡住
  private void OnButtonClick(object sender, EventArgs e)
  {
      var data = FetchDataFromDB();  // UI 線程被阻塞 5 秒
      textBox.Text = data;  // 用戶感到卡頓
  }
  ```

- **後果**：
  - 使用者無法互動（點擊、輸入無反應）
  - 視覺上似乎應用凍結
  - 系統可能認為程式無響應（強制結束）

- **正確做法**：
  ```csharp
  // ✓ 非同步，UI 不卡
  private async void OnButtonClick(object sender, EventArgs e)
  {
      var data = await FetchDataFromDBAsync();
      textBox.Text = data;
  }
  ```

- **場景**：
  - WPF / WinForms 應用
  - 桌面工具、編輯器
  - 任何 UI 應用

### 3. 多個獨立 I/O 操作必須並行

- **問題**：同步方式串聯執行，時間倍增
  ```csharp
  // ❌ 太慢
  public int FetchDataSync()
  {
      var data1 = httpClient.GetStringResult(url1);  // 500ms
      var data2 = httpClient.GetStringResult(url2);  // 500ms
      var data3 = httpClient.GetStringResult(url3);  // 500ms
      return data1.Length + data2.Length + data3.Length;  // 總耗時 1500ms
  }
  ```

- **後果**：
  - 3 個 500ms 操作，同步需 1500ms
  - 非同步並行只需 500ms（3 倍加速）
  - 在高並發下，延遲直接轉化為吞吐量低

- **正確做法**：
  ```csharp
  // ✓ 快得多
  public async Task<int> FetchDataAsync()
  {
      var task1 = httpClient.GetStringAsync(url1);
      var task2 = httpClient.GetStringAsync(url2);
      var task3 = httpClient.GetStringAsync(url3);
      var results = await Task.WhenAll(task1, task2, task3);
      return results.Sum(r => r.Length);  // 總耗時 500ms
  }
  ```

- **場景**：
  - 聚合 API（調用多個微服務）
  - 並行下載多個資源
  - 批量資料庫查詢
  - 並行任務管理

### 4. 需要精細控制並行度和資源

- **問題**：同步無法表達複雜的並行模式
  ```csharp
  // ❌ 同步無法限流
  var tasks = items.Select(FetchDataSync);
  var results = tasks.ToList();  // 所有 task 同時執行，無限流
  ```

- **為什麼不能用同步**：
  - 同步無法在等待期間做其他工作
  - 無法表達「創建任務但不立即等待」
  - 無法用 SemaphoreSlim 等機制限制併行度
  - 無法實現 Producer-Consumer 模式

- **必須用非同步**：
  ```csharp
  // ✓ 可限流
  var semaphore = new SemaphoreSlim(5);  // 最多 5 個並行
  var tasks = items.Select(async item =>
  {
      await semaphore.WaitAsync();
      try
      {
          return await FetchDataAsync(item);
      }
      finally
      {
          semaphore.Release();
      }
  });
  var results = await Task.WhenAll(tasks);
  ```

- **場景**：
  - 大批量 HTTP 請求（需限流避免被封）
  - 連接池管理
  - 資源配額控制
  - 背壓和流量控制

---

## 🏗️ 架構決策指南

| 場景 | 同步 | 非同步 | 原因 |
|------|------|--------|------|
| Web API | ❌ | ✅ | 高並發，線程數量有限 |
| 命令行工具 | ✅ | 可選 | 並發度低，邏輯簡單 |
| 桌面 UI | ❌ | ✅ | UI 線程被阻塞會卡 |
| CPU 計算 | ✅ | ❌ | 用 Task.Run 而不是 async |
| 檔案 I/O | ❌ | ✅ | 減少線程堵塞 |
| 資料庫 | ❌ | ✅ | 減少連接數，提高吞吐 |
| 網路請求 | ❌ | ✅ | 可並行多個請求 |
| 事件處理 | ❌ | ✅ | 避免線程飢餓 |
| 簡單驗證 | ✅ | ❌ | 無 I/O，無需複雜性 |
| 依賴串聯 | ✅ | 可選 | 無並行機會 |

---

## ⚠️ 常見陷阱

### 1. 使用 `.Result` 或 `.Wait()` 導致死鎖

```csharp
// ❌ 死鎖
public string GetData()
{
    return FetchDataAsync().Result;
}
```

**原因**：線程阻塞在 `.Result`，async 方法無法回到線程上下文執行

**正確做法 1（應用程式碼）**：
```csharp
// ✓ 使用 await
public async Task<string> GetData()
{
    return await FetchDataAsync();
}
```

**正確做法 2（函式庫程式碼）**：
```csharp
// ✓ 使用 ConfigureAwait(false) 避免死鎖
public async Task<string> GetData()
{
    return await FetchDataAsync().ConfigureAwait(false);
}
```

**說明**：
- `ConfigureAwait(false)` 告訴 await 不要嘗試回到原 SynchronizationContext
- 在函式庫程式碼中使用可避免死鎖，也能提升效能
- 應用程式碼（特別是 UI）通常需要回到原執行緒，不應使用 `ConfigureAwait(false)`

### 2. 在 async void 中拋出例外，無法捕捉

```csharp
// ❌ 例外無法捕捉
async void OnButtonClick()
{
    throw new Exception("error");
}
```

**原因**：async void 的例外會在 SynchronizationContext 中提升為崩潰

**正確做法**：
```csharp
// ✓ 用 async Task
async Task OnButtonClickAsync()
{
    throw new Exception("error");
}

try { await OnButtonClickAsync(); }
catch { ... }
```

### 3. 忘記 await，導致任務未執行

```csharp
// ❌ 任務不會執行
public async Task Process()
{
    FetchDataAsync();  // 忘記 await！
    ProcessData();  // 此時數據還未獲取
}
```

**正確做法**：
```csharp
// ✓ 加上 await
public async Task Process()
{
    await FetchDataAsync();
    ProcessData();
}
```

### 4. async void 事件處理器無法等待完成

```csharp
// ❌ UI 在方法返回時已更新，可能結果還未準備
async void OnButtonClick(object sender, EventArgs e)
{
    var data = await FetchDataAsync();
    label.Text = data;  // 可能在初始化時被 GC
}
```

**正確做法**（如果可能的話）：
```csharp
// ✓ 或至少在 try-catch 中
async void OnButtonClick(object sender, EventArgs e)
{
    try
    {
        var data = await FetchDataAsync();
        label.Text = data;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

### 5. 在循環中錯誤並行化

```csharp
// ❌ 代碼複雜，且可能產生過多並行任務
foreach (var item in items)
{
    await ProcessAsync(item);  // 此處順序執行
}

// ✓ 改為並行
await Task.WhenAll(items.Select(ProcessAsync));
```

### 6. 誤解 Task.WhenAll 的例外處理機制

```csharp
// ⚠️ 常見誤解：以為 await 會拋出 AggregateException
try
{
    await Task.WhenAll(task1, task2, task3);
}
catch (AggregateException ex)
{
    // ✗ 這個 catch 永遠不會被執行！
    // await 會展開 AggregateException
}
```

**原因**：
- `Task.WhenAll` 會將多個失敗任務的例外包裝成 `AggregateException`
- 但 `await` 會**展開 (unwrap)** AggregateException，只拋出第一個 InnerException
- 因此 catch 捕獲的是具體的例外類型，不是 AggregateException

**正確做法**：
```csharp
// ✓ 捕獲具體例外類型
try
{
    await Task.WhenAll(task1, task2, task3);
}
catch (HttpRequestException ex)
{
    // ✓ 捕獲第一個失敗任務的例外
    Console.WriteLine($"捕獲到例外: {ex.Message}");
}

// ✓ 如需存取所有例外，檢查 Task 物件
var whenAllTask = Task.WhenAll(task1, task2, task3);
try
{
    await whenAllTask;
}
catch
{
    // 取得所有例外
    var allExceptions = whenAllTask.Exception?.InnerExceptions;
    foreach (var ex in allExceptions)
    {
        Console.WriteLine($"例外: {ex.Message}");
    }
}
```

### 7. 用 Task.Run 包裝同步阻塞操作的反模式

```csharp
// ❌ 錯誤示範：把阻塞轉移到 ThreadPool，並非真正的非同步
public async Task<string> FetchDataAsync()
{
    return await Task.Run(() =>
    {
        Thread.Sleep(1000);  // 仍然阻塞 ThreadPool 線程
        return "Data";
    });
}
```

**問題**：
- 主執行緒沒有被阻塞（因為用了 await）
- 但只是把阻塞操作轉移到 ThreadPool 的工作線程
- 這**不是真正的非同步 I/O**，只是「異步等待一個阻塞操作」
- 浪費 ThreadPool 線程資源

**正確做法**：
```csharp
// ✓ 使用真正的非同步 I/O
public async Task<string> FetchDataAsync()
{
    await Task.Delay(1000);  // 不阻塞任何線程
    return "Data";
}

// ✓ 或使用非同步 API
public async Task<string> FetchDataAsync()
{
    var response = await httpClient.GetStringAsync(url);  // 真正的非同步
    return response;
}
```

**何時可以用 Task.Run**：
- CPU Bound 操作（計算密集），需要多線程並行
- 在同步上下文中執行 async 方法（不常見的 workaround）
- **不應用於**：I/O 操作（應使用非同步 API）

---

## 📊 效能數據示例

### I/O Bound 效能對比（3 個 500ms 操作）

| 方式 | 耗時 | 說明 |
|-----|------|------|
| 同步順序 | 1500ms | 500+500+500 |
| 非同步並行 | ~500ms | 3 個並行 |
| **加速倍數** | **3x** | |

### CPU Bound 效能對比（計算 Fibonacci(30)）

| 方式 | 耗時 | CPU 使用 |
|-----|------|----------|
| 同步順序 | 100ms | 1 核 100% |
| Task.Run 並行（4 核） | ~30ms | 4 核 100% |
| async（無 await） | 100ms | 1 核 100%（無幫助） |
| **加速倍數** | **3~4x** | 多核並行 |

### Web 伺服器吞吐量（同步 vs 非同步）

| 方案 | 併發能力 | 線程數 | 記憶體 |
|-----|---------|--------|--------|
| 同步（1 請求 1 線程） | 100 併發 | 100 | 100~200 MB |
| 非同步（線程池） | 10000 併發 | 4~8 | 10~20 MB |
| **倍數** | **100x** | | |

---

## 🎓 學習步驟

1. **理解基礎**：執行範例 1-5，理解 async/await 語法和基本概念
2. **認識陷阱**：特別關注範例 3（死鎖）和 4（async void）
3. **實踐應用**：執行範例 6-8，了解如何在實際中使用
4. **性能優化**：執行範例 9-10，理解效能考量
5. **檢視代碼**：在 Visual Studio 中逐步調試，看狀態機如何運作

---

## 📝 快速參考

### 何時用 async/await

✅ **用**：I/O 操作、網路、檔案、資料庫、UI 應用
✅ **用**：Web 伺服器（ASP.NET）
✅ **用**：並發任務
❌ **不用**：CPU Bound 計算（用 Task.Run）
❌ **不用**：簡單同步代碼（無等待）

### 何時用 Task.Run

✅ **用**：CPU Bound 操作（計算密集）
✅ **用**：在同步上下文中執行 async 方法（不常見）
❌ **不用**：I/O 操作（直接使用非同步 API，不要用 Task.Run 包裝 Thread.Sleep）

### 何時用 ConfigureAwait(false)

✅ **用**：函式庫程式碼（不需要回到原 SynchronizationContext）
✅ **用**：避免死鎖的情境
❌ **不用**：應用程式碼（特別是 UI，需要回到原執行緒）
❌ **不用**：需要特定同步上下文的情境

### 何時用 async void

✅ **只用**：事件處理器（如 Button.Click）
❌ **其他**：都用 async Task

---

## 🔗 相關資源

- [Microsoft Docs: Async/Await](https://docs.microsoft.com/en-us/dotnet/csharp/async)
- [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [ConfigureAwait Best Practices](https://blog.stephencleary.com/2012/02/async-and-await.html)

---

## 🎯 總結

同步和非同步各有用途：

- **同步**：簡單、清楚、適合無 I/O 的場景
- **非同步**：複雜但高效，適合 I/O 和高並發場景

選擇正確的工具是成為高效 .NET 工程師的關鍵！
