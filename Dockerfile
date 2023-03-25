# Use the official .NET 6 image as the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory
WORKDIR /src

# Copy the .csproj file and restore the dependencies
COPY ["App Parking System/App Parking System.csproj", "App Parking System/"]
RUN dotnet restore "App Parking System/App Parking System.csproj"

# Copy the rest of the source code
COPY . .

# Build the application in release mode
RUN dotnet build "App Parking System/App Parking System.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "App Parking System/App Parking System.csproj" -c Release -o /app/publish

# Use the .NET 6 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Set the working directory
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Expose the application on port 80
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "App Parking System.dll"]