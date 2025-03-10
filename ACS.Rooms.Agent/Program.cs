using ACS.Rooms.Agent.Data;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    string connectionString = configuration["CosmosCnx"];
    return new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
});

builder.Services.AddScoped<IRoomsDataContext>(provider =>
{
    var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();

    string databaseId = configuration["CosmosDatabaseId"];
    string containerId = configuration["CosmosContainerId"];
    string cosmosCnx = configuration["CosmosCnx"];
    string acsCnx = configuration["ACSCnx"];

    var logger = provider.GetRequiredService<ILogger<RoomsDataContext>>();

    return new RoomsDataContext(acsCnx, cosmosCnx, containerId, databaseId, logger);
});


builder.Build().Run();
