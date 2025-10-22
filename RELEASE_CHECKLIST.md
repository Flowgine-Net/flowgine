# Release Checklist

Use this checklist when preparing a new Flowgine release.

## Pre-Release

- [ ] All issues and PRs for this version are closed
- [ ] All tests pass successfully
- [ ] Build is successful without warnings in core projects
- [ ] All examples work correctly
- [ ] XML documentation is complete
- [ ] README.md is up to date
- [ ] CHANGELOG.md is updated

## Semantic Versioning

We use [SemVer](https://semver.org/):
- **MAJOR** (1.0.0): Breaking changes
- **MINOR** (0.1.0): New features (backwards compatible)
- **PATCH** (0.0.1): Bug fixes (backwards compatible)

Pre-release formats:
- `0.1.0-alpha1` - Alpha version
- `0.1.0-beta1` - Beta version
- `0.1.0-rc1` - Release candidate

## Release Process (Automated)

### Method 1: Using script (recommended)

```bash
# Creates tag, updates versions and commits
./scripts/create-release.sh 0.1.0

# Push changes and tags
git push origin main --tags
```

GitHub Actions automatically:
1. ✅ Builds the project
2. ✅ Runs tests
3. ✅ Creates NuGet packages
4. ✅ Publishes to NuGet.org
5. ✅ Creates GitHub Release

### Method 2: Manual

```bash
# 1. Update version in .csproj files
# <Version>0.1.0</Version>

# 2. Commit changes
git add .
git commit -m "chore: bump version to 0.1.0"

# 3. Create tag
git tag -a v0.1.0 -m "Release version 0.1.0"

# 4. Push
git push origin main --tags
```

## Release Process (Manual Publishing)

If you want to publish manually:

```bash
# 1. Set API key
export NUGET_API_KEY="your-api-key"

# 2. Use publish script
./scripts/publish-nuget.sh 0.1.0
```

## Post-Release

- [ ] Verify packages are available on NuGet.org
  - https://www.nuget.org/packages/Flowgine/
  - https://www.nuget.org/packages/Flowgine.LLM/
  - https://www.nuget.org/packages/Flowgine.LLM.OpenAI/
- [ ] Verify GitHub Release
- [ ] Test installation: `dotnet add package Flowgine --version X.Y.Z`
- [ ] Announce release (Discord, Twitter, Reddit, etc.)
- [ ] Update documentation

## Rollback (in case of issues)

NuGet doesn't allow deleting versions, but you can:

```bash
# Unlist version (hides from search results, but still available)
dotnet nuget delete Flowgine 0.1.0 --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json --non-interactive

# Publish fixed version
./scripts/publish-nuget.sh 0.1.1
```

## Support Contacts

- **GitHub Issues**: https://github.com/Flowgine-Net/flowgine/issues
- **GitHub Discussions**: https://github.com/Flowgine-Net/flowgine/discussions

