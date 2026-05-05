# 1. აწყობის ეტაპი (Build)
# 1. აწყობის ეტაპი (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვაკოპირებთ .csproj-ს და ვაკეთებთ restore-ს
COPY ["SCORE.csproj", "./"]
RUN dotnet restore "SCORE.csproj"

# ვაკოპირებთ ყველაფერს და ვაკეთებთ publish-ს
COPY . .
RUN dotnet publish "SCORE.csproj" -c Release -o /app/out

# 2. გაშვების ეტაპი (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# პორტის კონფიგურაცია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SCORE.dll"]
