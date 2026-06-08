FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BusStop.Web/BusStop.Web.csproj", "src/BusStop.Web/"]
COPY ["src/BusStop.Core/BusStop.Core.csproj", "src/BusStop.Core/"]
COPY ["src/BusStop.Infrastructure/BusStop.Infrastructure.csproj", "src/BusStop.Infrastructure/"]
COPY ["src/BusStop.UseCases/BusStop.UseCases.csproj", "src/BusStop.UseCases/"]
COPY ["src/BusStop.ServiceDefaults/BusStop.ServiceDefaults.csproj", "src/BusStop.ServiceDefaults/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]

RUN dotnet restore "src/BusStop.Web/BusStop.Web.csproj"

COPY . .
RUN dotnet build "src/BusStop.Web/BusStop.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/BusStop.Web/BusStop.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BusStop.Web.dll"]
