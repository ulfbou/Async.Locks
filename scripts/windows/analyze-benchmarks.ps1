# Parameters
$benchmarkResultsFile = "Async.Locks.Benchmarks/BenchmarkDotNet.Artifacts/results/Async.Lock.Benchmarks.AsyncLockBenchmarks-net9.0.json"
$regressionThreshold = 0.10 # 10%

# Function to calculate percentage change
function CalculatePercentageChange {
    param (
        [double]$newValue,
        [double]$baselineValue
    )
    if ($baselineValue -eq 0) {
        return 0 # Avoid division by zero
    }
    return (($newValue - $baselineValue) / $baselineValue)
}

# Function to check for regression
function CheckForRegression {
    param (
        [string]$benchmarkName,
        [double]$currentMean,
        [double]$baselineMean
    )
    $percentageChange = CalculatePercentageChange -newValue $currentMean -baselineValue $baselineMean
    if ($percentageChange -gt $regressionThreshold) {
        Write-Error "Regression detected in benchmark '$benchmarkName': Percentage change = $($percentageChange * 100)%"
        exit 1
    }
}

# Parse BenchmarkDotNet results
try {
    $results = Get-Content -Raw $benchmarkResultsFile | ConvertFrom-Json
} catch {
    Write-Error "Failed to parse benchmark results: $_"
    exit 1
}

# Baseline values (replace with actual baseline values)
$baselineMeans = @{
    "ConcurrentAcquireReleaseBenchmark" = 10.0 # Example baseline in milliseconds
    "CancellationBenchmark" = 20.0 # Example baseline in milliseconds
    "PartialCancellationBenchmark" = 30.0
    "ConcurrentCancellationBenchmark" = 40.0
    "MixedLockAcquisitionBenchmark" = 50.0
}

# Check for regressions
foreach ($result in $results) {
    $benchmarkName = $result.DisplayInfo
    $currentMean = $result.Statistics.Mean.Value

    if ($baselineMeans.ContainsKey($benchmarkName)) {
        $baselineMean = $baselineMeans[$benchmarkName]
        CheckForRegression -benchmarkName $benchmarkName -currentMean $currentMean -baselineMean $baselineMean
    }
}

Write-Host "Benchmark results analysis complete. No regressions detected."
