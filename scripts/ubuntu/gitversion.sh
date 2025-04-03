#!/bin/bash

# Run GitVersion and output properties to gitversion.props
gitversion_output=$(dotnet gitversion /output json)
echo "AssemblyVersion=$(echo "$gitversion_output" | jq -r '.AssemblySemVer')" > gitversion.props
echo "FileVersion=$(echo "$gitversion_output" | jq -r '.SemVer')" >> gitversion.props
echo "InformationalVersion=$(echo "$gitversion_output" | jq -r '.InformationalVersion')" >> gitversion.props