#!/bin/bash
set -e

echo "Starting setup script..."

# Install dependencies
apt-get update
apt-get install -y jq

# Load paths from config file
echo "Loading paths from ci-paths.json..."
export BUILD_OUTPUT_DIR=$(jq -r '.buildOutputDir' ci-paths.json)
export TEST_RESULTS_DIR=$(jq -r '.testResultsDir' ci-paths.json)
export GIT_VERSION_PROPS=$(jq -r '.gitVersionProps' ci-paths.json)
export BENCHMARKS_DIR=$(jq -r '.benchmarksDir' ci-paths.json)
export BENCHMARK_RESULTS_DIR=$(jq -r '.benchmarkResultsDir' ci-paths.json)
export TEST_PROJECT=$(jq -r '.testProject' ci-paths.json)
export SRC_PROJECT=$(jq -r '.srcProject' ci-paths.json)
export UBUNTU_SCRIPTS_DIR=/github/workspace/scripts/ubuntu
export PROJECT_ROOT=/github/workspace
export SCRIPTS_DIR=/github/workspace/scripts

# Clear .NET tools cache
echo "Clearing .NET tools cache..."
rm -rf ~/.dotnet/tools

# Install GitVersion (latest stable)
echo "Installing latest stable GitVersion..."
dotnet tool install --global GitVersion.Tool

# Add .NET global tools to PATH within the current script
export PATH="$PATH:/root/.dotnet/tools"

# Verify installations
echo "Verifying jq installation..."
jq --version || { echo "jq installation failed"; exit 1; }

# Try running gitversion with /version
echo "Verifying GitVersion installation..."
dotnet gitversion /version || {
    echo "GitVersion /version failed, trying current directory"
    dotnet gitversion . /version || {
        echo "GitVersion installation failed"; exit 1;
    }
}

echo "Setup complete."