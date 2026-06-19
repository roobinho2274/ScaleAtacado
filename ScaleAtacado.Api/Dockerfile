FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ScaleAtacado.Api/ScaleAtacado.Api.csproj",                   "ScaleAtacado.Api/"]
COPY ["ScaleAtacado.Application/ScaleAtacado.Application.csproj",   "ScaleAtacado.Application/"]
COPY ["ScaleAtacado.Domain/ScaleAtacado.Domain.csproj",             "ScaleAtacado.Domain/"]
COPY ["ScaleAtacado.Infrastructure/ScaleAtacado.Infrastructure.csproj", "ScaleAtacado.Infrastructure/"]
COPY ["ScaleAtacado.Shared/ScaleAtacado.Shared.csproj",             "ScaleAtacado.Shared/"]
RUN dotnet restore "ScaleAtacado.Api/ScaleAtacado.Api.csproj"

COPY . .
WORKDIR "/src/ScaleAtacado.Api"
RUN dotnet publish "ScaleAtacado.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ScaleAtacado.Api.dll"]
