FROM mcr.microsoft.com/dotnet/runtime:3.1-bullseye-slim-arm64v8 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ExpensesTelegramBot.csproj", "./"]
RUN dotnet restore "ExpensesTelegramBot.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ExpensesTelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExpensesTelegramBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# create directory for csv files
RUN mkdir data 
ENTRYPOINT ["dotnet", "ExpensesTelegramBot.dll"]