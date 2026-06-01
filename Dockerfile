FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY SistemaBCSmartPoint/Sistema_BC_SMART_POINT.csproj ./SistemaBCSmartPoint/
COPY TestProject1/TestProject1.csproj ./TestProject1/
RUN dotnet restore

COPY . .
RUN dotnet publish SistemaBCSmartPoint/Sistema_BC_SMART_POINT.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Sistema_BC_SMART_POINT.dll"]