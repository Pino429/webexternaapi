using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebClienteCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

var jsonCredential = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_B64");
if (jsonCredential != null && FirebaseApp.DefaultInstance == null)
{
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonCredential));
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromStream(stream)
    });
}

app.MapGet("/api/notificaciones", async () =>
{
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", "MiSuperTokenSecreto123");

    var response = await httpClient.GetAsync("https://gonpin.com/api/notificaciones");
    if (!response.IsSuccessStatusCode)
        return Results.Problem("No se pudo obtener la información de la API externa.");

    var json = await response.Content.ReadAsStringAsync();
    var notificaciones = JsonSerializer.Deserialize<List<Notificacion>>(json);

    if (notificaciones == null || notificaciones.Count == 0)
        return Results.Ok("No hay notificaciones para procesar.");

    var salida = new StringBuilder();
    foreach (var item in notificaciones)
    {
        var v_titulo = item.titulo;
        var v_descripcion = item.descripcion;
        var v_token = item.token;

        salida.AppendLine($"🔔 Título: {v_titulo}");
        salida.AppendLine($"📝 Descripción: {v_descripcion}");
        salida.AppendLine($"📱 Token: {v_token}");
        salida.AppendLine("------------------------------------------------");

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
});

app.Run();