
using CliWrap;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using TrashingService.Common;

namespace TrashingService;

enum ProcessStatus
{
    Starting,
    Running
}
public class TrashingProcessor
{

    private const string W_CREATE_BATCH = "the creation of batch";
    private const string W_FILE_DELETION = "the deletion of files";
    private const string W_DIRECTORY_DELETION = "the deletion of directories";
    private readonly ILogger<TrashingProcessor> _logger;
    private  Terminal _terminal;
    private Dictionary<string, int> _trashThresholds;
    public TrashingProcessor(ILogger<TrashingProcessor> logger)
    {
        _logger = logger;
        //_terminal = terminal;
        _trashThresholds = new Dictionary<string, int>();
    }
    public void Start()
    {
        Thread.Sleep(1*60*1000);
    }
    public async Task StartASync()
    {
        Dictionary<string, string> trashDirectoriesWithDirectoryBatchFile = GetTrashDirectoriesWithBatchFile("d");
        Dictionary<string, string> trashDirectoriesWithFileBatchFile = GetTrashDirectoriesWithBatchFile("f");
        //Check existing batch to process from previous run
        ProcessExistingBatch(trashDirectoriesWithDirectoryBatchFile);
        SheduleDeletionProcess(trashDirectoriesWithFileBatchFile, trashDirectoriesWithDirectoryBatchFile);
        // check trash threshold and do cleanup
        UpdateAndCheckTrashThresolds();

    }

    //Check existing directory batch to process from previous run
    private void ProcessExistingBatch(Dictionary<string, string> trashDirectoriesWithDirectoryBatchFile)
    {
        foreach (KeyValuePair<string, string> trashBatchPair in trashDirectoriesWithDirectoryBatchFile)
        {
            if (File.Exists(trashBatchPair.Value))
            {
                try
                {
                    RemoveDirectories(trashBatchPair.Key, trashBatchPair.Value);
                    File.Delete(trashBatchPair.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
               
            }
        }
    }
    private void SheduleDeletionProcess(Dictionary<string, string> trashDirectoriesWithFileBatchFile, Dictionary<string, string> trashDirectoriesWithDirectoryBatchFile)
    {
        try
        {
            _logger.LogInformation("Task sheduling started ...");
            Dictionary<string, Task> tasks = new Dictionary<string, Task>();
            IEnumerable<KeyValuePair<string, string>> nonEmptyTrashDirectoriesWithFileBatchFile = trashDirectoriesWithFileBatchFile.Where(x => !IsTrashingDirectoryEmpty(x.Key));

            if (nonEmptyTrashDirectoriesWithFileBatchFile.Any())
            {
                // Start all deletion processes simultaneously
                foreach (var trashDirectory in nonEmptyTrashDirectoriesWithFileBatchFile)
                {
                    //CheckAvgLoad(W_DELETION_PROCESS, trashDirectory.Key);
                    var task = Task.Factory.StartNew(() =>
                    {
                        CheckAvgLoad(W_CREATE_BATCH, trashDirectory.Key, ProcessStatus.Starting);
                        ListFiles(trashDirectory.Key, trashDirectory.Value);
                        CheckAvgLoad(W_FILE_DELETION, trashDirectory.Key, ProcessStatus.Starting);
                        HashSet<string> directories = RemoveFilesAndListDiectores(trashDirectory.Key, trashDirectory.Value);
                        CheckAvgLoad(W_DIRECTORY_DELETION, trashDirectory.Key, ProcessStatus.Starting);
                        WriteDirectories(directories, trashDirectory.Key, trashDirectoriesWithDirectoryBatchFile[trashDirectory.Key]);
                        //RemoveDirectories(directories, trashDirectory.Key);
                    }, TaskCreationOptions.LongRunning);

                    _logger.LogInformation($"Deletion process for {trashDirectory.Key} is started on Task:{task.Id}");
                    tasks.Add(trashDirectory.Key, task);
                }
                _logger.LogInformation("Task sheduling completed");
                _logger.LogInformation("Waiting for all deletion processes tasks to be completed..");
                // Wait for all deletion processes to complete
                Task.WaitAll(tasks.Values.ToArray());
                foreach (var task in tasks)
                {
                    _logger.LogInformation($"Deletion process for {task.Key} is completed on Task:{task.Value.Id}");
                }
                //Delete batch files

                foreach (var trashDirectory in nonEmptyTrashDirectoriesWithFileBatchFile)
                {
                    File.Delete(trashDirectory.Value);
                    _logger.LogInformation($"{trashDirectory.Value} is deleted");
                }

            }
            else
            {
                _logger.LogInformation("No non-empty trash directories are currently available to shedule for deletion process. The system will wait for 15 minutes before attempting again.");
                Thread.Sleep(15 * 60 * 1000);
            }

        }
        catch (Exception ex)
        {
            _logger.LogInformation($"An error occurred while running the deletion process: {ex.Message}");
        }
    }
    //List all the files from trashing directory to a batch file
    private void ListFiles(string trashingDirectory,string batchFilePath)
    {     
        _logger.LogInformation($"Started listing files from {trashingDirectory} to {batchFilePath}......" );
        if (string.IsNullOrEmpty(batchFilePath)|| string.IsNullOrEmpty(trashingDirectory))
        {
            return;
        }

        string cmd = $"find {trashingDirectory} -type f > {batchFilePath}";
        ////string cmd = $"find {trashingDirectory} -type f -delete -printf %h\n > {batchFilePath}";
        Terminal terminal = new Terminal();
        terminal.EnterCmd(cmd);

        _logger.LogInformation($"Listing files completed from {trashingDirectory} to {batchFilePath}");
    }
    //Delete files and extract directory by reading batch file
    private HashSet<string> RemoveFilesAndListDiectores(string trashingDirectory,string batchFilePath)
    {
        _logger.LogInformation($"Processing batch file {batchFilePath} and started file deletion from {trashingDirectory}" );
        // Create a set to store the unique lines
        HashSet<string> directories = new HashSet<string>();
        //string filePath = "/home/chintu/Batch/f_Directory/test.txt";
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        using (FileStream stream = new FileStream(batchFilePath, FileMode.Open, FileAccess.Read,FileShare.None,64*1024))// using a 64KB buffer size
        using (StreamReader reader = new StreamReader(stream))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    //check avgload on every 1 min 
                    if (sw.ElapsedMilliseconds >= 1*60*1000)
                    {
                        CheckAvgLoad(W_FILE_DELETION,trashingDirectory,ProcessStatus.Running);
                        sw.Restart();
                    }                 
                   
                    File.Delete(line);
                    //_logger.LogInformation($"Deleted file {line}");
                    string? directory = Path.GetDirectoryName(line);
                    if (!string.IsNullOrEmpty(directory) && (trashingDirectory.Length < directory.Length))
                    {
                        //Console.WriteLine($"{directory}");
                        bool flag = directories.Add($"{directory}");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{e.Message}");
                }
            }
        }

        _logger.LogInformation($"Batch file {batchFilePath} is processed and File deletion from {trashingDirectory} is completed" );
        return directories;
    }
    private void WriteDirectories(HashSet<string> directories,string trashingDirectory, string batchFilePath)
    {
        _logger.LogInformation($"Started Listing directories of {trashingDirectory} into {batchFilePath}");

        IEnumerable<string> sortedDirectories = directories
        .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        .OrderByDescending(tuple => tuple.depth)
        .Select(tuple => tuple.directory);
        try
        {
            using (FileStream fs = new FileStream(batchFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 64 * 1024)) // using a 64KB buffer size
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (string dir in sortedDirectories)
                    {
                        sw.WriteLine(dir);
                    }
                    sw.Close();
                }
                fs.Close();
            }
        }
        catch(Exception exception)
        {
            _logger.LogError(exception.Message);
        }
        _logger.LogInformation($"Completed Listing directories of {trashingDirectory} into {batchFilePath}");
    }
    //Remove empty directories
    private void RemoveDirectories(string trashingDirectory,string batchFilePath)
    {
        _logger.LogInformation($"Started removing directories from {trashingDirectory}");
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        using (FileStream fileStream = new FileStream(batchFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 64 * 1024)) // using a 64KB buffer size
        {
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    try
                    {
                        //check avgload on every 1 min 
                        if (sw.ElapsedMilliseconds >= 1 * 60 * 1000)
                        {
                            CheckAvgLoad(W_FILE_DELETION, trashingDirectory, ProcessStatus.Running);
                            sw.Restart();
                        }

                        if (!Directory.EnumerateFileSystemEntries(line).Any())
                        {                            
                            if (trashingDirectory.Length < line.Length)
                            {
                                //Console.WriteLine($"{directory}");
                                Directory.Delete(line);
                            }
                        }
                        
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"{e.Message}");
                    }
                }
                streamReader.Close();
            }
            fileStream.Close();
        }
        _logger.LogInformation($"Completed removing directories from {trashingDirectory}");
    }
    private void RemoveDirectories(HashSet<string> directories,string trashingDirectory)
    {
        _logger.LogInformation($"Started removing directories from {trashingDirectory}" );

        IEnumerable<string> sortedDirectories = directories
        .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
        .OrderByDescending(tuple => tuple.depth)
        .Select(tuple => tuple.directory);

        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        foreach (string directory in sortedDirectories)
        {
            if (sw.ElapsedMilliseconds >= 1*60*1000)
            {
                CheckAvgLoad(W_DIRECTORY_DELETION, trashingDirectory, ProcessStatus.Running);
                sw.Restart();
            }
            if ((trashingDirectory.Length < directory.Length))
            {
                try
                {
                    Directory.Delete(directory);
                    //_logger.LogInformation($"Deleted directory {directory}");
                }
                catch(Exception dirExp)
                {
                    _logger.LogError(dirExp.Message);
                    return;
                }
              
            }
           
        }
        // handle empty directories
        if(Directory.EnumerateFileSystemEntries(trashingDirectory,"*", SearchOption.TopDirectoryOnly).Count() > 0)
        {
            _logger.LogInformation($"Trash directory {trashingDirectory} didn't cleaned completly");

            DeleteRemainingFilesAndDirectories(trashingDirectory);
        }

        _logger.LogInformation($"Completed removing directories from {trashingDirectory}" );
    }  
    //get dictionary of trashing directory and batchFilePath
    private Dictionary<string,string> GetNoneEmptyTrashingDirectories()
    {
        _logger.LogInformation("Scanning non-empty trash directories......");
        Dictionary<string,string> result = new Dictionary<string, string>();

        string homePath=Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] dataDirectories=Directory.GetDirectories(homePath, "data*");

        foreach (string directory in dataDirectories) 
        {
            string trashDirectory = Path.Combine(directory, "trash");
            if (Directory.Exists(trashDirectory))
            {
                if (!IsTrashingDirectoryEmpty(trashDirectory))
                {
                    string lastFolderName = new DirectoryInfo(directory).Name;
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
        _logger.LogInformation("Scanning non-empty trash directories completed.......");
        return result;
    }
    private Dictionary<string, string> GetTrashDirectoriesWithBatchFile(string batchType)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] dataDirectories = Directory.GetDirectories(homePath, "data*");

        foreach (string directory in dataDirectories)
        {
            string trashDirectory = Path.Combine(directory, "trash");
            if (Directory.Exists(trashDirectory))
            {
                //if (!IsTrashingDirectoryEmpty(trashDirectory))
                //{
                    string lastFolderName = new DirectoryInfo(directory).Name;
                    //string batchFilePath = $"/var/e2/{lastFolderName}_batch.txt";
                    string batchDir = $"{homePath}/Batch";
                    if (!Directory.Exists(batchDir))
                    {
                        Directory.CreateDirectory(batchDir);
                    }
                    string batchFilePath = $"{batchDir}/{lastFolderName}_{batchType}_batch.txt";
                    result.Add(trashDirectory, batchFilePath);
                //}
            }
        }
        return result;
    }
    //keeps checking and Wait until the system load average falls below the maximum threshold
    private void CheckAvgLoad(string process, string trashingDirectory,ProcessStatus processStatus)
    {
        _terminal = new Terminal();
        //As a general rule, if your machine has 4 cores and you want to keep some overhead for other processes, you can set the threshold to a value between 3.0 to 4.0
        // Set the maximum system load threshold to the number of processor cores
        if(processStatus == ProcessStatus.Starting)
        {
            _logger.LogInformation($"Checking avg load before starting {process} for {trashingDirectory}");
        }
        else
        {
            _logger.LogInformation($"Checking avg load during running {process} for {trashingDirectory}");
        }
        
        double maxLoad = Environment.ProcessorCount;

        // Get the current system load average 
        double avgLoad = GetAvgLoad();

        // Wait until the system load average falls below the maximum threshold
        while (avgLoad >= maxLoad)
        {
            _logger.LogInformation($"System load ({avgLoad}) is greater than or equal to the maximum threshold ({maxLoad}). Waiting for {process} for {trashingDirectory} for 1 minute before checking again on Task:{Task.CurrentId}");
            Thread.Sleep(TimeSpan.FromMinutes(1));
            avgLoad = GetAvgLoad();
        }
        if (processStatus == ProcessStatus.Starting)
        {
            _logger.LogInformation($"System load ({avgLoad}) is below the maximum threshold ({maxLoad}) starting {process} for {trashingDirectory}");
        }
        else
        {
            _logger.LogInformation($"System load ({avgLoad}) is below the maximum threshold ({maxLoad}) resuming {process} for {trashingDirectory}");
        }
    }
    private double GetAvgLoad()
    {
        _terminal = new Terminal();
        string averageLoadCommand = "uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}'";
        double avgLoad;
        string avgLoadoutput = _terminal.Enter(averageLoadCommand);
        if (!double.TryParse(avgLoadoutput, out avgLoad))
        {
            _logger.LogError($"Failed to parse the average load value from the command output: {avgLoadoutput}");

            GetAvgLoad();
        }

        return avgLoad;
    }
    private bool IsTrashingDirectoryEmpty(string trashingDirectory)
    {
        // Check if the directory is empty
        if (Directory.GetFiles(trashingDirectory).Length == 0 && Directory.GetDirectories(trashingDirectory).Length == 0)
        {
            _logger.LogInformation($"Trashing Directory {trashingDirectory} is empty");
            return true;
        }
        else
        {
            _logger.LogInformation($"Trashing Directory {trashingDirectory} is not empty");
            return false;
        }
    }

    //Check last modified date of trash directory
    private bool IsReadyToShedule(string trashDirectory)
    {
        DirectoryInfo trashDirectoryInfo = new DirectoryInfo(trashDirectory);
        TimeSpan diff= DateTime.Now - trashDirectoryInfo.LastWriteTime;
        if(diff.TotalMinutes > 15)
        {
            _logger.LogInformation($"Ready to shedule Deletion process for {trashDirectory}");
            return true;
        }
        _logger.LogInformation($"Not ready to shedule Deletion process for {trashDirectory} as currently some other process is accessing it");
        return false;
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
    private void DeleteRemainingFilesAndDirectories(string trashDirectory)
    {
        var blockingCollection = new BlockingCollection<string>();
        // Create a set to store the unique lines
        Stopwatch sw = new Stopwatch();
        sw.Start();
        long fileCount = 0;
        long directoriesCount= 0;
       _logger.LogInformation($"Files deletion started from {trashDirectory} .......................");
        try
        {
            Parallel.ForEach(Directory.EnumerateFileSystemEntries(trashDirectory, "*", SearchOption.AllDirectories), item =>
            {
                if (File.Exists(item))
                {
                    File.Delete(item);
                    _logger.LogInformation(item);
                    fileCount++;
                }
                else if (Directory.Exists(item))
                {
                    blockingCollection.Add(item);
                    //directories.Add(item);
                    directoriesCount++;
                }
            });
            _logger.LogInformation($"Files deletion completed from {trashDirectory} ..................");
            sw.Stop();
            _logger.LogInformation($"Time stamp for files deletion:{sw.Elapsed}");
            blockingCollection.CompleteAdding();
            sw.Restart();
            _logger.LogInformation($"Directories deletion started from {trashDirectory} .......................");
            IEnumerable<string> sortedDirectories = blockingCollection
            .Select(dir => (directory: dir, depth: dir.Count(c => c == '/')))
            .OrderByDescending(tuple => tuple.depth)
            .Select(tuple => tuple.directory);

            _logger.LogInformation("Deletion of directories Started...........");

            foreach (var dir in sortedDirectories)
            {
                Directory.Delete(dir);
                Console.WriteLine(dir);
            }

            sw.Stop();
            _logger.LogInformation($"Time stamp for directories deletion:{sw.Elapsed}");
            _logger.LogInformation($"Directories deletion completed from {trashDirectory} ..................");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

    }
    private void UpdateAndCheckTrashThresolds()
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] dataDirectories = Directory.GetDirectories(homePath, "data*");

        foreach (string directory in dataDirectories)
        {
            string trashDirectory = Path.Combine(directory, "trash");
            if (Directory.Exists(trashDirectory))
            {
               if(_trashThresholds.ContainsKey(trashDirectory))
               {
                    int thresholdVal=_trashThresholds[trashDirectory]++;
                    if(thresholdVal > 10)
                    {
                        DeleteRemainingFilesAndDirectories(trashDirectory);
                    }
               }
               else
               {
                   _trashThresholds.Add(trashDirectory, 1);
               }
            }
        }
    }
}