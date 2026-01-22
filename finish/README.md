# Secure Deployment Demo - .NET 10 Application

A comprehensive demonstration repository for the Udemy course section: **"Secure Automated Deployments with GitHub"**

This repository demonstrates three core security practices:
1. **GitHub Repo Setup for Secure CI/CD** - Branch protection, permissions, environments
2. **Software Composition Analysis (SCA)** - Detecting and preventing vulnerable dependencies
3. **Secure Container Development** - Hardened Docker builds with vulnerability scanning

## Quick Start

### Prerequisites
- .NET 10 SDK: `dotnet --version`
- Docker: `docker --version`
- Git: `git --version`

### Build & Test Locally
```bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/SecureDeploy.DemoApi

# Test endpoints
curl https://localhost:5001/health
curl https://localhost:5001/api/weatherforecast
```

### Build & Run Docker Container
```bash
# Build hardened container (see Dockerfile)
docker build -t secure-deploy:latest .

# Run container (listens on port 8080, non-root user)
docker run -p 8080:8080 secure-deploy:latest

# Test
curl http://localhost:8080/health
```

## Repository Structure

```
secure-deployment-demo/
├── src/
│   ├── SecureDeploy.DemoApi/          # ASP.NET Core Web API
│   │   ├── Program.cs                 # Minimal APIs with health + demo endpoints
│   │   ├── SecureDeploy.DemoApi.csproj
│   │   └── README.md                  # API documentation
│   └── SecureDeploy.Shared/           # Shared models & utilities
│       ├── WeatherForecast.cs         # Demo model
│       ├── DemoMessageBuilder.cs      # Helper utilities
│       └── SecureDeploy.Shared.csproj
├── tests/
│   └── SecureDeploy.DemoApi.Tests/    # xUnit tests
│       ├── ApiEndpointsTests.cs       # Health + endpoint tests
│       └── SecureDeploy.DemoApi.Tests.csproj
├── .github/
│   ├── workflows/
│   │   ├── ci.yml                     # Build + SCA gate
│   │   ├── dependency-review.yml      # PR dependency check
│   │   └── container-release.yml      # Container scan + push to GHCR
│   └── dependabot.yml                 # Automated dependency updates
├── docs/
│   ├── SECTION-10-RUNBOOK.md          # Complete demo instructions
│   └── INSECURE-NOTES.md              # Security issues explained
├── Dockerfile                         # Hardened multi-stage build
├── Dockerfile.insecure                # Intentionally insecure (for demo)
├── SecureDeploy.sln                   # Solution file
└── README.md                          # This file

```

## Key Features

### 1. Secure Application Code
- ✅ Minimal APIs with health checks
- ✅ Structured logging (Serilog) - never logs sensitive data
- ✅ Dependency on shared library (realistic scenario)
- ✅ Proper error handling

### 2. Security Gates in CI/CD

#### SCA Gate (CI Workflow)
- Scans dependencies with `dotnet list package --vulnerable`
- Fails build if vulnerabilities detected
- Prevents insecure code from merging

#### Dependency Review Gate (PR Workflow)
- Uses GitHub's `actions/dependency-review-action`
- Checks against GitHub's advisory database
- Flags new vulnerabilities in PRs

#### Container Scan Gate (Release Workflow)
- Scans image with Trivy (OS + library vulnerabilities)
- Fails on HIGH/CRITICAL severity
- Only publishes to GHCR if scan passes

### 3. Hardened Dockerfile
- ✅ Multi-stage build (smaller final image)
- ✅ Non-root user (isolation)
- ✅ Pinned base image versions (reproducibility)
- ✅ Minimal runtime image (reduced attack surface)
- ✅ Listens on non-privileged port 8080

### 4. Insecure Assets (for demonstration)
- `Dockerfile.insecure` - Shows common security anti-patterns
- `docs/INSECURE-NOTES.md` - Explains why each issue is risky

## GitHub Workflows

### ci.yml - Build & Test with SCA Gate
```bash
# Triggered on: push to main/develop, all PRs
# Steps:
#  1. Checkout code
#  2. Setup .NET 10
#  3. Restore dependencies
#  4. Build solution
#  5. Run tests
#  6. SCA Gate: dotnet list package --vulnerable
# Permissions: read code, write checks
```

### dependency-review.yml - PR Dependency Review
```bash
# Triggered on: PR against main/develop
# Uses: actions/dependency-review-action v4
# Checks: GitHub's vulnerability advisory database
# Fails on: HIGH, CRITICAL severity
# Permissions: read code only
```

### container-release.yml - Build, Scan, Push
```bash
# Triggered on: push to main (src changes)
# Steps:
#  1. Checkout code
#  2. Setup Docker buildx
#  3. Login to GHCR
#  4. Build Docker image
#  5. Scan with Trivy (fail on HIGH/CRITICAL)
#  6. Conditional push to ghcr.io (only if scan passed)
# Permissions: read code, write packages
```

### dependabot.yml - Automated Updates
```bash
# Weekly NuGet dependency updates
# Creates PRs for new versions available
# Triggers ci.yml and dependency-review.yml automatically
# Configurable: auto-merge for minor/patch updates
```

## Demo Scenarios

### Demo 1: Repository Setup (10-15 min)
Follow [SECTION-10-RUNBOOK.md](docs/SECTION-10-RUNBOOK.md#demo-1-github-repo-setup-for-secure-cicd):
- Configure branch protection rules
- Create GitHub environments
- Set workflow permissions
- Enable security gates

### Demo 2: SCA in Action (15-20 min)
Follow [SECTION-10-RUNBOOK.md](docs/SECTION-10-RUNBOOK.md#demo-2-software-composition-analysis-sca-gate):
- Create PR with vulnerable dependency
- Show SCA gate failure
- Fix vulnerability
- Show gate passes
- Discuss shift-left security

### Demo 3: Container Hardening (20-25 min)
Follow [SECTION-10-RUNBOOK.md](docs/SECTION-10-RUNBOOK.md#demo-3-secure-container-development--deployment):
- Build insecure vs. secure containers
- Compare sizes and vulnerabilities
- Run containers locally
- Push to GHCR
- Show Trivy scanning in CI/CD

## Testing Locally

### Run Unit Tests
```bash
dotnet test --verbosity normal
```

Expected output:
```
Test Execution Summary
======================
Total Tests: 3
Passed: 3
Failed: 0
Skipped: 0
```

### Check Dependencies
```bash
dotnet list package
dotnet list package --vulnerable --include-transitive
```

### Manual API Testing
```bash
# Start application
dotnet run --project src/SecureDeploy.DemoApi

# In another terminal:
curl https://localhost:5001/health           # Should return 200
curl https://localhost:5001/                 # Welcome message
curl https://localhost:5001/api/weatherforecast  # Weather data
```

## Security Concepts Demonstrated

| Concept | Location | Why it Matters |
|---------|----------|----------------|
| **Branch Protection** | GitHub Settings | Prevents direct pushes; enforces review |
| **Status Checks** | .github/workflows/ | Blocks merge if security gates fail |
| **Least Privilege** | Workflow permissions | Limits token scope to prevent abuse |
| **SCA Gate** | ci.yml | Catches vulnerable dependencies early |
| **Container Hardening** | Dockerfile | Non-root user, pinned versions, minimal image |
| **Vulnerability Scanning** | container-release.yml | Detects OS/library vulnerabilities |
| **Artifact Publishing** | container-release.yml | Conditional: only push if secure |

## Package Dependencies

### DemoApi Project
- `Serilog.AspNetCore` (8.0.0) - Structured logging
- `Newtonsoft.Json` (13.0.3) - JSON serialization (intentionally older version for demo purposes)
- `Microsoft.Extensions.Diagnostics.HealthChecks` (10.0.0) - Health checks

### Shared Project
- (No external dependencies)

### Tests Project
- `xunit` (2.6.6) - Unit testing framework
- `Microsoft.NET.Test.Sdk` (17.8.2) - Test runner

## Troubleshooting

### Build fails
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore fresh
dotnet restore --force-evaluate
dotnet build
```

### Tests fail
```bash
# Run with verbose output
dotnet test --verbosity detailed --logger console;verbosity=detailed
```

### Docker build fails
```bash
# Check .NET 10 SDK availability
dotnet --list-sdks

# Try explicit target
docker build -f Dockerfile --target final -t secure-deploy:latest .
```

### Container won't start
```bash
# Port conflict?
lsof -i :8080

# Inspect logs
docker logs <container-id>
```

## Additional Resources

- **Runbook:** [SECTION-10-RUNBOOK.md](docs/SECTION-10-RUNBOOK.md) - Complete step-by-step demos
- **Security Issues:** [INSECURE-NOTES.md](docs/INSECURE-NOTES.md) - Why insecure Dockerfile is risky
- **API Docs:** [SecureDeploy.DemoApi/README.md](src/SecureDeploy.DemoApi/README.md)

## References

- [GitHub Actions Security Best Practices](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [OWASP Top 10 - Vulnerable Dependencies](https://owasp.org/www-project-top-ten/)
- [NIST Container Security](https://csrc.nist.gov/publications/detail/sp/800-190/final)
- [CIS Docker Benchmark](https://www.cisecurity.org/benchmark/docker)
- [Trivy Vulnerability Scanner](https://github.com/aquasecurity/trivy)

## License

This is a teaching repository for educational purposes. Feel free to use as a template for your own projects.

---

**Created for:** Udemy - Secure Coding in C# and .NET  
**Section:** 10 - Secure Automated Deployments with GitHub  
**Last Updated:** January 2026
