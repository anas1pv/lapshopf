# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all files to build container
COPY . .

# Automatically locate the solution file and restore dependencies
RUN dotnet restore $(find . -name "*.sln" -o -name "*.slnx" | head -n 1)

# Automatically locate lapshop.csproj and publish the release build
RUN dotnet publish $(find . -name "lapshop.csproj" | head -n 1) -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose standard port for Render deployment
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "lapshop.dll"]
