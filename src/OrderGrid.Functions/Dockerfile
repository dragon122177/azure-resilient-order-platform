FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore OrderGrid.slnx && dotnet publish src/OrderGrid.Functions/OrderGrid.Functions.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
COPY --from=build /app /home/site/wwwroot
