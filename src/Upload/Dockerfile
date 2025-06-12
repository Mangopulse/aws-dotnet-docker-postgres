FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
COPY ["src/Upload/Upload.csproj", "src/Upload/"]
RUN dotnet restore "src/Upload/Upload.csproj"
COPY . .
WORKDIR "/src/src/Upload"
RUN dotnet build "Upload.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Upload.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Copy configuration from root
COPY ["config.json", "/app/"]

ENTRYPOINT ["dotnet", "Upload.dll"] 