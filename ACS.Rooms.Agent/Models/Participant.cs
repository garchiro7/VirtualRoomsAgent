using Newtonsoft.Json;

namespace ACS.Rooms.Agent.Models
{
    public class Participant
    {
        [JsonProperty("acsParticipantId")]
        public string AcsParticipantId { get; internal set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

}
