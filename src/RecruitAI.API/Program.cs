using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecruitAI.API.Middleware;
using RecruitAI.Application;
using RecruitAI.Application.DTOs.Responses;
using RecruitAI.Application.Helpers;
using RecruitAI.Application.Interfaces.Services;
using RecruitAI.Application.Validators.Auths;
using RecruitAI.Domain.Enums;
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
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddJsonFile("appsettings.Docker.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

// 2. LOGGING
builder.Host.UseSerilog((context, config) =>
{
	config.ReadFrom.Configuration(context.Configuration)
		  .Enrich.WithProperty("Application", "RecruitAI-API")
		  .Enrich.WithEnvironmentName()
		  .WriteTo.Console()
		  .WriteTo.File(
			  path: "Logs/log-.txt",
			  rollingInterval: RollingInterval.Day,
			  retainedFileCountLimit: 7,
			  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

	Console.WriteLine($"Serilog configured for environment: {builder.Environment.EnvironmentName}");
});

Log.Information("=== APPLICATION STARTING ===");
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Connection String: {ConnectionString}",
	builder.Configuration.GetConnectionString("DefaultConnection")?.Replace(
		builder.Configuration.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault() ?? "", "***hidden***"));

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

// Lấy key theo logic giống JwtService
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key");
if (string.IsNullOrEmpty(jwtKey))
	jwtKey = builder.Configuration["Jwt:Key"];

// Auto-generate cho development nếu cần
if (string.IsNullOrEmpty(jwtKey) && builder.Environment.IsDevelopment())
{
	jwtKey = GenerateRandomKey(32);
	builder.Configuration["Jwt:Key"] = jwtKey; // Lưu lại để dùng
	Console.WriteLine($"Auto-generated JWT Key: {jwtKey}");
}

if (string.IsNullOrEmpty(jwtKey))
	throw new InvalidOperationException("JWT Key is not configured");

// Thêm method GenerateRandomKey (giống trong JwtService)
string GenerateRandomKey(int length)
{
	const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	var random = new Random();
	return new string(Enumerable.Repeat(chars, length)
		.Select(s => s[random.Next(s.Length)]).ToArray());
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddCookie(IdentityConstants.ExternalScheme)
 .AddJwtBearer(options =>
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
			 Encoding.UTF8.GetBytes(jwtKey)) 
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

			 context.HandleResponse();

			 var messageService = context.HttpContext.RequestServices.GetService<IMessageService>();
			 var errorMessage = messageService?.Business("Unauthorized") ?? "Bạn không có quyền truy cập";

			 var response = new ErrorResponseDto
			 {
				 StatusCode = StatusCodes.Status401Unauthorized,
				 ErrorCode = ErrorCode.Unauthorized,
				 Message = errorMessage,
				 TraceId = context.HttpContext.TraceIdentifier,
				 Timestamp = DateTime.UtcNow
			 };

			 context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
			 context.HttpContext.Response.ContentType = "application/json";

			 var json = System.Text.Json.JsonSerializer.Serialize(response);
			 return context.HttpContext.Response.WriteAsync(json);
		 },
		 OnForbidden = context =>
		 {
			 var messageService = context.HttpContext.RequestServices.GetService<IMessageService>();
			 var errorMessage = messageService?.Business("Forbidden") ?? "Bạn không có quyền thực hiện hành động này";

			 var response = new ErrorResponseDto
			 {
				 StatusCode = StatusCodes.Status403Forbidden,
				 ErrorCode = ErrorCode.Forbidden,
				 Message = errorMessage,
				 TraceId = context.HttpContext.TraceIdentifier,
				 Timestamp = DateTime.UtcNow
			 };

			 context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
			 context.HttpContext.Response.ContentType = "application/json";

			 var json = System.Text.Json.JsonSerializer.Serialize(response);
			 return context.HttpContext.Response.WriteAsync(json);
		 }
	 };
 })
.AddGoogle(options =>
{
	options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

	// Callback path cố định, nhưng domain sẽ thay đổi theo môi trường
	options.CallbackPath = "/signin-google";
	options.SaveTokens = true;
	options.Scope.Add("profile");
	options.Scope.Add("email");
})
.AddFacebook(options =>
{
	options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
	options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
	options.CallbackPath = "/signin-facebook";
	options.SaveTokens = true;
	options.Scope.Add("email");
	options.Scope.Add("public_profile");
})
.AddGitHub(options =>
{
	options.ClientId = builder.Configuration["Authentication:Github:ClientId"];
	options.ClientSecret = builder.Configuration["Authentication:Github:ClientSecret"];
	options.CallbackPath = "/signin-github";
	options.SaveTokens = true;
	options.Scope.Add("user:email");
});
builder.Services.AddAuthorization(options =>
{
	// ===== AUTH PERMISSIONS =====
	options.AddPolicy("ViewProfile", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P004") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("ChangePassword", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P005") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("ViewPermissions", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P013") || // ✅ P013 = Manage Roles
			context.User.IsInRole("ADMIN")
		));

	// ===== CV PERMISSIONS =====
	options.AddPolicy("UploadCV", policy =>
		policy.RequireAssertion(context =>
		{
			var permissions = context.User.Claims
				.Where(c => c.Type == "permission")
				.Select(c => c.Value)
				.ToList();

			return permissions.Contains("P003") ||
				   permissions.Contains("P101") ||
				   context.User.IsInRole("ADMIN");
		}));

	options.AddPolicy("ViewOwnCVs", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P102") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("DownloadOwnCV", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P103") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("DeleteOwnCV", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P104") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("ExtractCVText", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P105") ||
			context.User.IsInRole("ADMIN")
		));

	// ===== JOB PERMISSIONS =====
	options.AddPolicy("CreateJob", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P006") ||
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P201") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("EditOwnJob", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P007") ||
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P202") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("DeleteOwnJob", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P008") ||
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P203") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("ViewAllJobs", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P001") ||
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P204") ||
			context.User.IsInRole("ADMIN")
		));

	// ===== APPLICATION PERMISSIONS (API-06) =====

	/// <summary>
	/// Ứng tuyển công việc (P002)
	/// </summary>
	options.AddPolicy("ApplyJob", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P002") ||
			context.User.IsInRole("ADMIN")
		));

	/// <summary>
	/// Xem đơn ứng tuyển (P009)
	/// </summary>
	options.AddPolicy("ViewApplications", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P009") ||
			context.User.IsInRole("ADMIN")
		));

	/// <summary>
	/// Cập nhật trạng thái đơn ứng tuyển (P011)
	/// </summary>
	options.AddPolicy("UpdateApplicationStatus", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P011") ||
			context.User.IsInRole("ADMIN")
		));

	// ===== AI PERMISSIONS =====
	options.AddPolicy("ViewCVAnalysis", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P301") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("AnalyzeCV", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P302") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("MatchCVJob", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P303") ||
			context.User.IsInRole("ADMIN")
		));

	options.AddPolicy("ViewMatchResults", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P304") ||
			context.User.IsInRole("ADMIN")
		));

	// ===== ADMIN PERMISSIONS =====
	options.AddPolicy("AdminOnly", policy =>
		policy.RequireRole("ADMIN"));

	/// <summary>
	/// Quản lý người dùng (P012)
	/// </summary>
	options.AddPolicy("ManageUsers", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P012") ||
			context.User.IsInRole("ADMIN")
		));

	/// <summary>
	/// Quản lý vai trò (P013)
	/// </summary>
	options.AddPolicy("ManageRoles", policy =>
		policy.RequireAssertion(context =>
			context.User.HasClaim(c => c.Type == "permission" && c.Value == "P013") ||
			context.User.IsInRole("ADMIN")
		));
});

// 4. RAZOR RUNTIME COMPILATION
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
app.UseCors();

// 5.3 Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5.4 Controllers
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