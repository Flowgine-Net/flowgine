#!/bin/bash
# Script to create a new release
# Usage: ./scripts/create-release.sh <version>
# Example: ./scripts/create-release.sh 0.1.0

set -e

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "❌ Error: Version number is required"
    echo "Usage: $0 <version>"
    echo "Example: $0 0.1.0"
    exit 1
fi

echo "🏷️  Creating release v$VERSION..."
echo ""

# Check if tag already exists
if git rev-parse "v$VERSION" >/dev/null 2>&1; then
    echo "❌ Error: Tag v$VERSION already exists"
    exit 1
fi

# Check for uncommitted changes
if ! git diff-index --quiet HEAD --; then
    echo "⚠️  Warning: You have uncommitted changes"
    git status --short
    echo ""
    read -p "Continue anyway? (yes/no): " confirm
    if [ "$confirm" != "yes" ]; then
        echo "❌ Release cancelled"
        exit 0
    fi
fi

# Update version in csproj files
echo "📝 Updating version in .csproj files..."
sed -i.bak "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" src/Flowgine/Flowgine.csproj
sed -i.bak "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" src/Flowgine.LLM/Flowgine.LLM.csproj
sed -i.bak "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" src/Flowgine.LLM.OpenAI/Flowgine.LLM.OpenAI.csproj

# Remove backup files
rm -f src/Flowgine/Flowgine.csproj.bak
rm -f src/Flowgine.LLM/Flowgine.LLM.csproj.bak
rm -f src/Flowgine.LLM.OpenAI/Flowgine.LLM.OpenAI.csproj.bak

# Commit version bump
echo "💾 Committing version bump..."
git add src/Flowgine/Flowgine.csproj
git add src/Flowgine.LLM/Flowgine.LLM.csproj
git add src/Flowgine.LLM.OpenAI/Flowgine.LLM.OpenAI.csproj
git commit -m "chore: bump version to $VERSION"

# Create and push tag
echo "🏷️  Creating tag v$VERSION..."
git tag -a "v$VERSION" -m "Release version $VERSION"

echo ""
echo "✅ Release v$VERSION prepared!"
echo ""
echo "📋 Next steps:"
echo "  1. Push changes: git push origin main"
echo "  2. Push tag: git push origin v$VERSION"
echo "  3. GitHub Actions will automatically:"
echo "     - Build and test"
echo "     - Pack NuGet packages"
echo "     - Publish to NuGet.org"
echo "     - Create GitHub release"
echo ""
echo "💡 Or push both at once: git push origin main --tags"

