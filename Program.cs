var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "¡Web externa funcionando correctamente!");
app.Run();
