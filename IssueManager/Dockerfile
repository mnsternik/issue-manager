# Stage 1: Build the application using the SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["IssueManager.csproj", "./"]
RUN dotnet restore "./IssueManager.csproj"

# Copy the remaining files and build the project
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# HTTPS
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Copy published files from the build stage
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "IssueManager.dll"]