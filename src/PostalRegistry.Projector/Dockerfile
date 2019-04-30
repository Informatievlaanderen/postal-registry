FROM mcr.microsoft.com/dotnet/core/runtime-deps:2.2.4

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

EXPOSE 3006/tcp
ENV ASPNETCORE_URLS http://*:3006

ENTRYPOINT ["./PostalRegistry.Projector"]
