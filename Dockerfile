# ----- BUILD STAGE -----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BlogShare.Web.sln .
COPY BlogShare.Web/*.csproj ./BlogShare.Web/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the files
COPY . .

# Build the project
WORKDIR /src/BlogShare.Web
RUN dotnet publish -c Release -o /app/publish

# ----- RUNTIME STAGE -----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose default port
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "BlogShare.Web.dll"]
