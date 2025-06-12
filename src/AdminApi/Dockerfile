FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/AdminApi/AdminApi.csproj", "src/AdminApi/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/AdminApi/AdminApi.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "src/AdminApi/AdminApi.csproj" -c Release -o /app/build
RUN dotnet publish "src/AdminApi/AdminApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminApi.dll"] 