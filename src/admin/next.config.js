/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    // Use environment variables for API URLs, with fallbacks for development
    const adminApiUrl = process.env.NEXT_PUBLIC_ADMIN_API_URL || 'http://localhost:5001';
    const uploadApiUrl = process.env.NEXT_PUBLIC_UPLOAD_API_URL || 'http://localhost:5002';
    const mediaApiUrl = process.env.NEXT_PUBLIC_MEDIA_API_URL || 'http://localhost:5003';

    return [
      {
        source: '/api/admin/:path*',
        destination: `${adminApiUrl}/api/:path*`
      },
      {
        source: '/api/upload/:path*',
        destination: `${uploadApiUrl}/api/store/:path*`
      },
      {
        source: '/api/media/:path*',
        destination: `${mediaApiUrl}/api/media/:path*`
      }
    ]
  }
}

module.exports = nextConfig 