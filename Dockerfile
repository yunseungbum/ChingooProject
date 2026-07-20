FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Chingoo/Chingoo.csproj", "Chingoo/"]
RUN dotnet restore "Chingoo/Chingoo.csproj"

COPY . .
WORKDIR "/src/Chingoo"

RUN dotnet publish "Chingoo.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Chingoo.dll"]