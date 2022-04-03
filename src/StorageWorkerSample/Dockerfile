#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the sample for hosting application inside of Kubernetes Cluster.
#
# NOTE:         Please run commands from root of the solution.
#
# BUILD:        docker build --rm -f "StorageSample/Dockerfile" -t app:StorageSample .
#
#
# NOTE:        See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#------------------------------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/runtime:6.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["StorageSample/StorageSample.csproj", "StorageSample/"]
RUN dotnet restore "StorageSample/StorageSample.csproj"
COPY . .
WORKDIR "/src/StorageSample"
RUN dotnet build "StorageSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StorageSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StorageSample.dll"]
