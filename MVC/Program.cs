using MVC;
using MVC.Filters;
using Npgsql;
using RabbitMQ.Client;
using Repositories.Implementations;
using MVC.Service;

using Repositories.Interfaces;
using Repository;
using Repository.Implementations;
using Repository.Interfaces;
using StackExchange.Redis;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<NpgsqlConnection>(sp =>        
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
        throw new Exception("Database connection string is missing.");

    return new NpgsqlConnection(connectionString);
});


builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var cloudId = configuration["Elasticsearch:CloudId"];
    var apiKey = configuration["Elasticsearch:ApiKey"];
    var indexName = configuration["Elasticsearch:IndexName"] ?? "queries";

    if (string.IsNullOrWhiteSpace(cloudId))
        throw new Exception("Elasticsearch CloudId is missing.");

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Elasticsearch ApiKey is missing.");

    var settings = new ElasticsearchClientSettings(cloudId, new ApiKey(apiKey))
        .DefaultIndex(indexName);

    return new ElasticsearchClient(settings);
});

builder.Services.AddScoped<IElasticSearchService, ElasticSearchService>();

builder.Services.AddScoped<ElasticService>();

// RabbitMQ Connection (added from second program)
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var factory = new ConnectionFactory()
    {
        HostName = config["RabbitMQ:Host"],
        Port = int.Parse(config["RabbitMQ:Port"]),
        UserName = config["RabbitMQ:User"],
        Password = config["RabbitMQ:Password"],
        VirtualHost = config["RabbitMQ:VirtualHost"],

        DispatchConsumersAsync = true,

        // REQUIRED FOR CLOUDAMQP
        Ssl = new SslOption
        {
            Enabled = true,
            ServerName = config["RabbitMQ:Host"]
        }
    };

    return factory.CreateConnection();
});

// Redis Connection (added from second program)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var options = new ConfigurationOptions
    {
        EndPoints = { { config["Redis:Host"], int.Parse(config["Redis:Port"]) } },
        User = "default",
        Password = config["Redis:Password"],
        Ssl = false,
        AbortOnConnectFail = false,
        ConnectTimeout = 10000,
        SyncTimeout = 5000,
    };

    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddScoped<UserActionFilter>();
builder.Services.AddScoped<EmployeeRoleFilter>();
builder.Services.AddScoped<AdminFilter>();

//Email services 
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<EmailTemplateService>();

// Notification services (added from second program)
builder.Services.AddScoped<INotificationRedisService, NotificationRedisService>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<NotificationConsumer>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeInterface, EmployeeRepository>();
builder.Services.AddScoped<IAdminInterface, AdminRepository>();

builder.Services.AddScoped<IQueryRepository, QueryRepository>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();