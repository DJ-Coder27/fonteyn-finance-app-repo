FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Build.targets", "./"]

COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["src/Services/Services.csproj", "src/Services/"]
COPY ["src/Dto/Dto.csproj", "src/Dto/"]

RUN dotnet restore "src/Api/Api.csproj"

COPY ["src/Api/", "src/Api/"]
COPY ["src/Core/", "src/Core/"]
COPY ["src/Services/", "src/Services/"]
COPY ["src/Dto/", "src/Dto/"]

RUN dotnet publish "src/Api/Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8001

EXPOSE 8001

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]