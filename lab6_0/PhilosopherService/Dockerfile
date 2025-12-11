FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PhilosopherService/PhilosopherService.csproj", "PhilosopherService/"]
COPY ["DiningPhilosophers/src/DiningPhilosophers.Core/DiningPhilosophers.Core.csproj", "DiningPhilosophers/src/DiningPhilosophers.Core/"]
COPY ["DiningPhilosophers/src/DiningPhilosophers.Strategies/DiningPhilosophers.Strategies.csproj", "DiningPhilosophers/src/DiningPhilosophers.Strategies/"]
RUN dotnet restore "PhilosopherService/PhilosopherService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PhilosopherService/PhilosopherService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhilosopherService/PhilosopherService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhilosopherService.dll"]

