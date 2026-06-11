# Built from the repository ROOT (this is what Render uses by default).
# Produces the EventosVivos .NET 8 API image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/ ./
RUN dotnet restore EventosVivos.sln
RUN dotnet publish src/EventosVivos.Api/EventosVivos.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Hosts like Render inject PORT; Program.cs binds to it. 8080 is the local default.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventosVivos.Api.dll"]
