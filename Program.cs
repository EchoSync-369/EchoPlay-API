using EchoPlayAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://192.168.1.111:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<JWTService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new JWTService(
        secret: "your-super-secret-jwt-key-that-should-be-at-least-32-characters",
        issuer: "EchoPlay",
        audience: "EchoPlay-Client"
    );
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.MapControllers();

app.MapGet("/", () => "EchoPlay API is running!");

app.Run();