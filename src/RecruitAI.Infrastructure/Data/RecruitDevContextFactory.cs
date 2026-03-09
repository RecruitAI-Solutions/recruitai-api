// RecruitDevContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RecruitAI.Infrastructure.Data;

public class RecruitDevContextFactory : IDesignTimeDbContextFactory<RecruitDevContext>
{
    public RecruitDevContext CreateDbContext(string[] args)
    {
        Console.WriteLine("Using RecruitDevContextFactory!");

        var optionsBuilder = new DbContextOptionsBuilder<RecruitDevContext>();

        // Đọc connection string từ nhiều nguồn
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../RecruitAI.API");

        // Thử đọc từ API project trước
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Nếu không có, thử đọc từ environment variable
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        }

        // Nếu vẫn không có, dùng connection string mặc định để test
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=localhost;Database=RecruitDev;Trusted_Connection=True;TrustServerCertificate=True";
            Console.WriteLine("Using default connection string for testing!");
        }

        Console.WriteLine($"Using connection string: {connectionString}");

        optionsBuilder.UseSqlServer(connectionString);

        return new RecruitDevContext(optionsBuilder.Options);
    }
}