# Usa la imagen oficial de .NET SDK para construir
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo del proyecto y restaura dependencias
COPY ["PDFMergerAPI.csproj", "./"]
RUN dotnet restore "PDFMergerAPI.csproj"

# Copia el resto del código y construye
COPY . .
RUN dotnet build "PDFMergerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PDFMergerAPI.csproj" -c Release -o /app/publish

# Imagen final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render usa el puerto 10000 por defecto
ENV ASPNETCORE_URLS=http://*:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "PDFMergerAPI.dll"]