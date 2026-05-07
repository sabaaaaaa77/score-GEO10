using Microsoft.EntityFrameworkCore;
using SCORE.Data;
using SCORE.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// --- 2. Controllers + JSON ---
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// --- 3. Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 4. Database ---
var connectionString = "Server=db50800.databaseasp.net;Database=db50800;User Id=db50800;Password=7e+G#8ZocN?2;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// --- 5. Services ---
builder.Services.AddHttpClient();
builder.Services.AddScoped<SportsDataService>();
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddHostedService<SportsUpdateWorker>();

var app = builder.Build();
// app.Run(); - ის ნაცვლად ჩაწერე ეს:

// --- 6. Middleware Pipeline ---

// 🔥 1. HTTPS redirect (აუცილებელია ჰოსტინგზე)
app.UseHttpsRedirection();

// 🔥 2. CORS (ძალიან მნიშვნელოვანია)
app.UseCors("AllowAll");

// 🔥 3. Swagger (optional, მაგრამ კარგი)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCORE API V1");
    c.RoutePrefix = string.Empty;
});

// 🔥 4. Routing + Auth
app.UseRouting();
app.UseAuthorization();

// 🔥 5. Controllers
app.MapControllers();

// app.Run(); - ის ნაცვლად ჩაწერე ეს:
app.Run();