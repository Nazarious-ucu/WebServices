using Grpc.Net.Client;
using LoggingService; // Простір імен з .proto (csharp_namespace)
using System.Net.Http;
var builder = WebApplication.CreateBuilder(args);

var loggingServiceUrl = builder.Configuration["Logging:ServiceUrl"] ?? "http://localhost:5011";
var messageServiceUrl = builder.Configuration["MessageService:Url"] ?? "http://localhost:5012";

builder.Services.AddHttpClient("MessageClient", c =>
{
    c.BaseAddress = new Uri(messageServiceUrl);
});

builder.Services.AddSingleton(sp =>
{
    var channel = GrpcChannel.ForAddress(loggingServiceUrl);
    return new LoggingService.LoggingService.LoggingServiceClient(channel);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/msg", async (string msg, LoggingService.LoggingService.LoggingServiceClient client) =>
{
    var request = new SaveRequest
    {
        Id = Guid.NewGuid().ToString(),
        Msg = msg,
    };
    var reply = await client.SaveMessageAsync(request);
    return Results.Ok($"LoggingService replied: {reply.Success}");
});


app.MapGet("/api",
    async (LoggingService.LoggingService.LoggingServiceClient client, IHttpClientFactory httpClientFactory) =>
    {
        var request = new Empty { };
        var reply = await client.GetAllMessagesAsync(request);

        var logs = string.Join("; ", reply.Messages);
        
        var messageClient = httpClientFactory.CreateClient("MessageClient");
        var msgServiceReply = await messageClient.GetStringAsync("/api/msg");
        return Results.Ok($"LoggingService replied: {logs}, message: {msgServiceReply}");
    });

app.Run();