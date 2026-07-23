FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/DotnetAuditDemo.Api/DotnetAuditDemo.Api.csproj", "src/DotnetAuditDemo.Api/"]
COPY ["src/DotnetAuditDemo.Application/DotnetAuditDemo.Application.csproj", "src/DotnetAuditDemo.Application/"]
COPY ["src/DotnetAuditDemo.Domain/DotnetAuditDemo.Domain.csproj", "src/DotnetAuditDemo.Domain/"]
COPY ["src/DotnetAuditDemo.Infrastructure/DotnetAuditDemo.Infrastructure.csproj", "src/DotnetAuditDemo.Infrastructure/"]
RUN dotnet restore "src/DotnetAuditDemo.Api/DotnetAuditDemo.Api.csproj"

COPY . .
RUN dotnet publish "src/DotnetAuditDemo.Api/DotnetAuditDemo.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DotnetAuditDemo.Api.dll"]
