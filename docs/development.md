# Development Setup Guide

## Prerequisites

Before setting up the DockerX development environment, ensure you have the following tools installed:

### **Required Software**

| Tool | Version | Purpose | Download |
|------|---------|---------|----------|
| **Docker Desktop** | 4.15+ | Container orchestration | [docker.com](https://www.docker.com/products/docker-desktop/) |
| **.NET SDK** | 8.0+ | Backend development | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Node.js** | 18.0+ | Frontend development | [nodejs.org](https://nodejs.org/) |
| **PostgreSQL Client** | 15+ | Database management | [postgresql.org](https://www.postgresql.org/download/) |
| **Git** | 2.40+ | Version control | [git-scm.com](https://git-scm.com/) |

### **Recommended Tools**

| Tool | Purpose | Download |
|------|---------|----------|
| **Visual Studio Code** | Primary IDE | [code.visualstudio.com](https://code.visualstudio.com/) |
| **Postman** | API testing | [postman.com](https://www.postman.com/) |
| **pgAdmin** | Database GUI | [pgadmin.org](https://www.pgadmin.org/) |
| **Docker Compose** | Multi-container apps | Included with Docker Desktop |

## 🚀 Quick Start

### **1. Clone Repository**

```bash
git clone <repository-url>
cd aws-dotnet-docker-postgres
```

### **2. Environment Setup**

```bash
# Copy configuration template
cp config.json.template config.json

# Update configuration as needed
# See Configuration section below
```

### **3. Start All Services**

```powershell
# Windows PowerShell
.\scripts\start-all-services.ps1

# Alternative: Docker Compose
docker-compose up -d
```

### **4. Verify Installation**

Visit the following URLs to confirm services are running:

- **Front App**: http://localhost:3000
- **Admin App**: http://localhost:3001
- **Front API**: http://localhost:5000/swagger
- **Admin API**: http://localhost:5001/swagger
- **Upload Service**: http://localhost:5002/swagger
- **Media Service**: http://localhost:5003/swagger

## 🔧 Detailed Setup

### **Environment Configuration**

#### **Configuration File (config.json)**

```json
{
  "services": {
    "frontApi": "http://localhost:5000",
    "adminApi": "http://localhost:5001",
    "upload": "http://localhost:5002",
    "media": "http://localhost:5003"
  },
  "jwt": {
    "key": "YourSecretKeyHere_ChangeInProduction_32Characters",
    "issuer": "DockerX",
    "audience": "DockerX",
    "expirationHours": 24
  },
  "storage": {
    "provider": "local",
    "aws": {
      "bucketName": "dockerx-uploads",
      "region": "us-east-1"
    },
    "azure": {
      "containerName": "uploads"
    },
    "local": {
      "uploadPath": "./uploads"
    }
  },
  "database": {
    "connectionString": "Host=localhost;Port=5432;Database=dockerx_db;Username=postgres;Password=postgres123"
  }
}
```

#### **Environment Variables**

Create `.env` file for sensitive configurations:

```bash
# Database
POSTGRES_PASSWORD=postgres123
DATABASE_URL=postgresql://postgres:postgres123@localhost:5432/dockerx_db

# JWT Secret
JWT_SECRET_KEY=YourSecretKeyHere_ChangeInProduction_32Characters

# AWS Configuration
AWS_ACCESS_KEY_ID=your_access_key
AWS_SECRET_ACCESS_KEY=your_secret_key
AWS_REGION=us-east-1
AWS_BUCKET_NAME=dockerx-uploads

# Azure Configuration
AZURE_STORAGE_CONNECTION_STRING=your_connection_string
AZURE_CONTAINER_NAME=uploads

# Storage Provider (local|s3|azure)
STORAGE_PROVIDER=local
```

### **Database Setup**

#### **Using Docker (Recommended)**

```bash
# Start PostgreSQL container
docker run -d \
  --name dockerx-postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -e POSTGRES_DB=dockerx_db \
  -p 5432:5432 \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:15
```

#### **Manual Installation**

```bash
# Install PostgreSQL
# Windows: Download from postgresql.org
# macOS: brew install postgresql
# Linux: sudo apt-get install postgresql

# Create database
createdb -U postgres dockerx_db

# Run migrations
psql -U postgres -d dockerx_db -f migrations/001_create_initial_schema.sql
```

#### **Database Migrations**

```bash
# Apply all migrations
dotnet ef database update --project src/Shared

# Create new migration
dotnet ef migrations add MigrationName --project src/Shared

# Reset database (development only)
dotnet ef database drop --project src/Shared
dotnet ef database update --project src/Shared
```

### **Service Setup**

#### **Backend Services (.NET)**

```bash
# Restore dependencies
dotnet restore DockerX.sln

# Build solution
dotnet build DockerX.sln

# Run individual services
dotnet run --project src/FrontApi     # Port 5000
dotnet run --project src/AdminApi     # Port 5001
dotnet run --project src/Upload       # Port 5002
dotnet run --project src/Media        # Port 5003
```

#### **Frontend Applications (Next.js)**

```bash
# Front App setup
cd src/front
npm install
npm run dev                           # Port 3000

# Admin App setup
cd src/admin
npm install
npm run dev                           # Port 3001
```

## 🐳 Docker Development

### **Development with Docker Compose**

#### **docker-compose.dev.yml**

```yaml
version: '3.8'
services:
  # Database
  db:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: dockerx_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./migrations:/docker-entrypoint-initdb.d

  # Backend services with hot reload
  front-api:
    build:
      context: .
      dockerfile: src/FrontApi/Dockerfile.dev
    ports:
      - "5000:5000"
    volumes:
      - ./src:/app/src
      - ./config.json:/app/config.json
    depends_on:
      - db

  # Frontend with hot reload
  front:
    build:
      context: src/front
      dockerfile: Dockerfile.dev
    ports:
      - "3000:3000"
    volumes:
      - ./src/front:/app
      - /app/node_modules
    environment:
      - NODE_ENV=development

volumes:
  postgres_data:
```

#### **Development Commands**

```bash
# Start development environment
docker-compose -f docker-compose.dev.yml up

# Rebuild specific service
docker-compose -f docker-compose.dev.yml build front-api

# View logs
docker-compose -f docker-compose.dev.yml logs -f front-api

# Stop services
docker-compose -f docker-compose.dev.yml down
```

### **Hot Reload Configuration**

#### **.NET Hot Reload**

```dockerfile
# Dockerfile.dev for .NET services
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
RUN dotnet restore
EXPOSE 5000
CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
```

#### **Next.js Hot Reload**

```dockerfile
# Dockerfile.dev for Next.js
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 3000
CMD ["npm", "run", "dev"]
```

## 🛠️ Development Tools

### **Visual Studio Code Setup**

#### **Recommended Extensions**

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-vscode.vscode-typescript-next",
    "bradlc.vscode-tailwindcss",
    "ms-vscode.vscode-json",
    "humao.rest-client",
    "ms-vscode-remote.remote-containers",
    "mtxr.sqltools",
    "mtxr.sqltools-driver-pg"
  ]
}
```

#### **Workspace Settings**

```json
{
  "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
  "typescript.preferences.importModuleSpecifier": "relative",
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true
  },
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/node_modules": true,
    "**/.next": true
  }
}
```

#### **Launch Configuration**

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch FrontApi",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/FrontApi/bin/Debug/net8.0/FrontApi.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/FrontApi",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": "Launch AdminApi",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/src/AdminApi/bin/Debug/net8.0/AdminApi.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/AdminApi",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### **Database Tools**

#### **pgAdmin Configuration**

1. **Install pgAdmin**: Download from [pgadmin.org](https://www.pgadmin.org/)
2. **Connect to Database**:
   - Host: localhost
   - Port: 5432
   - Database: dockerx_db
   - Username: postgres
   - Password: postgres123

#### **Command Line Tools**

```bash
# Connect to database
psql -h localhost -U postgres -d dockerx_db

# Useful psql commands
\dt                    # List tables
\d posts              # Describe posts table
\q                    # Quit

# Run SQL scripts
psql -h localhost -U postgres -d dockerx_db -f script.sql
```

### **API Testing**

#### **Postman Collection**

Import the OpenAPI specifications from:
- http://localhost:5000/swagger/v1/swagger.json
- http://localhost:5001/swagger/v1/swagger.json
- http://localhost:5002/swagger/v1/swagger.json
- http://localhost:5003/swagger/v1/swagger.json

#### **REST Client (VS Code)**

Create `tests/api-tests.http`:

```http
### Front API - Get Posts
GET http://localhost:5000/api/posts

### Admin API - Login
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

### Admin API - Create Post
POST http://localhost:5001/api/adminposts
Authorization: Bearer {{token}}
Content-Type: multipart/form-data; boundary=boundary

--boundary
Content-Disposition: form-data; name="title"

Test Post Title
--boundary
Content-Disposition: form-data; name="jsonMeta"

{"category": "test", "tags": ["development"]}
--boundary--

### Upload Service - Health Check
GET http://localhost:5002/api/store/health

### Media Service - Health Check
GET http://localhost:5003/api/media/health
```

## 🔍 Debugging

### **.NET Debugging**

#### **Debug Configuration**

```bash
# Set debug environment
export ASPNETCORE_ENVIRONMENT=Development

# Enable detailed errors
export ASPNETCORE_DETAILEDERRORS=true

# Set logging level
export Logging__LogLevel__Default=Debug
```

#### **Logging Configuration**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### **Next.js Debugging**

#### **Debug Mode**

```bash
# Start with debug mode
NODE_OPTIONS='--inspect' npm run dev

# Debug in VS Code
# Add to launch.json:
{
  "name": "Next.js: debug client-side",
  "type": "chrome",
  "request": "launch",
  "url": "http://localhost:3000"
}
```

#### **Environment Variables**

```bash
# .env.local
NEXT_PUBLIC_DEBUG=true
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### **Docker Debugging**

```bash
# View container logs
docker logs <container_name>

# Execute commands in container
docker exec -it <container_name> bash

# Monitor resource usage
docker stats

# Inspect container details
docker inspect <container_name>
```

## 🧪 Testing Setup

### **Unit Testing**

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Tests/FrontApi.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### **Integration Testing**

```bash
# Start test database
docker run -d --name test-postgres \
  -e POSTGRES_PASSWORD=test123 \
  -e POSTGRES_DB=dockerx_test \
  -p 5433:5432 \
  postgres:15

# Run integration tests
dotnet test src/Tests/Integration.Tests \
  --environment Testing
```

### **Frontend Testing**

```bash
# Run Jest tests
cd src/front && npm test

# Run with coverage
cd src/front && npm run test:coverage

# Run E2E tests
cd src/front && npm run test:e2e
```

## 📋 Development Workflow

### **Git Workflow**

```bash
# Feature development
git checkout -b feature/new-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/new-feature

# Create pull request
# Merge after review
git checkout main
git pull origin main
git branch -d feature/new-feature
```

### **Code Quality**

#### **Linting**

```bash
# .NET formatting
dotnet format

# TypeScript/JavaScript linting
cd src/front && npm run lint
cd src/admin && npm run lint
```

#### **Pre-commit Hooks**

```bash
# Install husky
npm install --save-dev husky

# Setup pre-commit hook
npx husky add .husky/pre-commit "npm run lint && dotnet format --verify-no-changes"
```

### **Local Development Checklist**

- [ ] All services start without errors
- [ ] Database connection successful
- [ ] Front app loads at localhost:3000
- [ ] Admin app loads at localhost:3001
- [ ] API documentation accessible via Swagger
- [ ] File upload functionality works
- [ ] Image processing works
- [ ] Authentication works
- [ ] Tests pass

## 🚨 Troubleshooting

### **Common Issues**

#### **Port Conflicts**

```bash
# Check what's using a port
netstat -ano | findstr :5000

# Kill process using port
taskkill /PID <PID> /F
```

#### **Database Connection Issues**

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Reset database container
docker stop dockerx-postgres
docker rm dockerx-postgres
docker-compose up -d db
```

#### **Node Modules Issues**

```bash
# Clear npm cache
npm cache clean --force

# Delete node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

#### **Docker Issues**

```bash
# Reset Docker
docker system prune -a

# Rebuild containers
docker-compose down
docker-compose build --no-cache
docker-compose up
```

### **Performance Optimization**

#### **Development Performance**

```bash
# Use development Dockerfile with caching
docker-compose -f docker-compose.dev.yml up

# Enable file watching exclusions
# Add to .dockerignore:
node_modules
bin
obj
.git
.env
```

## 📚 Resources

### **Documentation Links**

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Next.js Documentation](https://nextjs.org/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

### **Community Resources**

- [Stack Overflow](https://stackoverflow.com/)
- [GitHub Issues](./issues)
- [Discord Community](./discord)
- [Development Blog](./blog)

This development setup guide provides everything needed to get started with DockerX development, from initial setup to advanced debugging and optimization techniques. 