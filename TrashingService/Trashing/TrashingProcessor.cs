
using System.Diagnostics;
using System.Threading;
using TrashingService.Common;

namespace TrashingService;
public class TrashingProcessor
{
    public TrashingProcessor()
    {
    }
    public void Start()
    {
       // CreateBatch($"/home/chintu/data1/trash", $"/var/e2/data1_batch.txt");

        Dictionary<string,Task> tasks = new Dictionary<string,Task>();
        Dictionary<string, string> trashDirectories = GetTrashingDirectories();
        if (trashDirectories != null)
        {
            foreach (var trashDirectory in trashDirectories)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    DeletionProcess(trashDirectory.Key, trashDirectory.Value);
                }, TaskCreationOptions.LongRunning);

                Console.WriteLine($"Deletion process for {trashDirectory.Key} is started on Task:{task.Id}");
                tasks.Add(trashDirectory.Key, task);
            }
        }
        Task.WaitAll(tasks.Values.ToArray());
              
    }
    private static void DeletionProcess(string trashingDirectory, string batchFilePath)
    {
        try
        {
            CheckAvgLoad();
            Console.WriteLine("Starting deletion process...");
            CreateBatch(trashingDirectory, batchFilePath);
            HashSet<string> directories = ProcessBatch(batchFilePath);
            RemoveDirectories(directories);
            Console.WriteLine("Deletion process completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while running the deletion process: {ex.Message}");
        }
    }

    //List all the files from trashing directory to a batch file
    private static void CreateBatch(string trashingDirectory,string batchFilePath)
    {
        //string trashingDirectory = "/home/chintu/Trash_Batching/Trash/";
        //string batchingDirectory = "/home/chintu/Batch/batch.txt";
        if (string.IsNullOrEmpty(batchFilePath)|| string.IsNullOrEmpty(trashingDirectory))
        {
            return;
        }

        string cmd = $"find {trashingDirectory} -type f > {batchFilePath}";
        Terminal terminal = new Terminal();
        terminal.EnterCmd(cmd);
    }

    //Delete files and extract directory by reading batch file
    private static HashSet<string> ProcessBatch(string batchFilePath)
    {
        // Create a set to store the unique lines
        HashSet<string> directories = new HashSet<string>();
        Console.WriteLine("Processing file...");
        //string filePath = "/home/chintu/Batch/f_Directory/test.txt";

        using (FileStream stream = new FileStream(batchFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(stream))
        {
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
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }

            }

        }
        return directories;
    }
    //Remove empty directories
    private static void RemoveDirectories(HashSet<string> directories)
    {
        Console.WriteLine("Removing directories");
            IEnumerable<string> sortedDirectories = directories
        .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        .OrderByDescending(tuple => tuple.depth)
        .Select(tuple => tuple.directory);

        foreach (string directory in sortedDirectories)
        {
            Directory.Delete(directory);
        }
    }  

    //get dictionary of trashing directory and batchFilePath
    private static Dictionary<string,string> GetTrashingDirectories()
    {
        Dictionary<string,string> result = new Dictionary<string, string>();
       
        //string lsCommand = $"ls";
        string homePathcmd = $"eval echo ~$USER";
        Terminal terminal = new Terminal();
        string homePath = terminal.Enter(homePathcmd).Trim();
        string lsCommand = $"/bin/ls -dm {homePath}/data*/ ";
        string lsresult=terminal.Enter(lsCommand,$"{homePath}/");
        string[] mainDirectories = lsresult.Split(new char[] { ',' });
        foreach (string directory in mainDirectories) 
        {
            string trimDir = directory.Trim();
            if (Directory.Exists(trimDir))
            {
                string trashDirectory = Path.Combine(trimDir, "Trash");
                if (Directory.Exists(trashDirectory))
                {
                    string lastFolderName = new DirectoryInfo(trimDir).Name;
                    //string batchFilePath = $"/var/e2/{lastFolderName}_batch.txt";
                    string batchFilePath = $"{homePath}/Batch/{lastFolderName}_batch.txt";
                    result.Add(trashDirectory, batchFilePath);
                }
            }
        }
        return result;
    }

    //keeps checking and Wait until the system load average falls below the maximum threshold
    private static void CheckAvgLoad()
    {
        //As a general rule, if your machine has 4 cores and you want to keep some overhead for other processes, you can set the threshold to a value between 3.0 to 4.0
        // Set the maximum system load threshold to the number of processor cores
        double maxLoad = Environment.ProcessorCount;

        // Get the current system load average 
        string averageLoadCommand = "uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}'";
        Terminal terminal = new Terminal();
        string avgLoadoutput = terminal.Enter(averageLoadCommand);
        if (!double.TryParse(avgLoadoutput, out double avgLoad))
        {
            throw new Exception($"Failed to parse the average load value from the command output: {avgLoadoutput}");
        }

        // Wait until the system load average falls below the maximum threshold
        while (avgLoad >= maxLoad)
        {
            Console.WriteLine($"System load ({avgLoad}) is greater than or equal to the maximum threshold ({maxLoad}). Waiting for 10 seconds before checking again...");
            Thread.Sleep(TimeSpan.FromSeconds(10));

            avgLoadoutput = terminal.Enter(averageLoadCommand);
            if (!double.TryParse(avgLoadoutput, out avgLoad))
            {
                throw new Exception($"Failed to parse the average load value from the command output: {avgLoadoutput}");
            }
        }
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
        Terminal terminal = new Terminal();
        string lsCommand = $"/bin/ls -d {basePath}*/";
        string output = terminal.EnterCmd(lsCommand);
        if (string.IsNullOrWhiteSpace(output))
        {
            //string fileoutput = terminal.EnterCmd("/bin/ls -f " + basePath);
            //delete files from directory which won't have subdirctories
            //use rm command instead of find and delete
            //terminal.Enter($"find {basePath} -type f -delete && rm -r {basePath}");
            //using rm -rf because it is leaf node and avoiding rm and rmdir to use it unnecessarily
            string rm = $"rm -rf {basePath}";
            terminal.EnterCmd(rm);
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
            terminal.EnterCmd(rm);
        }
    }
}