using ATI.Services.Common.Behaviors;
using Microsoft.Extensions.Options;
using Sorcer.Models.Event;
using Sorcer.Models.Options;

namespace Sorcer.DataAccess.Repositories;

public class EventsRepository : FileRepository<List<EventDto>>
{
    public EventsRepository(IOptions<FileSystemOptions> options) 
        : base(options.Value.EventsRepositoryFilePath)
    {
    }

    public Task<OperationResult<List<EventDto>>> GetEventsAsync()
    {
        return ReadAllAsync();
    }
    
    public Task<OperationResult<List<EventDto>>> AddEventAsync(EventDto eventDto)
    {
        return WriteAllAsync((events) =>
        {
            events.Add(eventDto);
        });
    }
    
    public Task<OperationResult<List<EventDto>>> RemoveEventAsync(Guid eventId)
    {
        return WriteAllAsync(events =>
        {
            var @event = events.FirstOrDefault(e => e.Id == eventId);
            if(@event == null)
                return;
            
            events.Remove(@event);
        });
    }
}