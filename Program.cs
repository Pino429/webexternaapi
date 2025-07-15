using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "¡Web externa funcionando correctamente!");

app.MapGet("/notificaciones", async () =>
{
    using var httpClient = new HttpClient();

    var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones");
    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("No se pudo obtener la información de la API externa.");
    }

    var json = await response.Content.ReadAsStringAsync();

    return Results.Content(json, "application/json");
});

app.Run();
