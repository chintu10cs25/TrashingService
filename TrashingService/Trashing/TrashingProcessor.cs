
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using TrashingService.Common;

namespace TrashingService;
public class TrashingProcessor
{
    //private const string W_TASK = "the Task to assign deletion process";
    private const string W_DELETION_PROCESS = "the Deletion Process";
    private const string W_CREATE_BATCH = "the creation of batch";
    private const string W_FILE_DELETION = "the deletion of files";
    private const string W_DIRECTORY_DELETION = "the deletion of directories";
    private readonly ILogger<TrashingProcessor> _logger;
    private readonly Terminal _terminal;
    public TrashingProcessor(ILogger<TrashingProcessor> logger, Terminal terminal)
    {
        _logger = logger;
        _terminal = terminal;
    }
    public void Start()
    {
        // CreateBatch($"/home/chintu/data1/trash", $"/var/e2/data1_batch.txt");
        //DeletionUsingCSharp();
        Dictionary<string,Task> tasks = new Dictionary<string,Task>();
        Dictionary<string, string> trashDirectories = GetTrashingDirectories();
        if (trashDirectories != null)
        {
            foreach (var trashDirectory in trashDirectories)
            {
                CheckAvgLoad(W_DELETION_PROCESS, trashDirectory.Key);
                var task = Task.Factory.StartNew(() =>
                {
                    DeletionProcess(trashDirectory.Key, trashDirectory.Value);
                }, TaskCreationOptions.LongRunning);

                _logger.LogInformation($"Deletion process for {@trashDirectory.Key} is started on Task:{task.Id}");
                tasks.Add(trashDirectory.Key, task);
                //task.Wait();
            }
        }
        Task.WaitAll(tasks.Values.ToArray());
        //Delete batch files
        if (trashDirectories != null)
        {
            foreach (var trashDirectory in trashDirectories)
            {
                File.Delete(trashDirectory.Value);
                _logger.LogInformation($"{trashDirectory.Value} is deleted");
            }
        }             
    }
    private void DeletionProcess(string trashingDirectory, string batchFilePath)
    {
        try
        {

            _logger.LogInformation("Starting deletion process..." );
            CheckAvgLoad(W_CREATE_BATCH,trashingDirectory);
            CreateBatch(trashingDirectory, batchFilePath);
            CheckAvgLoad(W_FILE_DELETION,trashingDirectory);
            HashSet<string> directories = ProcessBatch(trashingDirectory ,batchFilePath);
            CheckAvgLoad(W_DIRECTORY_DELETION,trashingDirectory);
            RemoveDirectories(directories, trashingDirectory);
            _logger.LogInformation("Deletion process completed." );
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"An error occurred while running the deletion process: {ex.Message}" );
        }
    }
    //List all the files from trashing directory to a batch file
    private void CreateBatch(string trashingDirectory,string batchFilePath)
    {
        //string trashingDirectory = "/home/chintu/Trash_Batching/Trash/";
        //string batchingDirectory = "/home/chintu/Batch/batch.txt";
        
        _logger.LogInformation("Started batching process" );
        if (string.IsNullOrEmpty(batchFilePath)|| string.IsNullOrEmpty(trashingDirectory))
        {
            return;
        }

        string cmd = $"find {trashingDirectory} -type f > {batchFilePath}";
        //string cmd = $"find {trashingDirectory} -type f -delete -printf %h\n > {batchFilePath}";
        _terminal.EnterCmd(cmd);
        _logger.LogInformation("Batching process completed" );
    }
    //Delete files and extract directory by reading batch file
    private HashSet<string> ProcessBatch(string trashingDirectory,string batchFilePath)
    {
        _logger.LogInformation($"Processing batch file {batchFilePath} and started file deletion from {trashingDirectory}" );
        // Create a set to store the unique lines
        HashSet<string> directories = new HashSet<string>();
        //string filePath = "/home/chintu/Batch/f_Directory/test.txt";

        using (FileStream stream = new FileStream(batchFilePath, FileMode.Open, FileAccess.Read))
        using (StreamReader reader = new StreamReader(stream))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    //Console.WriteLine(line);
                    File.Delete(line);
                    string? directory = Path.GetDirectoryName(line);
                    if (!string.IsNullOrEmpty(directory) && (directory!= trashingDirectory))
                    {
                        //Console.WriteLine($"{directory}");
                        bool flag = directories.Add($"{directory}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
            }
        }

        _logger.LogInformation($"Batch file {batchFilePath} is processed and File deletion from {trashingDirectory} is completed" );
        return directories;
    }
    //Remove empty directories
    private void RemoveDirectories(HashSet<string> directories,string trashingDirectory)
    {
        _logger.LogInformation($"Started emoving directories from {trashingDirectory}" );
            IEnumerable<string> sortedDirectories = directories
        .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        .OrderByDescending(tuple => tuple.depth)
        .Select(tuple => tuple.directory);

        foreach (string directory in sortedDirectories)
        {
            if(directory!= trashingDirectory)
            {
                Directory.Delete(directory);
            }
           
        }
        _logger.LogInformation($"Completed removing directories from {trashingDirectory}" );
    }  
    //get dictionary of trashing directory and batchFilePath
    private Dictionary<string,string> GetTrashingDirectories()
    {
        _logger.LogInformation("Scanning trash directories......");
        Dictionary<string,string> result = new Dictionary<string, string>();
       
        //string lsCommand = $"ls";
        string homePathcmd = $"eval echo ~$USER";
        //erminal terminal = new Terminal();
        string homePath = _terminal.Enter(homePathcmd).Trim();
        string lsCommand = $"/bin/ls -dm {homePath}/data*/ ";
        string lsresult=_terminal.Enter(lsCommand,$"{homePath}/");
        string[] mainDirectories = lsresult.Split(new char[] { ',' });
        foreach (string directory in mainDirectories) 
        {
            string trimDir = directory.Trim();
            if (Directory.Exists(trimDir))
            {
                string trashDirectory = Path.Combine(trimDir, "trash");
                if (Directory.Exists(trashDirectory))
                {
                    string lastFolderName = new DirectoryInfo(trimDir).Name;
                    //string batchFilePath = $"/var/e2/{lastFolderName}_batch.txt";
                    string batchDir = $"{homePath}/Batch";
                    if (!Directory.Exists(batchDir))
                    {
                        Directory.CreateDirectory(batchDir);
                    }
                    string batchFilePath = $"{batchDir}/{lastFolderName}_batch.txt";
                    result.Add(trashDirectory, batchFilePath);
                }
            }
        }
        _logger.LogInformation("Scanning trash directories completed.......");
        return result;
    }
    //keeps checking and Wait until the system load average falls below the maximum threshold
    private void CheckAvgLoad(string process,string trashingDirectory)
    {
        //As a general rule, if your machine has 4 cores and you want to keep some overhead for other processes, you can set the threshold to a value between 3.0 to 4.0
        // Set the maximum system load threshold to the number of processor cores
        _logger.LogInformation($"Checking avg load before starting {process} for {trashingDirectory}" );
        double maxLoad = Environment.ProcessorCount;

        // Get the current system load average 
        string averageLoadCommand = "uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}'";

        string avgLoadoutput = _terminal.Enter(averageLoadCommand);
        if (!double.TryParse(avgLoadoutput, out double avgLoad))
        {
            throw new Exception($"Failed to parse the average load value from the command output: {avgLoadoutput}");
        }

        // Wait until the system load average falls below the maximum threshold
        while (avgLoad >= maxLoad)
        {
            _logger.LogInformation($"System load ({avgLoad}) is greater than or equal to the maximum threshold ({maxLoad}). Waiting for {process} for {trashingDirectory} for 1 minute before checking again...{Task.CurrentId}" );
            Thread.Sleep(TimeSpan.FromMinutes(1));

            avgLoadoutput = _terminal.Enter(averageLoadCommand);
            if (!double.TryParse(avgLoadoutput, out avgLoad))
            {
                throw new Exception($"Failed to parse the average load value from the command output: {avgLoadoutput}");
            }
        }

        _logger.LogInformation($"System load ({avgLoad}) is below the maximum threshold ({maxLoad}) starting {process} for {trashingDirectory}" );
    }
    private static void ProcesBatchWatcher()
    {
        string filePath = "/home/chintu/Batch/f_Directory/test.txt";
        long currentPosition = 0;

        using (var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath)))
        {
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += (s, e) =>
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream))
                {
                    stream.Seek(currentPosition, SeekOrigin.Begin);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }

                    currentPosition = stream.Position;
                }
            };

            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Now watching file: {filePath}");
            Console.ReadLine();
        }
    }
    public void DeleteUsingLs(string basePath)
    {
        string lsCommand = $"/bin/ls -d {basePath}*/";
        string output = _terminal.EnterCmd(lsCommand);
        if (string.IsNullOrWhiteSpace(output))
        {
            //string fileoutput = terminal.EnterCmd("/bin/ls -f " + basePath);
            //delete files from directory which won't have subdirctories
            //use rm command instead of find and delete
            //terminal.Enter($"find {basePath} -type f -delete && rm -r {basePath}");
            //using rm -rf because it is leaf node and avoiding rm and rmdir to use it unnecessarily
            string rm = $"rm -rf {basePath}";
            _terminal.EnterCmd(rm);
        }
        else
        {
            string[] paths = output.Trim().Split("\n");
            foreach (var path in paths)
            {
                //keep calling till there is no subdirectories
                DeleteUsingLs(path);
            }
            //delete files after clearing the subdirectories
            //terminal.Enter($"find {basePath} -type f -delete && rm -r {basePath}");
            string rm = $"rm -rf {basePath}";
            _terminal.EnterCmd(rm);
        }
    }
    private void DeletionUsingCSharp()
    {
        var blockingCollection = new BlockingCollection<string>();
        // Create a set to store the unique lines
        Stopwatch sw = new Stopwatch();
        sw.Start();
        long fileCount = 0;
        long directoriesCount= 0;
       // HashSet<string> directories = new HashSet<string>();
        Console.WriteLine("Deletion of files Started");
        Parallel.ForEach(Directory.EnumerateFileSystemEntries($"/home/chintu/data1/trash", "*", SearchOption.AllDirectories), item =>
        {
            if (File.Exists(item))
            {
                File.Delete(item);
                //Console.WriteLine(item);
                fileCount++;
            }
            else if (Directory.Exists(item))
            {
                blockingCollection.Add(item);
                //directories.Add(item);
                directoriesCount++;
            }
        });
        Console.WriteLine("Deletion of files completed.......................");
        sw.Stop();
        Console.WriteLine($"Time stamp:{sw.Elapsed}");
        blockingCollection.CompleteAdding();

        IEnumerable<string> sortedDirectories = blockingCollection
        .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        .OrderByDescending(tuple => tuple.depth)
        .Select(tuple => tuple.directory);

        Console.WriteLine("Deletion of directories Started...........");

        foreach (var dir in sortedDirectories)
        {
            Directory.Delete(dir);
            Console.WriteLine(dir);
        }

        //Parallel.ForEach(blockingCollection
        //.Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        //.OrderByDescending(tuple => tuple.depth)
        //.Select(tuple => tuple.directory),
        //    item =>
        //    {
        //        Directory.Delete(item);

        //        Console.WriteLine(item);

        //    });
        Console.WriteLine("Deletion of directories Completed...........");

    }
}