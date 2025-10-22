#!/bin/bash
# Local NuGet package creation script

set -e

echo "🔨 Building and packing Flowgine packages locally..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean -c Release

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build
echo "🏗️  Building in Release mode..."
dotnet build -c Release --no-restore

# Create output directory
mkdir -p ./nupkgs

# Pack packages
echo "📦 Packing Flowgine..."
dotnet pack src/Flowgine/Flowgine.csproj -c Release -o ./nupkgs --no-build

echo "📦 Packing Flowgine.LLM..."
dotnet pack src/Flowgine.LLM/Flowgine.LLM.csproj -c Release -o ./nupkgs --no-build

echo "📦 Packing Flowgine.LLM.OpenAI..."
dotnet pack src/Flowgine.LLM.OpenAI/Flowgine.LLM.OpenAI.csproj -c Release -o ./nupkgs --no-build

# List created packages
echo ""
echo "✅ Packages created successfully:"
ls -lh ./nupkgs/*.nupkg

echo ""
echo "📋 Package details:"
for pkg in ./nupkgs/*.nupkg; do
    echo "  - $(basename $pkg)"
done

echo ""
echo "💡 To test locally, you can:"
echo "   1. Add a local NuGet source: dotnet nuget add source $(pwd)/nupkgs --name LocalFlowgine"
echo "   2. Install from local source: dotnet add package Flowgine --source LocalFlowgine"

