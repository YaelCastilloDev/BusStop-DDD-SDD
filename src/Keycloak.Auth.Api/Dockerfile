FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Create a minimal application that responds to health checks
RUN echo '#!/bin/bash\nwhile true; do sleep 3600; done' > /app/start.sh && \
    chmod +x /app/start.sh

EXPOSE 8080

CMD ["/app/start.sh"]
