# === STAGE 1: Development với hot reload ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src

# Cài đặt công cụ hot reload
RUN dotnet tool install -g Microsoft.dotnet-watch

# Copy file project
COPY src/RecruitAI.API/*.csproj RecruitAI.API/
COPY src/RecruitAI.Application/*.csproj RecruitAI.Application/
COPY src/RecruitAI.Domain/*.csproj RecruitAI.Domain/
COPY src/RecruitAI.Infrastructure/*.csproj RecruitAI.Infrastructure/

# Restore dependencies
RUN dotnet restore RecruitAI.API/RecruitAI.API.csproj

# Copy toàn bộ source
COPY src/ .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Environment cho development
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

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

# Tạo user non-root (cho Alpine)
RUN addgroup -S appuser && adduser -S appuser -G appuser

# Expose port
EXPOSE 8080
EXPOSE 8081

# Copy publish từ build stage
COPY --from=build /app/publish .

# Tạo thư mục cho uploads
RUN mkdir -p /app/uploads/cvs /app/uploads/avatars && chown -R appuser:appuser /app

# Chuyển sang user non-root
USER appuser

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "RecruitAI.API.dll"]