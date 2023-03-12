using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SettingsManager : IDisposable
{
    private readonly string _filePath;
    private readonly FileSystemWatcher _watcher;
    private Dictionary<string, object> _data;
    private bool _disposedValue;

    public SettingsManager(string filePath, bool autoReload = false)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (directory == null) throw new ArgumentException("Invalid file path.", nameof(filePath));

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        _filePath = filePath;

        if (!File.Exists(filePath))
        {
            var fileStream = File.Create(filePath);
            fileStream.Close();
        }
        else
        {
            LoadFile();
        }

        if (autoReload)
        {
            var fileName = Path.GetFileName(filePath);

            _watcher = new FileSystemWatcher(directory);
            _watcher.EnableRaisingEvents = true;
            _watcher.Filter = fileName;
            _watcher.IncludeSubdirectories = false;
            _watcher.Changed += (sender, args) =>
            {
                LoadFile();
                FileReloaded?.Invoke(this, EventArgs.Empty);
            };
        }

        _data = new Dictionary<string, object>();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler FileReloaded;

    public void Save()
    {
        SaveFile();
    }

    public void Reload()
    {
        LoadFile();
    }

    public bool Exists(string key)
    {
        if (_data == null) return false;

        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        return _data.ContainsKey(key);
    }

    public bool TrySet<T>(string key, T value, bool save = false)
    {
        if (_data == null) return false;

        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        try
        {
            if (_data.ContainsKey(key))
                _data[key] = value;
            else
                _data.Add(key, value);
            if (save) _ = Task.Run(SaveFile);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryGet(string key, out Type type, out object value)
    {
        if (_data == null)
        {
            type = null;
            value = null;
            return false;
        }

        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        if (Exists(key))
        {
            value = _data[key];

            type = value.GetType();

            return true;
        }

        type = null;
        value = null;

        return false;
    }

    public bool TryRemove(string key, bool save = false)
    {
        if (_data == null) return false;

        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var result = _data.Remove(key);

        if (save) _ = Task.Run(SaveFile);

        return result;
    }

    private void LoadFile()
    {
        var dataStr = File.ReadAllText(_filePath);
        _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
        dataStr = null;
    }

    private void SaveFile()
    {
        if (_watcher != null) _watcher.EnableRaisingEvents = false;

        var dataStr = JsonConvert.SerializeObject(_data);
        File.WriteAllText(_filePath, dataStr);
        dataStr = null;

        if (_watcher != null) _watcher.EnableRaisingEvents = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                SaveFile();

                _watcher?.Dispose();
            }

            _data = null;

            _disposedValue = true;
        }
    }

    public static class PathHelper
    {
        public static string BuildPath(Environment.SpecialFolder folder, string[] subfolders, string filename)
        {
            var folderPath = Environment.GetFolderPath(folder);

            if (subfolders != null && subfolders.Any())
                foreach (var sub in subfolders)
                    folderPath = Path.Combine(folderPath, sub);

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            return Path.Combine(folderPath, filename);
        }
    }
}