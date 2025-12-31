// ============================================
// .NET 10 同步 vs 非同步教學示範 (Async/Await)
// ============================================
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║    .NET 10 同步 vs 非同步教學示範 (Async/Await)     ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.WriteLine();

bool running = true;
while (running)
{
    Console.WriteLine("【選擇要執行的範例】");
    Console.WriteLine("簡單範例:");
    Console.WriteLine("  1. 同步 vs 非同步檔案讀寫 (I/O Bound)");
    Console.WriteLine("  2. 使用 Task.Delay 模擬 I/O 操作");
    Console.WriteLine("  3. 避免使用 .Result / .Wait() 導致死鎖");
    Console.WriteLine("  4. async void vs async Task 的差異");
    Console.WriteLine("  5. 多任務並行：Task.WhenAll vs 順序等待");
    Console.WriteLine();
    Console.WriteLine("中階範例:");
    Console.WriteLine("  6. 多任務並行 + 限流 + CancellationToken");
    Console.WriteLine("  7. 非同步例外處理與多任務");
    Console.WriteLine("  8. Producer-Consumer 並行度控制");
    Console.WriteLine();
    Console.WriteLine("進階範例:");
    Console.WriteLine("  9. 管線化處理 + 背壓控制 (Pipeline)");
    Console.WriteLine(" 10. I/O Bound vs CPU Bound 效能對比");
    Console.WriteLine();
    Console.WriteLine("  0. 結束程式");
    Console.WriteLine();
    Console.Write("請選擇 (0-10): ");

    if (int.TryParse(Console.ReadLine(), out int choice))
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        switch (choice)
        {
            case 1:
                await Example1_FileSyncVsAsync();
                break;
            case 2:
                await Example2_TaskDelaySimulation();
                break;
            case 3:
                await Example3_AvoidDeadlock();
                break;
            case 4:
                await Example4_AsyncVoidVsAsyncTask();
                break;
            case 5:
                await Example5_ConcurrentTasks();
                break;
            case 6:
                await Example6_RateLimitingWithCancellation();
                break;
            case 7:
                await Example7_ExceptionHandling();
                break;
            case 8:
                await Example8_ProducerConsumer();
                break;
            case 9:
                await Example9_PipelineWithBackpressure();
                break;
            case 10:
                await Example10_IOBoundVsCPUBound();
                break;
            case 0:
                running = false;
                break;
            default:
                Console.WriteLine("❌ 無效選擇，請重新輸入。");
                break;
        }

        if (choice != 0)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.Write("按 Enter 返回菜單...");
            Console.ReadLine();
            try { Console.Clear(); } catch { }
        }
    }
    else
    {
        Console.WriteLine("❌ 請輸入有效的數字。");
    }
}

Console.WriteLine("✓ 教程結束，再見！");

// ============================================
// 【範例 1】同步 vs 非同步檔案讀寫 (I/O Bound)
// ============================================
async Task Example1_FileSyncVsAsync()
{
    Console.WriteLine("【範例 1】同步 vs 非同步檔案讀寫");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   讀寫大檔案時，同步方式會阻塞線程，非同步方式讓線程");
    Console.WriteLine("   可繼續處理其他工作。這是 I/O Bound 操作的典型案例。");
    Console.WriteLine();
    Console.WriteLine("❓ 為什麼要用非同步？");
    Console.WriteLine("   - 不阻塞線程，可同時處理多個 I/O");
    Console.WriteLine("   - 提高伺服器吞吐量（可服務更多請求）");
    Console.WriteLine("   - UI 不會卡住");
    Console.WriteLine();
    Console.WriteLine("❌ 為什麼不能在此用同步？");
    Console.WriteLine("   - 大檔案讀寫阻塞線程，影響效能");
    Console.WriteLine("   - 在 UI/Web 環境會導致卡頓");
    Console.WriteLine();

    // 創建測試檔案
    string testFilePath = Path.Combine(Path.GetTempPath(), "test_file.txt");
    string testContent = string.Concat(Enumerable.Range(1, 50000).Select(i => $"Line {i}\n"));
    File.WriteAllText(testFilePath, testContent);

    try
    {
        // 方式 1: 同步讀取
        Console.WriteLine("▶ 【同步方式】File.ReadAllText");
        var sw = Stopwatch.StartNew();
        string syncContent = File.ReadAllText(testFilePath);
        sw.Stop();
        Console.WriteLine($"  ✓ 讀取 {syncContent.Length} 字符，耗時: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("  ⚠️  在此期間，線程被阻塞，無法做其他工作");
        Console.WriteLine();

        // 方式 2: 非同步讀取
        Console.WriteLine("▶ 【非同步方式】File.ReadAllTextAsync");
        sw.Restart();
        string asyncContent = await File.ReadAllTextAsync(testFilePath);
        sw.Stop();
        Console.WriteLine($"  ✓ 讀取 {asyncContent.Length} 字符，耗時: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("  ✨ 線程未被阻塞，可同時做其他工作");
        Console.WriteLine();

        // 模擬同時處理多個檔案讀取
        Console.WriteLine("▶ 【示範：多個檔案非同步讀取的優勢】");
        var filePaths = Enumerable.Range(1, 5)
            .Select(i =>
            {
                string path = Path.Combine(Path.GetTempPath(), $"test_file_{i}.txt");
                File.WriteAllText(path, testContent);
                return path;
            })
            .ToList();

        sw.Restart();
        var tasks = filePaths.Select(p => File.ReadAllTextAsync(p)).ToList();
        var results = await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"  ✓ 非同步讀取 {filePaths.Count} 個檔案，耗時: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("  💡 多個檔案可並行讀取，效率高！");

        // 清理
        foreach (var path in filePaths)
            File.Delete(path);
    }
    finally
    {
        File.Delete(testFilePath);
    }

    Console.WriteLine();
    Console.WriteLine("📊 預期輸出對比:");
    Console.WriteLine("   - 同步: 阻塞線程");
    Console.WriteLine("   - 非同步: 線程自由，可併行處理");
}

// ============================================
// 【範例 2】使用 Task.Delay 模擬 I/O 操作
// ============================================
async Task Example2_TaskDelaySimulation()
{
    Console.WriteLine("【範例 2】使用 Task.Delay 模擬 I/O 操作");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   當沒有真實網路時，用 Task.Delay 模擬 I/O 延遲。");
    Console.WriteLine("   展示非同步如何在等待期間不阻塞線程。");
    Console.WriteLine();
    Console.WriteLine("❓ 為什麼用 Task.Delay 而不是 Thread.Sleep？");
    Console.WriteLine("   - Thread.Sleep 阻塞線程（同步）");
    Console.WriteLine("   - Task.Delay 不阻塞線程（非同步），線程可處理其他工作");
    Console.WriteLine();

    // 模擬 API 呼叫
    async Task<string> FetchDataAsync(string resource, int delayMs)
    {
        Console.WriteLine($"  ⏳ 正在獲取 {resource}...");
        await Task.Delay(delayMs);
        return $"Data from {resource}";
    }

    Console.WriteLine("▶ 【錯誤示範：用 Task.Run 包裝同步阻塞操作】");
    Console.WriteLine("   說明：用 Thread.Sleep 模擬阻塞式 I/O，雖用 await Task.Run 不卡主執行緒，");
    Console.WriteLine("         但這只是把阻塞轉移到 ThreadPool 工作線程，並非真正的非同步，效率仍低。");
    Console.WriteLine("         注意：這不是「同步阻塞主執行緒」，而是「異步等待一個阻塞操作」。");
    var sw = Stopwatch.StartNew();
    string result1 = await Task.Run(() =>
    {
        Console.WriteLine("  ⏳ 正在獲取 Resource1... (等待 1000ms)");
        Thread.Sleep(1000); // 佔用線程 1 秒
        return "Data from Resource1";
    });
    string result2 = await Task.Run(() =>
    {
        Console.WriteLine("  ⏳ 正在獲取 Resource2... (等待 1000ms)");
        Thread.Sleep(1000);
        return "Data from Resource2";
    });
    sw.Stop();
    Console.WriteLine($"  ✓ 完成，總耗時: {sw.ElapsedMilliseconds} ms (順序 = 1000 + 1000)");
    Console.WriteLine();

    Console.WriteLine("▶ 【非同步方式 - 並行執行】");
    sw.Restart();
    var task1 = FetchDataAsync("Resource1", 1000);
    var task2 = FetchDataAsync("Resource2", 1000);
    await Task.WhenAll(task1, task2);
    sw.Stop();
    Console.WriteLine($"  ✓ 完成，總耗時: {sw.ElapsedMilliseconds} ms (並行 ≈ 1000)");
    Console.WriteLine();

    Console.WriteLine("💡 核心差異:");
    Console.WriteLine("   - 同步順序: 2000ms (等待完全是串聯)");
    Console.WriteLine("   - 非同步並行: 1000ms (等待可以重疊)");
}

// ============================================
// 【範例 3】避免使用 .Result / .Wait() 導致死鎖
// ============================================
async Task Example3_AvoidDeadlock()
{
    Console.WriteLine("【範例 3】避免使用 .Result / .Wait() 導致死鎖");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   在有 SynchronizationContext 的環境中（如 UI app），");
    Console.WriteLine("   用 .Result 或 .Wait() 會導致死鎖。Console app 通常只會阻塞。");
    Console.WriteLine("   正確做法是使用 await。");
    Console.WriteLine();
    Console.WriteLine("❌ 危險做法:");
    Console.WriteLine("   async Task<string> GetDataAsync() => ... ");
    Console.WriteLine("   string data = GetDataAsync().Result;");
    Console.WriteLine("   - Console app: 阻塞線程（但通常不死鎖）");
    Console.WriteLine("   - UI app (WPF/WinForms): 死鎖！");
    Console.WriteLine("   - ASP.NET (舊版): 死鎖！");
    Console.WriteLine();

    async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "Data retrieved";
    }

    // 演示危險的做法（但我們不會真的執行它）
    Console.WriteLine("▶ 【錯誤做法的解釋】");
    Console.WriteLine("   死鎖發生條件（需同時滿足）:");
    Console.WriteLine("   1. 有 SynchronizationContext (UI app, 舊版 ASP.NET)");
    Console.WriteLine("   2. 線程被阻塞在 .Result/.Wait()");
    Console.WriteLine("   3. async 方法試圖回到原 SynchronizationContext");
    Console.WriteLine("   4. 原線程被阻塞，無法執行 continuation → 死鎖");
    Console.WriteLine();
    Console.WriteLine("   Console app 為什麼通常不死鎖？");
    Console.WriteLine("   - 預設 SynchronizationContext.Current == null");
    Console.WriteLine("   - continuation 在 ThreadPool 執行，不依賴原線程");
    Console.WriteLine("   - 但仍會「阻塞線程」，這是不好的做法！");
    Console.WriteLine();

    // 演示正確的做法
    Console.WriteLine("▶ 【正確做法 1: 使用 await】");
    var sw = Stopwatch.StartNew();
    string result = await GetDataAsync();
    sw.Stop();
    Console.WriteLine($"   ✓ {result}，耗時: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine();

    Console.WriteLine("▶ 【正確做法 2: 使用 ConfigureAwait(false)（函式庫最佳實踐）】");
    Console.WriteLine("   說明：ConfigureAwait(false) 不會嘗試回到原 SynchronizationContext，");
    Console.WriteLine("         可避免死鎖，也能提升效能（適合函式庫程式碼）。");
    Console.WriteLine();
    async Task<string> GetDataWithConfigureAwaitAsync()
    {
        await Task.Delay(100).ConfigureAwait(false);
        return "Data retrieved with ConfigureAwait";
    }
    sw.Restart();
    string resultConfigureAwait = await GetDataWithConfigureAwaitAsync();
    sw.Stop();
    Console.WriteLine($"   ✓ {resultConfigureAwait}，耗時: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine("   💡 在函式庫程式碼中，若不需回到原 SynchronizationContext，建議使用 ConfigureAwait(false)");
    Console.WriteLine();

    Console.WriteLine("▶ 【❌ 錯誤做法：Task.Run 包裝 .Result（反模式）】");
    Console.WriteLine("   ⚠️  警告：以下程式碼是「錯誤示範」，僅用於說明為何不應這樣做！");
    Console.WriteLine();
    sw.Restart();
    // ❌ 反模式：用 Task.Run 包裝 .Result 只是把問題藏起來
    string resultFromRun = await Task.Run(() => GetDataAsync().Result);
    sw.Stop();
    Console.WriteLine($"   結果: {resultFromRun}，耗時: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine();
    Console.WriteLine("   ❌ 為什麼這是錯誤做法:");
    Console.WriteLine("   1. 仍會「同步阻塞」一個 ThreadPool 線程（問題只是轉移，沒有解決）");
    Console.WriteLine("   2. 例外會被包在 AggregateException，需額外處理");
    Console.WriteLine("   3. 浪費線程資源（用一個線程等待另一個 async 操作）");
    Console.WriteLine("   4. 改變了例外傳播行為，增加除錯難度");
    Console.WriteLine("   5. 這不是「替代做法」，而是「反模式」，不應在生產環境使用！");
    Console.WriteLine();

    Console.WriteLine("💡 記住:");
    Console.WriteLine("   ✓ 應用程式碼：使用 await");
    Console.WriteLine("   ✓ 函式庫程式碼（通常）：使用 await ... ConfigureAwait(false)");
    Console.WriteLine("     （除非需要回到特定的 SynchronizationContext）");
    Console.WriteLine("   ✗ 避免使用 .Result 或 .Wait()");
}

// ============================================
// 【範例 4】async void vs async Task 的差異
// ============================================
async Task Example4_AsyncVoidVsAsyncTask()
{
    Console.WriteLine("【範例 4】async void vs async Task 的差異");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   async void 用於事件處理器，async Task 用於其他地方。");
    Console.WriteLine("   誤用 async void 會導致例外無法被捕捉。");
    Console.WriteLine();

    // ❌ async void - 例外無法被捕捉
    async void BadAsyncVoid()
    {
        await Task.Delay(50);
        throw new InvalidOperationException("async void 中的例外");
    }

    // ✓ async Task - 例外可以被捕捉
    async Task GoodAsyncTask()
    {
        await Task.Delay(50);
        throw new InvalidOperationException("async Task 中的例外");
    }

    Console.WriteLine("▶ 【async void 的問題 - 例外無法被捕捉】");
    Console.WriteLine("   EventHandler += BadAsyncVoid;  // ❌ 例外會導致應用崩潰");
    Console.WriteLine("   我們無法用 try-catch 捕捉它");
    Console.WriteLine();
    Console.WriteLine("   ⚠️  教學環境說明:");
    Console.WriteLine("   - 為了演示，我們使用「自訂 SynchronizationContext」攔截例外");
    Console.WriteLine("   - 這只是教學模擬，真實環境的行為更嚴重:");
    Console.WriteLine();
    Console.WriteLine("   真實環境的 async void 例外:");
    Console.WriteLine("   • Console app: 觸發 UnhandledException → Process Crash");
    Console.WriteLine("   • WPF: 觸發 DispatcherUnhandledException → 可攔截但需設定");
    Console.WriteLine("   • WinForms: 觸發 ThreadException → 可攔截但需設定");
    Console.WriteLine("   • ASP.NET Core: 無 SynchronizationContext，例外拋到 ThreadPool → 行為依版本/宿主/設定不同，可能導致崩潰");
    Console.WriteLine();

    // 保存當前上下文
    var originalContext = SynchronizationContext.Current;
    // 設置攔截上下文
    var safeContext = new SafeAsyncVoidContext();
    SynchronizationContext.SetSynchronizationContext(safeContext);

    try
    {
        Console.WriteLine("   執行 async void...");
        BadAsyncVoid();
        
        // 等待 async void 執行並拋出例外
        // 因為 async void 是 "fire and forget"，我們只能輪詢檢查上下文是否捕獲到了錯誤
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(50);
            if (safeContext.CaughtException != null) break;
        }

        if (safeContext.CaughtException != null)
        {
            Console.WriteLine($"   ⚠️  [模擬崩潰] 系統偵測到未處理例外: {safeContext.CaughtException.Message}");
            Console.WriteLine("       注意：在正常 Console/WinForm 環境下，這會直接導致程式閃退！");
        }
        else
        {
            Console.WriteLine("   ❓ 未能及時捕捉到例外 (可能執行過快或被環境吞噬)");
        }
    }
    finally
    {
        // 恢復原始上下文
        SynchronizationContext.SetSynchronizationContext(originalContext);
    }
    Console.WriteLine();

    Console.WriteLine("▶ 【async Task 的優勢 - 例外可被捕捉】");
    Console.WriteLine("   var task = GoodAsyncTask();");
    Console.WriteLine("   await task;  // ✓ 例外會被拋出");
    Console.WriteLine();

    try
    {
        await GoodAsyncTask();
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"   ✓ 例外被成功捕捉: {ex.Message}");
    }
    Console.WriteLine();

    Console.WriteLine("💡 最佳實踐:");
    Console.WriteLine("   ✓ 使用 async Task（或 async Task<T>）");
    Console.WriteLine("   ✗ 避免使用 async void（僅在事件處理器中使用）");
}
// ============================================
// 【範例 5】多任務並行：Task.WhenAll vs 順序等待
// ============================================
async Task Example5_ConcurrentTasks()
{
    Console.WriteLine("【範例 5】多任務並行：Task.WhenAll vs 順序等待");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   需要執行多個 I/O 操作時，使用並行而非順序執行。");
    Console.WriteLine();

    async Task<int> SimulateAPICall(string name, int delayMs)
    {
        await Task.Delay(delayMs);
        return delayMs;
    }

    // 順序執行
    Console.WriteLine("▶ 【順序執行 (await 多次)】");
    var sw = Stopwatch.StartNew();
    int result1 = await SimulateAPICall("API1", 500);
    int result2 = await SimulateAPICall("API2", 500);
    int result3 = await SimulateAPICall("API3", 500);
    sw.Stop();
    int sequentialTotal = result1 + result2 + result3;
    Console.WriteLine($"  ✓ 完成 3 個 API 呼叫");
    Console.WriteLine($"  耗時: {sw.ElapsedMilliseconds} ms (預期 ≈ 1500 ms)");
    Console.WriteLine();

    // 並行執行
    Console.WriteLine("▶ 【並行執行 (Task.WhenAll)】");
    sw.Restart();
    var task1 = SimulateAPICall("API1", 500);
    var task2 = SimulateAPICall("API2", 500);
    var task3 = SimulateAPICall("API3", 500);
    int[] results = await Task.WhenAll(task1, task2, task3);
    sw.Stop();
    int parallelTotal = results.Sum();
    Console.WriteLine($"  ✓ 完成 3 個 API 呼叫");
    Console.WriteLine($"  耗時: {sw.ElapsedMilliseconds} ms (預期 ≈ 500 ms)");
    Console.WriteLine();

    Console.WriteLine("💡 效能對比:");
    Console.WriteLine($"   順序執行: {1500} ms");
    Console.WriteLine($"   並行執行: ≈ {500} ms");
    Console.WriteLine($"   加速比: 3x");
}

// ============================================
// 【範例 6】多任務並行 + 限流 + CancellationToken
// ============================================
async Task Example6_RateLimitingWithCancellation()
{
    Console.WriteLine("【範例 6】多任務並行 + 限流 + CancellationToken");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   同時執行許多任務但要限制並行度（避免資源耗盡），");
    Console.WriteLine("   並支持取消操作。");
    Console.WriteLine();
    Console.WriteLine("❓ 為什麼要限流？");
    Console.WriteLine("   - 避免同時建立太多連接（耗盡連接池）");
    Console.WriteLine("   - 控制 CPU/記憶體使用");
    Console.WriteLine("   - 避免對伺服器造成過大壓力");
    Console.WriteLine();
    Console.WriteLine("❓ 什麼是協作式取消 (Cooperative Cancellation)？");
    Console.WriteLine("   - CancellationToken 不會「強制終止」任務");
    Console.WriteLine("   - 任務必須「主動檢查」token 並配合停止");
    Console.WriteLine("   - 如: await Task.Delay(ms, ct) 會檢查取消");
    Console.WriteLine("   - 取消後可能會看到「部分完成/部分取消」，這是正常的");
    Console.WriteLine();

    async Task<int> ProcessItem(int id, int delayMs, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"  🔄 處理項目 {id} (延遲 {delayMs}ms)");
            await Task.Delay(delayMs, ct);
            Console.WriteLine($"  ✓ 項目 {id} 完成");
            return id;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"  ❌ 項目 {id} 已取消");
            throw;
        }
    }

    Console.WriteLine("▶ 【限流執行（每次最多 2 個並行任務）】");
    using var cts = new CancellationTokenSource();
    using var semaphore = new SemaphoreSlim(2); // 最多 2 個並行任務

    // 設置 1 秒後自動取消 (確保能觸發取消)
    cts.CancelAfter(TimeSpan.FromSeconds(1));

    var items = Enumerable.Range(1, 10).ToList();

    try
    {
        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(cts.Token);
            try
            {
                return await ProcessItem(item, 300, cts.Token);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var results = await Task.WhenAll(tasks);
        Console.WriteLine();
        Console.WriteLine($"  ✓ 全部 {results.Length} 項完成");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        Console.WriteLine("  ⚠️  操作已取消");
    }

    Console.WriteLine();
    Console.WriteLine("💡 核心概念:");
    Console.WriteLine("   - SemaphoreSlim: 控制並行度");
    Console.WriteLine("   - CancellationToken: 支持優雅取消");
}

// ============================================
// 【範例 7】非同步例外處理與多任務
// ============================================
async Task Example7_ExceptionHandling()
{
    Console.WriteLine("【範例 7】非同步例外處理與多任務");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   多任務執行時，某些任務可能失敗。正確的例外處理很重要。");
    Console.WriteLine();

    async Task<string> FetchDataAsync(int id, bool shouldFail)
    {
        await Task.Delay(100);
        if (shouldFail)
            throw new HttpRequestException($"任務 {id} 失敗");
        return $"任務 {id} 結果";
    }

    // 方式 1: 捕捉所有例外
    Console.WriteLine("▶ 【方式 1: Task.WhenAll + try-catch（只捕捉第一個例外）】");
    Console.WriteLine("   ⚠️  重要：Task.WhenAll 會將多個失敗例外包成 AggregateException");
    Console.WriteLine("   但 await 會展開它，只拋出第一個 InnerException");
    Console.WriteLine("   其他例外可從 Task.WhenAll 回傳的 task.Exception.InnerExceptions 取得");
    Console.WriteLine();
    try
    {
        var tasks = new[]
        {
            FetchDataAsync(1, false),
            FetchDataAsync(2, true),   // 會失敗
            FetchDataAsync(3, false)
        };
        var results = await Task.WhenAll(tasks);
        Console.WriteLine("  ✓ 全部成功");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"  ❌ 捕捉到例外: {ex.Message}");
        Console.WriteLine("     （注意：如有其他任務也失敗，在此 catch 中看不到，但可從各 Task 物件取得）");
    }
    Console.WriteLine();

    // 方式 2: 使用 Task.WhenAll 查看所有結果
    Console.WriteLine("▶ 【方式 2: 單獨檢查每個任務（查看所有結果）】");
    var task1 = FetchDataAsync(1, false);
    var task2 = FetchDataAsync(2, true);
    var task3 = FetchDataAsync(3, false);

    var allTasks = new[] { task1, task2, task3 };
    await Task.WhenAll(allTasks.Select(async t =>
    {
        try
        {
            await t;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ {ex.Message}");
        }
    }));
    Console.WriteLine();

    // 方式 3: 使用 Task.WhenAll 區分成功和失敗任務
    Console.WriteLine("▶ 【方式 3: 區分成功和失敗任務】");
    var tasks2 = new[]
    {
        FetchDataAsync(1, false),
        FetchDataAsync(2, true),
        FetchDataAsync(3, false)
    };

    var resultStatuses = await Task.WhenAll(
        tasks2.Select(async t =>
        {
            try
            {
                var result = await t;
                return (Success: true, Message: result);
            }
            catch (Exception ex)
            {
                return (Success: false, Message: ex.Message);
            }
        })
    );

    foreach (var result in resultStatuses)
    {
        if (result.Success)
            Console.WriteLine($"  ✓ {result.Message}");
        else
            Console.WriteLine($"  ❌ {result.Message}");
    }
    Console.WriteLine();

    // 補充說明：如何獲取所有被隱藏的例外
    Console.WriteLine("💡 如何獲取 Task.WhenAll 中所有被隱藏的例外？");
    var tasks3 = new[]
    {
        FetchDataAsync(1, false),
        FetchDataAsync(2, true),   // 會失敗
        FetchDataAsync(3, true)    // 也會失敗
    };

    try
    {
        await Task.WhenAll(tasks3);
    }
    catch
    {
        // 檢查每個任務的狀態
        var faultedTasks = tasks3.Where(t => t.IsFaulted).ToList();
        Console.WriteLine($"   發現 {faultedTasks.Count} 個失敗的任務:");
        foreach (var t in faultedTasks)
        {
            Console.WriteLine($"   ❌ {t.Exception?.InnerException?.Message}");
        }
    }
}

// ============================================
// 【範例 8】Producer-Consumer 並行度控制
// ============================================
async Task Example8_ProducerConsumer()
{
    Console.WriteLine("【範例 8】Producer-Consumer 並行度控制");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   生產者產生數據，消費者處理數據，兩者並行執行。");
    Console.WriteLine("   使用 Channel<T> 控制生產和消費的速度。");
    Console.WriteLine();

    var channel = Channel.CreateBounded<int>(
        new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait
        }
    );

    async Task ProducerAsync()
    {
        try
        {
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"📤 生產者: 生產項目 {i}");
                await channel.Writer.WriteAsync(i);
                await Task.Delay(200);
            }
            channel.Writer.TryComplete();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 生產者錯誤: {ex.Message}");
        }
    }

    async Task ConsumerAsync(int consumerId)
    {
        try
        {
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"📥 消費者 {consumerId}: 消費項目 {item}");
                await Task.Delay(300); // 消費比生產慢
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 消費者 {consumerId} 錯誤: {ex.Message}");
        }
    }

    Console.WriteLine("▶ 【Producer-Consumer 並行執行】");
    var sw = Stopwatch.StartNew();
    var producer = ProducerAsync();
    var consumer1 = ConsumerAsync(1);
    var consumer2 = ConsumerAsync(2);

    await Task.WhenAll(producer, consumer1, consumer2);
    sw.Stop();

    Console.WriteLine();
    Console.WriteLine($"✓ 完成，總耗時: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine();
    Console.WriteLine("💡 Channel 的優勢:");
    Console.WriteLine("   - 自動背壓控制（當緩衝滿時生產者會等待）");
    Console.WriteLine("   - 線程安全");
    Console.WriteLine("   - 支持多個消費者");
}

// ============================================
// 【範例 9】管線化處理 + 背壓控制 (Pipeline)
// ============================================
async Task Example9_PipelineWithBackpressure()
{
    Console.WriteLine("【範例 9】管線化處理 + 背壓控制 (Pipeline)");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   數據流經多個處理階段，每個階段可以並行執行。");
    Console.WriteLine("   背壓(Backpressure)確保緩衝區不會溢出。");
    Console.WriteLine();

    var channel1 = Channel.CreateBounded<int>(3);
    var channel2 = Channel.CreateBounded<string>(3);

    // 階段 1: 生產
    async Task Stage1_ProduceAsync()
    {
        for (int i = 1; i <= 8; i++)
        {
            Console.WriteLine($"[階段1] 📤 生產: {i}");
            await channel1.Writer.WriteAsync(i);
            await Task.Delay(100);
        }
        channel1.Writer.TryComplete();
    }

    // 階段 2: 轉換
    async Task Stage2_TransformAsync()
    {
        try
        {
            await foreach (var item in channel1.Reader.ReadAllAsync())
            {
                string transformed = $"Processed_{item}";
                Console.WriteLine($"[階段2] 🔄 轉換: {item} -> {transformed}");
                await channel2.Writer.WriteAsync(transformed);
                await Task.Delay(200); // 處理較慢
            }
            channel2.Writer.TryComplete();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 階段2錯誤: {ex.Message}");
        }
    }

    // 階段 3: 消費
    async Task Stage3_ConsumeAsync()
    {
        try
        {
            await foreach (var item in channel2.Reader.ReadAllAsync())
            {
                Console.WriteLine($"[階段3] 📥 最終結果: {item}");
                await Task.Delay(150);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 階段3錯誤: {ex.Message}");
        }
    }

    Console.WriteLine("▶ 【管線化執行】");
    var sw = Stopwatch.StartNew();
    await Task.WhenAll(
        Stage1_ProduceAsync(),
        Stage2_TransformAsync(),
        Stage3_ConsumeAsync()
    );
    sw.Stop();

    Console.WriteLine();
    Console.WriteLine($"✓ 管線完成，總耗時: {sw.ElapsedMilliseconds} ms");
    Console.WriteLine();
    Console.WriteLine("💡 背壓控制的效果:");
    Console.WriteLine("   - Channel 有限制的大小（3個項目）");
    Console.WriteLine("   - 當上游太快時會自動等待");
    Console.WriteLine("   - 防止記憶體無限增長");
}

// ============================================
// 【範例 10】I/O Bound vs CPU Bound 效能對比
// ============================================
async Task Example10_IOBoundVsCPUBound()
{
    Console.WriteLine("【範例 10】I/O Bound vs CPU Bound 效能對比");
    Console.WriteLine();
    Console.WriteLine("📌 情境說明:");
    Console.WriteLine("   - I/O Bound: 等待網路/磁盤，非同步有明顯優勢");
    Console.WriteLine("   - CPU Bound: 計算密集，非同步幫助不大，需用並行");
    Console.WriteLine();

    // I/O Bound 示例
    Console.WriteLine("▶ 【I/O Bound 示例】");
    async Task<int> SimulateIOAsync(int id)
    {
        await Task.Delay(200); // 模擬 I/O
        return id * 2;
    }

    // 順序 I/O
    var sw = Stopwatch.StartNew();
    int r1 = await SimulateIOAsync(1);
    int r2 = await SimulateIOAsync(2);
    int r3 = await SimulateIOAsync(3);
    sw.Stop();
    Console.WriteLine($"  順序 I/O: {sw.ElapsedMilliseconds} ms (≈ 600ms)");

    // 並行 I/O
    sw.Restart();
    int[] ioResults = await Task.WhenAll(
        SimulateIOAsync(1),
        SimulateIOAsync(2),
        SimulateIOAsync(3)
    );
    sw.Stop();
    Console.WriteLine($"  並行 I/O: {sw.ElapsedMilliseconds} ms (≈ 200ms)");
    Console.WriteLine("  ✨ 並行化大幅提升 I/O 效能！");
    Console.WriteLine();

    // CPU Bound 示例
    Console.WriteLine("▶ 【CPU Bound 示例】");

    // 計算密集函數（遞迴計算 Fibonacci 數列）
    long ComputeFibonacci(int n)
    {
        if (n <= 1) return n;
        return ComputeFibonacci(n - 1) + ComputeFibonacci(n - 2);
    }

    // 同步執行 CPU Bound（順序計算 3 次）
    sw.Restart();
    long cpu1 = ComputeFibonacci(35);
    long cpu2 = ComputeFibonacci(35);
    long cpu3 = ComputeFibonacci(35);
    sw.Stop();
    long syncCpuTime = sw.ElapsedMilliseconds;
    Console.WriteLine($"  同步執行（1個線程）: {syncCpuTime} ms");
    Console.WriteLine($"  結果: {cpu1}, {cpu2}, {cpu3}");

    // Task.Run 並行執行（使用多個線程）
    sw.Restart();
    long[] cpuResults = await Task.WhenAll(
        Task.Run(() => ComputeFibonacci(35)),
        Task.Run(() => ComputeFibonacci(35)),
        Task.Run(() => ComputeFibonacci(35))
    );
    sw.Stop();
    long parallelCpuTime = sw.ElapsedMilliseconds;
    Console.WriteLine($"  Task.Run 並行（多線程）: {parallelCpuTime} ms");
    Console.WriteLine($"  結果: {cpuResults[0]}, {cpuResults[1]}, {cpuResults[2]}");

    // 計算加速比
    if (parallelCpuTime > 0)
    {
        double speedup = (double)syncCpuTime / parallelCpuTime;
        Console.WriteLine($"  ⚡ 加速比: {speedup:F2}x (理想值接近 CPU 核心數)");
        Console.WriteLine("  ⚠️ 注意: 實際加速取決於機器 CPU 核心數與當前負載，單核機器無加速效果。");
    }
    Console.WriteLine();

    Console.WriteLine("💡 最佳實踐對比:");
    Console.WriteLine();
    Console.WriteLine("  ┌─ I/O Bound (網路、檔案、資料庫)");
    Console.WriteLine("  │  使用: async/await");
    Console.WriteLine("  │  原因: 等待時不阻塞線程");
    Console.WriteLine("  │");
    Console.WriteLine("  ├─ CPU Bound (計算、資料處理)");
    Console.WriteLine("  │  使用: Task.Run 或 Parallel");
    Console.WriteLine("  │  原因: 需要多個線程並行計算");
    Console.WriteLine("  │");
    Console.WriteLine("  └─ 混合情況");
    Console.WriteLine("     使用: 兼用 async/await 和 Task.Run");
}

// ---------------------------------------------------------
// 輔助類別：用於安全演示 async void 崩潰的 SynchronizationContext
// ---------------------------------------------------------
class SafeAsyncVoidContext : SynchronizationContext
{
    public Exception? CaughtException { get; private set; }

    public override void Post(SendOrPostCallback d, object? state)
    {
        try
        {
            d(state);
        }
        catch (Exception ex)
        {
            CaughtException = ex;
        }
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        try
        {
            d(state);
        }
        catch (Exception ex)
        {
            CaughtException = ex;
        }
    }
}
