using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecruitAI.API.Middleware;
using RecruitAI.Application;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Validators;
using RecruitAI.Infrastructure;
using RecruitAI.Infrastructure.Data;
using Serilog;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH CƠ BẢN
builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

// 2. LOGGING
builder.Host.UseSerilog((context, config) =>
{
	config.ReadFrom.Configuration(context.Configuration)
		  .Enrich.WithProperty("Application", "RecruitAI-API")
		  .Enrich.WithEnvironmentName();
});

// Log thông tin môi trường
Log.Information(ProgramMessages.Log("Environment"), builder.Environment.EnvironmentName);
Log.Information(ProgramMessages.Log("ConnectionString"),
	builder.Configuration.GetConnectionString("DefaultConnection"));

// 3. THÊM SERVICES 
// 3.1 MVC Controllers
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options =>
{
	options.Filters.AddService<ValidationFilter>();
});


// 3.2 API Explorer & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Recruit AI API",
		Version = "v1",
		Description = "API for RecruitAI application"
	});

	// Cấu hình JWT trong Swagger
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// Đăng ký HttpContextAccessor để lấy IP
builder.Services.AddHttpContextAccessor();

// 3.3 Localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
	var supportedCultures = new[]
	{
		new CultureInfo("vi-VN"),
		new CultureInfo("en-US")
	};
	options.DefaultRequestCulture = new RequestCulture("vi-VN");
	options.SupportedCultures = supportedCultures;
	options.SupportedUICultures = supportedCultures;
});

// 3.4 CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		var originsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
		string[] allowedOrigins;

		if (!string.IsNullOrEmpty(originsEnv))
		{
			allowedOrigins = originsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries);
		}
		else
		{
			allowedOrigins = builder.Configuration
				.GetSection("Cors:AllowedOrigins")
				.Get<string[]>();
		}

		if (allowedOrigins != null && allowedOrigins.Any())
		{
			policy.WithOrigins(allowedOrigins)
				  .AllowAnyMethod()
				  .AllowAnyHeader();
			if (!allowedOrigins.Contains("*"))
			{
				policy.AllowCredentials();
			}
		}
		else
		{
			policy.AllowAnyOrigin()
				  .AllowAnyMethod()
				  .AllowAnyHeader();
		}
	});
});

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// 3.6 Custom Validation Response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.SuppressModelStateInvalidFilter = true;
});

// 3.7 Infrastructure & Application Services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// 3.8 Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = !builder.Environment.IsDevelopment(),
		ValidateAudience = !builder.Environment.IsDevelopment(),
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Environment.IsDevelopment()
			? null
			: builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Environment.IsDevelopment()
			? null
			: builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};

	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
			logger.LogError(context.Exception, ProgramMessages.Log("AuthFailed"));
			return Task.CompletedTask;
		},
		OnChallenge = context =>
		{
			var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
			logger.LogWarning(ProgramMessages.Log("AuthChallenge"), context.Error, context.ErrorDescription);
			return Task.CompletedTask;
		}
	};
});
builder.Services.AddAuthorization();

// 4. RAZOR RUNTIME COMPILATION (CHỦ YẾU CHO DEVELOPMENT)
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddControllersWithViews()
		.AddRazorRuntimeCompilation();
}

var app = builder.Build();

// 5. MIDDLEWARE PIPELINE
// 5.1 Development-specific middleware
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recruit AI API V1");
	});
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/error");
	app.UseHsts();
}

// 5.2 Các middleware cơ bản
app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLocalization();
app.UseCors(); // Dùng policy mặc định

// 5.3 Authentication & Authorization 
app.UseAuthentication();
app.UseAuthorization();

// 5.4 Custom Middleware

// 5.5 Controllers
app.MapControllers();

// 6. DATABASE MIGRATION
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<RecruitDevContext>();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

	try
	{
		logger.LogInformation(ProgramMessages.Log("DatabaseCheck"));
		logger.LogInformation(ProgramMessages.Log("DatabaseEnsuring"));
		await db.Database.EnsureCreatedAsync();

		logger.LogInformation(ProgramMessages.Log("DatabaseConnected"));
		var canConnect = await db.Database.CanConnectAsync();
		logger.LogInformation(ProgramMessages.Log("DatabaseConnected"), canConnect);

		if (canConnect)
		{
			logger.LogInformation(ProgramMessages.Log("MigrationStarted"));
			var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
			var pendingList = pendingMigrations.ToList();

			if (pendingList.Any())
			{
				logger.LogInformation(ProgramMessages.Log("MigrationPending"), pendingList.Count);
				await db.Database.MigrateAsync();
				logger.LogInformation(ProgramMessages.Log("MigrationSuccess"));
			}
			else
			{
				logger.LogInformation(ProgramMessages.Log("NoPendingMigration"));
			}

			var tables = await db.Database.SqlQuery<string>($@"
				SELECT TABLE_NAME 
				FROM INFORMATION_SCHEMA.TABLES 
				WHERE TABLE_TYPE = 'BASE TABLE'").ToListAsync();

			logger.LogInformation(ProgramMessages.Log("TablesFound"), tables.Count);
		}
	}
	catch (Exception ex)
	{
		logger.LogError(ex, ProgramMessages.Log("DbError"));
	}
}

// 7. START APPLICATION
try
{
	Log.Information(ProgramMessages.Log("AppStarting"));
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, ProgramMessages.Log("AppFailed"));
}
finally
{
	await Log.CloseAndFlushAsync();
}