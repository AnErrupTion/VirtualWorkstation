namespace VirtualWorkstation;

/// <summary>
/// Used by MainWindow and VirtualMachineSettings to keep track of a tab page's status in a tab control.
/// </summary>
public interface ITabPage
{
    public bool Opened { get; set; }

    public int VmTabIndex { get; set; }

    /// <summary>
    /// Used only by controllers.
    /// </summary>
    public int Index { get; set; }
}