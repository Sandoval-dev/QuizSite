# 1. Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Solution dosyasını kopyala ve restore yap
COPY QuizSite.sln ./
COPY QuizSite/*.csproj ./QuizSite/
RUN dotnet restore

# Tüm kaynak kodu kopyala
COPY QuizSite/. ./QuizSite/
WORKDIR /src/QuizSite

# Release modunda publish et
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Build aşamasından publish edilen dosyaları al
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "QuizSite.dll"]
