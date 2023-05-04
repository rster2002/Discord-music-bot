docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t docker.jumpdrive.dev/discord-bot-application:latest \
  --push ./DiscordbotTest7

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t docker.jumpdrive.dev/discord-bot-lavalink:latest \
  --push ./lavalink
