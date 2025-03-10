using ACS.Rooms.Agent.Data;
using ACS.Rooms.Agent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ACS.Rooms.Agent
{
    public class BookRoomFnc
    {
        private readonly ILogger<BookRoomFnc> _logger;
        private readonly IRoomsDataContext _roomsContext;

        public BookRoomFnc(ILogger<BookRoomFnc> logger, IRoomsDataContext roomsContext)
        {
            _logger = logger;
            _roomsContext = roomsContext;
        }

        [Function("BookRoomFnc")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("BookRoom function triggered.");

            // Parse request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            DateTimeOffset startTime = data?.startTime ?? DateTimeOffset.UtcNow;
            DateTimeOffset endTime = data?.endTime ?? startTime.AddHours(1);

            // Deserialize participants list
            var participantsJson = data?.participants?.ToString();
            var participants = string.IsNullOrEmpty(participantsJson)
                ? new List<Participant>()
                : JsonConvert.DeserializeObject<List<Participant>>(participantsJson);

            string topic = data?.topic ?? "Untitled";
            var tagsJson = data?.tags?.ToString();
            var tags = string.IsNullOrEmpty(tagsJson)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(tagsJson);

            string bookedBy = data?.bookedBy ?? "Unknown";

            // Create the room
            var room = await _roomsContext.CreateRoomAsync(
                startTime,
                endTime,
                participants,
                topic,
                tags,
                bookedBy
            );

            // Return the new RoomRecord as JSON
            return new OkObjectResult(room);
        }
    }
}
