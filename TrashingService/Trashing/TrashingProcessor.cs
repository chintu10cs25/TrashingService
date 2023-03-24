using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TrashingService.Common;
using static System.Net.WebRequestMethods;

namespace TrashingService;
public class TrashingProcessor
{
    private Terminal terminal;
    private List<string> _files;
    public TrashingProcessor()
    {
        terminal = new Terminal();
        _files = new List<string>();
    }
    public void Strat()
    {
        Console.WriteLine("Trashing Started");
        string path = "/home/chintu/Repos/minio-dotnet";  
        var directories = Directory.EnumerateDirectories(path);
        foreach(var dir in directories)
        {
            Console.WriteLine(dir);
        }
        try
        {
            Random random = new Random();
            //string directoryPath = "/home/chintu/minio-dotnet";           
            string rmCommand = "rm -r /home/chintu/minio-dotnet";
            //string findCommand = "find /home/chintu/minio-dotnet -type f -delete"; // to delete all the files
            string findCommand = "find /home/chintu/htg -delete"; //delete all the files and folders
            string mkdir = "mkdir test";
            RunCommand(mkdir);
        }
        catch (Exception exception)
        {

        }
       
    }

    static void RunCommand(string command)
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
            Console.WriteLine(output);
        }
    }

    public static void DeleteProcess(string basePath)
    {
        // Set the maximum system load threshold
        double maxLoad = 1.0;
        // Get the current system load average 
        string averageLoadCommand = "uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}'";
        Terminal terminal = new Terminal();
        string avgLoadoutput = terminal.Enter(averageLoadCommand);
        string normalcmd = $"time find /home/chintu/Trash -type f ";
        string cmdxargs = $"time find /home/chintu/Trash -type f -print0 | xargs -0 -P 4 -n 10000";
        var normalwatch = Stopwatch.StartNew();
        string fsnormal = terminal.Enter(normalcmd);
        normalwatch.Stop();
        TimeSpan normalts = normalwatch.Elapsed;
        Console.WriteLine("Normal Time Span:" + normalts);

        var watchxargs = Stopwatch.StartNew();
        string fsxargs = terminal.Enter(cmdxargs);
        watchxargs.Stop();
        Console.WriteLine("Xargs Time Span:" + watchxargs.Elapsed);


        //Process process = new Process();
        //process.StartInfo.FileName = "/bin/bash";
        //process.StartInfo.Arguments = "-c \"uptime | awk '{print $10}' | sed 's/,//'\"";
        //process.StartInfo.RedirectStandardOutput = true;
        //process.Start();
        //string output = process.StandardOutput.ReadToEnd();
        //process.WaitForExit();
        double load = double.Parse(avgLoadoutput);
        Console.WriteLine("Current System Load: " + load);

        // Get the total number of files and directories in the directory $"find /home/chintu/Trash - type f - o - type d | wc - l"; 
        //string findtotalNumOfFilesDirectoriesCommand = $"find {basePath} -type f -o -type d | wc -l";
        //string pathsOutput = terminal.Enter(findtotalNumOfFilesDirectoriesCommand);
        //long TOTAL = int.Parse(pathsOutput);
        //int BATCH_SIZE = 10;
        //long BATCH_COUNT = ((TOTAL + BATCH_SIZE - 1) / BATCH_SIZE);
        //for (int i = 0; i <= BATCH_SIZE; i++)
        //{
        //    string deleteCommand = $"find {basePath} -type f -o -type d -print0 | head -zn {BATCH_SIZE} | xargs  -0 rm -rf";
        //    string output = terminal.Enter(deleteCommand);
        //}
        string filesAndDirectories = $"find {basePath} -type f -o -type d | wc -l";
        string files = $"find {basePath} -type f | wc -l";
        string directories = $"find {basePath} -type d | wc -l";
        string fds = terminal.Enter(filesAndDirectories);
        var watch = Stopwatch.StartNew();
        string fs = terminal.Enter(files);
        watch.Stop();
        Console.WriteLine("Time taken to get 1M files using normal find Time stamp:"+watch.Elapsed);
        string ds = terminal.Enter(directories);
        long TOTAL = int.Parse(fds);
        int BATCH_SIZE = 10;
        long BATCH_COUNT = ((TOTAL + BATCH_SIZE - 1) / BATCH_SIZE);
        for (int i = 0; i < BATCH_COUNT; i++)
        {
            //string deleteCommand = $"find {basePath} -type f -o -type d | head -n {BATCH_SIZE} | xargs rm -r";

            string deleteFileCommand = $"find {basePath} -type f | head -z -q -n {BATCH_SIZE} | xargs rm";
            string deleEmptydirectoies = $"find {basePath} -type d -empty -delete";
            string deleFileAndEmptyDirctories = string.Join(" && ", deleteFileCommand, deleEmptydirectoies);
            string test = $"find {basePath} -depth -mindepth 1 -print0 | head -z q -n {BATCH_SIZE} | xargs -0 -r rm -rf";
            string output = terminal.Enter(deleteFileCommand);
        }
        // Check if the load is below the threshold 
        if (load < maxLoad)
        {
            string deleteCommand = $"find {basePath} -type f -o -type d -print0 | head -zn {BATCH_SIZE} | xargs -0 -r -n {BATCH_SIZE} rm -rf";
            //string deleteCommand = $"find {basePath} -depth -type f -o -type d -print0 | xargs -0 -n 100 rm -rf";
            //string deleteCommand = $"find {basePath} -type f -o -type d -print0 | xargs -0 -r -n {BATCH_SIZE} rm -rf";
            string output = terminal.Enter(deleteCommand);
            //find /path/to/directory -depth -type f -o -type d | head -n 100 | xargs rm -rf


            //Process deleteProcess = new Process();
            //deleteProcess.StartInfo.FileName = "/bin/bash";
            //deleteProcess.StartInfo.Arguments = $"C:\\Users\\Chintu\\source\\repos\\TrashingService\\TrashingService\\Trashing\\Script\\delete.sh {basePath}" ;
            //deleteProcess.Start();
            //deleteProcess.WaitForExit();
            //find /home/chintu/Trash -type f | head -z -q -n 100000 | wc -l
        }

    }
    public void ThrottlingInBatches(string basePath, string workingDirectory)
    {
        long batchsize = 10000;
        long start = 1;
        long end = batchsize;
        string checkEmpty = $"find {basePath} -maxdepth 0 -empty";
        string dirctory = terminal.Enter(checkEmpty);

        while (string.IsNullOrEmpty(dirctory))
        {
            //string deleteCommand = $"find {basePath} -type f | head -n {end} | tail -n +{start}";
            //string deleteCommand = $"find {basePath} -type f -o-type d | head -n {end} | tail -n +{start}|xargs rm -rf";

            // use the start and end values to generate the delete command
            string fileName = $"{start}-{end}.txt";
            //string batchingCmd = $"find {basePath} -type f | xargs -n 1 | head -n {end} | tail -n +{start} > {fileName}";
            string batchingCmd = $"find {basePath} -type f | head -n {end} | tail -n +{start} > {fileName}";
            terminal.Enter(batchingCmd);

            // increment the start and end values by batchsize
            start += batchsize;
            end += batchsize;
        }
      
    }
    public void DeleteInBatches(string workingDirectory)
    {
        //string fileToDelete = $"/home/chintu/BatchesIn10K/1-10000.txt";
        //string rmCmd = $"xargs -P 4 rm -vf < {fileToDelete}";

        //terminal.Enter(rmCmd);


        string scriptPath = $"/home/chintu/deletionProcess.sh";

        terminal.RunBashScript(workingDirectory, scriptPath);


        //string deleteCommand = $"xargs rm < /home/chintu/TestBatchDeletion/3-4.txt";
        //terminal.Enter(deleteCommand);


        // create a new process
        //Process process = new Process();

        //// specify the bash executable as the process to start
        //process.StartInfo.FileName = "/bin/bash";

        //// pass the path to your bash script as an argument
        //process.StartInfo.Arguments = cmd;

        //// set the working directory for the process
        //process.StartInfo.WorkingDirectory = "/home/chintu/TestBatchDeletion/";

        //// redirect the output of the process to the console
        //process.StartInfo.RedirectStandardOutput = true;

        //// start the process
        //process.Start();

        //// read the output of the process
        //string output = process.StandardOutput.ReadToEnd();

        //// wait for the process to exit
        //process.WaitForExit();

        //// print the output to the console
        //Console.WriteLine(output);

    }
    public void DeleteUsingLs(string basePath)
    {
        string output = terminal.Enter("ls -d " + basePath+ "*/");
        if (string.IsNullOrWhiteSpace(output))
        {
            string fileoutput = terminal.Enter("ls -f " + basePath);
            //delete files from directory which won't have subdirctories
            terminal.Enter($"find {basePath} -type f -delete && rm -r {basePath}");
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
            terminal.Enter($"find {basePath} -type f -delete && rm -r {basePath}");
        }
    }
}