echo tag:
read -r TAG

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t docker.jumpdrive.dev/discord-bot-application:$TAG \
  --push ./DiscordbotTest7

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t docker.jumpdrive.dev/discord-bot-lavalink:$TAG \
  --push ./lavalink
