namespace Sorcer.Models.CallbackHandler;

/// <summary>
/// Существует лимит на размер CallbackData
/// </summary>
public class CallbackData
{
    /// <summary>
    /// button type
    /// </summary>
    public Buttons BT { get; set; }
    /// <summary>
    /// additional data
    /// </summary>
    public string AD { get; set; }
}