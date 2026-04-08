using BE.Models;
using BE.Patterns.Factory;
using BE.Patterns.Observer;
using BE.Patterns.Singleton;
using BE.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add DbContext with SQL Server
builder.Services.AddDbContext<StoryReaderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// BUSINESS SERVICES (Logic moved from Controllers)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IReadingModeService, ReadingModeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// FACTORY PATTERN - Story Factory
builder.Services.AddScoped<IStoryFactory, StoryFactory>();

// SINGLETON PATTERN - Reading Progress Manager
builder.Services.AddSingleton<IReadingProgressManager>(ReadingProgressManager.Instance);

// OBSERVER PATTERN - Story Subject
builder.Services.AddScoped<IStoryObserver, StoryObserver>();

// TEMPLATE METHOD PATTERN - Report Factory
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var host = new Uri(origin).Host;
            return host == "localhost" || host == "127.0.0.1";
        })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

// Initialize Singleton Pattern with ServiceProvider
ReadingProgressManager.Initialize(app.Services);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoryReaderDbContext>();
    context.Database.Migrate();
}

app.Run();
