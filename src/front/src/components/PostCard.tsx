import React from 'react';

interface Post {
  id: string;
  title: string;
  publicId: number;
  mediaUrl?: string;
  createdAt: string;
}

interface PostCardProps {
  post: Post;
}

export default function PostCard({ post }: PostCardProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow duration-300">
      {/* Image Section */}
      <div className="aspect-w-16 aspect-h-9 bg-gray-200">
        {post.mediaUrl ? (
          <img
            src={post.mediaUrl}
            alt={post.title}
            className="w-full h-48 object-cover"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = '/placeholder-image.png';
              target.onerror = null;
            }}
          />
        ) : (
          <div className="w-full h-48 bg-gradient-to-br from-blue-400 to-purple-500 flex items-center justify-center">
            <div className="text-white text-center">
              <svg
                className="w-12 h-12 mx-auto mb-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
              <p className="text-sm font-medium">No Image</p>
            </div>
          </div>
        )}
      </div>

      {/* Content Section */}
      <div className="p-4">
        <div className="flex items-start justify-between mb-2">
          <h3 className="text-lg font-semibold text-gray-900 line-clamp-2 flex-1">
            {post.title}
          </h3>
          <span className="ml-2 px-2 py-1 bg-blue-100 text-blue-800 text-xs font-medium rounded-full flex-shrink-0">
            #{post.publicId}
          </span>
        </div>

        <div className="flex items-center justify-between text-sm text-gray-500">
          <span>Posted on {formatDate(post.createdAt)}</span>
          <button className="text-blue-600 hover:text-blue-800 font-medium">
            View Details
          </button>
        </div>
      </div>
    </div>
  );
} 