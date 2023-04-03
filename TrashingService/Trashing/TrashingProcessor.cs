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

    public TrashingProcessor()
    {
        terminal = new Terminal();
    }
    public void Start(string[] args)
    {
        //// Set the maximum system load threshold
        //double maxLoad = 1.0;
        //// Get the current system load average 
        //string averageLoadCommand = "uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}'";
        //Terminal terminal = new Terminal();
        //string avgLoadoutput = terminal.Enter(averageLoadCommand);
        string workingDirectory = "/home/chintu/Batch/";
        string batchingScriptPath = "/home/chintu/script/batching.sh";
        string deletionScriptPath = "/home/chintu/script/deletion.sh";
        string trashingDirectory = "/home/chintu/Trash3_3/";
        string batchingDirectory = "/home/chintu/Batch/";
        int batchsize = 1000;

        // Assign a process to batching.sh
        Process batchingProcess = terminal.AssignProcessToBashScript(workingDirectory, batchingScriptPath, trashingDirectory, batchingDirectory, batchsize.ToString());
        
        // Assign a process to  deletion.sh 
        Process deletionProcess = terminal.AssignProcessToBashScript(workingDirectory, deletionScriptPath, trashingDirectory, batchingDirectory);

        // start the processes asynchronously using Task.Run
        //Task batchingTask = Task.Run(() => terminal.StartProcess(batchingProcess));
        Task deletionTask = Task.Run(() => terminal.StartProcess(deletionProcess));

        // wait for both tasks to complete
        Task.WaitAll(deletionTask);

        // Wait for both threads to finish
        //Task.WaitAll();
        //RunBatchingAndDeletionScripts(workingDirectory, batchingScriptPath, deletionScriptPath,trashingDirectory, batchingDirectory, batchsize);

    }
    public void DeleteUsingLs(string basePath)
    {
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