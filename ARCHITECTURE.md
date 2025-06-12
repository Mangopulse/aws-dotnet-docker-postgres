# DockerX Architecture Documentation

## Overview

DockerX is a comprehensive web application platform built with .NET 8 APIs, Next.js frontends, PostgreSQL database, and containerized deployment. The system follows a microservices architecture with clear separation of concerns.

## Architecture Components

### 🏗️ **Microservices Architecture**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Front App     │    │   Admin App     │    │   Legacy UI     │
│  (Next.js)      │    │  (Next.js)      │    │  (Next.js)      │
│   Port: 3000    │    │   Port: 3001    │    │   Port: 3100    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Front API     │    │   Admin API     │    │   Legacy API    │
│   (Public)      │    │  (JWT Auth)     │    │ (Compatibility) │
│   Port: 5000    │    │   Port: 5001    │    │   Port: 5100    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 ▼
                    ┌─────────────────┐    ┌─────────────────┐
                    │  Upload Service │    │  Media Service  │
                    │ (File Storage)  │    │ (ImageFlow)     │
                    │   Port: 5002    │    │   Port: 5003    │
                    └─────────────────┘    └─────────────────┘
                                 │                   │
                                 └─────────┬─────────┘
                                           ▼
                              ┌─────────────────┐
                              │   PostgreSQL    │
                              │   Database      │
                              │   Port: 5432    │
                              └─────────────────┘
```

## 📋 **Service Details**

### **Frontend Applications**

#### 1. **Front App** (Public Interface)
- **Technology**: Next.js 15 with TypeScript
- **Port**: 3000
- **Purpose**: Public-facing interface for viewing posts
- **Features**:
  - Post listing with cards
  - Image display with fallbacks
  - Responsive design with Tailwind CSS
  - Error handling with retry functionality

#### 2. **Admin App** (Management Interface)
- **Technology**: Next.js 15 with TypeScript
- **Port**: 3001
- **Purpose**: Administrative interface for content management
- **Features**:
  - JWT authentication
  - Post creation and editing
  - Media upload and management
  - User authentication (admin/admin123)

#### 3. **Legacy UI** (Backward Compatibility)
- **Port**: 3100
- **Purpose**: Maintains compatibility with existing systems

### **Backend APIs**

#### 1. **Front API** (Public API)
- **Technology**: .NET 8 Web API
- **Port**: 5000
- **Purpose**: Public API for fetching posts
- **Features**:
  - GET /api/posts (all posts)
  - GET /api/posts/{id} (specific post)
  - GET /api/posts/paged (pagination)
  - CORS enabled for frontend access
  - Entity Framework with PostgreSQL

#### 2. **Admin API** (Authenticated API)
- **Technology**: .NET 8 Web API with JWT
- **Port**: 5001
- **Purpose**: Administrative operations with authentication
- **Features**:
  - JWT token authentication
  - POST /api/auth/login (authentication)
  - CRUD operations for posts
  - Media integration with S3
  - Swagger documentation with auth

#### 3. **Upload Service** (File Management)
- **Technology**: .NET 8 Web API
- **Port**: 5002
- **Purpose**: File upload and storage management
- **Features**:
  - Multi-storage support (AWS S3, Azure Blob, Local)
  - File validation (type, size)
  - POST /api/store/upload
  - GET /api/store/health
  - Configurable storage providers

#### 4. **Media Service** (Image Processing)
- **Technology**: .NET 8 Web API with ImageFlow
- **Port**: 5003
- **Purpose**: Image processing and serving
- **Features**:
  - Dynamic image resizing
  - Image cropping with parameters
  - Format conversion (JPEG, PNG, GIF, WebP)
  - Quality adjustment
  - GET /api/media/image/{fileName}
  - GET /api/media/crop/{fileName}
  - POST /api/media/process

#### 5. **Legacy API** (Backward Compatibility)
- **Port**: 5100
- **Purpose**: Maintains compatibility with existing systems

### **Database**

#### **PostgreSQL Database**
- **Version**: PostgreSQL 15
- **Port**: 5432
- **Database**: dockerx_db
- **Credentials**: postgres/postgres123
- **Features**:
  - Posts table with media relationships
  - Media table for file metadata
  - Entity Framework migrations
  - Docker volume persistence

## 🔧 **Configuration Management**

### **Global Configuration** (`config.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dockerx_db;Username=postgres;Password=postgres123",
    "AzureStorage": "..."
  },
  "JWT": {
    "Key": "supersecretkey12345678901234567890",
    "Issuer": "DockerX",
    "Audience": "DockerX",
    "ExpirationInHours": 24
  },
  "AWS": {
    "Region": "us-east-1",
    "S3": {
      "BucketName": "dockerx-media-bucket"
    }
  },
  "Services": {
    "FrontApi": "http://localhost:5000",
    "AdminApi": "http://localhost:5001",
    "Upload": "http://localhost:5002",
    "Media": "http://localhost:5003",
    "Front": "http://localhost:3000",
    "Admin": "http://localhost:3001"
  },
  "StorageProvider": "local"
}
```

## 🚀 **Deployment**

### **Docker Compose Services**

| Service | Port | Purpose | Dependencies |
|---------|------|---------|--------------|
| front | 3000 | Public frontend | front-api |
| admin | 3001 | Admin frontend | admin-api, upload, media |
| front-api | 5000 | Public API | db |
| admin-api | 5001 | Admin API | db |
| upload | 5002 | File upload | - |
| media | 5003 | Image processing | upload (shared volume) |
| db | 5432 | PostgreSQL | - |
| Legacy services | 3100, 5100 | Backward compatibility | - |

### **Volume Management**
- `postgres_data`: Database persistence
- `upload_data`: Shared file storage between upload and media services

## 🔐 **Security Features**

### **Authentication & Authorization**
- **JWT Tokens**: 24-hour expiration
- **Default Admin**: admin/admin123
- **Protected Endpoints**: Admin API requires Bearer token
- **CORS Configuration**: Cross-origin support for frontends

### **File Upload Security**
- **File Type Validation**: JPEG, PNG, GIF, WebP only
- **Size Limits**: 10MB maximum
- **Secure Storage**: Multiple provider options

## 🧪 **Testing Infrastructure**

### **Test Database**
- **Docker Compose**: `docker-compose.test.yml`
- **PostgreSQL**: Separate test instance on port 5433
- **Real Database Testing**: Integration tests with actual PostgreSQL
- **Test Management**: PowerShell scripts for database lifecycle

### **Test Coverage**
- **Unit Tests**: In-memory database tests
- **Integration Tests**: Real PostgreSQL container tests
- **API Testing**: All endpoints covered
- **Build Verification**: Automated testing pipeline

## 📁 **Project Structure**

```
aws-dotnet-docker-postgres/
├── src/
│   ├── Shared/              # Common models and interfaces
│   ├── FrontApi/            # Public API service
│   ├── AdminApi/            # Admin API with JWT
│   ├── Upload/              # File upload service
│   ├── Media/               # Image processing service
│   ├── front/               # Public Next.js app
│   ├── admin/               # Admin Next.js app
│   ├── API/                 # Legacy API
│   └── Tests/               # Test projects
├── docker/                  # Docker configurations
├── scripts/                 # Deployment and management scripts
├── terraform/               # Infrastructure as code
├── .github/                 # CI/CD workflows
├── config.json              # Global configuration
├── docker-compose.yml       # Main compose file
├── docker-compose.test.yml  # Test compose file
└── DockerX.sln             # Solution file
```

## 🔄 **Development Workflow**

### **Local Development**
1. **Database**: `docker-compose -f docker-compose.test.yml up db-test`
2. **APIs**: Run individual services on configured ports
3. **Frontends**: `npm run dev` for Next.js applications
4. **Testing**: `dotnet test` for backend, `npm test` for frontend

### **Production Deployment**
1. **Build**: `docker-compose build`
2. **Deploy**: `docker-compose up -d`
3. **Monitoring**: Health check endpoints available
4. **Scaling**: Individual service scaling support

## 🎯 **Key Features Implemented**

✅ **Complete Microservices Architecture**  
✅ **Multi-Frontend Support** (Public + Admin)  
✅ **JWT Authentication System**  
✅ **File Upload with Multiple Storage Providers**  
✅ **Image Processing with ImageFlow**  
✅ **Real Database Integration Testing**  
✅ **Docker Containerization**  
✅ **CORS Configuration**  
✅ **Swagger API Documentation**  
✅ **Health Check Endpoints**  
✅ **Configuration Management**  
✅ **Volume Persistence**  
✅ **Legacy System Compatibility**

## 🚧 **Future Enhancements**

- [ ] Complete ImageFlow implementation
- [ ] Kubernetes deployment manifests
- [ ] AWS infrastructure automation
- [ ] Performance monitoring and logging
- [ ] API rate limiting and throttling
- [ ] Advanced media processing features
- [ ] User management system
- [ ] Content versioning and workflow
- [ ] Advanced search and filtering
- [ ] Analytics and reporting dashboard 