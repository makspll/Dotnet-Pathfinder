var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "conventionalprefix/{controller}/{action}"
);

app.MapControllerRoute(
    name: "hello",
    pattern: "conventionalprefix2/{controller}",
    defaults: new { action = "DefaultAction" }
);

app.MapControllerRoute(
    name: "hello2",
    pattern: "conventionalwithnoactionspecs",
    defaults: new { action = "DefaultAction", controller = "DefaultConventional" }
);

app.Run();
