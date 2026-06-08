FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Build.targets", "./"]

COPY ["src/Dto/Dto.csproj", "src/Dto/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/AccountGoWeb/AccountGoWeb.csproj", "src/AccountGoWeb/"]

RUN dotnet restore "src/AccountGoWeb/AccountGoWeb.csproj"

COPY ["src/Dto/", "src/Dto/"]
COPY ["src/Infrastructure/", "src/Infrastructure/"]
COPY ["src/AccountGoWeb/", "src/AccountGoWeb/"]

ENV NODE_VERSION=19.8.1
ENV NODE_DOWNLOAD_SHA=77b47ce0ed17ef90c1df6ef1ca6ea0f1375346ae4d5099708d9ee39d805f9d6e

RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

WORKDIR /src/src/AccountGoWeb

RUN npm install --legacy-peer-deps
RUN npm rebuild node-sass

RUN ./node_modules/.bin/node-sass Scss/dark.scss wwwroot/css/dark.css \
    && ./node_modules/.bin/node-sass Scss/light.scss wwwroot/css/light.css

WORKDIR /src

RUN dotnet publish "src/AccountGoWeb/AccountGoWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV APIURL=http://localhost:8001/api/

EXPOSE 8000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AccountGoWeb.dll"]