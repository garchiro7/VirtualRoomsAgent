using Newtonsoft.Json;

namespace ACS.Rooms.Agent.Models
{
    public class RoomAppt
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        // The Azure Communication Services room ID
        public string RoomId { get; set; }

        // Start & End times for the room
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        // A list of participants (names, IDs, or emails)
        public List<Participant> Participants { get; set; }

        // Topic or subject (e.g., "Physics")
        public string Topic { get; set; }

        // Any additional tags (e.g., ["education", "lecture"])
        public List<string> Tags { get; set; }

        // Optionally, who booked this room
        public string BookedBy { get; set; }

    }
}
