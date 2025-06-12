/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/admin/:path*',
        destination: 'http://localhost:5001/api/:path*'
      },
      {
        source: '/api/upload/:path*',
        destination: 'http://localhost:5002/api/store/:path*'
      },
      {
        source: '/api/media/:path*',
        destination: 'http://localhost:5003/api/media/:path*'
      }
    ]
  }
}

module.exports = nextConfig 