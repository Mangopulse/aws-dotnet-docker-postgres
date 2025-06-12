# API Documentation

## Overview

DockerX provides a comprehensive set of RESTful APIs across multiple services. Each API follows consistent patterns and includes OpenAPI/Swagger documentation.

## 🌐 API Services Overview

| Service | Base URL | Authentication | Purpose |
|---------|----------|----------------|---------|
| **Front API** | `http://localhost:5000/api` | None | Public content access |
| **Admin API** | `http://localhost:5001/api` | JWT Bearer | Administrative operations |
| **Upload Service** | `http://localhost:5002/api` | None | File upload management |
| **Media Service** | `http://localhost:5003/api` | None | Image processing |

## 🔓 Front API (Public)

**Base URL**: `http://localhost:5000/api`  
**Authentication**: None required  
**Purpose**: Public content retrieval

### **Get All Posts**

```http
GET /api/posts
```

**Description**: Retrieves all posts with media information

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Sample Post Title",
      "mediaId": "456e7890-e89b-12d3-a456-426614174001",
      "publicId": 1,
      "jsonMeta": {
        "tags": ["technology", "programming"],
        "author": "John Doe"
      },
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "mediaUrl": "http://localhost:5003/api/media/image/sample.jpg"
    }
  ],
  "count": 1
}
```

**Status Codes**:
- `200 OK` - Success
- `500 Internal Server Error` - Server error

### **Get Post by ID**

```http
GET /api/posts/{id}
```

**Parameters**:
- `id` (string, required) - Post UUID

**Response**:
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Sample Post Title",
    "mediaId": "456e7890-e89b-12d3-a456-426614174001",
    "publicId": 1,
    "jsonMeta": {
      "tags": ["technology", "programming"],
      "author": "John Doe"
    },
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "mediaUrl": "http://localhost:5003/api/media/image/sample.jpg"
  }
}
```

**Status Codes**:
- `200 OK` - Success
- `404 Not Found` - Post not found
- `500 Internal Server Error` - Server error

### **Get Paginated Posts**

```http
GET /api/posts/paged?page={page}&size={size}
```

**Query Parameters**:
- `page` (integer, optional) - Page number (default: 1)
- `size` (integer, optional) - Page size (default: 10, max: 100)

**Response**:
```json
{
  "success": true,
  "data": {
    "posts": [...],
    "totalCount": 25,
    "page": 1,
    "size": 10,
    "totalPages": 3,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

## 🔐 Admin API (Authenticated)

**Base URL**: `http://localhost:5001/api`  
**Authentication**: JWT Bearer Token required  
**Purpose**: Administrative content management

### **Authentication**

#### **Login**

```http
POST /api/auth/login
```

**Request Body**:
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-16T10:30:00Z",
  "username": "admin"
}
```

**Status Codes**:
- `200 OK` - Authentication successful
- `401 Unauthorized` - Invalid credentials
- `400 Bad Request` - Invalid request format

### **Admin Posts Management**

#### **Get Admin Posts**

```http
GET /api/adminposts
Authorization: Bearer {token}
```

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Admin Post Title",
      "mediaId": "456e7890-e89b-12d3-a456-426614174001",
      "publicId": 1,
      "jsonMeta": {
        "status": "published",
        "category": "technology"
      },
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z",
      "mediaUrl": "http://localhost:5003/api/media/image/sample.jpg"
    }
  ]
}
```

#### **Create Post**

```http
POST /api/adminposts
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Form Data**:
- `title` (string, required) - Post title
- `jsonMeta` (string, optional) - JSON metadata
- `file` (file, optional) - Media file

**Example**:
```http
POST /api/adminposts
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data

title=My New Post
jsonMeta={"category":"technology","tags":["api","documentation"]}
file=@image.jpg
```

**Response**:
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174002",
    "title": "My New Post",
    "mediaId": "012e3456-e89b-12d3-a456-426614174003",
    "publicId": 2,
    "jsonMeta": {
      "category": "technology",
      "tags": ["api", "documentation"]
    },
    "createdAt": "2024-01-15T11:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z",
    "mediaUrl": "http://localhost:5003/api/media/image/new-image.jpg"
  }
}
```

#### **Update Post**

```http
PUT /api/adminposts/{id}
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Parameters**:
- `id` (string, required) - Post UUID

**Form Data**:
- `title` (string, optional) - Updated title
- `jsonMeta` (string, optional) - Updated JSON metadata
- `file` (file, optional) - New media file

#### **Delete Post**

```http
DELETE /api/adminposts/{id}
Authorization: Bearer {token}
```

**Parameters**:
- `id` (string, required) - Post UUID

**Response**:
```json
{
  "success": true,
  "message": "Post deleted successfully"
}
```

## 📁 Upload Service

**Base URL**: `http://localhost:5002/api`  
**Authentication**: None required  
**Purpose**: File upload and storage management

### **Upload File**

```http
POST /api/store/upload
Content-Type: multipart/form-data
```

**Form Data**:
- `file` (file, required) - File to upload

**File Restrictions**:
- **Allowed Types**: JPEG, PNG, GIF, WebP
- **Max Size**: 10MB
- **Extensions**: .jpg, .jpeg, .png, .gif, .webp

**Example**:
```http
POST /api/store/upload
Content-Type: multipart/form-data

file=@image.jpg
```

**Response**:
```json
{
  "fileName": "a1b2c3d4-e5f6-7890-1234-567890abcdef.jpg",
  "originalFileName": "image.jpg",
  "fileUrl": "http://localhost:5002/uploads/a1b2c3d4-e5f6-7890-1234-567890abcdef.jpg",
  "size": 1024768,
  "storageProvider": "local",
  "uploadedAt": "2024-01-15T11:30:00Z"
}
```

**Status Codes**:
- `200 OK` - Upload successful
- `400 Bad Request` - Invalid file type or size
- `500 Internal Server Error` - Upload failed

### **Health Check**

```http
GET /api/store/health
```

**Response**:
```json
{
  "service": "Upload Service",
  "status": "Healthy",
  "storageProvider": "local",
  "timestamp": "2024-01-15T11:30:00Z"
}
```

## 🖼️ Media Service

**Base URL**: `http://localhost:5003/api`  
**Authentication**: None required  
**Purpose**: Image processing and serving

### **Serve Image**

```http
GET /api/media/image/{fileName}?width={width}&height={height}&format={format}&quality={quality}
```

**Parameters**:
- `fileName` (string, required) - Image file name
- `width` (integer, optional) - Desired width in pixels
- `height` (integer, optional) - Desired height in pixels
- `format` (string, optional) - Output format (jpeg, png, gif, webp)
- `quality` (integer, optional) - Quality level (1-100, default: 90)

**Examples**:
```http
# Original image
GET /api/media/image/sample.jpg

# Resized image
GET /api/media/image/sample.jpg?width=300&height=200

# Format conversion with quality
GET /api/media/image/sample.jpg?format=webp&quality=80
```

**Response**: Binary image data with appropriate Content-Type header

### **Crop Image**

```http
GET /api/media/crop/{fileName}?x={x}&y={y}&width={width}&height={height}&format={format}&quality={quality}
```

**Parameters**:
- `fileName` (string, required) - Image file name
- `x` (integer, required) - X coordinate for crop start
- `y` (integer, required) - Y coordinate for crop start
- `width` (integer, required) - Crop width in pixels
- `height` (integer, required) - Crop height in pixels
- `format` (string, optional) - Output format (default: jpeg)
- `quality` (integer, optional) - Quality level (default: 90)

**Example**:
```http
GET /api/media/crop/sample.jpg?x=100&y=50&width=200&height=200&format=png
```

### **Process Uploaded Image**

```http
POST /api/media/process?width={width}&height={height}&format={format}&quality={quality}
Content-Type: multipart/form-data
```

**Form Data**:
- `file` (file, required) - Image file to process

**Query Parameters**:
- `width` (integer, optional) - Desired width
- `height` (integer, optional) - Desired height
- `format` (string, optional) - Output format
- `quality` (integer, optional) - Quality level

**Response**: Processed image binary data

### **Health Check**

```http
GET /api/media/health
```

**Response**:
```json
{
  "service": "Media Processing Service",
  "status": "Healthy",
  "timestamp": "2024-01-15T11:30:00Z",
  "supportedFormats": ["jpeg", "png", "gif", "webp"],
  "features": ["resize", "crop", "format_conversion", "quality_adjustment"]
}
```

## 🔧 Common Response Patterns

### **Success Response**

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

### **Error Response**

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input provided",
    "details": {
      "field": "title",
      "issue": "Title is required"
    }
  },
  "timestamp": "2024-01-15T11:30:00Z"
}
```

### **Validation Error**

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "title",
        "message": "Title is required"
      },
      {
        "field": "file",
        "message": "File size exceeds 10MB limit"
      }
    ]
  }
}
```

## 🔐 Authentication

### **JWT Token Usage**

All authenticated endpoints require a Bearer token in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### **Token Structure**

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "admin",
    "iss": "DockerX",
    "aud": "DockerX",
    "exp": 1642234567,
    "iat": 1642147567
  }
}
```

### **Token Expiration**

- **Default Expiration**: 24 hours
- **Renewal**: Re-authenticate to get new token
- **Validation**: Server validates signature and expiration

## 📊 Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| `200 OK` | Success | Successful GET, PUT, POST operations |
| `201 Created` | Resource created | Successful POST with resource creation |
| `204 No Content` | Success with no body | Successful DELETE operations |
| `400 Bad Request` | Client error | Invalid request format or parameters |
| `401 Unauthorized` | Authentication required | Missing or invalid credentials |
| `403 Forbidden` | Access denied | Valid credentials but insufficient permissions |
| `404 Not Found` | Resource not found | Requested resource doesn't exist |
| `409 Conflict` | Resource conflict | Duplicate resource or state conflict |
| `422 Unprocessable Entity` | Validation error | Valid format but failed business rules |
| `429 Too Many Requests` | Rate limit exceeded | Too many requests in time window |
| `500 Internal Server Error` | Server error | Unexpected server-side error |
| `503 Service Unavailable` | Service down | Service temporarily unavailable |

## 🔄 Rate Limiting

### **Current Limits**

```yaml
Upload Service:
  - 10 requests per minute per IP
  - 50MB total upload per hour per IP

Admin API:
  - 100 requests per minute per authenticated user
  - 1000 requests per hour per authenticated user

Front API:
  - 1000 requests per minute per IP
  - No hourly limit

Media Service:
  - 500 requests per minute per IP
  - CDN caching reduces actual requests
```

### **Rate Limit Headers**

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1642234567
```

## 📝 Request/Response Examples

### **Complete Post Creation Flow**

1. **Authenticate**:
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

2. **Upload Media**:
```bash
curl -X POST http://localhost:5002/api/store/upload \
  -F "file=@image.jpg"
```

3. **Create Post**:
```bash
curl -X POST http://localhost:5001/api/adminposts \
  -H "Authorization: Bearer {token}" \
  -F "title=My New Post" \
  -F 'jsonMeta={"category":"technology"}' \
  -F "file=@image.jpg"
```

4. **Fetch Public Posts**:
```bash
curl http://localhost:5000/api/posts
```

5. **Get Processed Image**:
```bash
curl "http://localhost:5003/api/media/image/filename.jpg?width=300&height=200"
```

## 🐛 Error Handling

### **Common Error Scenarios**

```yaml
Authentication Errors:
  - Invalid credentials (401)
  - Expired token (401)
  - Missing authorization header (401)

Validation Errors:
  - Invalid file type (400)
  - File size too large (400)
  - Missing required fields (400)
  - Invalid JSON format (400)

Resource Errors:
  - Post not found (404)
  - Media file not found (404)
  - Duplicate resource (409)

Server Errors:
  - Database connection failed (503)
  - File storage unavailable (503)
  - Internal processing error (500)
```

### **Error Recovery**

```yaml
Client Retry Strategy:
  - 401 Errors: Re-authenticate and retry
  - 429 Errors: Exponential backoff retry
  - 500 Errors: Retry with backoff (max 3 times)
  - 503 Errors: Retry after delay

Server Recovery:
  - Health check endpoints for monitoring
  - Circuit breaker patterns
  - Graceful degradation
  - Automatic restart policies
```

## 🔧 Development Tools

### **Swagger/OpenAPI**

Each service provides interactive API documentation:

- **Front API**: `http://localhost:5000/swagger`
- **Admin API**: `http://localhost:5001/swagger`
- **Upload Service**: `http://localhost:5002/swagger`
- **Media Service**: `http://localhost:5003/swagger`

### **Testing with Postman**

Import the OpenAPI specs for comprehensive testing collections with pre-configured requests and authentication.

### **API Client Libraries**

Type-safe client libraries can be generated from OpenAPI specifications for various languages:
- TypeScript/JavaScript
- C#/.NET
- Python
- Java
- Go

This comprehensive API documentation provides everything needed to integrate with the DockerX platform effectively. 