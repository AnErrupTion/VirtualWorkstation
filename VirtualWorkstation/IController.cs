namespace VirtualWorkstation;

/// <summary>
/// Used only by controllers requiring a USB controller for at least 1 option.
/// </summary>
public interface IController
{
    public void RefreshUsbControllerList(int startIndex);

    public void TriggerSelectionChangedEvent();
}