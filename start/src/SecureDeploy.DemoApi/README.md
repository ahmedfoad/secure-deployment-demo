## SecureDeploy.DemoApi

A .NET 10 minimal API demonstrating secure deployment practices.

### Features
- Health check endpoint (`/health`)
- Weather forecast API endpoint (`/api/weatherforecast`)
- Structured logging with Serilog
- Dependency on shared library for real-world scenario

### Security Notes
- **Logging**: Sensitive data (tokens, passwords, PII) must never be logged
- **Environment Variables**: Use secure secret management in CI/CD (GitHub Secrets)
- **Container**: See [Dockerfile](../../Dockerfile) for hardening practices
- **Dependencies**: See `.github/workflows/ci.yml` for Software Composition Analysis (SCA) gates

### Running Locally
```bash
dotnet run --project src/SecureDeploy.DemoApi
curl https://localhost:5001/health
curl https://localhost:5001/
curl https://localhost:5001/api/weatherforecast
```

### Docker
```bash
# Build using secure Dockerfile
docker build -t secure-deploy:latest -f Dockerfile .

# Run (exposes port 8080 for non-root user)
docker run -p 8080:8080 secure-deploy:latest
```
