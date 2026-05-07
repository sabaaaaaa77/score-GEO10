using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCORE; // ყველა ფაილი ამ Namespace-ში უნდა იყოს (ფოლდერების გარეშე)

var builder = WebApplication.CreateBuilder(args);

// --- 1. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// --- 2. Controllers + JSON Handling ---
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 3. Database ---
var connectionString = "Server=db50800.databaseasp.net;Database=db50800;User Id=db50800;Password=7e+G#8ZocN?2;TrustServerCertificate=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    }));

// --- 4. Services ---
builder.Services.AddHttpClient();
builder.Services.AddScoped<SportsDataService>();
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddHostedService<SportsUpdateWorker>();

var app = builder.Build();

// --- 🔥 5. ავტომატური ბაზის შექმნა (რომ ცარიელი არ იყოს) ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Database Migrated Successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

// --- 6. Middleware Pipeline ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCORE API V1");
    c.RoutePrefix = string.Empty;
});

// მნიშვნელოვანია, რომ UseCors იყოს UseRouting-სა და UseEndpoints-ს შორის
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// --- 7. Port Configuration (Render-ისთვის) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
