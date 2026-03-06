# recruitai-api
ASP.NET Core Web API for CV analysis and AI-based recruitment support


# Các lệnh docker

======================== Môi trường Dev ========================
## Lệnh build
docker compose -f docker/docker-compose.dev.yml up -d --build

### Dừng và xóa containers, networks (giữ lại volumes nếu muốn giữ data)
docker compose -f docker/docker-compose.dev.yml down

### Nếu muốn xóa luôn volumes (xóa hết data SQL)
docker compose -f docker/docker-compose.dev.yml down -v

### Kiểm tra containers đã xóa chưa
docker ps -a | findstr recruitai

### Xóa image cũ để build lại hoàn toàn
docker rmi recruitai-api.dev

### Hoặc xóa tất cả images không dùng
docker image prune -f

### Kiểm tra SQL Server log
docker logs recruitai-sql.dev --tail 20

### Kiểm tra API log
docker logs recruitai-api.dev

### Lệnh query docker sql 
docker exec -it recruitai-sql.dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "DevPass@8386" -C -d RecruitDev -Q "...sql command..."