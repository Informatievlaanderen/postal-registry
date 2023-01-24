FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="postal-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 3002/tcp
ENV ASPNETCORE_URLS http://*:3002

ENTRYPOINT ["./init.sh"]
