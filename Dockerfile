# === STAGE 1: Development với hot reload ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src

# Copy file project
COPY src/RecruitAI.API/*.csproj RecruitAI.API/
COPY src/RecruitAI.Application/*.csproj RecruitAI.Application/
COPY src/RecruitAI.Domain/*.csproj RecruitAI.Domain/
COPY src/RecruitAI.Infrastructure/*.csproj RecruitAI.Infrastructure/

# Restore dependencies
RUN dotnet restore RecruitAI.API/RecruitAI.API.csproj

# Copy toàn bộ source
COPY src/ .

# Câu lệnh cho development (hot reload)
ENTRYPOINT ["dotnet", "watch", "run", "--project", "RecruitAI.API/RecruitAI.API.csproj", "--urls", "http://0.0.0.0:8080"]

# === STAGE 2: Build cho production ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file project
COPY src/RecruitAI.API/*.csproj RecruitAI.API/
COPY src/RecruitAI.Application/*.csproj RecruitAI.Application/
COPY src/RecruitAI.Domain/*.csproj RecruitAI.Domain/
COPY src/RecruitAI.Infrastructure/*.csproj RecruitAI.Infrastructure/

RUN dotnet restore RecruitAI.API/RecruitAI.API.csproj

# Copy source và publish
COPY src/ .
RUN dotnet publish RecruitAI.API/RecruitAI.API.csproj -c Release -o /app/publish

# === STAGE 3: Runtime cho production ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RecruitAI.API.dll"]