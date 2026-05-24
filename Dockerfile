FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/FaqApp.Api/FaqApp.Api.csproj backend/FaqApp.Api/
RUN dotnet restore backend/FaqApp.Api/FaqApp.Api.csproj

COPY backend/FaqApp.Api/ backend/FaqApp.Api/

WORKDIR /src/backend/FaqApp.Api
RUN dotnet publish FaqApp.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

CMD dotnet FaqApp.Api.dll --urls http://0.0.0.0:$PORT