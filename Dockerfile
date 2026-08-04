FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY VRSimulator.sln ./
COPY src/VRSimulator.Api/VRSimulator.Api.csproj src/VRSimulator.Api/
RUN dotnet restore src/VRSimulator.Api/VRSimulator.Api.csproj

COPY src/VRSimulator.Api/ src/VRSimulator.Api/
RUN dotnet publish src/VRSimulator.Api/VRSimulator.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:7860
ENV Database__Provider=PostgreSql
ENV Database__EnsureCreated=true
ENV Database__FallbackToInMemory=false
ENV Cors__AllowAnyOrigin=true

COPY --from=build /app/publish ./
COPY index.html pricing.html certificates.html login.html platform.html verify.html worker.html ./
COPY styles.css script.js ./
COPY assets ./assets

RUN mkdir -p wwwroot \
    && cp index.html pricing.html certificates.html login.html platform.html verify.html worker.html styles.css script.js wwwroot/ \
    && cp -r assets wwwroot/assets

EXPOSE 7860

ENTRYPOINT ["dotnet", "VRSimulator.Api.dll"]
