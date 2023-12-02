using System.Diagnostics;

const string EOF = "\r\r";

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));

var app = builder.Build();

app.UseCors();

app.MapGet("sse", async context =>
{
    try
    {
        if(!context.Request.Query.TryGetValue("msg", out var clientMessage))
        {
            Debug.WriteLine($"[SSE][CLIENT][MESSAGE] {clientMessage}");
        }

        Debug.WriteLine("[SSE][CONNECTION] Client connectiong...");

        var response = context.Response;
        response.Headers.Append("Content-Type", "text/event-stream");

        await response.WriteAsync($"event: myevent{EOF}");
        await response.WriteAsync($"data: My event data...{EOF}");
        await response.Body.FlushAsync();

        Debug.WriteLine("[SSE][CONNECTION] Client connected");

        try
        {
            for(var i = 0; i < 10; i++)
            {
                var message = $"Hello server sent events {i}. The message from client is '{clientMessage}'";

                await response.WriteAsync($"data: {message}{EOF}");
                await response.Body.FlushAsync();

                Debug.WriteLine($"[SSE][MESSAGE] {message}");

                await Task.Delay(200);
            }
        }
        catch(Exception exception)
        {
            Debug.WriteLine($"[SSE][ERROR][GENERATE MESSAGE] {exception.Message}");
        }
    }
    catch(Exception exception)
    {
        Debug.WriteLine($"[SSE][CONNECTION][ERROR] {exception.Message}");
    }

    Debug.WriteLine("[SSE][CONNECTION] Finished");
});

await app.RunAsync();
