FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base-sdk
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base-runtime

FROM base-sdk AS base-sdk-arm64
ARG TARGET=linux-arm

FROM base-sdk AS base-sdk-amd64
ARG TARGET=linux-x64

FROM base-runtime AS base-runtime-arm64
ARG TARGET=linux-arm

FROM base-runtime AS base-runtime-amd64
ARG TARGET=linux-x64

ARG TARGETARCH
FROM base-sdk-$TARGETARCH AS restore
WORKDIR /src
COPY . .
RUN dotnet restore "DiscordbotTest7.csproj"

FROM base-runtime-$TARGETARCH AS build
COPY --from=restore . .
WORKDIR /src
RUN dotnet build "DiscordbotTest7.csproj" -c Release -o /app/build -r $TARGET

FROM build AS publish
RUN dotnet publish "DiscordbotTest7.csproj" -c Release -o /app/publish --self-contained -r $TARGET

FROM ubuntu AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./runtimeconfig.json /app/DiscordbotTest7.runtimeconfig.json
#RUN apt-get update -y
#CMD ["tail", "-n", "1", "-f", "/dev/null"]
CMD ["./DiscordbotTest7"]
