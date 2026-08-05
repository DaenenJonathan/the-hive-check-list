FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Domain/TheHive.Domain.csproj src/Domain/
COPY src/Application/TheHive.Application.csproj src/Application/
COPY src/Infrastructure/TheHive.Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/TheHive.WebApi.csproj src/WebApi/
RUN dotnet restore src/WebApi/TheHive.WebApi.csproj

COPY src/Domain/ src/Domain/
COPY src/Application/ src/Application/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/WebApi/ src/WebApi/
RUN dotnet publish src/WebApi/TheHive.WebApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app .

RUN mkdir -p /app/wwwroot/images/items \
    && chown -R app:app /app/wwwroot
USER app

ENTRYPOINT ["dotnet", "TheHive.WebApi.dll"]
