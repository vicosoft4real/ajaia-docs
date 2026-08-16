# Build the React bundle separately so the final image only contains published API assets.
FROM node:22-alpine AS web-build
WORKDIR /src

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY web/package.json ./web/package.json
RUN corepack enable && pnpm install --frozen-lockfile

COPY web ./web
RUN pnpm --dir web build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY AjaiaDocs.sln Directory.Build.props Directory.Packages.props global.json ./
COPY src/AjaiaDocs.Core/AjaiaDocs.Core.csproj src/AjaiaDocs.Core/
COPY src/AjaiaDocs.Application/AjaiaDocs.Application.csproj src/AjaiaDocs.Application/
COPY src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj src/AjaiaDocs.Infrastructure/
COPY src/AjaiaDocs.Api/AjaiaDocs.Api.csproj src/AjaiaDocs.Api/
RUN dotnet restore src/AjaiaDocs.Api/AjaiaDocs.Api.csproj

COPY src ./src
RUN dotnet publish src/AjaiaDocs.Api/AjaiaDocs.Api.csproj --configuration Release --no-restore --output /app/publish
COPY --from=web-build /src/web/dist /app/publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The .NET runtime image supplies the non-root app identity through APP_UID.
COPY --chown=$APP_UID:$APP_UID --from=api-build /app/publish ./
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render provides PORT at runtime; Docker Compose falls back to 8080 for local use.
ENTRYPOINT ["sh", "-c", "exec dotnet AjaiaDocs.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
