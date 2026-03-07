using Microsoft.EntityFrameworkCore;
using RecruitAI.Application;
using RecruitAI.Infrastructure;
using RecruitAI.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews()
        .AddRazorRuntimeCompilation();
}

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.WithProperty("Application", "RecruitAI-API")
          .Enrich.WithEnvironmentName();
});

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Cho phép mọi nguồn
              .AllowAnyMethod()      // Cho phép mọi method (GET, POST, PUT, DELETE)
              .AllowAnyHeader();     // Cho phép mọi header
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// THÊM LOGGING CHO PHẦN MIGRATION
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecruitDevContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("=== KIỂM TRA DATABASE ===");

        // ĐẢM BẢO DATABASE ĐƯỢC TẠO TRƯỚC KHI MIGRATE
        logger.LogInformation("Đảm bảo database tồn tại...");
        await db.Database.EnsureCreatedAsync();  // Tạo database nếu chưa có

        logger.LogInformation("Kiểm tra kết nối...");
        var canConnect = await db.Database.CanConnectAsync();
        logger.LogInformation($"Kết nối: {canConnect}");

        if (canConnect)
        {
            logger.LogInformation("Chạy migration...");
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Any())
            {
                logger.LogInformation($"Có {pendingList.Count} migrations cần áp dụng");
                await db.Database.MigrateAsync();
                logger.LogInformation("Migration thành công!");
            }
            else
            {
                logger.LogInformation("Không có migration pending");
            }

            // Kiểm tra bảng
            var tables = await db.Database.SqlQuery<string>($@"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE'").ToListAsync();

            logger.LogInformation($"Tìm thấy {tables.Count} bảng");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi database!");
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Recruit AI API V1");
    });

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();

// Log khi ứng dụng start
try
{
    Log.Information("Starting RecruitAI-API application");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    await Log.CloseAndFlushAsync();
}