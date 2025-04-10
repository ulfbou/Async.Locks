FROM mcr.microsoft.com/dotnet/sdk:9.0.203

# Install wget if it's not already present
RUN apt-get update && apt-get install -y wget --no-install-recommends

# Install Node.js and npm
RUN apt-get update && apt-get install -y nodejs npm

# Install jq and add its directory to PATH (common location)
RUN apt-get update && apt-get install -y jq
ENV PATH="/usr/local/bin:${PATH}"

# Set the working directory
WORKDIR /github/workspace

# Install GitVersion.Tool globally within the container
RUN dotnet tool install --global GitVersion.Tool

# Download nuget.exe
RUN wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -P /usr/local/bin/

# Make nuget.exe executable
RUN chmod +x /usr/local/bin/nuget.exe

# Set environment variables for .NET SDK and tools
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools:/root/.dotnet/tools:/usr/local/bin"

# Set environment variables for paths
ENV PROJECT_ROOT=/github/workspace
ENV SRC_PROJECT=${PROJECT_ROOT}/src/Async.Locks/Async.Locks.csproj
ENV TEST_PROJECT=${PROJECT_ROOT}/Tests/Async.Locks.Tests/Async.Locks.Tests.csproj
ENV BENCHMARKS_DIR=${PROJECT_ROOT}/Benchmarks
ENV TEST_RESULTS_DIR=${PROJECT_ROOT}/TestResults
ENV GIT_VERSION_PROPS=${PROJECT_ROOT}/gitversion.props
ENV BUILD_OUTPUT_DIR=${PROJECT_ROOT}/build-output
ENV BENCHMARK_RESULTS_DIR=${PROJECT_ROOT}/benchmark-results
ENV UBUNTU_SCRIPTS_DIR=${PROJECT_ROOT}/scripts/ubuntu
ENV SCRIPTS_DIR=${PROJECT_ROOT}/scripts

# Copy the rest of your application code
COPY . .
