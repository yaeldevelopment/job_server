# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Set the environment variable to use a custom NuGet package directory
ENV NUGET_PACKAGES=/root/.nuget/packages

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Publish the application
RUN dotnet publish -c Release -o /app/publish
# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output
COPY --from=build /app/publish .

# Set environment variable for ASP.NET Core URLs
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "server.dll"]
# Expose port 8080
EXPOSE 8080
EXPOSE 80
EXPOSE 443
# Run the application
