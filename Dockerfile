FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

# Install node
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash
RUN apt-get update && apt-get install -y nodejs
RUN npm install yarn --global
RUN yarn --version

WORKDIR /workspace
COPY . .
RUN dotnet --list-sdks
RUN dotnet tool restore
RUN dotnet run Publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0.7-bullseye-slim-arm32v7
#FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=build /workspace/publish/app /app
WORKDIR /app
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Alexandria.Server.dll" ]
