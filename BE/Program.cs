using BE.Models;
using BE.Patterns.Factory;
using BE.Patterns.Observer;
using BE.Patterns.Singleton;
using BE.Patterns.TemplateMethod;
using BE.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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

// FACTORY PATTERN - Story Factory
builder.Services.AddScoped<IStoryFactory, StoryFactory>();

// SINGLETON PATTERN - Reading Progress Manager
builder.Services.AddScoped<IReadingProgressManager, ReadingProgressManager>();

// OBSERVER PATTERN - Notification Service
builder.Services.AddScoped<NotificationService>();

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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
