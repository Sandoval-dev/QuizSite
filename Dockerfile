# BUILD aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Öncelikle csproj dosyalarını kopyalayıp restore ediyoruz (cache avantajı için)
COPY *.csproj ./
RUN dotnet restore

# Sonra tüm dosyaları kopyalıyoruz
COPY . ./

# Projeyi release modunda yayınlıyoruz
RUN dotnet publish -c Release -o /out

# RUNTIME aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Yayınlanan dosyaları build aşamasından alıyoruz
COPY --from=build /out .

# Uygulamayı başlatıyoruz
ENTRYPOINT ["dotnet", "QuizSite.dll"]
