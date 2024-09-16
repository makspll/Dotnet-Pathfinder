var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    var route = "program/{controller}/{action}";
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: route
    );
});

app.Run();
