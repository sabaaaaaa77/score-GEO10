using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCORE;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// --- 2. Controllers ---
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 3. Database (Neon PostgreSQL) ---
var connectionString = "postgresql://neondb_owner:npg_G8gqCofWT3VR@ep-muddy-band-aq67cygr-pooler.c-8.us-east-1.aws.neon.tech/neondb?sslmode=require";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // შეცვლილია Npgsql-ზე

// --- 4. Services ---
builder.Services.AddHttpClient();
builder.Services.AddScoped<SportsDataService>();
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddHostedService<SportsUpdateWorker>();

var app = builder.Build();

// --- 5. ავტომატური ბაზის შექმნა ---
using (var scope = app.Services.CreateScope())
{
    try {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // PostgreSQL-ისთვის ეს ყველაზე საიმედოა პირველ ჯერზე
        db.Database.EnsureCreated(); 
        Console.WriteLine("Neon Database Connected!");
    } catch (Exception ex) {
        Console.WriteLine($"DB Error: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCORE API V1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
