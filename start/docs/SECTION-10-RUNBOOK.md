# Section 10 Runbook: Secure Automated Deployments with GitHub

This runbook provides step-by-step instructions for demonstrating secure CI/CD with GitHub Actions, Software Composition Analysis (SCA), and hardened container deployments.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Demo 1: GitHub Repo Setup for Secure CI/CD](#demo-1-github-repo-setup-for-secure-cicd)
3. [Demo 2: Software Composition Analysis (SCA) Gate](#demo-2-software-composition-analysis-sca-gate)
4. [Demo 3: Secure Container Development & Deployment](#demo-3-secure-container-development--deployment)

---

## Prerequisites

Before starting, ensure you have:

- [ ] This repository cloned locally
- [ ] .NET 10 SDK installed: `dotnet --version`
- [ ] Docker installed: `docker --version`
- [ ] Git configured: `git config --global user.name "Your Name"`
- [ ] GitHub account with repo access
- [ ] (Optional) Trivy installed for local container scanning: `trivy --version`

Verify setup:
```bash
cd secure-deployment-demo
dotnet build
dotnet test
```

---

## Demo 1: GitHub Repo Setup for Secure CI/CD

**Duration:** ~10-15 minutes  
**Goal:** Show students how to configure GitHub for secure deployments

### 1.1 Create GitHub Repository

1. Navigate to https://github.com/new
2. Create a **public** repository named `secure-deployment-demo`
3. Clone this demo repository into it:
   ```bash
   git clone https://github.com/YOUR-USERNAME/secure-deployment-demo.git
   git add .
   git commit -m "Initial secure deployment demo"
   git push origin main
   ```

### 1.2 Configure Branch Protection Rules

**Why:** Enforce security gates before code reaches `main`

Steps:
1. Go to **Settings** → **Branches**
2. Click **Add rule** under "Branch protection rules"
3. Apply to branch: `main`
4. Configure:
   - ✅ **Require a pull request before merging**
     - ✅ Require approvals (2)
     - ✅ Dismiss stale PR approvals when new commits are pushed
   - ✅ **Require status checks to pass before merging**
     - ✓ CI - Build and Test with SCA Gate (from `.github/workflows/ci.yml`)
     - ✓ Dependency Review (from `.github/workflows/dependency-review.yml`)
   - ✅ **Require branches to be up to date before merging**
   - ✅ **Include administrators** (enforce for everyone)

**Demo Talking Points:**
> "These rules prevent direct pushes to main. Every change must go through PR review AND automated security checks. Notice how we require TWO status checks: our own SCA gate, plus GitHub's Dependency Review. This is defense-in-depth."

### 1.3 Create GitHub Environments

**Why:** Different secrets/permissions for dev, staging, production

1. Go to **Settings** → **Environments**
2. Create new environment: `production`
3. Configure deployment branches:
   - ✅ **Selected branch restriction** → `main`
4. (Optional) Add environment secrets:
   - For future demos: `REGISTRY_PASSWORD`, `DEPLOY_TOKEN`, etc.

### 1.4 Configure Workflow Permissions

**Why:** Limit what GitHub Actions can do (least privilege)

1. Go to **Settings** → **Actions** → **General**
2. **Workflow permissions:**
   - ✅ **Read repository contents permission**
   - ✅ **Allow GitHub Actions to create and approve pull requests**
3. **Expiration of GITHUB_TOKEN:**
   - Select **30 days** (shorter = better security)

**Demo Talking Points:**
> "GitHub Actions workflows run with a temporary token. We limit what that token can do: read code, write to packages registry, but NOT delete secrets or modify settings. This follows the principle of least privilege."

### 1.5 (PLACEHOLDER) Secrets for Production Deployment

These would be populated in a real deployment scenario:
- `GHCR_TOKEN` - Personal access token for pushing to GitHub Container Registry
- `DEPLOY_SERVER_URL` - Deployment target (Kubernetes, App Service, etc.)
- `DEPLOY_SSH_KEY` - Private key for deployment authentication

For this demo, we use `GITHUB_TOKEN` (built-in), so no secrets needed initially.

---

## Demo 2: Software Composition Analysis (SCA) Gate

**Duration:** ~15-20 minutes  
**Goal:** Show how to detect vulnerable dependencies and prevent insecure code from merging

### 2.1 Understand the Three SCA Gates

We have **three layers** of vulnerability detection:

| Gate | Tool | When | What it checks |
|------|------|------|----------------|
| **Gate 1** | `dotnet list package --vulnerable` | CI build | Direct + transitive deps in .csproj |
| **Gate 2** | `dependency-review-action` | PR check | GitHub's advisory DB + license changes |
| **Gate 3** | `trivy` | Container scan | OS packages + libraries in final image |

### 2.2 Create a Test Branch with Vulnerable Package

Demonstrate the SCA gate in action:

```bash
# Create a feature branch
git checkout -b demo/vulnerable-package

# Edit src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj
# Change: <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
# To:     <PackageReference Include="Newtonsoft.Json" Version="12.0.1" />

# Commit and push
git add src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj
git commit -m "Demo: Introduce vulnerable dependency"
git push origin demo/vulnerable-package
```

### 2.3 Open Pull Request and Observe Checks

1. Go to GitHub and create a PR: `demo/vulnerable-package` → `main`

2. **Watch CI Run:**
   - Click **Actions** tab
   - Select the "CI - Build and Test with SCA Gate" workflow
   - Show the output:
     ```
     Security Gate: Check for Vulnerable Dependencies
     =========================================
     Checking for vulnerable NuGet packages...
     Newtonsoft.Json 12.0.1 -> 13.0.3 (VULNERABLE)
     ```

3. **Show Dependency Review Comment:**
   - On PR, GitHub will post a comment about new vulnerabilities
   - Click to see details (CVE ID, severity, fix version)

4. **Explain Why PR Cannot Merge:**
   - Status checks are RED (failed)
   - Branch protection rule requires all checks to pass
   - Team cannot merge without fixing vulnerability

### 2.4 Fix the Vulnerability and Re-push

```bash
# Edit the .csproj back to 13.0.3
# src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj
# Change: <PackageReference Include="Newtonsoft.Json" Version="12.0.1" />
# To:     <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

git add src/SecureDeploy.DemoApi/SecureDeploy.DemoApi.csproj
git commit -m "Fix: Update Newtonsoft.Json to patched version"
git push origin demo/vulnerable-package
```

1. Return to PR on GitHub
2. Watch **new CI run** automatically trigger
3. Show that `dotnet list package --vulnerable` now passes
4. All checks turn GREEN ✓
5. PR is now mergeable!

### 2.5 Demonstrate Local SCA Scanning

Show students they can scan locally before pushing:

```bash
# Restore packages first
dotnet restore

# Check for vulnerabilities
dotnet list package --vulnerable --include-transitive

# Output should show:
# - OK for current versions
# - VULNERABLE for demo/vulnerable-package branch
```

**Demo Talking Points:**
> "SCA gates prevent vulnerable code from reaching production. Notice how:
> 1. CI automatically runs when PR is created
> 2. Scan fails because Newtonsoft.Json 12.0.1 has a known CVE
> 3. Branch protection blocks the merge (red X on PR)
> 4. We fix by upgrading to 13.0.3 (patched version)
> 5. Scan passes (green ✓), PR can now merge
> 
> This is **shift-left security**: catch problems BEFORE production, not after."

---

## Demo 3: Secure Container Development & Deployment

**Duration:** ~20-25 minutes  
**Goal:** Show how insecure containers fail security scans and hardened containers pass

### 3.1 Build and Compare Insecure vs. Secure Dockerfile

#### Step A: Build Insecure Container

```bash
# Build using insecure Dockerfile
docker build -f Dockerfile.insecure -t secure-demo:insecure .

# Check image size
docker images secure-demo:insecure
# Output: ~1.5GB (includes build tools)

# Check what user it runs as
docker run --rm secure-demo:insecure whoami
# Output: root (BAD - not isolated)

# Scan with Trivy (if installed)
trivy image secure-demo:insecure
# Shows vulnerabilities due to larger base image
```

#### Step B: Build Secure Container

```bash
# Build using secure Dockerfile
docker build -f Dockerfile -t secure-demo:secure .

# Check image size
docker images secure-demo:secure
# Output: ~500MB (smaller, fewer vulnerabilities)

# Check what user it runs as
docker run --rm secure-demo:secure whoami
# Output: appuser (good - non-root isolation)

# Scan with Trivy (if installed)
trivy image secure-demo:secure
# Output: FEWER vulnerabilities than insecure version
```

**Demo Talking Points:**
> "Notice the differences:
> - **Size:** Insecure is 3x larger (includes build tools we don't need in production)
> - **User:** Insecure runs as root, secure runs as appuser (non-root)
> - **Vulnerabilities:** Secure image has fewer attack surface (smaller = fewer packages to exploit)
> 
> This is why we use multi-stage builds: build in SDK, copy artifacts to smaller runtime image."

### 3.2 Run Local Container and Test Endpoints

```bash
# Run secure container in background
docker run -d -p 8080:8080 --name secure-demo secure-demo:secure

# Test health endpoint
curl http://localhost:8080/health
# Expected: 200 OK

# Test API endpoint
curl http://localhost:8080/api/weatherforecast
# Expected: JSON array of weather data

# Test welcome endpoint
curl http://localhost:8080/
# Expected: JSON with welcome message

# Stop container
docker stop secure-demo
docker rm secure-demo
```

### 3.3 Push to GitHub Container Registry (GHCR)

This simulates what `container-release.yml` workflow does:

```bash
# Log in to GHCR
echo YOUR_GITHUB_TOKEN | docker login ghcr.io -u YOUR-USERNAME --password-stdin

# Tag image for GHCR
docker tag secure-demo:secure ghcr.io/YOUR-USERNAME/secure-deployment-demo:latest
docker tag secure-demo:secure ghcr.io/YOUR-USERNAME/secure-deployment-demo:$(git rev-parse --short HEAD)

# Push to GHCR
docker push ghcr.io/YOUR-USERNAME/secure-deployment-demo:latest
docker push ghcr.io/YOUR-USERNAME/secure-deployment-demo:$(git rev-parse --short HEAD)

# Verify on GitHub
# Go to: https://github.com/YOUR-USERNAME/secure-deployment-demo/pkgs/container/secure-deployment-demo
```

### 3.4 Show Workflow Execution (Automated)

1. Push a change to `main`:
   ```bash
   git checkout main
   echo "# Updated version" >> README.md
   git add README.md
   git commit -m "Trigger container workflow"
   git push origin main
   ```

2. Go to **Actions** tab and watch the `Container Build, Scan, and Push to GHCR` workflow:
   - Shows **Build** step
   - Shows **Trivy Scan** results
   - Shows **Conditional Push** - only pushes if scan passed
   - If scan found HIGH/CRITICAL vulnerabilities, push would be skipped

**Demo Talking Points:**
> "Our CI/CD pipeline does this automatically:
> 1. **Build** - Compiles .NET code using multi-stage Dockerfile
> 2. **Scan** - Trivy checks image for vulnerabilities (OS packages + libraries)
> 3. **Gate** - If HIGH/CRITICAL found, BLOCK publication (don't push to registry)
> 4. **Push** - Only push to GHCR if scan passed
> 
> This prevents vulnerable images from ever reaching a registry. Even if a developer accidentally includes a vulnerable package, Trivy catches it."

### 3.5 Demonstrate Scanning Failure

Create a scenario where container scan fails:

1. Temporarily modify `Dockerfile` to use an intentionally vulnerable base image
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0  # Old version with known vulns
   ```

2. Push to a branch and open PR:
   ```bash
   git checkout -b demo/vulnerable-image
   # (make Dockerfile change above)
   git add Dockerfile
   git commit -m "Demo: Use vulnerable base image"
   git push origin demo/vulnerable-image
   ```

3. Watch the workflow in **Actions**:
   - **Build** step succeeds
   - **Scan** step fails (detects vulnerabilities)
   - **Push** step is skipped (conditional: `if: success()`)
   - Image is NOT published to GHCR

4. Revert and show it passes again:
   ```bash
   git checkout Dockerfile
   git commit -m "Fix: Use patched base image"
   git push origin demo/vulnerable-image
   ```

---

## Summary: Three-Layer Security Defense

```
┌─────────────────────────────────────────────────────────────────┐
│                    Secure CI/CD Pipeline                        │
└─────────────────────────────────────────────────────────────────┘

        PR Created on GitHub
                 │
                 ▼
        ┌────────────────────┐
        │  Gate 1: SCA       │
        │  dotnet list pkg   │  ◄── Checks NuGet dependencies
        │  --vulnerable      │      Fails if vulns found
        └────────────────────┘
                 │
                 ▼
        ┌────────────────────┐
        │  Gate 2: Dep Revw   │
        │  GitHub Actions    │  ◄── Checks GitHub advisory DB
        │  dependency-review │      Fails if HIGH/CRITICAL
        └────────────────────┘
                 │
                 ▼
        ┌────────────────────┐
        │  Branch Protection │
        │  All checks pass?  │  ◄── Both gates must pass
        └────────────────────┘      before merge allowed
                 │
        (only on merge to main)
                 ▼
        ┌────────────────────┐
        │  Gate 3: Container │
        │  Trivy Scan        │  ◄── Checks container image
        │  Scan image        │      Fails if HIGH/CRITICAL
        └────────────────────┘
                 │
                 ▼
        ┌────────────────────┐
        │  Conditional Push  │
        │  to GHCR           │  ◄── Only push if scan passed
        │  (only if scan OK) │
        └────────────────────┘
                 │
                 ▼
        ✓ Image published to registry
        ✓ Ready for deployment
```

---

## Talking Points for Instructors

### Security Principle: "Defense in Depth"
> "We don't rely on a single security gate. We have THREE:
> 1. Package vulnerability scanning (what we use)
> 2. GitHub's vulnerability advisory database (what GitHub knows)
> 3. Container image scanning (what's actually in the image)
> 
> If one gate misses something, the others catch it. This is defense in depth."

### "Shift Left" Principle
> "We catch security issues BEFORE they reach production:
> - SCA gate: catches vulnerable packages in PR (before merge)
> - Container scan: catches vulnerable images before publishing
> - If we only scanned production? Too late! Vulnerability is live.
> 
> Shifting left = fixing problems earlier = cheaper + safer."

### Why We Fail Loudly
> "Notice: We BLOCK publication if vulnerabilities are found.
> - Could we just warn instead? Sure.
> - But then developers might ignore warnings.
> - By blocking, we force the team to fix it.
> - This is called 'gate enforcement' - gates actually gate."

### Cost of Security
> "You might think: 'Why add all these checks? They slow us down.'
> 
> Reality:
> - Security breach = millions in damage (data loss, downtime, lawsuits)
> - 5-minute security scan prevents billion-dollar breach
> - Cost-benefit is clear."

---

## Cleanup and Reset

To reset for another demo run:

```bash
# Delete test branches
git push origin --delete demo/vulnerable-package
git push origin --delete demo/vulnerable-image

# Remove local Docker images
docker rmi secure-demo:insecure secure-demo:secure
docker logout ghcr.io

# Return to main branch
git checkout main
git pull origin main
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **Workflow fails to run** | Check `.github/workflows/` files are valid YAML (use YAML linter) |
| **Trivy scan times out** | Trivy downloads DB on first run. Increase timeout in workflow. |
| **GHCR push fails** | Verify `GITHUB_TOKEN` has `packages:write` permission. Check tokens in repo settings. |
| **Docker build fails** | Ensure `.NET 10 SDK` is on build machine. Check Dockerfile paths. |
| **Tests fail locally** | Run `dotnet test --verbosity normal` to see error details. |
| **Container won't start** | Check port 8080 isn't already in use: `lsof -i :8080` |

---

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [OWASP Dependency-Check](https://owasp.org/www-project-dependency-check/)
- [Trivy Scanner](https://github.com/aquasecurity/trivy)
- [Container Security Best Practices (NIST)](https://csrc.nist.gov/publications/detail/sp/800-190/final)
- [CIS Docker Benchmark](https://www.cisecurity.org/benchmark/docker)

---

**Last Updated:** January 2026  
**Demo Repository:** https://github.com/YOUR-USERNAME/secure-deployment-demo
