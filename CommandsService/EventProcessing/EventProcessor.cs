using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;


        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            System.Console.WriteLine("---> Determining event...");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    System.Console.WriteLine("---> Platform_Published event detected");
                    return EventType.PlatformPublished;
                default:
                    System.Console.WriteLine("Could not determine event type");
                    return EventType.Undetermined;
            }

        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);

                    if (!repo.ExternalPlatformExists(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        System.Console.WriteLine("---> Platform added");

                    }
                    else
                    {
                        System.Console.WriteLine($" Platform already exists {plat.ExternalId}");
                    }



                }
                catch (System.Exception ex)
                {
                    
                    System.Console.WriteLine($"---> Could not add platform to DB {ex.Message}");
                }
            }
        }




    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }

}