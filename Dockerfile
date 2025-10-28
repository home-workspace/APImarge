# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY pdfmergeapi/*.csproj ./pdfmergeapi/
RUN dotnet restore ./pdfmergeapi/pdfmergeapi.csproj

# Copiar todo y compilar
COPY . .
WORKDIR /src/pdfmergeapi
RUN dotnet publish -c Release -o /app/out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Puerto
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "pdfmergeapi.dll"]
