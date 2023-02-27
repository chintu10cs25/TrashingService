using System.Diagnostics;

public class Terminal
{
    private Process process;
    public Terminal(){
        process = new Process();
        process.StartInfo.FileName = "/bin/bash";
    }
    public void Enter(string command){        
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
    }
}