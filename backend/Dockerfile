# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so dependency layers cache between builds
COPY src/MovieReview.Api/MovieReview.Api.csproj src/MovieReview.Api/
RUN dotnet restore src/MovieReview.Api/MovieReview.Api.csproj

COPY src/ src/
RUN dotnet publish src/MovieReview.Api/MovieReview.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render injects PORT; default to 8080 for local docker runs
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet MovieReview.Api.dll
