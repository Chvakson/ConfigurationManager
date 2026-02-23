using ConfigurationManager.Core.Infrastructure.Hubs;
using ConfigurationManager.Core.Infrastructure.Notification;
using ConfigurationManager.Core.Models.Dto;
using ConfigurationManager.Core.Services;
using ConfigurationManager.Db;
using ConfigurationManager.Db.Repositories;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters();
    var basePath = AppContext.BaseDirectory;
    var xmlPath = Path.Combine(basePath, "ConfigurationManager.xml");
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters
            .Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateConfigurationRequestExample>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHub<ConfigurationHub>("/configurationHub");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var tables = dbContext.Database.SqlQuery<string>(
    $"SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");

    Console.WriteLine("Созданные таблицы:");
    foreach (var table in tables)
    {
        Console.WriteLine($"- {table}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
