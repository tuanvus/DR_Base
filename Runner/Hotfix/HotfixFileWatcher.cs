using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

public sealed class HotfixFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly ConcurrentDictionary<string, Timer> _debounce = new();

    public HotfixFileWatcher(string hotfixRootDir)
    {
        _watcher = new FileSystemWatcher(hotfixRootDir)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += OnEvent;
        _watcher.Renamed += OnEvent;
        _watcher.Changed += OnEvent; // Changed rất “ồn”, nên cần debounce [web:220]
    }

    private void OnEvent(object sender, FileSystemEventArgs e)
    {
        // Chỉ quan tâm manifest
        if (!e.FullPath.EndsWith("READY.txt", StringComparison.OrdinalIgnoreCase))
            return;

        // Debounce vì 1 lần copy/rename có thể ra nhiều event [web:220][web:207]
        _debounce.AddOrUpdate(
            e.FullPath,
            _ => new Timer(_ => OnStableReady(e.FullPath), null, 500, Timeout.Infinite),
            (_, old) => { old.Change(500, Timeout.Infinite); return old; }
        );
    }

    private void OnStableReady(string readyPath)
    {
        // 1) Derive version folder
        var versionDir = Path.GetDirectoryName(readyPath)!; // hotfix/v1.1
        var version = Path.GetFileName(versionDir);         // v1.1

        // 2) Chờ HotfixImpl.dll “mở được” (copy xong)
        var dllPath = Path.Combine(versionDir, "HotfixImpl.dll");
        WaitUntilReadable(dllPath, timeoutMs: 5000);

        // 3) Trigger hotfix
        // hotfixManager.LoadVersion(version, dllPath, "YourNS.HotfixModule");
        // hotfixManager.SwitchDefault(version);
    }

    private static void WaitUntilReadable(string path, int timeoutMs)
    {
        var start = Environment.TickCount;
        while (true)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return;
            }
            catch
            {
                if (Environment.TickCount - start > timeoutMs) throw;
                Thread.Sleep(100);
            }
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        foreach (var kv in _debounce) kv.Value.Dispose();
    }
}
