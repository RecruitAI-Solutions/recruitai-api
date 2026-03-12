# RecruitAI API

ASP.NET Core Web API for CV analysis and AI-based recruitment support

## Yêu cầu hệ thống

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

======================================================================

## Bắt đầu nhanh


# Load biến môi trường cho dev (Windows - PowerShell)
$env:ENVIRONMENT="dev"
$env:DB_PASSWORD="DevPass@8386"
$env:DB_PORT="1434"
$env:API_PORT="5000"
.......

# Hoặc (Windows - Command Prompt)
set ENVIRONMENT=dev
set DB_PASSWORD=DevPass@8386
set DB_PORT=1434
set API_PORT=5000
....

# Hoặc (Linux/Mac)
export ENVIRONMENT=dev
export DB_PASSWORD=DevPass@8386
export DB_PORT=1434
export API_PORT=5000
......

# Chạy Docker Compose
docker-compose up -d --build


# Load biến môi trường từ file
set -a; source docker/dev.env; set +a  # Linux/Mac
# Hoặc copy từ file dev.env paste vào terminal (Windows)

# Build và chạy
docker-compose up -d --build

# Dừng và xóa containers cũ (giữ lại volume data)
docker-compose down

# Dừng, xóa containers và xóa luôn volume (MẤT DATA)
docker-compose down -v

# Xem 50 dòng cuối
docker-compose logs --tail=50 api

# Xem 50 dòng cuối
docker-compose logs --tail=50 sqlserver

======================================================================

# Liệt kê databases
docker exec -it recruitai-sql-dev /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "DevPass@8386" -C \
  -Q "SELECT name FROM sys.databases"

# Chạy câu lệnh SQL bất kỳ
docker exec -it recruitai-sql-dev /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "DevPass@8386" -C -d RecruitDev \
  -Q "SELECT * FROM Tests"

# Tạo database mới (nếu chưa có)
docker exec -it recruitai-sql-dev /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "DevPass@8386" -C \
  -Q "CREATE DATABASE RecruitDev"

# Chạy migration
docker exec recruitai-api-dev dotnet ef database update \
  --project RecruitAI.Infrastructure \
  --startup-project RecruitAI.API
  
  
  
 =====================================================================
 
 #Social login
 
 #Google
 https://localhost:7203/api/auth/login/Google?returnUrl=https://localhost:7203/swagger
 
 #Facebook
 https://localhost:7203/api/auth/login/Facebook?returnUrl=https://localhost:7203/swagger
 
 #GitHub
 https://localhost:7203/api/auth/login/GitHub?returnUrl=https://localhost:7203/swagger