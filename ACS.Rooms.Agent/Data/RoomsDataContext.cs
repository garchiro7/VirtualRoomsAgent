using ACS.Rooms.Agent.Models;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Communication.Rooms;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ACS.Rooms.Agent.Data
{
    public class RoomsDataContext : IRoomsDataContext
    {
        private readonly RoomsClient _roomsClient;
        private readonly CommunicationIdentityClient _identityClient;
        private readonly Container _cosmosContainer;
        private readonly ILogger<RoomsDataContext> _logger;
        public RoomsDataContext(string? acsCnx, string? cosmosCnx, string? containerId, string? databaseId, ILogger<RoomsDataContext> logger)
        {
            // Initialize ACS clients
            _roomsClient = new RoomsClient(acsCnx);
            _identityClient = new CommunicationIdentityClient(acsCnx);

            // Initialize Cosmos DB client & container
            var cosmosClient = new CosmosClient(cosmosCnx);
            _cosmosContainer = cosmosClient.GetContainer(databaseId, containerId);
            _logger = logger;
        }

        /// <summary>
        /// Create an ACS virtual room and store metadata in Cosmos DB.
        /// </summary>
        public async Task<RoomAppt> CreateRoomAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            List<Participant> participants,
            string topic,
            List<string> tags,
            string bookedBy)
        {
            
            var createRoomOptions = new CreateRoomOptions()
            {
                ValidFrom = startTime,
                ValidUntil = endTime
            };

            var response = await _roomsClient.CreateRoomAsync(createRoomOptions);

            var acsRoom = response.Value;

            var RoomAppt = new RoomAppt
            {
                Id = Guid.NewGuid().ToString(),
                RoomId = acsRoom.Id,
                StartTime = startTime,
                EndTime = endTime,
                Participants = participants ?? new List<Participant>(),
                Topic = topic,
                Tags = tags ?? new List<string>(),
                BookedBy = bookedBy
            };

            _logger.LogInformation($"Room create:{RoomAppt.Id} into the Rooms SDK");

            if (participants.Count > 0)
            {
                var roomParticipants = new List<RoomParticipant>();
                foreach (var p in participants)
                {
                    // For simple I'll generate ACS identity to each participant

                    var newParticipant = await _identityClient.CreateUserAsync();
                    roomParticipants.Add(new RoomParticipant(newParticipant)
                    {
                        Role = ParseParticipantRole(p.Role)
                    });

                    p.AcsParticipantId = newParticipant.Value.RawId;
                    
                    _logger.LogInformation($"Add participant:{p.Email} into the Room");
                }

                // Add them to the ACS room
                await _roomsClient.AddOrUpdateParticipantsAsync(RoomAppt.RoomId, roomParticipants);
            }
            _logger.LogInformation($"Room created:{RoomAppt.Id} with all participants");

            await _cosmosContainer.CreateItemAsync(RoomAppt);
            _logger.LogInformation($"Room persisted:{RoomAppt.Id} into Cosmos");

            return RoomAppt;
        }

        /// <summary>
        /// Query rooms by participant (email, name, or ID), date, topic, tag, etc.
        /// For simplicity, we’re demonstrating a basic query approach.
        /// </summary>
        public async Task<List<RoomAppt>> SearchRoomsAsync(
            string participantEmail = null,
            string participantName = null,
            string acsParticipantId = null,
            DateTimeOffset? date = null,
            string topic = null,
            string tag = null,
            string bookedBy = null)
        {
            var queryText = new StringBuilder("SELECT * FROM c WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(participantEmail))
            {
                queryText.Append(" AND ARRAY_CONTAINS(c.Participants, {\"Email\": @participantEmail}, true)");
                parameters.Add("@participantEmail", participantEmail);
            }

            if (!string.IsNullOrEmpty(participantName))
            {
                queryText.Append(" AND ARRAY_CONTAINS(c.Participants, {\"Name\": @participantName}, true)");
                parameters.Add("@participantName", participantName);
            }

            if (!string.IsNullOrEmpty(acsParticipantId))
            {
                queryText.Append(" AND ARRAY_CONTAINS(c.Participants, {\"AcsParticipantId\": @acsParticipantId}, true)");
                parameters.Add("@acsParticipantId", acsParticipantId);
            }

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);

                queryText.Append(" AND c.StartTime >= @startOfDay AND c.StartTime < @endOfDay");
                parameters.Add("@startOfDay", startOfDay);
                parameters.Add("@endOfDay", endOfDay);
            }
            if (!string.IsNullOrEmpty(topic))
            {
                queryText.Append(" AND CONTAINS(c.Topic, @topic)");
                parameters.Add("@topic", topic);
            }

            if (!string.IsNullOrEmpty(tag))
            {

                queryText.Append(" AND ARRAY_CONTAINS(c.Tags, @tag)");
                parameters.Add("@tag", tag);
            }

            if (!string.IsNullOrEmpty(bookedBy))
            {
                queryText.Append(" AND c.BookedBy = @bookedBy");
                parameters.Add("@bookedBy", bookedBy);
            }

            var queryDef = new QueryDefinition(queryText.ToString());
            foreach (var kvp in parameters)
            {
                queryDef = queryDef.WithParameter(kvp.Key, kvp.Value);
            }


            _logger.LogInformation($"Query: {queryDef.QueryText }");

            var results = new List<RoomAppt>();
            using var feed = _cosmosContainer.GetItemQueryIterator<RoomAppt>(queryDef);
            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }


        private static ParticipantRole ParseParticipantRole(string role)
        {
            // Valid roles: Presenter, Attendee, Consumer
            return role?.ToLower() switch
            {
                "presenter" => ParticipantRole.Presenter,
                "attendee" => ParticipantRole.Attendee,
                _ => ParticipantRole.Attendee // Default
            };
        }
    }
}