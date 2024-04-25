using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QemuSharp.Structs;
using QemuSharp.Structs.Enums;

namespace VirtualWorkstation;

// TODO: Make a feature disappear when it's added (and appear again when it's removed)
public partial class ProcessorSettingsPage : UserControl, ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    public int Index { get; set; }

    private readonly VirtualMachine _vm;

    public ProcessorSettingsPage(ref VirtualMachine vm)
    {
        InitializeComponent();

        _vm = vm;

        // FIXME: Determine correct amount of sockets, cores and threads
        Sockets.Maximum = Environment.ProcessorCount;
        Cores.Maximum = Environment.ProcessorCount;
        Threads.Maximum = Environment.ProcessorCount;

        CheckForUnsupportedOptions();

        Model.SelectedIndex = (int)vm.Processor.Model;
        CustomModel.Text = vm.Processor.CustomModel;

        for (var i = 0; i < vm.Processor.AddFeatures.Count; i++) AddedFeatures.Children.Add(new ProcessorFeatureControl(
            _vm.Architecture, ref EnableUnsupportedFeatureOptions, vm.Processor.AddFeatures, AddedFeatures.Children, i));

        for (var i = 0; i < vm.Processor.RemoveFeatures.Count; i++) RemovedFeatures.Children.Add(new ProcessorFeatureControl(
            _vm.Architecture, ref EnableUnsupportedFeatureOptions, vm.Processor.RemoveFeatures, RemovedFeatures.Children, i));

        Sockets.Value = vm.Processor.Sockets;
        Cores.Value = vm.Processor.Cores;
        Threads.Value = vm.Processor.Threads;
    }

    private void Model_OnSelectionChanged(object? _, SelectionChangedEventArgs e)
    {
        _vm.Processor.Model = (ProcessorModel)Model.SelectedIndex;
        CheckForUnsupportedOptions();

        CustomModel.IsEnabled = _vm.Processor.Model == ProcessorModel.Custom;
    }

    private void EnableUnsupportedOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
        => CheckForUnsupportedOptions();

    private void CustomModel_OnTextChanged(object? _, TextChangedEventArgs e)
        => _vm.Processor.CustomModel = CustomModel.Text!;

    private void AddFeature_OnClick(object? _, RoutedEventArgs e)
    {
        _vm.Processor.AddFeatures.Add(GetDefaultFeature());
        AddedFeatures.Children.Add(new ProcessorFeatureControl(_vm.Architecture, ref EnableUnsupportedFeatureOptions,
            _vm.Processor.AddFeatures, AddedFeatures.Children, _vm.Processor.AddFeatures.Count - 1));
    }

    private void RemoveFeature_OnClick(object? _, RoutedEventArgs e)
    {
        _vm.Processor.RemoveFeatures.Add(GetDefaultFeature());
        RemovedFeatures.Children.Add(new ProcessorFeatureControl(_vm.Architecture, ref EnableUnsupportedFeatureOptions,
            _vm.Processor.RemoveFeatures, RemovedFeatures.Children, _vm.Processor.RemoveFeatures.Count - 1));
    }

    private void EnableUnsupportedFeatureOptions_OnIsCheckedChanged(object? _, RoutedEventArgs e)
    {
        for (var i = 1; i < AddedFeatures.Children.Count; i++)
        {
            if (AddedFeatures.Children[i] is not ProcessorFeatureControl addedFeatureControl) throw new UnreachableException();
            addedFeatureControl.CheckForUnsupportedOptions();
        }

        for (var i = 1; i < RemovedFeatures.Children.Count; i++)
        {
            if (RemovedFeatures.Children[i] is not ProcessorFeatureControl removedFeatureControl) throw new UnreachableException();
            removedFeatureControl.CheckForUnsupportedOptions();
        }
    }

    private void Sockets_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
        => _vm.Processor.Sockets = (ulong)e.NewValue!.Value;

    private void Cores_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
        => _vm.Processor.Cores = (ulong)e.NewValue!.Value;

    private void Threads_OnValueChanged(object? _, NumericUpDownValueChangedEventArgs e)
        => _vm.Processor.Threads = (ulong)e.NewValue!.Value;

    private void CheckForUnsupportedOptions()
    {
        if (_vm.Architecture is Architecture.Amd64 or Architecture.I386)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = false;
            return;
        }

        if (_vm.Processor.Model is >= ProcessorModel.X86Host and <= ProcessorModel.X86Max)
        {
            EnableUnsupportedOptions.IsEnabled = false;
            EnableUnsupportedOptions.IsChecked = true;
            return;
        }

        EnableUnsupportedOptions.IsEnabled = true;

        for (var i = ProcessorModel.X86Host; i <= ProcessorModel.X86Max; i++)
        {
            if (Model.Items[(int)i] is not ComboBoxItem item) throw new UnreachableException();
            item.IsEnabled = EnableUnsupportedOptions.IsChecked!.Value;
        }
    }

    private ProcessorFeature GetDefaultFeature() => _vm.Architecture switch
    {
        Architecture.Amd64 or Architecture.I386 => ProcessorFeature.X86Kvm,
        _ => throw new UnreachableException()
    };
}