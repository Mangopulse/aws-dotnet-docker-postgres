# Start Front API
Start-Process powershell -ArgumentList "cd src/FrontApi; dotnet run"

# Start Admin API
Start-Process powershell -ArgumentList "cd src/AdminApi; dotnet run"

# Start Front Next.js App
Start-Process powershell -ArgumentList "cd src/front; npm run dev"

# Start Admin Next.js App
Start-Process powershell -ArgumentList "cd src/admin; npm run dev"

Write-Host "Starting local services..."
Write-Host "Front API: http://localhost:5000"
Write-Host "Admin API: http://localhost:5001"
Write-Host "Front App: http://localhost:3000"
Write-Host "Admin App: http://localhost:3001"
Write-Host "Upload Service: http://localhost:5002 (Docker)"
Write-Host "Media Service: http://localhost:5003 (Docker)"
Write-Host "Database: localhost:5432 (Docker)" 