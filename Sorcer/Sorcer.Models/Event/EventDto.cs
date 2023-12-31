namespace Sorcer.Models.Event;

public class EventDto
{
    public Guid Id { get; set; }
    public string EventDescription { get; set; }
    public string StringifyEventDateTime { get; set; }
    public DateTimeOffset EventDateTime { get; set; }
    public string ImagePath { get; set; }
}