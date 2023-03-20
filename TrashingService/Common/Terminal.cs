using System.Diagnostics;
namespace TrashingService.Common;
public class Terminal
{
    private Process process;
    public Terminal(){
        process = new Process();
        process.StartInfo.FileName = "/bin/bash";
    }
    public string Enter(string command){        
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
         return  process.StandardOutput.ReadToEnd();
    }
}