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
ENV Database__Provider=SqlServer
ENV Database__EnsureCreated=false
ENV Database__ApplyMigrations=false
ENV Database__FallbackToInMemory=false
ENV Cors__AllowAnyOrigin=false

COPY --from=build /app/publish ./
COPY index.html pricing.html certificates.html certificate-view.html login.html platform.html platform-workers.html platform-training.html platform-assigned-training.html platform-certificates.html platform-account.html system-admin.html system-company.html verify.html worker.html training.html check.html ./
COPY styles.css script.js global-languages.js ./
COPY assets ./assets

RUN mkdir -p wwwroot \
    && cp index.html pricing.html certificates.html certificate-view.html login.html platform.html platform-workers.html platform-training.html platform-assigned-training.html platform-certificates.html platform-account.html system-admin.html system-company.html verify.html worker.html training.html check.html styles.css script.js global-languages.js wwwroot/ \
    && cp -r assets wwwroot/assets

EXPOSE 7860

ENTRYPOINT ["dotnet", "VRAcademy.Api.dll"]
