FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.1

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="postal-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 3004/tcp
ENV ASPNETCORE_URLS http://*:3004

ENTRYPOINT ["./init.sh"]
