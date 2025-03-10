# VirtualRoomsAgent
## Introduction
Create a Virtual Room Booking Agent for scheduling and managing virtual meetings using Azure Communication Services (ACS). The agent interprets natural language commands, extracts relevant details (date/time, participants, topic, etc.), and issues API requests to create or modify ACS rooms. Data about these rooms (e.g., participants, time range, tags) is stored in a Cosmos DB or similar repository for easy querying.

Primary Goals:
Book Rooms – Create a new virtual room with start/end times, a topic, participants, tags, etc. 
Search Rooms – Retrieve stored rooms based on filters like date, topic, participants, or tags. 

## Architecture
Copilot Studio: Provides an environment where you can create, configure, and deploy AI agents. You define actions (skills) using custom connectors that map to your Azure Functions or REST APIs.  
Azure Functions: The serverless layer that exposes APIs like: 
BookRoom to create a new ACS virtual room. 
SearchRooms to retrieve appointments from a data store.   
Azure Communication Services (ACS) Virtual Rooms: Handles the underlying real-time communication features (meeting links, roles, participant identities).  
Data Storage (Cosmos DB ): Stores persistent metadata about each booked room, including time ranges, participants, tags, and so on.  

## Agent behavior
Agent Prompts
Booking / Scheduling 
“Book me a room with Dr. Brown next Monday at 10 AM about telehealth.” 
“I need a virtual meeting with three attendees tomorrow at 2 PM on Physics.” 
“Schedule a room for me, Alice, and Bob on Friday from 3–4 PM.” 
“I want a one-hour meeting to discuss the project plan.” 
Searching / Viewing Rooms 
“Show me all my rooms related to telehealth.” 
“Find appointments with Dr. Brown next week.” 
“Do I have any meetings tomorrow?” 
“List all rooms tagged with ‘physics’ this month.” 
Ambiguous / Incomplete 
“Book a meeting.” (Agent must ask for time/date, participants, topic, etc.) 
“Add participants to my room.” (Agent must clarify which room, who, roles, etc.) 

## Data Model
The agent deals with two main object types: RoomRecord and Participant.

###RoomRecord Fields
- RoomId (string): Unique ACS room identifier. 
- StartTime (DateTimeOffset): When the meeting starts. 
- EndTime (DateTimeOffset): When the meeting ends. 
- Topic (string): Subject or purpose of the meeting (e.g., “Telehealth Check-up”). 
- Tags (string[]): Arbitrary labels for searching (e.g., ["telehealth", "private"]). 
- BookedBy (string): The user who scheduled the room. 
- Participants (Participant[]): A list of participants in the room. 

### Participant Fields
- AcsParticipantId (string): The user’s ACS ID (e.g., "8:acs:xyz"). 
- Role (string): Role in the meeting (“Presenter”, “Attendee”, etc.). 
- Email (string): Participant’s email address. 
- Name (string): Display name (e.g., “Dr. Brown”). 
 
## Endpoints & Request Details
The backend provides three key endpoints. The agent must construct requests to these endpoints in the correct format.

### POST /BookRoom
Purpose: Create a new ACS virtual room
Request Body (JSON): 
{
  "startTime": "2025-05-10T15:00:00Z",
  "endTime": "2025-05-10T16:00:00Z",
  "participants": [
    {
      "AcsParticipantId": "8:acs:12345abcd",
      "Role": "Presenter",
      "Email": "[email protected]",
      "Name": "Dr. Brown"
    }
  ],
  "topic": "Telehealth Check-up",
  "tags": ["telehealth", "private"],
  "bookedBy": "User123"
}


Response Example (JSON): 
{
  "id": "some-cosmos-id",
  "roomId": "8:acs:room-id-value",
  "startTime": "2025-05-10T15:00:00Z",
  "endTime": "2025-05-10T16:00:00Z",
  "participants": [
    {
      "AcsParticipantId": "8:acs:12345abcd",
      "Role": "Presenter",
      "Email": "brown@domain.com",
      "Name": "Dr. Brown"
    }
  ],
  "topic": "Telehealth Check-up",
  "tags": ["telehealth", "private"],
  "bookedBy": "User123"
}

### GET /SearchRooms

Purpose: Query existing rooms by filters. 
Query Parameters (optional): 
participantEmail: e.g., ?participantEmail=[email protected] 
topic: e.g., ?topic=telehealth 
tag: e.g., ?tag=private 
date: e.g., ?date=2025-05-10

Response Example (JSON): 
[
  {
    "id": "some-cosmos-id",
    "roomId": "8:acs:room-id-value",
    "startTime": "2025-05-10T15:00:00Z",
    "endTime": "2025-05-10T16:00:00Z",
    "participants": [
      {
        "AcsParticipantId": "8:acs:12345abcd",
        "Role": "Presenter",
        "Email": "[email protected]",
        "Name": "Dr. Brown"
      }
    ],
    "topic": "Telehealth Check-up",
    "tags": ["telehealth", "private"],
    "bookedBy": "User123"
  }
]





