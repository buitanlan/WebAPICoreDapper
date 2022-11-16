#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:7.0 AS build
WORKDIR /src
COPY ["WebAPICoreDapper/WebAPICoreDapper.csproj", "WebAPICoreDapper/"]
COPY ["WebAPICoreDapper.Data/WebAPICoreDapper.Data.csproj", "WebAPICoreDapper.Data/"]
COPY ["WebAPICoreDapper.Utilities/WebAPICoreDapper.Utilities.csproj", "WebAPICoreDapper.Utilities/"]
RUN dotnet restore "WebAPICoreDapper/WebAPICoreDapper.csproj"
COPY . .
WORKDIR "/src/WebAPICoreDapper"
RUN dotnet build "WebAPICoreDapper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPICoreDapper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPICoreDapper.dll"]