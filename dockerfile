# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY Zello.sln ./
COPY src/Zello.Api/Zello.Api.csproj ./src/Zello.Api/
RUN dotnet restore src/Zello.Api

# Copy the rest of the files and publish the app
COPY . .
RUN dotnet publish src/Zello.Api -c Release -o /publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /publish .

# Expose port 80
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "Zello.Api.dll"]