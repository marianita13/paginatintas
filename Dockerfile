FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY paginatintas.sln ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Persistence/Persistence.csproj Persistence/
COPY API.Tests/API.Tests.csproj API.Tests/
RUN dotnet restore API/API.csproj

COPY API API
COPY Application Application
COPY Domain Domain
COPY Persistence Persistence
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
COPY Frontend ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
