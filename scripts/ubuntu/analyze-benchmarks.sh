#!/bin/bash

# Parameters
BENCHMARK_RESULTS_FILE="Async.Locks.Benchmarks/BenchmarkDotNet.Artifacts/results/Async.Lock.Benchmarks.AsyncLockBenchmarks-net9.0.json"
REGRESSION_THRESHOLD=0.10 # 10%

# Function to calculate percentage change
calculate_percentage_change() {
    local new_value=$1
    local baseline_value=$2
    if (( $(echo "$baseline_value == 0" | bc -l) )); then
        echo 0
        return
    fi
    echo "($new_value - $baseline_value) / $baseline_value" | bc -l
}

# Function to check for regression
check_for_regression() {
    local benchmark_name=$1
    local current_mean=$2
    local baseline_mean=$3
    percentage_change=$(calculate_percentage_change $current_mean $baseline_mean)
    if (( $(echo "$percentage_change > $REGRESSION_THRESHOLD" | bc -l) )); then
        echo "Error: Regression detected in benchmark '$benchmark_name': Percentage change = $(echo "$percentage_change * 100" | bc -l)%"
        exit 1
    fi
}

# Parse BenchmarkDotNet results
if [[ ! -f "$BENCHMARK_RESULTS_FILE" ]]; then
    echo "Error: Benchmark results file not found: $BENCHMARK_RESULTS_FILE"
    exit 1
fi

results=$(jq -r '.[] | {DisplayInfo: .DisplayInfo, Statistics: .Statistics.Mean}' $BENCHMARK_RESULTS_FILE)

# Baseline values
declare -A baseline_means=(
    ["ConcurrentAcquireReleaseBenchmark"]=10.0
    ["CancellationBenchmark"]=20.0
    ["PartialCancellationBenchmark"]=30.0
    ["ConcurrentCancellationBenchmark"]=40.0
    ["MixedLockAcquisitionBenchmark"]=50.0
)

# Check for regressions
echo "$results" | while read -r line; do
    benchmark_name=$(echo "$line" | jq -r '.DisplayInfo')
    current_mean=$(echo "$line" | jq -r '.Statistics')

    if [[ -n "${baseline_means[$benchmark_name]}" ]]; then
        baseline_mean=${baseline_means[$benchmark_name]}
        check_for_regression "$benchmark_name" "$current_mean" "$baseline_mean"
    fi
done

echo "Benchmark results analysis complete. No regressions detected."
