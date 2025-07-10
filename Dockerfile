# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını ve solution'u kopyala
COPY *.csproj ./
COPY ../QuizSite.sln ../

# Restore yap
RUN dotnet restore ../QuizSite.sln

# Tüm kaynak kodu kopyala (şu anki klasör = /src)
COPY . ./

# Publish et
RUN dotnet publish -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QuizSite.dll"]
