# Deployment Guide

## Overview

This guide covers the deployment of DockerX to various environments, from local development to production on AWS. The application is containerized using Docker and can be deployed using Docker Compose or AWS ECS.

## 🐳 Docker Deployment

### **Prerequisites**

- Docker Engine 24.0.0 or later
- Docker Compose v2.20.0 or later
- Git

### **Local Deployment**

1. **Clone the Repository**

```bash
git clone https://github.com/yourusername/dockerx.git
cd dockerx
```

2. **Build Images**

```bash
docker-compose build
```

3. **Start Services**

```bash
docker-compose up -d
```

4. **Verify Deployment**

```bash
docker-compose ps
```

### **Docker Compose Configuration**

```yaml
# docker-compose.yml
version: '3.8'

services:
  front:
    build:
      context: ./front
      dockerfile: Dockerfile
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
      - NEXT_PUBLIC_API_URL=http://localhost:5000
    depends_on:
      - front-api

  admin:
    build:
      context: ./admin
      dockerfile: Dockerfile
    ports:
      - "3001:3001"
    environment:
      - NODE_ENV=production
      - NEXT_PUBLIC_API_URL=http://localhost:5001
    depends_on:
      - admin-api

  front-api:
    build:
      context: ./src/FrontApi
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=dockerx_db;Username=postgres;Password=postgres123
    depends_on:
      - db

  admin-api:
    build:
      context: ./src/AdminApi
      dockerfile: Dockerfile
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=dockerx_db;Username=postgres;Password=postgres123
      - Jwt__Key=YourSecretKeyHere_ChangeInProduction_32Characters
    depends_on:
      - db

  upload:
    build:
      context: ./src/Upload
      dockerfile: Dockerfile
    ports:
      - "5002:5002"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Storage__Provider=local
      - Storage__Local__UploadPath=/app/uploads
    volumes:
      - upload_data:/app/uploads

  media:
    build:
      context: ./src/Media
      dockerfile: Dockerfile
    ports:
      - "5003:5003"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - upload_data:/app/uploads

  db:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=dockerx_db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
  upload_data:
```

### **Dockerfile Examples**

```dockerfile
# Front API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FrontApi/FrontApi.csproj", "src/FrontApi/"]
RUN dotnet restore "src/FrontApi/FrontApi.csproj"
COPY . .
WORKDIR "/src/src/FrontApi"
RUN dotnet build "FrontApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FrontApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrontApi.dll"]
```

```dockerfile
# Front Next.js Dockerfile
FROM node:20-alpine AS base
WORKDIR /app

FROM base AS deps
COPY package.json package-lock.json ./
RUN npm ci

FROM base AS builder
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

FROM base AS runner
ENV NODE_ENV production
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000
ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

CMD ["node", "server.js"]
```

## ☁️ AWS Deployment

### **Prerequisites**

- AWS CLI configured with appropriate credentials
- AWS ECS CLI installed
- AWS ECR repository created

### **AWS ECS Deployment**

1. **Create ECR Repository**

```bash
aws ecr create-repository --repository-name dockerx
```

2. **Build and Push Images**

```bash
# Login to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# Build and tag images
docker-compose build
docker tag dockerx_front $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:front
docker tag dockerx_admin $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:admin
docker tag dockerx_front-api $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:front-api
docker tag dockerx_admin-api $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:admin-api
docker tag dockerx_upload $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:upload
docker tag dockerx_media $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:media

# Push images
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:front
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:admin
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:front-api
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:admin-api
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:upload
docker push $AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/dockerx:media
```

3. **Create ECS Cluster**

```bash
aws ecs create-cluster --cluster-name dockerx-cluster
```

4. **Create Task Definitions**

```json
{
  "family": "dockerx-front",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "containerDefinitions": [
    {
      "name": "front",
      "image": "${AWS_ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/dockerx:front",
      "portMappings": [
        {
          "containerPort": 3000,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "NODE_ENV",
          "value": "production"
        },
        {
          "name": "NEXT_PUBLIC_API_URL",
          "value": "http://front-api.internal"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/dockerx",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "front"
        }
      }
    }
  ]
}
```

5. **Create Services**

```bash
aws ecs create-service \
  --cluster dockerx-cluster \
  --service-name front \
  --task-definition dockerx-front:1 \
  --desired-count 1 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxxxx],securityGroups=[sg-xxxxx],assignPublicIp=ENABLED}"
```

### **AWS RDS Setup**

1. **Create RDS Instance**

```bash
aws rds create-db-instance \
  --db-instance-identifier dockerx-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --master-username postgres \
  --master-user-password postgres123 \
  --allocated-storage 20
```

2. **Update Security Group**

```bash
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp \
  --port 5432 \
  --source-group sg-yyyyy
```

### **AWS S3 Setup**

1. **Create S3 Bucket**

```bash
aws s3api create-bucket \
  --bucket dockerx-uploads \
  --region us-east-1
```

2. **Configure CORS**

```json
{
  "CORSRules": [
    {
      "AllowedHeaders": ["*"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
      "AllowedOrigins": ["*"],
      "ExposeHeaders": []
    }
  ]
}
```

3. **Create IAM Policy**

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject"
      ],
      "Resource": "arn:aws:s3:::dockerx-uploads/*"
    }
  ]
}
```

## 🔒 Production Security

### **SSL/TLS Configuration**

1. **Obtain SSL Certificate**

```bash
aws acm request-certificate \
  --domain-name dockerx.example.com \
  --validation-method DNS
```

2. **Configure ALB**

```bash
aws elbv2 create-load-balancer \
  --name dockerx-alb \
  --subnets subnet-xxxxx subnet-yyyyy \
  --security-groups sg-zzzzz \
  --scheme internet-facing \
  --type application

aws elbv2 create-listener \
  --load-balancer-arn arn:aws:elasticloadbalancing:us-east-1:123456789012:loadbalancer/app/dockerx-alb/1234567890abcdef \
  --protocol HTTPS \
  --port 443 \
  --certificates CertificateArn=arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012 \
  --default-actions Type=forward,TargetGroupArn=arn:aws:elasticloadbalancing:us-east-1:123456789012:targetgroup/dockerx-front/1234567890abcdef
```

### **Security Groups**

```bash
# Create security group for ALB
aws ec2 create-security-group \
  --group-name dockerx-alb-sg \
  --description "Security group for DockerX ALB"

# Create security group for ECS tasks
aws ec2 create-security-group \
  --group-name dockerx-ecs-sg \
  --description "Security group for DockerX ECS tasks"

# Allow inbound traffic to ALB
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp \
  --port 443 \
  --cidr 0.0.0.0/0

# Allow traffic from ALB to ECS tasks
aws ec2 authorize-security-group-ingress \
  --group-id sg-yyyyy \
  --protocol tcp \
  --port 3000 \
  --source-group sg-xxxxx
```

## 📊 Monitoring & Logging

### **CloudWatch Setup**

1. **Create Log Groups**

```bash
aws logs create-log-group --log-group-name /ecs/dockerx
aws logs create-log-group --log-group-name /ecs/dockerx-front
aws logs create-log-group --log-group-name /ecs/dockerx-admin
aws logs create-log-group --log-group-name /ecs/dockerx-front-api
aws logs create-log-group --log-group-name /ecs/dockerx-admin-api
aws logs create-log-group --log-group-name /ecs/dockerx-upload
aws logs create-log-group --log-group-name /ecs/dockerx-media
```

2. **Create Metrics**

```bash
aws cloudwatch put-metric-alarm \
  --alarm-name dockerx-cpu-utilization \
  --alarm-description "Alarm when CPU exceeds 70%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 70 \
  --comparison-operator GreaterThanThreshold \
  --dimensions Name=ClusterName,Value=dockerx-cluster \
  --evaluation-periods 2 \
  --alarm-actions arn:aws:sns:us-east-1:123456789012:dockerx-alerts
```

### **X-Ray Integration**

1. **Enable X-Ray in ECS Task Definition**

```json
{
  "containerDefinitions": [
    {
      "name": "front-api",
      "image": "${AWS_ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/dockerx:front-api",
      "environment": [
        {
          "name": "AWS_XRAY_DAEMON_ADDRESS",
          "value": "xray-daemon:2000"
        }
      ]
    },
    {
      "name": "xray-daemon",
      "image": "amazon/aws-xray-daemon",
      "portMappings": [
        {
          "containerPort": 2000,
          "protocol": "udp"
        }
      ]
    }
  ]
}
```

## 🔄 CI/CD Pipeline

### **GitHub Actions Workflow**

```yaml
name: Deploy to AWS

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v1
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: us-east-1
    
    - name: Login to Amazon ECR
      id: login-ecr
      uses: aws-actions/amazon-ecr-login@v1
    
    - name: Build and push images
      env:
        ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
      run: |
        docker-compose build
        docker tag dockerx_front $ECR_REGISTRY/dockerx:front
        docker tag dockerx_admin $ECR_REGISTRY/dockerx:admin
        docker tag dockerx_front-api $ECR_REGISTRY/dockerx:front-api
        docker tag dockerx_admin-api $ECR_REGISTRY/dockerx:admin-api
        docker tag dockerx_upload $ECR_REGISTRY/dockerx:upload
        docker tag dockerx_media $ECR_REGISTRY/dockerx:media
        docker push $ECR_REGISTRY/dockerx:front
        docker push $ECR_REGISTRY/dockerx:admin
        docker push $ECR_REGISTRY/dockerx:front-api
        docker push $ECR_REGISTRY/dockerx:admin-api
        docker push $ECR_REGISTRY/dockerx:upload
        docker push $ECR_REGISTRY/dockerx:media
    
    - name: Update ECS services
      run: |
        aws ecs update-service --cluster dockerx-cluster --service front --force-new-deployment
        aws ecs update-service --cluster dockerx-cluster --service admin --force-new-deployment
        aws ecs update-service --cluster dockerx-cluster --service front-api --force-new-deployment
        aws ecs update-service --cluster dockerx-cluster --service admin-api --force-new-deployment
        aws ecs update-service --cluster dockerx-cluster --service upload --force-new-deployment
        aws ecs update-service --cluster dockerx-cluster --service media --force-new-deployment
```

## 🔍 Troubleshooting

### **Common Issues**

1. **Container Health Checks Failing**

```bash
# Check container logs
docker-compose logs front-api

# Check container health
docker inspect --format='{{.State.Health.Status}}' dockerx_front-api_1
```

2. **Database Connection Issues**

```bash
# Check database logs
docker-compose logs db

# Test database connection
docker-compose exec db psql -U postgres -d dockerx_db -c "\l"
```

3. **S3 Access Issues**

```bash
# Check IAM permissions
aws iam get-user-policy --user-name dockerx-user --policy-name S3Access

# Test S3 access
aws s3 ls s3://dockerx-uploads/
```

### **Monitoring Commands**

```bash
# Check service status
docker-compose ps

# View logs
docker-compose logs -f

# Check resource usage
docker stats

# Check network connectivity
docker network inspect dockerx_default
```

## 📈 Scaling

### **Auto Scaling Configuration**

```bash
# Create auto scaling target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/dockerx-cluster/front \
  --min-capacity 1 \
  --max-capacity 10

# Create scaling policy
aws application-autoscaling put-scaling-policy \
  --policy-name dockerx-cpu-scaling \
  --policy-type TargetTrackingScaling \
  --resource-id service/dockerx-cluster/front \
  --scalable-dimension ecs:service:DesiredCount \
  --target-tracking-scaling-policy-configuration '{
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    }
  }'
```

This comprehensive deployment guide covers all aspects of deploying DockerX to various environments, from local development to production on AWS. 