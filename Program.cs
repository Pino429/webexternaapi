using System.Net.Http;
using System.Net.Http.Headers;


//using System.Net.Http;
using System.Text;
using System.Text.Json;




var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.MapGet("/api/notificaciones", async () =>
{
using var httpClient = new HttpClient();

  


    httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "MiSuperTokenSecreto123");

var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones");
if (!response.IsSuccessStatusCode)
{
return Results.Problem("No se pudo obtener la información de la API externa.");
}

var json = await response.Content.ReadAsStringAsync();

// Deserializar la lista de notificaciones
var notificaciones = JsonSerializer.Deserialize<List<Notificacion>>(json);

if (notificaciones == null || notificaciones.Count == 0)
{
return Results.Ok("No hay notificaciones para procesar.");
}

    var salida = new StringBuilder();
    foreach (var item in notificaciones)
{
        // ✅ Guardar los campos en variables
        var v_titulo = item.titulo;
        var v_descripcion = item.descripcion;
        var v_token = item.token;


        salida.AppendLine($"🔔 Título: {v_titulo}");
        salida.AppendLine($"📝 Descripción: {v_descripcion}");
        salida.AppendLine($"📱 Token: {v_token}");
        salida.AppendLine("------------------------------------------------");

        // 👉 Aquí podrías llamar a otro método que envíe la notificación FCM, por ejemplo
    }

return Results.Ok("Notificaciones procesadas correctamente.");
});

public class Notificacion
{
    public string titulo { get; set; }
    public string descripcion { get; set; }
    public string token { get; set; }
}





/*app.MapGet("/", () => "¡Web externa funcionando correctamente!");

app.MapGet("/notificaciones", async () =>
{
    using var httpClient = new HttpClient();

    // ⬇️ Agregás el token aquí
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", "MiSuperTokenSecreto123");

    var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones");
    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("No se pudo obtener la información de la API externa.");
    }

    var json = await response.Content.ReadAsStringAsync();

    return Results.Content(json, "application/json");
});

app.Run();*/
