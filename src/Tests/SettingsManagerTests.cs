namespace Tests;

[TestFixture]
public class SettingsManagerTests : IDisposable
{
    public void Dispose()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);

        var dir = Path.GetDirectoryName(_filePath);
        if (dir != null) Directory.Delete(dir);
    }

    public SettingsManagerTests()
    {
        _filePath = SettingsManager.PathHelper.BuildPath(Environment.SpecialFolder.ApplicationData, new[] { "SettingsManagerTests" }, "TestSettings.json");
        _testClass = new SettingsManager(_filePath, true);
    }

    private readonly SettingsManager _testClass;
    private readonly string _filePath;

    [Test]
    [Order(0)]
    public void CanConstruct()
    {
        var instance = new SettingsManager(_filePath);

        Assert.That(instance, Is.Not.Null);

        Assert.That(File.Exists(_filePath), Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Order(1)]
    public void CannotConstructWithInvalidFilePath(string value)
    {
        Assert.Throws<ArgumentNullException>(() => new SettingsManager(value));
    }

    [Test]
    [Order(2)]
    public void CanCallTrySet()
    {
        var key = "TestStringValue932526661";
        var value = "Helloooooo!";

        var result = _testClass.TrySet(key, value, true);

        Assert.That(result, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Order(3)]
    public void CannotCallTrySetWithInvalidKey(string value)
    {
        Assert.Throws<ArgumentNullException>(() => _testClass.TrySet(value, "INVALID_KEY"));
    }

    [Test]
    [Order(4)]
    public void CanCallTryGet()
    {
        var key = "TestStringValue932526661";

        var result = _testClass.TryGet(key, out var type, out var value);

        Assert.That(result, Is.True);

        Assert.That(type, Is.Not.Null);

        Assert.That(value, Is.Not.Null);

        Assert.That(type, Is.EqualTo(typeof(string)));

        Assert.That(value.GetType(), Is.EqualTo(typeof(string)));

        Assert.That(value, Is.EqualTo("Helloooooo!"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Order(5)]
    public void CannotCallTryGetWithInvalidKey(string value)
    {
        Assert.Throws<ArgumentNullException>(() => _testClass.TryGet(value, out _, out _));
    }

    [Test]
    [Order(6)]
    public void CanCallExists()
    {
        var key = "TestStringValue932526661";

        var result = _testClass.Exists(key);

        Assert.That(result, Is.True);
    }

    [Test]
    [Order(7)]
    public void CanCallTryRemove()
    {
        var key = "TestStringValue932526661";

        var result = _testClass.TryRemove(key, true);

        Assert.That(result, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Order(8)]
    public void CannotCallTryRemoveWithInvalidKey(string value)
    {
        Assert.Throws<ArgumentNullException>(() => _testClass.TryRemove(value));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Order(9)]
    public void CannotCallExistsWithInvalidKey(string value)
    {
        Assert.Throws<ArgumentNullException>(() => _testClass.Exists(value));
    }

    [Test]
    [Order(999)]
    public async Task WaitForFileChangeAutoReloadAsync()
    {
        _testClass.FileReloaded += delegate
        {
            var key = "TestStringValue932526662";

            var result = _testClass.TryRemove(key, true);

            Assert.That(result, Is.False);
        };

        File.WriteAllText(_filePath, "");

        await Task.Delay(2000);
    }
}