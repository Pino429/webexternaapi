
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", async (HttpContext context) =>
{
    using var http = new HttpClient();
    var json = await http.GetStringAsync("https://tudominio/api/notificacion/obtener");
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(json);
});
app.Run();
