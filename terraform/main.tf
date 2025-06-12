provider "aws" {
  region = var.aws_region
}

# VPC and Networking
module "vpc" {
  source = "terraform-aws-modules/vpc/aws"
  version = "3.14.0"

  name = "dockerx-vpc"
  cidr = "10.0.0.0/16"

  azs             = ["${var.aws_region}a", "${var.aws_region}b"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24"]

  enable_nat_gateway = true
  single_nat_gateway = true
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "dockerx-cluster"
}

# ECR Repositories
resource "aws_ecr_repository" "api" {
  name = "dockerx-api"
}

resource "aws_ecr_repository" "ui" {
  name = "dockerx-ui"
}

# S3 Bucket for media
resource "aws_s3_bucket" "media" {
  bucket = "dockerx-media"
}

# RDS Instance
resource "aws_db_instance" "postgres" {
  identifier        = "dockerx-db"
  engine            = "postgres"
  engine_version    = "15"
  instance_class    = "db.t3.micro"
  allocated_storage = 20

  db_name  = "dockerx"
  username = var.db_username
  password = var.db_password

  vpc_security_group_ids = [aws_security_group.rds.id]
  db_subnet_group_name   = aws_db_subnet_group.main.name

  skip_final_snapshot = true
}

# Security Groups
resource "aws_security_group" "rds" {
  name        = "dockerx-rds-sg"
  description = "Security group for RDS"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/16"]
  }
}

# DB Subnet Group
resource "aws_db_subnet_group" "main" {
  name       = "dockerx-db-subnet-group"
  subnet_ids = module.vpc.private_subnets
} 