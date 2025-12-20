using Application.Compression;
using Application.Services;
using System.Diagnostics;

IFileService fileService = new FileService();
ILz77Compressor compressor = new Lz77Compressor();
ILz77Decompressor decompressor = new Lz77Decompressor();

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Binary LZ77 Compressor ===");
    Console.WriteLine("1. Compress File (Linear Search)");
    Console.WriteLine("2. Compress File (Hash Verification - LZHV)");
    Console.WriteLine("3. Decompress File");
    Console.WriteLine("4. Exit");
    Console.Write("Select: ");

    var choice = Console.ReadLine();

    try
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
        {
            Console.Write("Enter .lz77 file path: ");
            string inputPath = Console.ReadLine() ?? "";

            Console.WriteLine("Reading...");
            byte[] compressedData = fileService.ReadBytes(inputPath);

            Console.WriteLine("Decompressing...");
            Stopwatch sw = Stopwatch.StartNew();

            byte[] restoredData = decompressor.Decompress(compressedData);

            sw.Stop();

            string outputPath = inputPath.Replace(".lz77", "") + "_decoded.txt";
            fileService.WriteBytes(outputPath, restoredData);

            Console.WriteLine("\n=== Results ===");
            Console.WriteLine($"Time Taken:      {sw.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Restored Size:   {restoredData.Length} bytes");
            Console.WriteLine($"Saved to:        {outputPath}");
        }
        else if (choice == "4")
        {
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine("\nPress any key...");
    Console.ReadKey();
}