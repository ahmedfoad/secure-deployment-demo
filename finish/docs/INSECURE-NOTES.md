# Security Issues in Dockerfile.insecure

This document explains why `Dockerfile.insecure` is risky and should not be used in production.

## Issue 1: Running as Root User ⚠️ CRITICAL

```dockerfile
# ❌ BAD - No USER directive, runs as root
FROM mcr.microsoft.com/dotnet/aspnet:latest
COPY --from=build /app/bin/Release/net10.0/publish/ .
ENTRYPOINT ["dotnet", "SecureDeploy.DemoApi.dll"]
```

**Why it's dangerous:**
- Container process runs as `root` (UID 0)
- If attacker gains access to the container, they have full system privileges
- Attacker can modify files, install backdoors, escape container
- Called "privilege escalation" - turns container breach into host compromise

**Real-world impact:**
- Shodan scan finds thousands of exposed root containers
- Security audits will flag this as a critical failure
- Container registries (like GHCR) may reject or flag images

**Fix:** Create non-root user before running app (see `Dockerfile`)

---

## Issue 2: Unpinned Version Tags ⚠️ HIGH

```dockerfile
# ❌ BAD - 'latest' tag is not reproducible
FROM mcr.microsoft.com/dotnet/aspnet:latest
FROM mcr.microsoft.com/dotnet/sdk:latest
```

**Why it's dangerous:**
- `latest` tag changes when new versions are released
- You might build same Dockerfile two days apart and get different images
- No way to reproduce bugs or security issues
- If new base image introduces vulnerability, all builds become vulnerable
- Violates reproducibility principle in supply chain security

**Real-world impact:**
- Build A on Monday: safe, no vulnerabilities
- Build B on Wednesday: same Dockerfile, but `latest` now includes CVE → blocked by security scan
- Debugging failures becomes impossible: "Works on my machine" (last month's image)

**Fix:** Pin to specific version: `FROM mcr.microsoft.com/dotnet/aspnet:10.0` or `10.0.1` (patch-level pinning recommended)

---

## Issue 3: Large Image Size ⚠️ MEDIUM

```dockerfile
# ❌ BAD - Using SDK image in final stage
FROM mcr.microsoft.com/dotnet/sdk:latest
COPY --from=build /app/bin/Release/net10.0/publish/ .
```

**Why it's dangerous:**
- SDK image contains build tools (compilers, development files)
- Final image is 2-3x larger than necessary
- Larger attack surface: more packages = more potential vulnerabilities
- Slower deployment: larger images take longer to push/pull
- Higher storage costs in registries

**Real-world impact:**
- `dotnet/sdk:10.0` = ~1.5 GB
- `dotnet/aspnet:10.0` = ~500 MB
- When deploying to 100 replicas: 1.5 TB vs 500 GB difference

**Fix:** Multi-stage build: build in SDK, copy artifacts to smaller runtime image

---

## Issue 4: Copying All Source Early ⚠️ MEDIUM

```dockerfile
# ❌ BAD - Defeats Docker layer caching
COPY . .  # Copies entire repo
RUN dotnet restore
RUN dotnet build -c Release
```

**Why it's slow:**
- If you change ANY file, layer cache is invalidated
- Re-runs `dotnet restore` (slow, downloads 100+ packages)
- Building locally takes 3-5 minutes instead of 30 seconds
- CI/CD pipeline becomes slower: every code change = full rebuild

**Real-world impact:**
- Developer changes a comment in `Program.cs` → entire restore re-runs
- PR feedback loop becomes slow: 5 min build instead of 30s cache hit
- CI minutes cost money: slow builds = expensive GitHub Actions usage

**Fix:** Copy project files first, restore, then copy source code

---

## Issue 5: No Health Check ⚠️ MEDIUM

```dockerfile
# ❌ BAD - No HEALTHCHECK
FROM mcr.microsoft.com/dotnet/aspnet:latest
COPY --from=build /app/bin/Release/net10.0/publish/ .
ENTRYPOINT ["dotnet", "SecureDeploy.DemoApi.dll"]
```

**Why it's dangerous:**
- Container might be running but app is hung/crashed
- Kubernetes/orchestrators can't detect unhealthy containers
- Container stays "up" but not actually serving traffic
- Requests keep hitting broken container instead of healthy replica

**Real-world impact:**
- App crashes in container, process exits
- Docker still shows container as "Up"
- Load balancer routes traffic to dead container
- Users see "connection refused" errors

**Fix:** Add HEALTHCHECK directive (app should respond on `/health`)

---

## Issue 6: No Entrypoint ⚠️ MEDIUM

```dockerfile
# ❌ BAD - Entrypoint can be overridden
ENTRYPOINT ["dotnet", "SecureDeploy.DemoApi.dll"]
```

Without explicit exec form, someone can do:
```bash
docker run -it myimage /bin/bash
```

**Real-world impact:**
- Security auditor accidentally adds `/bin/sh` override
- Attacker might attempt shell escape (though less likely with non-root)
- Image intent is not enforced

**Fix:** Explicit ENTRYPOINT in exec form (prevents shell bypass)

---

## Summary: Insecure vs. Secure

| Issue | Insecure | Secure |
|-------|----------|--------|
| Root user | ❌ Runs as root | ✓ Non-root user |
| Version pinning | ❌ 'latest' tag | ✓ Pinned to 10.0 |
| Image size | ❌ 1.5 GB (SDK) | ✓ 500 MB (runtime) |
| Layer caching | ❌ Copy all early | ✓ Copy files strategically |
| Health check | ❌ None | ✓ /health endpoint |
| Entrypoint | ❌ Not enforced | ✓ Exec form |

---

## How to Demo This

1. Build both images:
   ```bash
   docker build -f Dockerfile.insecure -t demo:insecure .
   docker build -f Dockerfile -t demo:secure .
   ```

2. Compare sizes:
   ```bash
   docker images | grep demo
   # Output: demo:insecure = ~1.5GB, demo:secure = ~500MB
   ```

3. Check running user:
   ```bash
   docker run -it demo:insecure whoami  # Returns 'root'
   docker run -it demo:secure whoami    # Returns 'appuser'
   ```

4. Scan both with Trivy:
   ```bash
   trivy image demo:insecure  # Shows more vulnerabilities
   trivy image demo:secure    # Fewer vulnerabilities (smaller attack surface)
   ```

---

## References

- [NIST Container Security Best Practices](https://csrc.nist.gov/publications/detail/sp/800-190/final)
- [CIS Docker Benchmark](https://www.cisecurity.org/benchmark/docker)
- [Docker Security Best Practices](https://docs.docker.com/develop/security-best-practices/)
