using ACS.Rooms.Agent.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ACS.Rooms.Agent
{
    public class SearchRoomFnc
    {
        private readonly ILogger<BookRoomFnc> _logger;
        private readonly IRoomsDataContext _roomsContext;

        public SearchRoomFnc(ILogger<BookRoomFnc> logger, IRoomsDataContext roomsContext)
        {
            _logger = logger;
            _roomsContext = roomsContext;
        }

        [Function("SearchRoom")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("SearchRooms function triggered.");

            // Extract possible query parameters
            string participantEmail = req.Query["participantEmail"];
            string participantName = req.Query["participantName"];
            string acsParticipantId = req.Query["acsParticipantId"];

            string dateString = req.Query["date"];
            string topic = req.Query["topic"];
            string tag = req.Query["tag"];
            string bookedBy = req.Query["bookedBy"];

            // Parse date
            System.DateTimeOffset? date = null;
            if (!string.IsNullOrEmpty(dateString) && System.DateTimeOffset.TryParse(dateString, out var parsedDate))
            {
                date = parsedDate;
            }

            // Query the service
            var rooms = await _roomsContext.SearchRoomsAsync(
                participantEmail: participantEmail,
                participantName: participantName,
                acsParticipantId: acsParticipantId,
                date: date,
                topic: topic,
                tag: tag,
                bookedBy: bookedBy
            );

            // Return result
            return new OkObjectResult(rooms);
        }
    }
}
