#!/bin/bash
set -e

echo "Starting setup script..."

# Install dependencies
sudo apt-get update
sudo apt-get install -y jq

# Install GitVersion (latest stable)
echo "Installing latest stable GitVersion..."
dotnet tool install --global GitVersion.Tool

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
