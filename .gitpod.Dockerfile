FROM mcr.microsoft.com/dotnet/sdk:6.0

USER root

# Install Node.js for building frontend assets
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash -
RUN apt-get install -y nodejs

USER gitpod