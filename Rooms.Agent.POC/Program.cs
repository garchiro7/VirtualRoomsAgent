using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using System.IO;
using System.Runtime.CompilerServices;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Rooms Agent!");
//

var connectionString = "eastus2.api.azureml.ms;7436226f-5ec0-4fc0-aadf-e889822bf8ab;acs-rooms;jorgarchiro-1161";
AgentsClient client = new AgentsClient(connectionString, new DefaultAzureCredential());

string agentInstructions = "You are a Virtual Room Booking Agent specializing in Azure Communication Services (ACS).\r\nYour responsibilities:\r\n\r\n1. Understand the user's intent: \r\n   - If they want to schedule a new appointment (virtual room), \r\n   - Add participants to an existing appointment, or \r\n   - Search existing appointments.\r\n\r\n2. Collect necessary details:\r\n   - Dates/Times (startTime, endTime)\r\n   - Participant info (names, roles, ACS IDs, emails)\r\n   - Appointment topic\r\n   - Tags or any metadata\r\n   - The user who is booking (“bookedBy”) if relevant";
string openAPIFile = "rooms-agent-api.swagger.json";

var dirName = Path.GetDirectoryName(GetFile()) ?? "";
string dirFile =  Path.Combine(dirName, openAPIFile);

OpenApiAuthDetails openApiAuthDetails = new OpenApiAnonymousAuthDetails();

OpenApiToolDefinition openapiTool = new(
    name: "rooms api",
    description: "Book virtual rooms appointments and search virtual rooms",
    spec: BinaryData.FromBytes(File.ReadAllBytes(dirFile)),
    auth: openApiAuthDetails
);

// Step 1: Create an agent
Response<Agent> agentResponse = await client.CreateAgentAsync(
    model: "gpt-35-turbo",
    name: "Virtual Rooms Agent ",
    instructions: agentInstructions,
    tools: new List<ToolDefinition> { openapiTool });



Agent agent = agentResponse.Value;


static string GetFile([CallerFilePath] string pth = "")
{
    var dirName = Path.GetDirectoryName(pth) ?? "";
    return Path.Combine(dirName, "weather_openapi.json");
}