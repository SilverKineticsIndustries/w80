FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
EXPOSE 80
WORKDIR /api

COPY . ./

WORKDIR /api/controller
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /api
COPY --from=build-env /api/controller/out .
ENTRYPOINT ["dotnet", "w80.Controller.dll"]

