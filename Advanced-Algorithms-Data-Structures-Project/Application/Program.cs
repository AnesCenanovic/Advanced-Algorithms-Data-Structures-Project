@ -1,112 +1,418 @@
﻿using Application.Compression;
using Application.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

// === Configuration ===
const string GUTENBERG_DIR = @"C:\Users\anes.cenanovic\Documents\GitHub\Advanced-Algorithms-Data-Structures-Project\D184MB";
const string TRIAL_FILES_DIR = @".\trial_files";
const string RESULTS_DIR = @".\results";
const string RESULTS_JSON = @".\results\compression_results.json";

IFileService fileService = new FileService();
ILz77Compressor compressor = new Lz77Compressor();
ILz77Decompressor decompressor = new Lz77Decompressor();

while (true)
// Create directories
Directory.CreateDirectory(TRIAL_FILES_DIR);
Directory.CreateDirectory(RESULTS_DIR);

Console.WriteLine("=== LZ77 Compression Analysis Suite ===\n");
Console.WriteLine("1. Generate Trial Files");
Console.WriteLine("2. Run Compression Tests on Gutenberg Dataset");
Console.WriteLine("3. Run Compression Tests on Trial Files");
Console.WriteLine("4. Run Full Test Suite (Trial Files + Gutenberg)");
Console.WriteLine("5. Analyze Results");
Console.WriteLine("6. Exit");
Console.Write("\nSelect: ");

var choice = Console.ReadLine();

switch (choice)
{
    Console.Clear();
    Console.WriteLine("=== Binary LZ77 Compressor ===");
    Console.WriteLine("1. Compress File (Linear Search)");
    Console.WriteLine("2. Compress File (Hash Verification - LZHV)");
    Console.WriteLine("3. Decompress File");
    Console.WriteLine("4. Exit");
    Console.Write("Select: ");
    case "1":
        GenerateTrialFiles();
        break;
    case "2":
        RunCompressionTests(GUTENBERG_DIR, "gutenberg");
        break;
    case "3":
        RunCompressionTests(TRIAL_FILES_DIR, "trial");
        break;
    case "4":
        GenerateTrialFiles();
        RunCompressionTests(TRIAL_FILES_DIR, "trial");
        RunCompressionTests(GUTENBERG_DIR, "gutenberg");
        break;
    case "5":
        AnalyzeResults();
        break;
    case "6":
        return;
}

    var choice = Console.ReadLine();
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

    try
// ========================================
// TRIAL FILE GENERATION
// ========================================
void GenerateTrialFiles()
{
    Console.WriteLine("\n=== Generating Trial Files ===");

    var testCases = new[]
    {
        if (choice == "1" || choice == "2")
        {
            bool useHash = choice == "2";
            Console.Write("Enter file path: ");
            string inputPath = Console.ReadLine() ?? "";

            // 1. Read
            Console.WriteLine("Reading file...");
            byte[] inputData = fileService.ReadBytes(inputPath);
            Console.WriteLine($"Original Size: {inputData.Length} bytes");

            // 2. Compress (Measure Time)
            Console.WriteLine(useHash
                ? "Compressing with Hash Verification (LZHV)..."
                : "Compressing with Linear Search...");
            Stopwatch sw = Stopwatch.StartNew();

            // Window: 4096 (4KB), Lookahead: 255
            var result = useHash
                ? compressor.CompressWithHashVerification(inputData, 4096, 255)
                : compressor.Compress(inputData, 4096, 255);

            sw.Stop();

            // 3. Write
            string outputPath = inputPath + ".lz77";
            fileService.WriteBytes(outputPath, result.CompressedData);

            // 4. Report Stats
            long originalBytes = inputData.Length;
            long compressedBytes = result.CompressedData.Length;
            double ratio = ((double)compressedBytes / originalBytes) * 100;
            double saved = 100 - ratio;

            Console.WriteLine("\n=== Results ===");
            Console.WriteLine($"Method:          {(useHash ? "LZHV (Hash-based)" : "Linear Search")}");
            Console.WriteLine($"Time Taken:      {sw.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Compressed Size: {compressedBytes} bytes");
            Console.WriteLine($"Tokens Created:  {result.TokenCount}");

            if (compressedBytes < originalBytes)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Success! Reduced size to {ratio:F2}% (Saved {saved:F2}%)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File grew larger ({ratio:F2}%). (Normal for small or random files)");
            }
            Console.ResetColor();
            string fullPath = Path.GetFullPath(outputPath);
            Console.WriteLine($"Saved to: {fullPath}");
        }
        else if (choice == "3")
        // Small files to show overhead
        ("tiny_10b.txt", GenerateRandomText(10), "10 bytes - random"),
        ("tiny_50b.txt", GenerateRandomText(50), "50 bytes - random"),
        ("tiny_100b.txt", GenerateRandomText(100), "100 bytes - random"),
        ("small_500b.txt", GenerateRandomText(500), "500 bytes - random"),
        ("small_1kb.txt", GenerateRandomText(1024), "1KB - random"),
        
        // Highly repetitive content
        ("repeat_1kb.txt", GenerateRepetitiveText(1024, "abc"), "1KB - highly repetitive"),
        ("repeat_5kb.txt", GenerateRepetitiveText(5120, "hello world "), "5KB - highly repetitive"),
        ("repeat_10kb.txt", GenerateRepetitiveText(10240, "The quick brown fox "), "10KB - repetitive"),
        
        // Low repetition (random-like)
        ("random_1kb.txt", GenerateRandomText(1024), "1KB - low repetition"),
        ("random_5kb.txt", GenerateRandomText(5120), "5KB - low repetition"),
        ("random_10kb.txt", GenerateRandomText(10240), "10KB - low repetition"),
        
        // Natural language patterns
        ("natural_1kb.txt", GenerateNaturalText(1024), "1KB - natural prose"),
        ("natural_5kb.txt", GenerateNaturalText(5120), "5KB - natural prose"),
        ("natural_10kb.txt", GenerateNaturalText(10240), "10KB - natural prose"),
        
        // Mixed patterns
        ("mixed_5kb.txt", GenerateMixedText(5120), "5KB - mixed patterns"),
        ("mixed_20kb.txt", GenerateMixedText(20480), "20KB - mixed patterns"),
    };

    foreach (var (filename, content, description) in testCases)
    {
        string path = Path.Combine(TRIAL_FILES_DIR, filename);
        File.WriteAllText(path, content);
        Console.WriteLine($"✓ Created: {filename} ({description})");
    }

    Console.WriteLine($"\nGenerated {testCases.Length} trial files in {TRIAL_FILES_DIR}");
}

string GenerateRandomText(int targetBytes)
{
    var random = new Random(42); // Fixed seed for reproducibility
    var sb = new StringBuilder();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 \n";

    while (Encoding.UTF8.GetByteCount(sb.ToString()) < targetBytes)
    {
        sb.Append(chars[random.Next(chars.Length)]);
    }

    return sb.ToString().Substring(0, sb.Length - 1);
}

string GenerateRepetitiveText(int targetBytes, string pattern)
{
    var sb = new StringBuilder();
    while (Encoding.UTF8.GetByteCount(sb.ToString()) < targetBytes)
    {
        sb.Append(pattern);
    }
    return sb.ToString().Substring(0, Math.Min(sb.Length, targetBytes));
}

string GenerateNaturalText(int targetBytes)
{
    const string template = "The quick brown fox jumps over the lazy dog. " +
                          "A journey of a thousand miles begins with a single step. " +
                          "To be or not to be, that is the question. " +
                          "All that glitters is not gold. " +
                          "Better late than never. ";

    var sb = new StringBuilder();
    while (Encoding.UTF8.GetByteCount(sb.ToString()) < targetBytes)
    {
        sb.Append(template);
    }

    return sb.ToString().Substring(0, Math.Min(sb.Length, targetBytes));
}

string GenerateMixedText(int targetBytes)
{
    var random = new Random(42);
    var sb = new StringBuilder();
    var patterns = new[] { "abc", "xyz123", "hello world", GenerateRandomText(20) };

    while (Encoding.UTF8.GetByteCount(sb.ToString()) < targetBytes)
    {
        sb.Append(patterns[random.Next(patterns.Length)]);
        sb.Append(' ');
    }

    return sb.ToString().Substring(0, Math.Min(sb.Length, targetBytes));
}

// ========================================
// COMPRESSION TESTING
// ========================================
void RunCompressionTests(string directory, string datasetName)
{
    Console.WriteLine($"\n=== Running Compression Tests on {datasetName} ===");

    var files = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
    Console.WriteLine($"Found {files.Length} files");

    // Limit to 50 files for Gutenberg dataset
    if (datasetName == "gutenberg" && files.Length > 50)
    {
        files = files.Take(50).ToArray();
        Console.WriteLine($"Limited to first 50 files for testing\n");
    }
    else
    {
        Console.WriteLine();
    }

    var results = new List<CompressionResult>();
    int processed = 0;

    foreach (var filePath in files)
    {
        try
        {
            Console.Write("Enter .lz77 file path: ");
            string inputPath = Console.ReadLine() ?? "";
            processed++;
            Console.WriteLine($"[{processed}/{files.Length}] Processing: {Path.GetFileName(filePath)}");

            byte[] inputData = fileService.ReadBytes(filePath);
            long originalSize = inputData.Length;

            Console.WriteLine("Reading...");
            byte[] compressedData = fileService.ReadBytes(inputPath);
            // Test Linear Search
            var swLinear = Stopwatch.StartNew();
            var resultLinear = compressor.Compress(inputData, 4096, 255);
            swLinear.Stop();

            Console.WriteLine("Decompressing...");
            Stopwatch sw = Stopwatch.StartNew();
            // Test Hash Verification
            var swHash = Stopwatch.StartNew();
            var resultHash = compressor.CompressWithHashVerification(inputData, 4096, 255);
            swHash.Stop();

            byte[] restoredData = decompressor.Decompress(compressedData);
            // Calculate repetition score (simple heuristic)
            double repetitionScore = CalculateRepetitionScore(inputData);

            sw.Stop();
            var result = new CompressionResult
            {
                FileName = Path.GetFileName(filePath),
                DatasetName = datasetName,
                OriginalSize = originalSize,
                LinearCompressedSize = resultLinear.CompressedData.Length,
                LinearTokenCount = resultLinear.TokenCount,
                LinearTimeMs = swLinear.Elapsed.TotalMilliseconds,
                HashCompressedSize = resultHash.CompressedData.Length,
                HashTokenCount = resultHash.TokenCount,
                HashTimeMs = swHash.Elapsed.TotalMilliseconds,
                RepetitionScore = repetitionScore
            };

            string outputPath = inputPath.Replace(".lz77", "") + "_decoded.txt";
            fileService.WriteBytes(outputPath, restoredData);
            results.Add(result);

            Console.WriteLine("\n=== Results ===");
            Console.WriteLine($"Time Taken:      {sw.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Restored Size:   {restoredData.Length} bytes");
            Console.WriteLine($"Saved to:        {outputPath}");
            Console.WriteLine($"  Original: {FormatBytes(originalSize)}");
            Console.WriteLine($"  Linear:   {FormatBytes(result.LinearCompressedSize)} ({result.LinearCompressionRatio:F2}%) in {result.LinearTimeMs:F2}ms");
            Console.WriteLine($"  Hash:     {FormatBytes(result.HashCompressedSize)} ({result.HashCompressionRatio:F2}%) in {result.HashTimeMs:F2}ms");
            Console.WriteLine($"  Speedup:  {result.SpeedupFactor:F2}x faster with hash");
            Console.WriteLine();
        }
        else if (choice == "4")
        catch (Exception ex)
        {
            break;
            Console.WriteLine($"  ERROR: {ex.Message}\n");
        }
    }
    catch (Exception ex)

    // Save results
    SaveResults(results, datasetName);
    Console.WriteLine($"\n✓ Completed {processed} files. Results saved to {RESULTS_DIR}");
}

double CalculateRepetitionScore(byte[] data)
{
    // Simple repetition heuristic: count unique byte sequences
    var window = 10; // Look at 10-byte windows
    var uniqueWindows = new HashSet<string>();

    for (int i = 0; i <= data.Length - window; i++)
    {
        var windowStr = Convert.ToBase64String(data, i, Math.Min(window, data.Length - i));
        uniqueWindows.Add(windowStr);
    }

    // Higher score = more unique patterns = less repetition
    return Math.Max(1, data.Length - window) > 0
        ? 100.0 * uniqueWindows.Count / Math.Max(1, data.Length - window + 1)
        : 0;
}

void SaveResults(List<CompressionResult> results, string datasetName)
{
    string jsonPath = Path.Combine(RESULTS_DIR, $"{datasetName}_results.json");
    string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(jsonPath, json);

    // Also save CSV for easier analysis
    string csvPath = Path.Combine(RESULTS_DIR, $"{datasetName}_results.csv");
    var csv = new StringBuilder();
    csv.AppendLine("FileName,Dataset,OriginalSize,LinearCompressedSize,LinearRatio,LinearTimeMs,HashCompressedSize,HashRatio,HashTimeMs,SpeedupFactor,RepetitionScore");

    foreach (var r in results)
    {
        csv.AppendLine($"{r.FileName},{r.DatasetName},{r.OriginalSize},{r.LinearCompressedSize},{r.LinearCompressionRatio:F2},{r.LinearTimeMs:F2},{r.HashCompressedSize},{r.HashCompressionRatio:F2},{r.HashTimeMs:F2},{r.SpeedupFactor:F2},{r.RepetitionScore:F2}");
    }

    File.WriteAllText(csvPath, csv.ToString());
}

// ========================================
// RESULTS ANALYSIS
// ========================================
void AnalyzeResults()
{
    Console.WriteLine("\n=== Analyzing Results ===\n");

    var allResults = new List<CompressionResult>();

    // Load all result files
    foreach (var jsonFile in Directory.GetFiles(RESULTS_DIR, "*_results.json"))
    {
        string json = File.ReadAllText(jsonFile);
        var results = JsonSerializer.Deserialize<List<CompressionResult>>(json);
        if (results != null) allResults.AddRange(results);
    }

    if (allResults.Count == 0)
    {
        Console.WriteLine("No results found. Run tests first.");
        return;
    }

    Console.WriteLine($"Loaded {allResults.Count} test results\n");

    // Analysis 1: When is compression worthwhile?
    Console.WriteLine("=== Compression Effectiveness by File Size ===");
    var sizeGroups = allResults.GroupBy(r => GetSizeCategory(r.OriginalSize));
    foreach (var group in sizeGroups.OrderBy(g => g.Key))
    {
        var avgRatio = group.Average(r => r.LinearCompressionRatio);
        var worthwhile = group.Count(r => r.LinearCompressedSize < r.OriginalSize);
        Console.WriteLine($"{group.Key,-15} Avg Ratio: {avgRatio:F2}%  Worthwhile: {worthwhile}/{group.Count()}");
    }

    // Analysis 2: Repetition impact
    Console.WriteLine("\n=== Impact of Repetition on Compression ===");
    var repetitionGroups = allResults.GroupBy(r => GetRepetitionCategory(r.RepetitionScore));
    foreach (var group in repetitionGroups.OrderBy(g => g.Key))
    {
        Console.WriteLine($"Error: {ex.Message}");
        var avgRatio = group.Average(r => r.LinearCompressionRatio);
        Console.WriteLine($"{group.Key,-20} Avg Ratio: {avgRatio:F2}%");
    }

    Console.WriteLine("\nPress any key...");
    Console.ReadKey();
    // Analysis 3: Hash vs Linear performance
    Console.WriteLine("\n=== Hash Verification Performance ===");
    var avgSpeedup = allResults.Average(r => r.SpeedupFactor);
    var avgLinearTime = allResults.Average(r => r.LinearTimeMs);
    var avgHashTime = allResults.Average(r => r.HashTimeMs);
    Console.WriteLine($"Average Speedup: {avgSpeedup:F2}x");
    Console.WriteLine($"Linear Search:   {avgLinearTime:F2}ms avg");
    Console.WriteLine($"Hash Verify:     {avgHashTime:F2}ms avg");

    // Analysis 4: Best and worst compression
    Console.WriteLine("\n=== Best Compression Results ===");
    foreach (var r in allResults.OrderBy(r => r.LinearCompressionRatio).Take(5))
    {
        Console.WriteLine($"{r.FileName,-40} {r.LinearCompressionRatio:F2}% (saved {100 - r.LinearCompressionRatio:F2}%)");
    }

    Console.WriteLine("\n=== Worst Compression Results (File Growth) ===");
    foreach (var r in allResults.OrderByDescending(r => r.LinearCompressionRatio).Take(5))
    {
        Console.WriteLine($"{r.FileName,-40} {r.LinearCompressionRatio:F2}% (overhead: {r.LinearCompressionRatio - 100:F2}%)");
    }

    // Analysis 5: Break-even point
    Console.WriteLine("\n=== Compression Break-even Analysis ===");
    var breakEvenSize = allResults
        .Where(r => r.LinearCompressedSize >= r.OriginalSize)
        .OrderByDescending(r => r.OriginalSize)
        .FirstOrDefault();

    if (breakEvenSize != null)
    {
        Console.WriteLine($"Largest file that grew: {breakEvenSize.FileName} ({FormatBytes(breakEvenSize.OriginalSize)})");
    }

    var smallestSuccess = allResults
        .Where(r => r.LinearCompressedSize < r.OriginalSize)
        .OrderBy(r => r.OriginalSize)
        .FirstOrDefault();

    if (smallestSuccess != null)
    {
        Console.WriteLine($"Smallest successful compression: {smallestSuccess.FileName} ({FormatBytes(smallestSuccess.OriginalSize)})");
    }
}

string GetSizeCategory(long bytes)
{
    if (bytes < 100) return "< 100 B";
    if (bytes < 1024) return "100 B - 1 KB";
    if (bytes < 5120) return "1-5 KB";
    if (bytes < 10240) return "5-10 KB";
    if (bytes < 51200) return "10-50 KB";
    if (bytes < 102400) return "50-100 KB";
    if (bytes < 512000) return "100-500 KB";
    return "> 500 KB";
}

string GetRepetitionCategory(double score)
{
    if (score < 20) return "Very High Repetition";
    if (score < 40) return "High Repetition";
    if (score < 60) return "Medium Repetition";
    if (score < 80) return "Low Repetition";
    return "Very Low Repetition";
}

string FormatBytes(long bytes)
{
    string[] sizes = { "B", "KB", "MB", "GB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len /= 1024;
    }
    return $"{len:F2} {sizes[order]}";
}

// ========================================
// DATA CLASSES
// ========================================
class CompressionResult
{
    public string FileName { get; set; } = "";
    public string DatasetName { get; set; } = "";
    public long OriginalSize { get; set; }
    public long LinearCompressedSize { get; set; }
    public int LinearTokenCount { get; set; }
    public double LinearTimeMs { get; set; }
    public long HashCompressedSize { get; set; }
    public int HashTokenCount { get; set; }
    public double HashTimeMs { get; set; }
    public double RepetitionScore { get; set; }

    public double LinearCompressionRatio => (double)LinearCompressedSize / OriginalSize * 100;
    public double HashCompressionRatio => (double)HashCompressedSize / OriginalSize * 100;
    public double SpeedupFactor => LinearTimeMs / Math.Max(0.001, HashTimeMs);
}
