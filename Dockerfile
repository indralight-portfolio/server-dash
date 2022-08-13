FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ENV ASPNETCORE_ENVIRONMENT live
ENV APP_HOME /usr/app
ENV SERVICE lobby
EXPOSE 8081 8082

COPY app/ $APP_HOME/
WORKDIR $APP_HOME/

ENTRYPOINT ["sh", "-c"]
CMD ["./run-${SERVICE}.sh"]
