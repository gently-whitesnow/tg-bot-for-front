using Sorcer.Models.Event;

namespace Sorcer.Models.User;

public class TempUserData
{
    public long UserId { get; set; }
    public UserState State { get; set; }
    public EventDto EventDto { get; set; }
}