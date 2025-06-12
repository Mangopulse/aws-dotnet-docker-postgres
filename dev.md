# DockerX Development Guide

This guide provides all the necessary commands to run the DockerX project locally.

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- PostgreSQL (if running without Docker)
- AWS CLI (for S3 storage)
- Azure CLI (for Azure Blob storage)

## Environment Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd aws-dotnet-docker-postgres
```

2. Create a `config.json` file in the root directory:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dockerx;Username=postgres;Password=postgres"
  },
  "JWT": {
    "Key": "your-super-secret-jwt-key-that-should-be-at-least-256-bits-long-for-security",
    "Issuer": "AdminApi",
    "Audience": "AdminApiUsers"
  },
  "AWS": {
    "BucketName": "your-bucket-name",
    "Region": "us-east-1"
  },
  "Azure": {
    "ConnectionString": "your-connection-string",
    "ContainerName": "your-container-name"
  },
  "Storage": {
    "Provider": "local",
    "Local": {
      "BasePath": "uploads"
    }
  },
  "Admin": {
    "Username": "admin",
    "Password": "admin123"
  }
}
```

## Running with Docker Compose

1. Build and start all services:
```bash
docker-compose up --build
```

2. Stop all services:
```bash
docker-compose down
```

3. View logs:
```bash
docker-compose logs -f
```

## Running Individual Services

### Backend Services

1. Front API (Port 5000):
```bash
cd src/FrontApi
dotnet run
```

2. Admin API (Port 5001):
```bash
cd src/AdminApi
dotnet run
```

3. Upload Service (Port 5002):
```bash
cd src/Upload
dotnet run
```

4. Media Service (Port 5003):
```bash
cd src/Media
dotnet run
```

### Frontend Services

1. Front Next.js App (Port 3000):
```bash
cd src/front
npm install
npm run dev
```

2. Admin Next.js App (Port 3001):
```bash
cd src/admin
npm install
npm run dev
```

## Database Management

1. Create database:
```bash
psql -U postgres
CREATE DATABASE dockerx;
```

2. Run migrations:
```bash
cd src/Shared
dotnet ef database update
```

## Testing

1. Run all tests:
```bash
dotnet test
```

2. Run specific test project:
```bash
dotnet test src/Shared.Tests/Shared.Tests.csproj
```

3. Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

## Development Tools

1. Start all services (PowerShell script):
```powershell
.\start-all-services.ps1
```

2. Stop all services (PowerShell script):
```powershell
.\stop-all-services.ps1
```

## Service URLs

- Front App: http://localhost:3000
- Admin App: http://localhost:3001
- Front API: http://localhost:5000
- Admin API: http://localhost:5001
- Upload Service: http://localhost:5002
- Media Service: http://localhost:5003
- PostgreSQL: localhost:5432

## API Documentation

- Front API Swagger: http://localhost:5000/swagger
- Admin API Swagger: http://localhost:5001/swagger
- Upload Service Swagger: http://localhost:5002/swagger
- Media Service Swagger: http://localhost:5003/swagger

## Default Credentials

- Username: admin
- Password: admin123

## Troubleshooting

1. Clear Docker resources:
```bash
docker system prune -a
```

2. Reset database:
```bash
docker-compose down -v
docker-compose up -d db
```

3. Check service logs:
```bash
docker-compose logs [service-name]
```

4. Rebuild specific service:
```bash
docker-compose up -d --build [service-name]
```

## Development Workflow

1. Start the development environment:
```bash
# Start all services
docker-compose up -d

# Or use the PowerShell script
.\start-all-services.ps1
```

2. Make changes to the code

3. Rebuild affected services:
```bash
docker-compose up -d --build [service-name]
```

4. Run tests:
```bash
dotnet test
```

5. Stop the development environment:
```bash
# Stop all services
docker-compose down

# Or use the PowerShell script
.\stop-all-services.ps1
```

## Common Issues

1. Port conflicts:
   - Check if any service is already using the required ports
   - Use `netstat -ano | findstr :<port>` to find processes using specific ports
   - Kill the process or change the port in the configuration

2. Database connection issues:
   - Ensure PostgreSQL is running
   - Check connection string in config.json
   - Verify database exists and migrations are applied

3. Storage issues:
   - For local storage: Ensure uploads directory exists and has proper permissions
   - For S3: Verify AWS credentials and bucket configuration
   - For Azure: Verify connection string and container configuration

4. Build errors:
   - Clean solution: `dotnet clean`
   - Delete bin and obj folders
   - Restore packages: `dotnet restore`
   - Rebuild: `dotnet build` 