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
        get
        {
            Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
            return (string)_configModel["vm_folder"];
        }
        set
        {
            Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
            _configModel["vm_folder"] = value;
        }
    }

    public static string CustomQemuPath
    {
        get
        {
            Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
            return (string)_configModel["custom_qemu_path"];
        }
        set
        {
            Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
            _configModel["custom_qemu_path"] = value;
        }
    }

    private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "VirtualWorkstation.toml");

    private static TomlTable? _configModel;

    public static void Load() => _configModel = Toml.ToModel(File.ReadAllText(ConfigPath));

    public static void Save()
    {
        Debug.Assert(_configModel != null, $"{nameof(_configModel)} != null");
        File.WriteAllText(ConfigPath, Toml.FromModel(_configModel));
    }
}