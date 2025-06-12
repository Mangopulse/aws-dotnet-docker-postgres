import { AuthService } from './auth';
import { Post, CreatePostRequest, Media } from '@/types';

export class ApiService {
  private static async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    // Add auth headers if available
    const authHeaders = AuthService.getAuthHeaders();
    if (authHeaders.Authorization) {
      headers.Authorization = authHeaders.Authorization;
    }

    // Add any additional headers from options
    if (options.headers) {
      Object.entries(options.headers as Record<string, string>).forEach(([key, value]) => {
        headers[key] = value;
      });
    }

    // Remove Content-Type for FormData
    if (options.body instanceof FormData) {
      delete headers['Content-Type'];
    }

    const config: RequestInit = {
      ...options,
      headers,
    };

    const response = await fetch(url, config);

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.logout();
        window.location.href = '/login';
        throw new Error('Unauthorized');
      }
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }
    
    return response.text() as any;
  }

  // Posts API
  static async getPosts(): Promise<Post[]> {
    return this.request<Post[]>('/api/admin/posts');
  }

  static async getPost(id: string): Promise<Post> {
    return this.request<Post>(`/api/admin/posts/${id}`);
  }

  static async createPost(data: CreatePostRequest): Promise<Post> {
    const formData = new FormData();
    formData.append('title', data.title);
    
    if (data.jsonMeta) {
      formData.append('jsonMeta', JSON.stringify(data.jsonMeta));
    }
    
    if (data.file) {
      formData.append('file', data.file);
    }

    return this.request<Post>('/api/admin/posts', {
      method: 'POST',
      body: formData,
    });
  }

  static async updatePost(id: string, data: Partial<CreatePostRequest>): Promise<Post> {
    const formData = new FormData();
    
    if (data.title) {
      formData.append('title', data.title);
    }
    
    if (data.jsonMeta) {
      formData.append('jsonMeta', JSON.stringify(data.jsonMeta));
    }
    
    if (data.file) {
      formData.append('file', data.file);
    }

    return this.request<Post>(`/api/admin/posts/${id}`, {
      method: 'PUT',
      body: formData,
    });
  }

  static async deletePost(id: string): Promise<void> {
    return this.request<void>(`/api/admin/posts/${id}`, {
      method: 'DELETE',
    });
  }

  // Media API
  static async uploadMedia(file: File): Promise<Media> {
    const formData = new FormData();
    formData.append('file', file);

    return this.request<Media>('/api/upload/file', {
      method: 'POST',
      body: formData,
    });
  }

  static async getMedia(id: string): Promise<Media> {
    return this.request<Media>(`/api/admin/media/${id}`);
  }

  // Health checks
  static async checkHealth(): Promise<{ status: string }> {
    return this.request<{ status: string }>('/api/admin/health');
  }
} 