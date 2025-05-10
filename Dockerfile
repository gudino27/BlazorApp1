# 1) Build your project and install Playwright
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
ARG CONFIG=Release
WORKDIR /src

# Copy csproj & restore to leverage layer cache
COPY ["BlazorApp1.csproj", "./"]
RUN dotnet restore "BlazorApp1.csproj"

# Copy all source files
COPY . .

# Install Playwright CLI and browser in the build stage
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet add package Microsoft.Playwright
RUN dotnet build "BlazorApp1.csproj" -c $CONFIG
# Install Playwright browsers
RUN playwright install chromium --with-deps

# Now publish the application
RUN dotnet publish "BlazorApp1.csproj" \
    -c $CONFIG \
    -o /app/publish \
    /p:UseAppHost=false

# 2) Runtime stage with browser dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install Playwright browser dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    wget \
    ca-certificates \
    fonts-liberation \
    libasound2 \
    libatk1.0-0 \
    libc6 \
    libcairo2 \
    libcups2 \
    libdbus-1-3 \
    libexpat1 \
    libfontconfig1 \
    libgbm1 \
    libgcc-s1 \
    libglib2.0-0 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libstdc++6 \
    libx11-6 \
    libx11-xcb1 \
    libxcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxi6 \
    libxrandr2 \
    libxrender1 \
    libxss1 \
    libxtst6 \
    lsb-release \
    xdg-utils \
    libdrm2 \
    libxshmfence1 \
    && rm -rf /var/lib/apt/lists/*

# Create necessary directories with proper permissions
WORKDIR /app
RUN mkdir -p /app/keys \
    && chmod -R 777 /app/keys \
    && mkdir -p /root/.cache/ms-playwright \
    && chmod -R 777 /root/.cache

# Copy published app from build stage
COPY --from=builder /app/publish .

# Copy Playwright browsers from build stage
COPY --from=builder /root/.cache/ms-playwright /root/.cache/ms-playwright

# Tell ASP.NET to bind to port 5000
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    # Playwright-specific settings
    PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright \
    PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1 \
    PLAYWRIGHT_HEADLESS_MODE=true

EXPOSE 5000

# Launch the app
ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
