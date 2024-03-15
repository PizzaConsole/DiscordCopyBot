# Use the official .NET SDK image with version 6.0 as the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory in the container
WORKDIR /var/www/poisncopy

# Copy only the necessary files for restoring dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the entire project and build it
COPY . .
RUN dotnet build --configuration Release

# Publish the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image with version 6.0 as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory in the container
WORKDIR /var/www/poisncopy

# Copy the published application to the container
COPY --from=build /var/www/poisncopy/out .

# Start the application
ENTRYPOINT ["/usr/bin/dotnet", "/var/www/poisncopy/PoisnCopy.dll"]




# How to build and run the Docker image

# 1. Build the Docker image
# docker build -t poisncopy:latest .

# 2. Run the Docker image
# docker run poisncopy:latest

# 3. Enjoy !!!