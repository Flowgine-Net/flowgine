# GitHub Actions Setup

Tento adresář obsahuje CI/CD workflows pro Flowgine.

## Workflows

### 1. `ci.yml` - Continuous Integration
**Trigger:** Push na main/develop/feat branches, Pull Requests

**Akce:**
- ✅ Build projektu v Release módu
- ✅ Spuštění testů
- ✅ Kontrola warningů v core projektech

### 2. `publish.yml` - Publish to NuGet
**Trigger:** Push tagu ve formátu `v*.*.*` (např. `v0.1.0`, `v1.2.3`)

**Akce:**
- ✅ Build a test
- ✅ Vytvoření NuGet balíčků
- ✅ Publikace na NuGet.org
- ✅ Vytvoření GitHub Release

### 3. `preview-publish.yml` - Publish Preview
**Trigger:** Push pre-release tagu `v*.*.*-*` (např. `v0.1.0-beta1`, `v1.0.0-rc1`)

**Akce:**
- ✅ Vytvoření a publikace pre-release balíčků
- ✅ Vytvoření pre-release na GitHubu

## První nastavení

### 1. Nastavení NuGet API klíče

1. Získej API klíč z https://www.nuget.org/account/apikeys
2. Přidej jako GitHub Secret:
   - Přejdi na: `Settings → Secrets and variables → Actions`
   - Klikni **"New repository secret"**
   - **Name**: `NUGET_API_KEY`
   - **Value**: `tvůj-api-klíč-z-nuget.org`
   - Klikni **"Add secret"**

### 2. Permissions pro GitHub Actions

Workflows potřebují oprávnění pro vytváření releases:

1. Přejdi na: `Settings → Actions → General`
2. Sekce **"Workflow permissions"**
3. Vyber **"Read and write permissions"**
4. Zaškrtni **"Allow GitHub Actions to create and approve pull requests"**
5. Klikni **"Save"**

## Jak publikovat novou verzi

### Automaticky (doporučeno)

```bash
# Použij helper script
./scripts/create-release.sh 0.1.0

# Push změn a tagu
git push origin main --tags
```

GitHub Actions se automaticky spustí a publikuje balíčky!

### Ručně

```bash
# 1. Aktualizuj verzi v .csproj souborech
# <Version>0.1.0</Version> → <Version>0.2.0</Version>

# 2. Commit a vytvoř tag
git add .
git commit -m "chore: bump version to 0.2.0"
git tag -a v0.2.0 -m "Release version 0.2.0"

# 3. Push
git push origin main --tags
```

## Pre-release verze

Pro publikaci beta/alpha verzí:

```bash
# Vytvoř pre-release tag
git tag -a v0.1.0-beta1 -m "Beta release 0.1.0-beta1"
git push origin v0.1.0-beta1
```

Spustí se `preview-publish.yml` workflow.

## Sledování průběhu

1. Přejdi na: `Actions` tab v GitHubu
2. Vyber workflow run
3. Sleduj progress jednotlivých kroků

## Troubleshooting

### "Package already exists"
- NuGet neumožňuje přepsat existující verzi
- Musíš použít novou verzi (např. `0.1.1`)

### "Invalid API key"
- Zkontroluj, že `NUGET_API_KEY` secret je správně nastaven
- API klíč musí mít scope `Push new packages and package versions`

### "Permission denied to create release"
- Zkontroluj workflow permissions v Settings → Actions

### Build failuje
- Zkontroluj, že všechny projekty buildují lokálně: `dotnet build -c Release`
- Zkontroluj warnings: `dotnet build --warnaserror`

## Lokální testování

Před publikací otestuj lokálně:

```bash
# Build a pack
./scripts/pack-local.sh

# Zkontroluj vytvořené balíčky
ls -lh ./nupkgs/

# Otestuj instalaci
dotnet nuget add source $(pwd)/nupkgs --name LocalFlowgine
dotnet add package Flowgine --source LocalFlowgine
```

## Další informace

Viz [RELEASE_CHECKLIST.md](../RELEASE_CHECKLIST.md) pro kompletní release proces.

