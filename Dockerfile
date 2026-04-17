FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TranHuuPhuoc_2123110236.sln ./
COPY TranHuuPhuoc_2123110236/TranHuuPhuoc_2123110236.csproj TranHuuPhuoc_2123110236/

RUN dotnet restore TranHuuPhuoc_2123110236.sln

COPY TranHuuPhuoc_2123110236/ TranHuuPhuoc_2123110236/

RUN dotnet publish TranHuuPhuoc_2123110236/TranHuuPhuoc_2123110236.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80

ENTRYPOINT ["dotnet", "TranHuuPhuoc_2123110236.dll"]
