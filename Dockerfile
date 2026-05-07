FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვაკოპირებთ პროექტს და ვაკეთებთ restore-ს
COPY ["SCORE.csproj", "./"]
RUN dotnet restore "SCORE.csproj"

# ვაკოპირებთ ყველაფერს (Data, Services და ა.შ.)
COPY . .
RUN dotnet publish "SCORE.csproj" -c Release -o /app/out

# Runtime ეტაპი
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# პორტის გარემო ცვლადი
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SCORE.dll"]
