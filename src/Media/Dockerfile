FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install ImageFlow dependencies
RUN apt-get update && apt-get install -y \
    libc6-dev \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Media/Media.csproj", "src/Media/"]
RUN dotnet restore "src/Media/Media.csproj"
COPY . .
WORKDIR "/src/src/Media"
RUN dotnet build "Media.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Media.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Copy configuration from root
COPY ["config.json", "/app/"]

ENTRYPOINT ["dotnet", "Media.dll"] 