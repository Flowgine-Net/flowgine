#!/bin/bash
# Script to publish Flowgine packages to NuGet.org
# Usage: ./scripts/publish-nuget.sh <version>
# Example: ./scripts/publish-nuget.sh 0.1.0

set -e

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "❌ Error: Version number is required"
    echo "Usage: $0 <version>"
    echo "Example: $0 0.1.0"
    exit 1
fi

if [ -z "$NUGET_API_KEY" ]; then
    echo "❌ Error: NUGET_API_KEY environment variable is not set"
    echo "Please set it first: export NUGET_API_KEY='your-api-key'"
    exit 1
fi

echo "🚀 Publishing Flowgine v$VERSION to NuGet.org..."
echo ""

# Confirm
read -p "Are you sure you want to publish version $VERSION? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "❌ Publish cancelled"
    exit 0
fi

# Clean and build
echo "🧹 Cleaning..."
dotnet clean -c Release

echo "📦 Restoring dependencies..."
dotnet restore

echo "🏗️  Building..."
dotnet build -c Release --no-restore

echo "🧪 Running tests..."
dotnet test -c Release --no-build --verbosity normal

# Create packages directory
mkdir -p ./nupkgs

# Pack with version
echo "📦 Packing packages with version $VERSION..."
dotnet pack src/Flowgine/Flowgine.csproj \
    -c Release \
    -o ./nupkgs \
    -p:PackageVersion=$VERSION \
    --no-build

dotnet pack src/Flowgine.LLM/Flowgine.LLM.csproj \
    -c Release \
    -o ./nupkgs \
    -p:PackageVersion=$VERSION \
    --no-build

dotnet pack src/Flowgine.LLM.OpenAI/Flowgine.LLM.OpenAI.csproj \
    -c Release \
    -o ./nupkgs \
    -p:PackageVersion=$VERSION \
    --no-build

echo ""
echo "📋 Packages to publish:"
ls -lh ./nupkgs/*.nupkg

# Push to NuGet
echo ""
echo "⬆️  Pushing to NuGet.org..."
dotnet nuget push ./nupkgs/*.nupkg \
    --api-key $NUGET_API_KEY \
    --source https://api.nuget.org/v3/index.json \
    --skip-duplicate

echo ""
echo "✅ Successfully published Flowgine v$VERSION!"
echo ""
echo "📦 Published packages:"
echo "  - Flowgine.$VERSION"
echo "  - Flowgine.LLM.$VERSION"
echo "  - Flowgine.LLM.OpenAI.$VERSION"
echo ""
echo "🔗 View on NuGet.org:"
echo "  - https://www.nuget.org/packages/Flowgine/$VERSION"
echo "  - https://www.nuget.org/packages/Flowgine.LLM/$VERSION"
echo "  - https://www.nuget.org/packages/Flowgine.LLM.OpenAI/$VERSION"
echo ""
echo "💡 Next steps:"
echo "  1. Create a GitHub release: gh release create v$VERSION"
echo "  2. Update CHANGELOG.md"
echo "  3. Announce the release"

