# ============================================================================
# SECURE DOCKERFILE - Production-Ready with Security Best Practices
# This Dockerfile demonstrates all security hardening techniques
# ============================================================================

# Stage 1: Build
# ✓ Use specific SDK version (not latest)
# Note: In real organizations, pin patch versions too (e.g., 10.0.1)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# ✓ Copy project files first (layer caching)
COPY ["src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj", "SecureDeploy.DemoApi/"]
COPY ["src/SecureDeploy.Shared/SecureDeploy.Shared.csproj", "SecureDeploy.Shared/"]

# ✓ Restore dependencies
RUN dotnet restore "SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj"

# ✓ Copy source code after restore (better layer caching)
COPY ["src/", "."]

# ✓ Build application
WORKDIR "/src/SecureDeploy.DemoApi"
RUN dotnet build "SecureDeploy.DemoApi.csproj" -c Release -o /app/build

# ============================================================================

# Stage 2: Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish

WORKDIR /src

COPY ["src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj", "SecureDeploy.DemoApi/"]
COPY ["src/SecureDeploy.Shared/SecureDeploy.Shared.csproj", "SecureDeploy.Shared/"]

RUN dotnet restore "SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj"

COPY ["src/", "."]

WORKDIR "/src/SecureDeploy.DemoApi"
RUN dotnet publish "SecureDeploy.DemoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================================================

# Stage 3: Runtime
# ✓ Use specific version (not latest)
# ✓ Use smaller aspnet image (not sdk)
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# ✓ Set non-root user for running application
# This prevents container breakouts from escalating to host
RUN groupadd -r appuser && useradd -r -g appuser appuser

WORKDIR /app

# ✓ Copy published artifacts from build stage
COPY --from=publish /app/publish .

# ✓ Change ownership to non-root user
RUN chown -R appuser:appuser /app

# ✓ Switch to non-root user BEFORE exposing ports
USER appuser

# ✓ Expose specific port (non-privileged, >1024)
# Application should be configured to listen on 8080
EXPOSE 8080

# ✓ Set explicit ENTRYPOINT (harder to override)
# This ensures the container always runs the app, preventing shell escape
ENTRYPOINT ["dotnet", "SecureDeploy.DemoApi.dll"]

# ============================================================================
# SECURITY SUMMARY
# ✓ Multi-stage build: reduces final image size
# ✓ Non-root user: limits blast radius if container is compromised
# ✓ Pinned versions: reproducible builds, no surprise updates
# ✓ Non-privileged port: cannot bind to system ports without elevated privileges
# ✓ Proper layer caching: efficient rebuilds
# ============================================================================
