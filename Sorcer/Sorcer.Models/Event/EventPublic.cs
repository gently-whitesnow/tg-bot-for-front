namespace Sorcer.Models.Event;

public class EventPublic
{
    public string EventDescription { get; set; }
    public DateTimeOffset DateTimeOffset { get; set; }
    public string StringifyEventDateTime { get; set; }
    public string ImageUrl { get; set; }
    public bool IsNextEvent { get; set; }
}