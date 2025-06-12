DockerX/
├── src/
│   ├── Core/                              # All business logic lives here
│   │   ├── DockerX.Domain/               # Domain entities & interfaces
│   │   │   ├── Entities/
│   │   │   │   ├── Media.cs
│   │   │   │   ├── Post.cs
│   │   │   │   └── User.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IMediaRepository.cs
│   │   │   │   ├── IPostRepository.cs
│   │   │   │   └── IStorageService.cs
│   │   │   └── Enums/
│   │   │
│   │   ├── DockerX.Application/          # ALL business logic here
│   │   │   ├── Services/                 # Business services
│   │   │   │   ├── MediaService.cs
│   │   │   │   ├── UploadService.cs
│   │   │   │   ├── AuthService.cs
│   │   │   │   └── PostService.cs
│   │   │   ├── DTOs/                     # Data transfer objects
│   │   │   │   ├── MediaDto.cs
│   │   │   │   ├── PostDto.cs
│   │   │   │   └── UploadDto.cs
│   │   │   ├── Validators/               # Business validation
│   │   │   │   ├── MediaValidator.cs
│   │   │   │   └── PostValidator.cs
│   │   │   └── Interfaces/
│   │   │       ├── IMediaService.cs
│   │   │       ├── IUploadService.cs
│   │   │       └── IAuthService.cs
│   │   │
│   │   └── DockerX.Infrastructure/       # Data access & external services
│   │       ├── Data/
│   │       │   ├── ApplicationDbContext.cs
│   │       │   └── Repositories/
│   │       │       ├── MediaRepository.cs
│   │       │       └── PostRepository.cs
│   │       ├── Services/
│   │       │   ├── S3StorageService.cs
│   │       │   ├── LocalStorageService.cs
│   │       │   └── EmailService.cs
│   │       └── Configuration/
│   │           └── DependencyInjection.cs
│   │
│   ├── WebApis/                          # THIN APIs - NO BUSINESS LOGIC
│   │   ├── DockerX.MediaApi/            # Only controllers & middleware
│   │   │   ├── Controllers/
│   │   │   │   └── MediaController.cs    # Only calls Application services
│   │   │   ├── Middleware/
│   │   │   ├── Program.cs
│   │   │   └── Dockerfile
│   │   │
│   │   ├── DockerX.UploadApi/
│   │   │   ├── Controllers/
│   │   │   │   └── UploadController.cs   # Only calls Application services
│   │   │   ├── Program.cs
│   │   │   └── Dockerfile
│   │   │
│   │   └── DockerX.AdminApi/
│   │       ├── Controllers/
│   │       │   ├── AuthController.cs     # Only calls AuthService
│   │       │   └── AdminController.cs
│   │       ├── Program.cs
│   │       └── Dockerfile
│   │
│   ├── WebApps/                          # UI Applications
│   │   ├── DockerX.AdminPanel/          # Admin Dashboard (React/Next.js)
│   │   │   ├── src/
│   │   │   │   ├── components/
│   │   │   │   │   ├── common/           # Shared UI components
│   │   │   │   │   │   ├── Layout/
│   │   │   │   │   │   ├── Forms/
│   │   │   │   │   │   └── Tables/
│   │   │   │   │   ├── media/            # Media management components
│   │   │   │   │   │   ├── MediaList.tsx
│   │   │   │   │   │   ├── MediaUpload.tsx
│   │   │   │   │   │   └── MediaCard.tsx
│   │   │   │   │   ├── posts/            # Post management components
│   │   │   │   │   │   ├── PostList.tsx
│   │   │   │   │   │   ├── PostEditor.tsx
│   │   │   │   │   │   └── PostCard.tsx
│   │   │   │   │   └── auth/             # Authentication components
│   │   │   │   │       ├── LoginForm.tsx
│   │   │   │   │       └── ProtectedRoute.tsx
│   │   │   │   │
│   │   │   │   ├── pages/                # Page components
│   │   │   │   │   ├── dashboard/
│   │   │   │   │   ├── media/
│   │   │   │   │   ├── posts/
│   │   │   │   │   └── auth/
│   │   │   │   │
│   │   │   │   ├── services/             # API client services
│   │   │   │   │   ├── apiClient.ts      # Base HTTP client
│   │   │   │   │   ├── mediaService.ts   # Media API calls
│   │   │   │   │   ├── postService.ts    # Post API calls
│   │   │   │   │   └── authService.ts    # Auth API calls
│   │   │   │   │
│   │   │   │   ├── hooks/                # Custom React hooks
│   │   │   │   │   ├── useAuth.ts
│   │   │   │   │   ├── useMedia.ts
│   │   │   │   │   └── usePosts.ts
│   │   │   │   │
│   │   │   │   ├── store/                # State management (Redux/Zustand)
│   │   │   │   │   ├── authSlice.ts
│   │   │   │   │   ├── mediaSlice.ts
│   │   │   │   │   └── postSlice.ts
│   │   │   │   │
│   │   │   │   ├── types/                # TypeScript types
│   │   │   │   │   ├── media.ts
│   │   │   │   │   ├── post.ts
│   │   │   │   │   └── auth.ts
│   │   │   │   │
│   │   │   │   └── utils/                # Utility functions
│   │   │   │       ├── constants.ts
│   │   │   │       ├── helpers.ts
│   │   │   │       └── validation.ts
│   │   │   │
│   │   │   ├── public/
│   │   │   ├── package.json
│   │   │   ├── next.config.js
│   │   │   ├── tailwind.config.js
│   │   │   └── Dockerfile
│   │   │
│   │   ├── DockerX.PublicSite/          # Public Website (Next.js)
│   │   │   ├── src/
│   │   │   │   ├── components/
│   │   │   │   │   ├── layout/
│   │   │   │   │   │   ├── Header.tsx
│   │   │   │   │   │   ├── Footer.tsx
│   │   │   │   │   │   └── Navigation.tsx
│   │   │   │   │   ├── gallery/          # Media gallery components
│   │   │   │   │   │   ├── Gallery.tsx
│   │   │   │   │   │   ├── ImageCard.tsx
│   │   │   │   │   │   └── ImageModal.tsx
│   │   │   │   │   └── common/
│   │   │   │   │       ├── Button.tsx
│   │   │   │   │       └── Loading.tsx
│   │   │   │   │
│   │   │   │   ├── pages/
│   │   │   │   │   ├── index.tsx         # Home page
│   │   │   │   │   ├── gallery.tsx       # Gallery page
│   │   │   │   │   └── about.tsx         # About page
│   │   │   │   │
│   │   │   │   ├── services/
│   │   │   │   │   └── publicApiService.ts # Public API calls
│   │   │   │   │
│   │   │   │   └── types/
│   │   │   │       └── public.ts
│   │   │   │
│   │   │   ├── package.json
│   │   │   ├── next.config.js
│   │   │   └── Dockerfile
│   │   │
│   │   └── DockerX.MobileApp/           # Mobile App (React Native/Flutter)
│   │       ├── src/
│   │       │   ├── components/
│   │       │   ├── screens/
│   │       │   ├── services/
│   │       │   ├── navigation/
│   │       │   └── store/
│   │       │
│   │       ├── package.json
│   │       └── Dockerfile
│   │
│   └── Shared/                           # Cross-cutting concerns
│       ├── DockerX.Shared.Common/        # Extensions, utilities
│       ├── DockerX.Shared.Security/      # JWT, Auth helpers
│       └── DockerX.Shared.Logging/       # Logging configuration
│
├── tests/                                # Can test ALL business logic
│   ├── UnitTests/
│   │   ├── DockerX.Application.Tests/    # Test all services
│   │   │   ├── Services/
│   │   │   │   ├── MediaServiceTests.cs
│   │   │   │   ├── UploadServiceTests.cs
│   │   │   │   └── AuthServiceTests.cs
│   │   │   └── Validators/
│   │   │       └── MediaValidatorTests.cs
│   │   │
│   │   ├── DockerX.Domain.Tests/         # Test domain logic
│   │   └── DockerX.Infrastructure.Tests/ # Test repositories
│   │
│   ├── IntegrationTests/
│   │   ├── DockerX.MediaApi.Tests/       # Test API endpoints
│   │   ├── DockerX.UploadApi.Tests/
│   │   └── DockerX.AdminApi.Tests/
│   │
│   ├── UITests/                          # UI Testing
│   │   ├── AdminPanel.Tests/             # Admin panel E2E tests
│   │   │   ├── __tests__/
│   │   │   │   ├── components/
│   │   │   │   └── pages/
│   │   │   ├── jest.config.js
│   │   │   └── package.json
│   │   │
│   │   ├── PublicSite.Tests/             # Public site E2E tests
│   │   └── MobileApp.Tests/              # Mobile app tests
│   │
│   └── E2ETests/
│       └── DockerX.E2ETests/             # Full system E2E tests
│
├── docker/
│   ├── docker-compose.yml               # All services
│   ├── docker-compose.dev.yml           # Development overrides
│   ├── docker-compose.test.yml          # Testing environment
│   └── nginx/                           # Reverse proxy for UIs
│       ├── nginx.conf
│       └── ssl/
│
├── scripts/
│   ├── build/
│   │   ├── build-apis.sh
│   │   ├── build-ui.sh
│   │   └── build-all.sh
│   │
│   ├── deploy/
│   │   ├── deploy-dev.sh
│   │   └── deploy-prod.sh
│   │
│   └── dev/
│       ├── start-dev.sh                 # Start all services for development
│       └── seed-data.sh
│
├── docs/
│   ├── api/                             # API documentation
│   ├── ui/                              # UI documentation
│   │   ├── admin-panel.md
│   │   ├── public-site.md
│   │   └── mobile-app.md
│   │
│   └── deployment/
│
└── DockerX.sln                         # .NET solution (APIs + Core only)