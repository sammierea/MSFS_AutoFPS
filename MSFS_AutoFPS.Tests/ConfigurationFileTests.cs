using System.Collections.Generic;
using System.IO;
using MSFS_AutoFPS;

namespace MSFS_AutoFPS.Tests
{
    public class ConfigurationFileTests : IDisposable
    {
        private readonly string _configPath;
        private const string XmlTemplate = """
            <?xml version="1.0" encoding="utf-8"?>
            <appSettings>
              <add key="key1" value="value1" />
              <add key="key2" value="value2" />
            </appSettings>
            """;

        public ConfigurationFileTests()
        {
            _configPath = Path.Combine(Path.GetTempPath(), $"AutoFPS_test_{Guid.NewGuid()}.config");
            File.WriteAllText(_configPath, XmlTemplate);
        }

        public void Dispose()
        {
            if (File.Exists(_configPath))
                File.Delete(_configPath);
        }

        private ConfigurationFile CreateAndLoad()
        {
            var cf = new ConfigurationFile(_configPath);
            cf.LoadConfigurationFromFile();
            return cf;
        }

        [Fact]
        public void LoadConfigurationFromFile_ReturnsFalse_WhenPathUnchanged()
        {
            var cf = new ConfigurationFile(_configPath);
            // First load: ConfigFile == ConfigFileLast (same path set in ctor), returns false
            bool changed = cf.LoadConfigurationFromFile();
            Assert.False(changed);
        }

        [Fact]
        public void GetSetting_ReturnsValue_ForExistingKey()
        {
            var cf = CreateAndLoad();
            string val = cf.GetSetting("key1");
            Assert.Equal("value1", val);
        }

        [Fact]
        public void GetSetting_ReturnsDefault_ForMissingKey()
        {
            var cf = CreateAndLoad();
            string val = cf.GetSetting("missingKey", "defaultVal");
            Assert.Equal("defaultVal", val);
        }

        [Fact]
        public void GetSetting_PersistsMissingKey_AfterFirstAccess()
        {
            var cf = CreateAndLoad();
            cf.GetSetting("newKey", "newDefault");

            // Reload from disk to confirm key was saved
            var cf2 = new ConfigurationFile(_configPath);
            cf2.LoadConfigurationFromFile();
            Assert.Equal("newDefault", cf2.GetSetting("newKey"));
        }

        [Fact]
        public void SettingExists_ReturnsTrue_ForExistingKey()
        {
            var cf = CreateAndLoad();
            Assert.True(cf.SettingExists("key1"));
        }

        [Fact]
        public void SettingExists_ReturnsFalse_ForMissingKey()
        {
            var cf = CreateAndLoad();
            Assert.False(cf.SettingExists("nonexistent"));
        }

        [Fact]
        public void SetSetting_UpdatesValue_ForExistingKey()
        {
            var cf = CreateAndLoad();
            cf.SetSetting("key1", "updated");
            Assert.Equal("updated", cf.GetSetting("key1"));
        }

        [Fact]
        public void SetSetting_PersistsUpdatedValue_ToDisk()
        {
            var cf = CreateAndLoad();
            cf.SetSetting("key1", "persistedValue");

            var cf2 = new ConfigurationFile(_configPath);
            cf2.LoadConfigurationFromFile();
            Assert.Equal("persistedValue", cf2.GetSetting("key1"));
        }

        [Fact]
        public void RemoveSetting_RemovesExistingKey()
        {
            var cf = CreateAndLoad();
            cf.RemoveSetting("key1");
            Assert.False(cf.SettingExists("key1"));
        }

        [Fact]
        public void RemoveSetting_PersistsRemoval_ToDisk()
        {
            var cf = CreateAndLoad();
            cf.RemoveSetting("key1");

            var cf2 = new ConfigurationFile(_configPath);
            cf2.LoadConfigurationFromFile();
            Assert.False(cf2.SettingExists("key1"));
        }

        [Fact]
        public void RemoveSetting_DoesNotThrow_ForMissingKey()
        {
            var cf = CreateAndLoad();
            var ex = Record.Exception(() => cf.RemoveSetting("nonexistent"));
            Assert.Null(ex);
        }

        [Fact]
        public void Indexer_Get_ReturnsValue_ForExistingKey()
        {
            var cf = CreateAndLoad();
            Assert.Equal("value2", cf["key2"]);
        }

        [Fact]
        public void Indexer_Set_UpdatesValue()
        {
            var cf = CreateAndLoad();
            cf["key2"] = "newValue2";
            Assert.Equal("newValue2", cf["key2"]);
        }

        [Fact]
        public void SaveConfiguration_WritesAllSettings_ToDisk()
        {
            var cf = CreateAndLoad();
            cf.SetSetting("key1", "saved1");
            cf.SetSetting("key2", "saved2");

            var cf2 = new ConfigurationFile(_configPath);
            cf2.LoadConfigurationFromFile();
            Assert.Equal("saved1", cf2.GetSetting("key1"));
            Assert.Equal("saved2", cf2.GetSetting("key2"));
        }

        [Fact]
        public void MultipleSettings_AllLoaded_Correctly()
        {
            var cf = CreateAndLoad();
            Assert.Equal("value1", cf.GetSetting("key1"));
            Assert.Equal("value2", cf.GetSetting("key2"));
        }
    }
}
