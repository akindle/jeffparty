FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Jeffparty.Interfaces/Jeffparty.Interfaces.csproj Jeffparty.Interfaces/
COPY Jeffparty.Web/Jeffparty.Web.csproj Jeffparty.Web/
COPY Jeffparty.Server/Jeffparty.Server.csproj Jeffparty.Server/
RUN dotnet restore Jeffparty.Server/Jeffparty.Server.csproj

COPY Jeffparty.Interfaces/ Jeffparty.Interfaces/
COPY Jeffparty.Web/ Jeffparty.Web/
COPY Jeffparty.Server/ Jeffparty.Server/
RUN dotnet publish Jeffparty.Server/Jeffparty.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Jeffparty.Server.dll"]
