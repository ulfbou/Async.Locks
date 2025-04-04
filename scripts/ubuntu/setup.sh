#!/bin/bash
set -e

echo "Starting setup script..."

# Install dependencies
sudo apt-get update
sudo apt-get install -y jq

# Load paths from config file
echo "Loading paths from ci-paths.json..."
export BUILD_OUTPUT_DIR=$(jq -r '.buildOutputDir' ci-paths.json)
export TEST_RESULTS_DIR=$(jq -r '.testResultsDir' ci-paths.json)
export GIT_VERSION_PROPS=$(jq -r '.gitVersionProps' ci-paths.json)
export BENCHMARKS_DIR=$(jq -r '.benchmarksDir' ci-paths.json)
export BENCHMARK_RESULTS_DIR=$(jq -r '.benchmarkResultsDir' ci-paths.json)
export TEST_PROJECT=$(jq -r '.testProject' ci-paths.json)
export SRC_PROJECT=$(jq -r '.srcProject' ci-paths.json)

# Install GitVersion (latest stable)
echo "Installing latest stable GitVersion..."
dotnet tool install --global GitVersion.Tool

# Install Coverlet Collector
echo "Installing Coverlet Collector..."
dotnet tool install --global coverlet.collector

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

# Verify Coverlet installation
echo "Verifying Coverlet installation..."
dotnet tool list --global | grep coverlet.collector || { echo "Coverlet installation failed"; exit 1;}

echo "Setup complete."