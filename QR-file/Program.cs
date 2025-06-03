using Microsoft.EntityFrameworkCore;
using QRFileManager.Data;
using QRFileManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіси
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Налаштовуємо Entity Framework з SQLite
builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=files.db"));

// Реєструємо сервіси
builder.Services.AddScoped<FileService>();

// Додаємо підтримку CORS для розробки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Створюємо базу даних при старті
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileDbContext>();
    context.Database.EnsureCreated();
}

// Налаштовуємо middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");
}

// Статичні файли (uploads, qr-codes, index.html)
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

// Головна сторінка
app.MapGet("/", () => Results.Redirect("/index.html"));

// Запускаємо додаток
Console.WriteLine("🚀 QR Файлообмінник запущено!");
Console.WriteLine($"📂 Відкрийте браузер: http://localhost:{builder.Configuration["urls"]?.Split(';')[0]?.Split(':')[2] ?? "5000"}");

app.Run();