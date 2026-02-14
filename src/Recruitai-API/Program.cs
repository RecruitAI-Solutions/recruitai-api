using RecruitAI.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
	config.ReadFrom.Configuration(context.Configuration)
		  .Enrich.WithProperty("Application", "RecruitAI-API")
		  .Enrich.WithEnvironmentName();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
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