# 1. აწყობის ეტაპი (Build)
# 1. აწყობის ეტაპი (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ჯერ მხოლოდ პროექტის ფაილს ვაკოპირებთ
COPY ["SCORE.csproj", "./"]
RUN dotnet restore "./SCORE.csproj"

# ახლა ვაკოპირებთ აბსოლუტურად ყველაფერს
COPY . .

# ვამატებთ build ბრძანებას, რომ დარწმუნდეთ Namespace-ებში
RUN dotnet build "SCORE.csproj" -c Release -o /app/build

# ვაკეთებთ ფინალურ publish-ს
RUN dotnet publish "SCORE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. გაშვების ეტაპი (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# პორტები
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SCORE.dll"]
