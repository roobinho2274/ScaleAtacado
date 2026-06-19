FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ScaleAtacado.Blazor/ScaleAtacado.Blazor.csproj",           "ScaleAtacado.Blazor/"]
COPY ["ScaleAtacado.Application/ScaleAtacado.Application.csproj", "ScaleAtacado.Application/"]
COPY ["ScaleAtacado.Domain/ScaleAtacado.Domain.csproj",           "ScaleAtacado.Domain/"]
COPY ["ScaleAtacado.Shared/ScaleAtacado.Shared.csproj",           "ScaleAtacado.Shared/"]
RUN dotnet restore "ScaleAtacado.Blazor/ScaleAtacado.Blazor.csproj"

COPY . .
WORKDIR "/src/ScaleAtacado.Blazor"
RUN dotnet publish "ScaleAtacado.Blazor.csproj" -c Release -o /app/publish

# Em produção o Blazor usa HostEnvironment.BaseAddress (URL do nginx)
# Removemos o ApiUrl do appsettings para forçar esse fallback
RUN echo '{}' > /app/publish/wwwroot/appsettings.json

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY ScaleAtacado.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
