using Microsoft.EntityFrameworkCore;
using SCORE.Data;
using SCORE.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS-ის კონფიგურაცია (რომ ფრონტენდიდან პრობლემა არ გქონდეს)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 2. კონტროლერების და NewtonsoftJson-ის დამატება
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// 3. Swagger/OpenAPI კონფიგურაცია
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. მონაცემთა ბაზის (SQL Server) კავშირი
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 5. HttpClient-ის ზოგადი რეგისტრაცია
builder.Services.AddHttpClient();

// --- 6. სერვისების რეგისტრაცია (Dependency Injection) ---

// სპორტის მონაცემების ზოგადი სერვისი
builder.Services.AddHttpClient<SportsDataService>();
builder.Services.AddScoped<SportsDataService>();

// !!! აი აქ არის მთავარი გასწორება !!!
// ვიყენებთ AddHttpClient-ს, რომელიც ავტომატურად არეგისტრირებს ინტერფეისსაც და კლასსაც.
// დამატებითი AddScoped ამ სერვისისთვის აღარ გჭირდება.
builder.Services.AddHttpClient<IStandingsService, StandingsService>();

// Background ვორკერი მონაცემების ავტომატური განახლებისთვის
builder.Services.AddHostedService<SportsUpdateWorker>();

var app = builder.Build();

// --- 7. HTTP Pipeline-ის კონფიგურაცია ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS აუცილებლად Authorization-მდე უნდა იყოს
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();