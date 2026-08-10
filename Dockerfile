FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY VRAcademy.sln ./
COPY src/VRAcademy.Api/VRAcademy.Api.csproj src/VRAcademy.Api/
RUN dotnet restore src/VRAcademy.Api/VRAcademy.Api.csproj

COPY src/VRAcademy.Api/ src/VRAcademy.Api/
RUN dotnet publish src/VRAcademy.Api/VRAcademy.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:7860
ENV Database__Provider=PostgreSql
ENV Database__EnsureCreated=true
ENV Database__FallbackToInMemory=false
ENV Cors__AllowAnyOrigin=true

COPY --from=build /app/publish ./
COPY index.html pricing.html certificates.html certificate-view.html login.html platform.html system-admin.html verify.html worker.html ./
COPY styles.css script.js ./
COPY assets ./assets

RUN mkdir -p wwwroot \
    && cp index.html pricing.html certificates.html certificate-view.html login.html platform.html system-admin.html verify.html worker.html styles.css script.js wwwroot/ \
    && cp -r assets wwwroot/assets

EXPOSE 7860

ENTRYPOINT ["dotnet", "VRAcademy.Api.dll"]
