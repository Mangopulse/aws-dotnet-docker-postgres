# Test Database Management Script
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "restart", "logs", "status")]
    [string]$Action
)

$ComposeFile = "docker-compose.test.yml"

switch ($Action) {
    "start" {
        Write-Host "Starting test database..." -ForegroundColor Green
        docker-compose -f $ComposeFile up -d
        Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        docker-compose -f $ComposeFile exec postgres-test pg_isready -U testuser -d testdb
        Write-Host "Test database is ready!" -ForegroundColor Green
    }
    "stop" {
        Write-Host "Stopping test database..." -ForegroundColor Yellow
        docker-compose -f $ComposeFile down
        Write-Host "Test database stopped." -ForegroundColor Green
    }
    "restart" {
        Write-Host "Restarting test database..." -ForegroundColor Yellow
        docker-compose -f $ComposeFile down
        docker-compose -f $ComposeFile up -d
        Start-Sleep -Seconds 5
        docker-compose -f $ComposeFile exec postgres-test pg_isready -U testuser -d testdb
        Write-Host "Test database restarted and ready!" -ForegroundColor Green
    }
    "logs" {
        docker-compose -f $ComposeFile logs -f postgres-test
    }
    "status" {
        Write-Host "Test database status:" -ForegroundColor Cyan
        docker-compose -f $ComposeFile ps
        Write-Host "`nDatabase connection test:" -ForegroundColor Cyan
        docker-compose -f $ComposeFile exec postgres-test pg_isready -U testuser -d testdb
    }
} 