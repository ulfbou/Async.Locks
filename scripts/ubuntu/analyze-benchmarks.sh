#!/bin/bash

# Parameters
BENCHMARK_RESULTS_FILE="Async.Locks.Benchmarks/BenchmarkDotNet.Artifacts/results/Async.Lock.Benchmarks.AsyncLockBenchmarks-net9.0.json"
BASELINE_FILE="benchmarks_baseline.json"
REGRESSION_THRESHOLD=0.05 # 5%
IMPROVEMENT_THRESHOLD=-0.03 # -3% (optional: flag significant improvements)
STATISTICAL_SIGNIFICANCE_ALPHA=0.05 # Alpha level for t-test

# Helper Functions

calculate_percentage_change() {
    local new_value=$1
    local baseline_value=$2
    if (( $(echo "$baseline_value == 0" | bc -l) )); then
        echo 0
        return
    fi
    echo "($new_value - $baseline_value) / $baseline_value" | bc -l
}

is_statistically_significant() {
    local current_mean=$1
    local current_stddev=$2
    local current_n=$3
    local baseline_mean=$4
    local baseline_stddev=$5
    local baseline_n=$6

    # Welch's t-test (for unequal variances and sample sizes)
    local numerator=$(echo "($current_mean - $baseline_mean)" | bc -l)
    local term1=$(echo "($current_stddev^2) / $current_n" | bc -l)
    local term2=$(echo "($baseline_stddev^2) / $baseline_n" | bc -l)
    local denominator_squared=$(echo "$term1 + $term2" | bc -l)
    if (( $(echo "$denominator_squared == 0" | bc -l) )); then
        echo "false" # Cannot calculate t-statistic
        return
    fi
    local t_statistic=$(echo "$numerator / sqrt($denominator_squared)" | bc -l)

    # Approximate degrees of freedom (Welch-Satterthwaite equation)
    local num1=$(echo "$term1 + $term2" | bc -l)
    local den1=$(echo "($term1^2) / ($current_n - 1)" | bc -l)
    local den2=$(echo "($term2^2) / ($baseline_n - 1)" | bc -l)
    local degrees_of_freedom=$(echo "$num1^2 / ($den1 + $den2)" | bc -l)

    # Placeholder for t-distribution CDF (requires external tools like R or Python for accurate calculation)
    # For a simplified approach, we can use a large enough df to approximate a standard normal distribution
    local critical_value=$(echo "1.96" | bc -l) # Approx. for alpha=0.05, two-tailed

    if (( $(echo "abs($t_statistic) > $critical_value" | bc -l) )); then
        echo "true"
    else
        echo "false"
    fi
}

analyze_benchmark() {
    local benchmark_name=$1
    local current_mean=$2
    local current_stddev=$3
    local current_n=$4
    local baseline_data=$(jq -r ".$benchmark_name" "$BASELINE_FILE")

    if [[ -z "$baseline_data" ]]; then
        echo "Warning: No baseline data found for benchmark '$benchmark_name'."
        return
    fi

    local baseline_mean=$(echo "$baseline_data" | jq -r ".Mean")
    local baseline_stddev=$(echo "$baseline_data" | jq -r ".StandardDeviation")
    local baseline_n=$(echo "$baseline_data" | jq -r ".N")

    percentage_change=$(calculate_percentage_change "$current_mean" "$baseline_mean")

    significant=$(is_statistically_significant "$current_mean" "$current_stddev" "$current_n" "$baseline_mean" "$baseline_stddev" "$baseline_n")

    echo "Analyzing '$benchmark_name':"
    echo "  Current Mean: $current_mean"
    echo "  Baseline Mean: $baseline_mean"
    echo "  Percentage Change: $(echo "$percentage_change * 100" | bc -l)%"

    if [[ "$significant" == "true" ]]; then
        if (( $(echo "$percentage_change > $REGRESSION_THRESHOLD" | bc -l) )); then
            echo "::error::Significant REGRESSION detected in '$benchmark_name': Percentage change = $(echo "$percentage_change * 100" | bc -l)%"
            exit 1
        elif (( $(echo "$percentage_change < $IMPROVEMENT_THRESHOLD" | bc -l) )); then
            echo "::notice::Significant IMPROVEMENT detected in '$benchmark_name': Percentage change = $(echo "$percentage_change * 100" | bc -l)%"
        else
            echo "  No significant performance change detected."
        fi
    else
        echo "  Performance change is not statistically significant."
    fi
    echo ""
}

# Load baseline data from JSON file
if [[ ! -f "$BASELINE_FILE" ]]; then
    echo "Warning: Baseline file '$BASELINE_FILE' not found. Skipping thorough analysis."
    exit 0
fi

# Parse BenchmarkDotNet results
if [[ ! -f "$BENCHMARK_RESULTS_FILE" ]]; then
    echo "Error: Benchmark results file not found: $BENCHMARK_RESULTS_FILE"
    exit 1
fi

benchmarks=$(jq -r '.[] | .DisplayInfo' "$BENCHMARK_RESULTS_FILE")

# Analyze each benchmark
echo "$benchmarks" | while IFS= read -r benchmark_display_info; do
    current_data=$(jq -r ".[] | select(.DisplayInfo == \"$benchmark_display_info\") | {Mean: .Statistics.Mean, StandardDeviation: .Statistics.StandardDeviation, N: .Statistics.N}" "$BENCHMARK_RESULTS_FILE")

    if [[ -n "$current_data" ]]; then
        current_mean=$(echo "$current_data" | jq -r ".Mean")
        current_stddev=$(echo "$current_data" | jq -r ".StandardDeviation")
        current_n=$(echo "$current_data" | jq -r ".N")
        analyze_benchmark "$benchmark_display_info" "$current_mean" "$current_stddev" "$current_n"
    else
        echo "Warning: Could not find data for benchmark '$benchmark_display_info' in results."
    fi
done

echo "Thorough benchmark analysis complete."