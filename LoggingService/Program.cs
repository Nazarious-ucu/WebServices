using Grpc.Core;
using LoggingService;

var builder = WebApplication.CreateBuilder(args);

var facadeServiceConfiguration = builder.Configuration["Facade:Address"] ?? "http://localhost:5010";

builder.Services.AddGrpc();
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenLocalhost(5011, o => o.UseHttps());
// });
var app = builder.Build();

app.MapGrpcService<LoggingServiceImpl>();

app.UseHttpsRedirection();
app.MapGet("/", () => "This is the LoggingService. Use a gRPC client to call it.");

app.Run();



public class LoggingServiceImpl : LoggingService.LoggingService.LoggingServiceBase {
    private static readonly Dictionary<string, string> Messages = new();

    public override Task<LogReply> SaveMessage(SaveRequest message, ServerCallContext context)
    {
        if (Messages.ContainsKey(message.Id))
        {
            Console.WriteLine($"[LoggingService] Duplicate skipped: [{message.Id}] = {message.Msg}");
            return Task.FromResult(new LogReply { Success = "Duplicate skipped" });
        }
        
        Messages.Add(message.Id, message.Msg);
        return Task.FromResult(new LogReply { Success = "Success" });
    }

    public override Task<AllMessageReply> GetAllMessages(Empty request, ServerCallContext context)
    {
        var reply = new AllMessageReply();
        
        reply.Messages.AddRange(Messages.Values);
        return Task.FromResult(reply);
    }
}