using System.Threading.Tasks;
using ATI.Services.Common.Behaviors.OperationBuilder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Sorcer.DataAccess.Managers;

namespace Sorcer.Api;

public class EventsController : Controller
{
    private readonly EventsManager _eventsManager;

    public EventsController(EventsManager eventsManager)
    {
        _eventsManager = eventsManager;
    }

    [HttpGet]
    [Route("events")]
    public Task<IActionResult> GetEventsAsync()
    {
        return _eventsManager.GetEventsForApiAsync().AsActionResultAsync();
    }
}