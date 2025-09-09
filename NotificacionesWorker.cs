
using FirebaseAdmin.Messaging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebClienteCore
{
    public class NotificacionesWorker : BackgroundService
    {
        private readonly ILogger<NotificacionesWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificacionesWorker(ILogger<NotificacionesWorker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de notificaciones iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarNotificaciones(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de notificaciones");
                }

                // Espera 5 minutos
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        public class RespuestaApi
        {
            public List<Notificacion> data { get; set; }
        }

        private async Task ProcesarNotificaciones(CancellationToken stoppingToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "MiSuperTokenSecreto123"); // Cambiar si tu token es distinto

            // Obtener datos de la API externa
            var response = await client.GetAsync("https://gonpin.com/api/notificaciones", stoppingToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("⚠️ No se pudo obtener la información de la API externa. Status: {status}", response.StatusCode);
                return;
            }

            var json = await response.Content.ReadAsStringAsync(stoppingToken);
            _logger.LogInformation("📥 JSON recibido (preview): {json}", json.Length > 300 ? json.Substring(0, 300) + "..." : json);

            List<Notificacion>? notificaciones = null;

            try
            {
                // Intentar deserializar como lista directa
                notificaciones = JsonSerializer.Deserialize<List<Notificacion>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if ((notificaciones == null || notificaciones.Count == 0) && !json.TrimStart().StartsWith("["))
                {
                    // Intentar deserializar como objeto con propiedad 'data'
                    var respuesta = JsonSerializer.Deserialize<RespuestaApi>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    notificaciones = respuesta?.data;
                }

                if (notificaciones != null)
                {
                    _logger.LogInformation("✅ Notificaciones deserializadas con {count} elementos", notificaciones.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al deserializar el JSON. Preview: {preview}", json.Length > 300 ? json.Substring(0, 300) + "..." : json);
                return;
            }

            if (notificaciones == null || notificaciones.Count == 0)
            {
                _logger.LogInformation("📭 No hay notificaciones para procesar.");
                return;
            }

            // Revisar que Firebase esté inicializado
            var firebase = FirebaseMessaging.DefaultInstance;
            if (firebase == null)
            {
                _logger.LogError("Firebase no se inicializó. Revisa GOOGLE_CREDENTIALS_B64");
                return;
            }

            // Enviar notificaciones
            foreach (var item in notificaciones)
            {
                try
                {
                    var message = new Message()
                    {
                        Token = item.token,
                        Notification = new Notification
                        {
                            Title = item.titulo,
                            Body = item.descripcion
                        }
                    };

                    string result = await firebase.SendAsync(message, stoppingToken);
                    _logger.LogInformation("✅ Notificación enviada. ID: {result}", result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al enviar notificación a {token}", item.token);
                }
            }
        }
    }
}

