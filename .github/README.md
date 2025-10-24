# GitHub Actions Setup

This directory contains CI/CD workflows for Flowgine.

## Workflows

### 1. `ci.yml` - Continuous Integration
**Trigger:** Push to main/develop/feat branches, Pull Requests

**Actions:**
- ✅ Build project in Release mode
- ✅ Run tests
- ✅ Check warnings in core projects

### 2. `publish.yml` - Publish to NuGet
**Trigger:** Push tag in format `v*.*.*` (e.g. `v0.1.0`, `v1.2.3`)

**Actions:**
- ✅ Build and test
- ✅ Create NuGet packages
- ✅ Publish to NuGet.org
- ✅ Create GitHub Release

### 3. `preview-publish.yml` - Publish Preview
**Trigger:** Push pre-release tag `v*.*.*-*` (e.g. `v0.1.0-beta1`, `v1.0.0-rc1`)

**Actions:**
- ✅ Create and publish pre-release packages
- ✅ Create pre-release on GitHub

## Initial Setup

### 1. Configure NuGet API Key

1. Get API key from https://www.nuget.org/account/apikeys
2. Add as GitHub Secret:
   - Go to: `Settings → Secrets and variables → Actions`
   - Click **"New repository secret"**
   - **Name**: `NUGET_API_KEY`
   - **Value**: `your-api-key-from-nuget.org`
   - Click **"Add secret"**

### 2. Permissions for GitHub Actions

Workflows need permissions to create releases:

1. Go to: `Settings → Actions → General`
2. Section **"Workflow permissions"**
3. Select **"Read and write permissions"**
4. Check **"Allow GitHub Actions to create and approve pull requests"**
5. Click **"Save"**

## How to Publish a New Version

### Automatically (recommended)

```bash
# Use helper script
./scripts/create-release.sh 0.1.0

# Push changes and tag
git push origin main --tags
```

GitHub Actions will automatically run and publish the packages!

### Manually

```bash
# 1. Update version in .csproj files
# <Version>0.1.0</Version> → <Version>0.2.0</Version>

# 2. Commit and create tag
git add .
git commit -m "chore: bump version to 0.2.0"
git tag -a v0.2.0 -m "Release version 0.2.0"

# 3. Push
git push origin main --tags
```

## Pre-release Versions

To publish beta/alpha versions:

```bash
# Create pre-release tag
git tag -a v0.1.0-beta1 -m "Beta release 0.1.0-beta1"
git push origin v0.1.0-beta1
```

This will trigger the `preview-publish.yml` workflow.

## Monitoring Progress

1. Go to: `Actions` tab in GitHub
2. Select workflow run
3. Monitor progress of individual steps

## Troubleshooting

### "Package already exists"
- NuGet doesn't allow overwriting existing versions
- You must use a new version (e.g. `0.1.1`)

### "Invalid API key"
- Check that `NUGET_API_KEY` secret is correctly configured
- API key must have scope `Push new packages and package versions`

### "Permission denied to create release"
- Check workflow permissions in Settings → Actions

### Build fails
- Check that all projects build locally: `dotnet build -c Release`
- Check warnings: `dotnet build --warnaserror`

## Local Testing

Before publishing, test locally:

```bash
# Build and pack
./scripts/pack-local.sh

# Check created packages
ls -lh ./nupkgs/

# Test installation
dotnet nuget add source $(pwd)/nupkgs --name LocalFlowgine
dotnet add package Flowgine --source LocalFlowgine
```

## Additional Information

See [RELEASE_CHECKLIST.md](../RELEASE_CHECKLIST.md) for complete release process.

