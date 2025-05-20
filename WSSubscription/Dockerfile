# Etapa 1: Construir la aplicación
     FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
     WORKDIR /src
     COPY . .
     RUN dotnet restore
     RUN dotnet publish -c Release -o /app/publish --no-restore

     # Etapa 2: Crear la imagen final
     FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
     WORKDIR /app
     COPY --from=build /app/publish .
     EXPOSE 8080
     ENV ASPNETCORE_URLS=http://+:8080
     ENTRYPOINT ["dotnet", "WSSubscription.dll"]