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

RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*
ENV TZ=America/Lima

COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sistema_BC_SMART_POINT.dll"]
