FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-amd64 AS base
WORKDIR /app
EXPOSE 5224

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine-amd64 AS build
WORKDIR /src
COPY ["authgateway-bff.csproj", "./"]
RUN dotnet restore "authgateway-bff.csproj"
COPY . .
RUN dotnet build "authgateway-bff.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "authgateway-bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:5224
ENTRYPOINT ["dotnet", "authgateway-bff.dll"]
