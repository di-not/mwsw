FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MWSW.Backend.csproj ./
RUN dotnet restore ./MWSW.Backend.csproj

COPY . .
RUN dotnet publish ./MWSW.Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "dotnet MWSW.Backend.dll --urls http://0.0.0.0:${PORT:-8080}"]
