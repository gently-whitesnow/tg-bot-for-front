namespace Sorcer.Models.User;

public enum UserState
{
    None,
    GettingEventImage,
    GettingEventDateTime,
    GettingEventParticipants,
    GettingSaveApproval,
}