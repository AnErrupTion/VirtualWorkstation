using System;
using System.Diagnostics;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace VirtualWorkstation;

public static class GlobalSettings
{
    public static string VmFolder
    {
        get => GetValue("vm_folder");
        set => SetValue("vm_folder", value);
    }

    public static string CustomQemuPath
    {
        get => GetValue("custom_qemu_path");
        set => SetValue("custom_qemu_path", value);
    }

    public static string CustomSwtpmPath
    {
        get => GetValue("custom_swtpm_path");
        set => SetValue("custom_swtpm_path", value);
    }

    public static string CustomVirtioFsDPath
    {
        get => GetValue("custom_virtiofsd_path");
        set => SetValue("custom_virtiofsd_path", value);
    }

    private static readonly string ConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VirtualWorkstation");
    private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "VirtualWorkstation.toml");

    private static TomlTable? _configModel;

    public static void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            Directory.CreateDirectory(ConfigDirectory);
            _configModel = [];
            Directory.CreateDirectory(VmFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualWorkstation"));
            CustomQemuPath = string.Empty;
            CustomSwtpmPath = string.Empty;
            CustomVirtioFsDPath = string.Empty;
            return;
        }

        _configModel = Toml.ToModel(File.ReadAllText(ConfigPath));
    }

    public static void Save()
    {
        Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
        File.WriteAllText(ConfigPath, Toml.FromModel(_configModel));
    }

    private static string GetValue(string key)
    {
        Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
        return (string)_configModel[key];
    }

    public static void SetValue(string key, string value)
    {
        Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
        _configModel[key] = value;
    }
}