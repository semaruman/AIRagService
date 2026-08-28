FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AIRagService.Api/AIRagService.Api.csproj", "src/AIRagService.Api/"]
COPY ["src/AIRagService.Application/AIRagService.Application.csproj", "src/AIRagService.Application/"]
COPY ["src/AIRagService.Domain/AIRagService.Domain.csproj", "src/AIRagService.Domain/"]
COPY ["src/AIRagService.Infrastructure/AIRagService.Infrastructure.csproj", "src/AIRagService.Infrastructure/"]

RUN dotnet restore "src/AIRagService.Api/AIRagService.Api.csproj"

COPY . .
WORKDIR "/src/src/AIRagService.Api"
RUN dotnet publish "AIRagService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AIRagService.Api.dll"]
