FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/API/API.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "src/API/API.csproj" -c Release -o /app/build
RUN dotnet publish "src/API/API.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"] 