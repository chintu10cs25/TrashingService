
using System.Diagnostics;
using System.Threading;
using TrashingService.Common;

namespace TrashingService;
public class TrashingProcessorAsync
{
    private CancellationTokenSource _cancellationTokenSource;
    public TrashingProcessorAsync()
    {
    }
    //public void Start(string[] args)
    //{

    //    string trashingDirectory = "/home/chintu/Trash_Batching/Trash/";
    //    string batchingDirectory = "/home/chintu/Batch/";
    //    string cmd = $"find {trashingDirectory} -type f > {batchingDirectory}f_Directory/batch1.txt";
    //    //Terminal terminal = new Terminal();
    //    //terminal.EnterCmd(cmd);
    //    HashSet<string> directories = new HashSet<string>();
    //    ListFiles(cmd);
    //    ProcesslistedFiles(directories);
    //    RemoveDirectories(directories);

    //}

    public void Start(string[] args)
    {
        string trashingDirectory = "/home/chintu/Trash3/";
        string batchingDirectory = "/home/chintu/Batch/";
        string cmd = $"find {trashingDirectory} -type f > {batchingDirectory}/batch.txt";
        HashSet<string> directories = new HashSet<string>();

        // Create a cancellation token source
        CancellationTokenSource cts = new CancellationTokenSource();

        // Register a console event handler to cancel the task when "Ctrl+C" is pressed

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent the console from closing
            Console.WriteLine("Cancellation requested. Cancelling ListFiles task...");
            cts.Cancel(); // Cancel the task
        };

        // Start the ListFiles task and pass in the cancellation token
        Task listFilesTask = ListFilesAsync(cmd, cts.Token);
        Task processFilesTask = ProcesslistedFilesAsync(directories);

        try
        {
            Console.WriteLine("Batching started");
            Console.WriteLine($"Press Ctrl+C to cancel the batching process");
            Task.WhenAll(listFilesTask, processFilesTask).Wait();
            RemoveDirectories(directories);
        }
        catch (AggregateException ex)
        {
            if (ex.InnerExceptions.Any(e => e is OperationCanceledException))
            {
                Console.WriteLine("ListFiles task was cancelled.");
            }
            else
            {
                Console.WriteLine($"ListFiles task threw an exception: {ex.InnerException}");
            }
        }

        // If the ListFiles task was cancelled, exit without processing files
        if (listFilesTask.IsCanceled)
        {
            return;
        }

        ProcesslistedFiles(directories);
        //RemoveDirectories(directories);
    }
    public void ListFiles(string command)
    {
        // create a new process
        Process process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
    }
    public async Task ListFilesAsync(string command, CancellationToken cancellationToken)
    {
        // create a new process
        Process process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        // Wait for the process to exit or cancellation to be requested
        try
        {
            await Task.Run(() =>
            {
                process.WaitForExit();
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Task {0} cancelled", "ListFiles");
                    cancellationToken.ThrowIfCancellationRequested();
                }
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            // handle the cancellation gracefully
            Console.WriteLine($"ListFilesAsync was cancelled: {ex.Message}");
            // kill the process and wait for it to exit
            process.Kill();
            await Task.Run(() => process.WaitForExit());
        }

    }
    public void RemoveDirectories(HashSet<string> directories)
    {
        IEnumerable<string> sortedDirectories = directories
    .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
    .OrderByDescending(tuple => tuple.depth)
    .Select(tuple => tuple.directory);

        foreach (string directory in sortedDirectories)
        {
            Directory.Delete(directory);
        }
    }
    public void ProcesslistedFiles(HashSet<string> directories)
    {
        // Create a sorted set to store the lines
        //SortedSet<string> directories = new SortedSet<string>(new LineLengthComparer());
        //HashSet<string> directories = new HashSet<string> ();
        // List<string> directories = new List<string>();

        string filePath = "/home/chintu/Batch/f_Directory/test.txt";
        long currentPosition = 0;
        bool hasContent = false;
        bool hasNewLines = false;

        // Check if file has content to process
        using (StreamReader reader = new StreamReader(filePath))
        {
            hasContent = !reader.EndOfStream;
        }

        if (hasContent)
        {
            Console.WriteLine("Processing file...");

            while (true)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream))
                {
                    stream.Seek(currentPosition, SeekOrigin.Begin);

                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            Console.WriteLine(line);
                            File.Delete(line);
                            string? directory = Path.GetDirectoryName(line);
                            if (!string.IsNullOrEmpty(directory))
                            {
                                Console.WriteLine($"{directory}");
                                bool flag = directories.Add($"{directory}");
                            }

                            hasNewLines = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e.Message}");
                        }

                    }

                    currentPosition = stream.Position;
                }

                if (!hasNewLines)
                {
                    Console.WriteLine("No new lines found. Waiting for new lines...");
                }

                // Reset flag for new lines
                hasNewLines = false;
                // Wait for new lines to be added
                Thread.Sleep(1000);
            }
        }
        else
        {
            Console.WriteLine("File is empty. No content to process.");
        }

    }
    public async Task ProcesslistedFilesAsync(HashSet<string> directories)
    {
        string filePath = "/home/chintu/Batch/f_Directory/test.txt";
        long currentPosition = 0;
        bool hasContent = false;
        bool hasNewLines = false;

        // Check if file has content to process
        using (StreamReader reader = new StreamReader(filePath))
        {
            hasContent = !reader.EndOfStream;
        }

        if (hasContent)
        {
            Console.WriteLine("Processing file...");

            while (true)
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream))
                {
                    stream.Seek(currentPosition, SeekOrigin.Begin);

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        try
                        {
                            Console.WriteLine(line);
                            string? directory = Path.GetDirectoryName(line);
                            if (!string.IsNullOrEmpty(directory))
                            {
                                Console.WriteLine($"{directory}");
                                bool flag = directories.Add($"{directory}");
                            }

                            hasNewLines = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e.Message}");
                        }

                    }

                    currentPosition = stream.Position;
                }

                if (!hasNewLines)
                {
                    Console.WriteLine("No new lines found. Waiting for new lines...");
                }

                // Reset flag for new lines
                hasNewLines = false;
                // Wait for new lines to be added
                await Task.Delay(1000);
            }
        }
        else
        {
            Console.WriteLine("File is empty. No content to process.");
        }
    }
    //private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    //{
    //    Console.WriteLine("Cancellation requested.");

    //    // Cancel the ListFilesAsync task if it is running
    //    if (!listFilesTask.IsCompleted)
    //    {
    //        cts.Cancel();
    //        Console.WriteLine("ListFilesAsync task cancelled.");
    //    }

    //    e.Cancel = true;
    //}

}