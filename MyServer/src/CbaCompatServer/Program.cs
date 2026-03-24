using CbaCompatServer;
using CbaCompatServer.Net;
using CbaCompatServer.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.SectionName));
builder.Services.AddSingleton<InMemoryGameState>();
builder.Services.AddHostedService<TcpGameServer>();

var app = builder.Build();
var options = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServerOptions>>().Value;

app.MapGet("/", () => Results.Redirect("/health"));

app.MapGet("/health", () => Results.Ok(new
{
    service = "cba-compat-server",
    httpPort = options.HttpPort,
    tcpPort = options.TcpPort,
    mode = "bootstrap+tcp-skeleton"
}));

app.MapGet("/cba/server_selector.php", () =>
{
    var host = string.IsNullOrWhiteSpace(options.PublicHost) ? "127.0.0.1" : options.PublicHost;
    return Results.Ok(new
    {
        id = 1,
        status = 1,
        officialName = "LocalCompat",
        aliasName = "本地服",
        host = $"{host}:{options.TcpPort}",
        @new = true
    });
});

app.MapGet("/cba/server_list.php", () =>
{
    var host = string.IsNullOrWhiteSpace(options.PublicHost) ? "127.0.0.1" : options.PublicHost;
    return Results.Ok(new[]
    {
        new
        {
            id = 1,
            status = 1,
            officialName = "LocalCompat",
            aliasName = "本地服",
            host = $"{host}:{options.TcpPort}",
            @new = true
        }
    });
});

app.MapGet("/hotfix/CbaServerNotice.json", () => Results.Ok(new
{
    noticeJsonVersion = 1,
    noticeList = Array.Empty<object>()
}));

app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{options.HttpPort}");
app.Run();
