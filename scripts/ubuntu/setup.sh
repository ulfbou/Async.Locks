#!/bin/bash
chmod +x "$0"

# Install dependencies
sudo apt-get update
sudo apt-get install -y jq
dotnet tool install --global GitVersion.Tool --version 6.2.0

# Verify installations
echo "Verifying jq installation..."
jq --version || { echo "jq installation failed"; exit 1; }
echo "Verifying GitVersion installation..."
gitversion --version || { echo "GitVersion installation failed"; exit 1; }
