#!/bin/bash

# Define the project root within the container
export PROJECT_ROOT="/github/workspace"

# Define the local repository path on your machine
export LOCAL_REPO_PATH="C:/Users/Uffe/Source/repos/Async/Locks"

# Define your project paths using the PROJECT_ROOT variable
export BUILD_OUTPUT_DIR="${PROJECT_ROOT}/Src/bin/Release/net9.0"
export TEST_RESULTS_DIR="${PROJECT_ROOT}/TestResults"
export GIT_VERSION_PROPS="${PROJECT_ROOT}/gitversion.props"
export BENCHMARKS_DIR="${PROJECT_ROOT}/Benchmarks/Async.Locks.Benchmarks.csproj"
export BENCHMARK_RESULTS_DIR="${PROJECT_ROOT}/Benchmarks/BenchmarkDotNet.Artifacts/results"
export TEST_PROJECT="${PROJECT_ROOT}/Tests/Async.Locks.Tests.csproj"
export SRC_PROJECT="${PROJECT_ROOT}/Src/Async.Locks.csproj"
export SCRIPTS_DIR="${PROJECT_ROOT}/scripts"
export UBUNTU_SCRIPTS_DIR="${SCRIPTS_DIR}/ubuntu"
