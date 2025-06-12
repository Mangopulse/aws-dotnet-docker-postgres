# DockerX - Start All Services
# This script starts all services for local development

Write-Host "🚀 Starting DockerX Services..." -ForegroundColor Green

# Start test database
Write-Host "📊 Starting PostgreSQL database..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml up -d db-test

# Wait for database to be ready
Write-Host "⏳ Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if database is running
$dbStatus = docker ps --filter "name=db-test" --format "table {{.Names}}\t{{.Status}}"
Write-Host "Database Status: $dbStatus" -ForegroundColor Cyan

Write-Host ""
Write-Host "🌐 Service URLs:" -ForegroundColor Green
Write-Host "  • Front App (Public):     http://localhost:3000" -ForegroundColor Cyan
Write-Host "  • Admin App (Management): http://localhost:3001" -ForegroundColor Cyan
Write-Host "  • Front API (Public):     http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "  • Admin API (Auth):       http://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host "  • Upload Service:         http://localhost:5002/swagger" -ForegroundColor Cyan
Write-Host "  • Media Service:          http://localhost:5003/swagger" -ForegroundColor Cyan
Write-Host "  • PostgreSQL Database:    localhost:5433" -ForegroundColor Cyan

Write-Host ""
Write-Host "🔐 Admin Credentials:" -ForegroundColor Green
Write-Host "  • Username: admin" -ForegroundColor Yellow
Write-Host "  • Password: admin123" -ForegroundColor Yellow

Write-Host ""
Write-Host "📝 To start individual services:" -ForegroundColor Green
Write-Host "  • cd src/FrontApi && dotnet run" -ForegroundColor Gray
Write-Host "  • cd src/AdminApi && dotnet run" -ForegroundColor Gray
Write-Host "  • cd src/Upload && dotnet run" -ForegroundColor Gray
Write-Host "  • cd src/Media && dotnet run" -ForegroundColor Gray
Write-Host "  • cd src/front && npm run dev" -ForegroundColor Gray
Write-Host "  • cd src/admin && npm install && npm run dev" -ForegroundColor Gray

Write-Host ""
Write-Host "🧪 To run tests:" -ForegroundColor Green
Write-Host "  • dotnet test" -ForegroundColor Gray
Write-Host "  • ./scripts/test-db.ps1 start" -ForegroundColor Gray

Write-Host ""
Write-Host "🛑 To stop all services:" -ForegroundColor Green
Write-Host "  • docker-compose -f docker-compose.test.yml down" -ForegroundColor Gray
Write-Host "  • ./scripts/start-all-services.ps1 stop" -ForegroundColor Gray

if ($args[0] -eq "stop") {
    Write-Host ""
    Write-Host "🛑 Stopping all services..." -ForegroundColor Red
    docker-compose -f docker-compose.test.yml down
    Write-Host "✅ All services stopped." -ForegroundColor Green
} 