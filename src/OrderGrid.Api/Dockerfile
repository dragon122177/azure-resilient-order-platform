FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore OrderGrid.slnx && dotnet publish src/OrderGrid.Api/OrderGrid.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN addgroup -S ordergrid && adduser -S ordergrid -G ordergrid
COPY --from=build --chown=ordergrid:ordergrid /app .
USER ordergrid
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderGrid.Api.dll"]
