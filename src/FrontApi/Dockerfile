FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/FrontApi/FrontApi.csproj", "src/FrontApi/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/FrontApi/FrontApi.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "src/FrontApi/FrontApi.csproj" -c Release -o /app/build
RUN dotnet publish "src/FrontApi/FrontApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FrontApi.dll"] 