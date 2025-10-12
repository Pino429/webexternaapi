using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using System.Net.Http.Headers;


//using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebClienteCore;






var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");


var b64 = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_B64");
if (b64 != null && FirebaseApp.DefaultInstance == null)
{
    var jsonBytes = Convert.FromBase64String(b64); // ✅ decodifica correctamente
    var stream = new MemoryStream(jsonBytes);
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromStream(stream)
    });
}



app.MapGet("/", () => "🚀 Web externa actualizada perfectamente - 18/07/2025 21:00");

// ✅ Endpoint liviano para monitoreo
//p.MapGet("/ping", () => Results.Ok("✅ App activa"));
app.MapMethods("/ping", new[] { "GET", "HEAD" }, () => Results.Ok("✅ App activa"));

app.MapGet("/api/notificaciones", async () =>
{
using var httpClient = new HttpClient();

    //  httpClient.DefaultRequestHeaders.Authorization =
    // new AuthenticationHeaderValue("Bearer", "MiSuperTokenSecreto123");
    var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones/auto?key=ClaveUptime123");
    //var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones");
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
        try
        {
            var message = new Message()
            {
                Token = v_token,
                Notification = new Notification
                {
                    Title = v_titulo,
                    Body = v_descripcion
                }
            };

            string result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            salida.AppendLine($"✅ Notificación enviada. ID: {result}");
        }
        catch (Exception ex)
        {
            salida.AppendLine($"❌ Error al enviar notificación: {ex.Message}");
        }
    }

  
     return Results.Ok(salida.ToString());
  //  return Results.Ok("Notificaciones procesadas correctamente.");
});
app.Run();







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
