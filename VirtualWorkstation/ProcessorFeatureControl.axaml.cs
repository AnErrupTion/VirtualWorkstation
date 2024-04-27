using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

public partial class ProcessorFeatureControl : UserControl
{
    private readonly Architecture _architecture;
    private readonly CheckBox _enableUnsupportedOptions;
    private readonly List<ProcessorFeature> _features;
    private readonly Controls _featureControls;
    private int _index;

    public ProcessorFeatureControl(Architecture architecture, ref CheckBox enableUnsupportedOptions,
        List<ProcessorFeature> features, Controls featureControls, int index)
    {
        InitializeComponent();

        _architecture = architecture;
        _enableUnsupportedOptions = enableUnsupportedOptions;
        _features = features;
        _featureControls = featureControls;
        _index = index;

        CheckForUnsupportedOptions();

        Features.SelectedIndex = (int)features[index];
    }

    private void Features_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _features[_index] = (ProcessorFeature)Features.SelectedIndex;
        CheckForUnsupportedOptions();
    }
    
    private void Remove_OnClick(object? _, RoutedEventArgs e)
    {
        var featureControlIndex = _index + 1; // Our first child is the "Add" button so we must skip it

        _features.RemoveAt(_index);
        _featureControls.RemoveAt(featureControlIndex);

        for (var i = featureControlIndex; i < _featureControls.Count; i++)
        {
            if (_featureControls[i] is not ProcessorFeatureControl featureControl) throw new UnreachableException();
            featureControl._index--;
        }
    }

    public void CheckForUnsupportedOptions()
    {
        if (_architecture is Architecture.Amd64 or Architecture.I386)
        {
            _enableUnsupportedOptions.IsEnabled = false;
            _enableUnsupportedOptions.IsChecked = false;
            return;
        }

        var currentFeature = _features[_index];
        if (currentFeature is >= ProcessorFeature.X86Pae and <= ProcessorFeature.X86HyperVPassThrough)
        {
            _enableUnsupportedOptions.IsEnabled = false;
            _enableUnsupportedOptions.IsChecked = true;
            return;
        }

        _enableUnsupportedOptions.IsEnabled = true;

        for (var i = ProcessorFeature.X86Pae; i <= ProcessorFeature.X86HyperVPassThrough; i++)
        {
            if (Features.Items[(int)i] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = _enableUnsupportedOptions.IsChecked!.Value;
        }
    }
}