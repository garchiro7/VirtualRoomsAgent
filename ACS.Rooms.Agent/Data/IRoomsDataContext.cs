using ACS.Rooms.Agent.Models;

namespace ACS.Rooms.Agent.Data
{
    public interface IRoomsDataContext
    {
        Task<RoomAppt> CreateRoomAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            List<Participant> participants,
            string topic,
            List<string> tags,
            string bookedBy);

        Task<List<RoomAppt>> SearchRoomsAsync(
            string participantEmail = null,
            string participantName = null,
            string acsParticipantId = null,
            DateTimeOffset? date = null,
            string topic = null,
            string tag = null,
            string bookedBy = null);
    }
}
