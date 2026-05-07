using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCORE; 

var builder = WebApplication.CreateBuilder(args);

// --- 1. CORS (აბსოლუტურად ყველაფრის უფლება) ---
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

// --- 3. Database (დავამატე პორტი 1433 და კავშირის პარამეტრები) ---
var connectionString = "Server=db50800.databaseasp.net,1433;Database=db50800;User Id=db50800;Password=7e+G#8ZocN?2;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=30;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// --- 4. Services ---
builder.Services.AddHttpClient();
builder.Services.AddScoped<SportsDataService>();
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddHostedService<SportsUpdateWorker>();

var app = builder.Build();

// --- 5. ავტომატური ბაზის შემოწმება/შექმნა ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // EnsureCreated უფრო საიმედოა უფასო ჰოსტინგებზე, ვიდრე Migrate
        db.Database.EnsureCreated(); 
        Console.WriteLine("Database Connection Check: OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Error: {ex.Message}");
    }
}

// --- 6. Middleware Pipeline ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCORE API V1");
    c.RoutePrefix = string.Empty;
});

// CORS უნდა იყოს Routing-მდე Render-ზე რომ არ აურიოს
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// --- 7. Port Configuration ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
