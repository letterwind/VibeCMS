# 使用 .NET 10 SDK 作為建置映像
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 複製專案檔案並還原套件
COPY ["src/WebCMS.Api/WebCMS.Api.csproj", "WebCMS.Api/"]
COPY ["src/WebCMS.Core/WebCMS.Core.csproj", "WebCMS.Core/"]
COPY ["src/WebCMS.Infrastructure/WebCMS.Infrastructure.csproj", "WebCMS.Infrastructure/"]
RUN dotnet restore "WebCMS.Api/WebCMS.Api.csproj"

# 複製所有原始碼並建置
COPY src/ .
WORKDIR "/src/WebCMS.Api"
RUN dotnet build "WebCMS.Api.csproj" -c Release -o /app/build

# 發布應用程式
FROM build AS publish
RUN dotnet publish "WebCMS.Api.csproj" -c Release -o /app/publish

# 使用 ASP.NET Core Runtime 作為執行映像
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5000

# 複製發布的檔案
COPY --from=publish /app/publish .

# 安裝 EF Core 工具用於資料庫遷移
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# 啟動應用程式
ENTRYPOINT ["dotnet", "WebCMS.Api.dll"]
