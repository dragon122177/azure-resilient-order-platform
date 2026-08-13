FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore OrderGrid.slnx && dotnet publish src/OrderGrid.Worker/OrderGrid.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS runtime
WORKDIR /app
RUN addgroup -S ordergrid && adduser -S ordergrid -G ordergrid
COPY --from=build --chown=ordergrid:ordergrid /app .
USER ordergrid
ENTRYPOINT ["dotnet", "OrderGrid.Worker.dll"]
