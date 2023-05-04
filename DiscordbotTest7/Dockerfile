#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DiscordbotTest7.csproj", "DiscordbotTest7.csproj"]
RUN dotnet restore "DiscordbotTest7.csproj" # -r linux-arm
COPY . .
RUN dotnet build "DiscordbotTest7.csproj" -c Release -o /app/build # -r linux-arm

FROM build AS publish
RUN dotnet publish "DiscordbotTest7.csproj" -c Release -o /app/publish --self-contained -r linux-x64 # -r linux-arm

FROM ubuntu AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./runtimeconfig.json /app/DiscordbotTest7.runtimeconfig.json
RUN apt-get update -y
RUN apt-get install less -y
CMD ["tail", "-n", "1", "-f", "/dev/null"]
