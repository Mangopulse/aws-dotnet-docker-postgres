export interface Post {
  id: string;
  title: string;
  mediaId?: string;
  publicId: number;
  jsonMeta?: any;
  createdAt: string;
  updatedAt: string;
  mediaUrl?: string;
}

export interface Media {
  id: string;
  awsS3Path: string;
  fileName?: string;
  contentType?: string;
  size?: number;
}

export interface CreatePostRequest {
  title: string;
  jsonMeta?: any;
  file?: File;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface User {
  username: string;
  isAuthenticated: boolean;
} 