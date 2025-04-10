#!/bin/bash

# Run GitVersion and output properties to gitversion.props
gitversion_output=$(dotnet gitversion /output json)
echo "gitversion output: $gitversion_output"

# Run GitVersion and output properties to gitversion.props
gitversion_output=$($GITVERSION_COMMAND)
echo "gitversion output: $gitversion_output"

# Extract major, minor, and patch from AssemblySemVer
major=$(echo "$gitversion_output" | jq -r '.Major')
minor=$(echo "$gitversion_output" | jq -r '.Minor')
patch=$(echo "$gitversion_output" | jq -r '.Patch')

assembly_version="${major}.${minor}.${patch}.0"

# Extract major, minor, and patch from AssemblySemVer
major=$(echo "$gitversion_output" | jq -r '.Major')
minor=$(echo "$gitversion_output" | jq -r '.Minor')
patch=$(echo "$gitversion_output" | jq -r '.Patch')

assembly_version="${major}.${minor}.${patch}.0"

# writing debug info
echo "GitVersion output: $gitversion_output"
echo "AssemblyVersion: $assembly_version"
echo "Major: $major"
echo "Minor: $minor"
echo "Patch: $patch"


# Create and write the properties to gitversion.props
echo "<Project>" > gitversion.props
echo "  <PropertyGroup>" >> gitversion.props
echo "    <AssemblyVersion>${assembly_version}</AssemblyVersion>" >> gitversion.props
echo "    <FileVersion>$(echo "$gitversion_output" | jq -r '.SemVer')</FileVersion>" >> gitversion.props
echo "    <Version>$(echo "$gitversion_output" | jq -r '.SemVer')</Version>" >> gitversion.props
echo "    <InformationalVersion>$(echo "$gitversion_output" | jq -r '.InformationalVersion')</InformationalVersion>" >> gitversion.props
echo "  </PropertyGroup>" >> gitversion.props
echo "</Project>" >> gitversion.props

# Check if the file was created successfully
if [ -f gitversion.props ]; then
	echo "gitversion.props created successfully."
else
	echo "Failed to create gitversion.props."
	exit 1
fi

# Check if the file contains the expected content
if grep -q "<AssemblyVersion>${assembly_version}</AssemblyVersion>" gitversion.props; then
	echo "gitversion.props contains the expected AssemblyVersion."
else
	echo "gitversion.props does not contain the expected AssemblyVersion."
	exit 1
fi

# Check if the file contains the expected FileVersion
if grep -q "<FileVersion>$(echo "$gitversion_output" | jq -r '.SemVer')</FileVersion>" gitversion.props; then
	echo "gitversion.props contains the expected FileVersion."
else
	echo "gitversion.props does not contain the expected FileVersion."
	exit 1
fi

# Check if the file contains the expected Version
if grep -q "<Version>$(echo "$gitversion_output" | jq -r '.SemVer')</Version>" gitversion.props; then
	echo "gitversion.props contains the expected Version."
else
	echo "gitversion.props does not contain the expected Version."
	exit 1
fi

# Check if the file contains the expected InformationalVersion
if grep -q "<InformationalVersion>$(echo "$gitversion_output" | jq -r '.InformationalVersion')</InformationalVersion>" gitversion.props; then
	echo "gitversion.props contains the expected InformationalVersion."
else
	echo "gitversion.props does not contain the expected InformationalVersion."
	exit 1
fi

# Log the contents of the gitversion.props file
echo "Contents of gitversion.props:"
cat gitversion.props
echo "GitVersion script completed successfully."