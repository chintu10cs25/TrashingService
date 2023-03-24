using System;
using System.Diagnostics;
namespace TrashingService.Common;
public class Terminal
{
    private Process process;
    public Terminal(){
        // create a new process
        process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
    }
    public string Enter(string command)
    {        
        process.StartInfo.Arguments = $"-c \"{command}\"";
        //process.StartInfo.UseShellExecute = false;
        //process.StartInfo.RedirectStandardOutput = true;

        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = "/home/chintu/BatchesIn10K/";

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
         return  process.StandardOutput.ReadToEnd();
    }
    public void Enter(string command, string workingDirectory)
    {
        process.StartInfo.Arguments = $"-c \"{command}\"";
        //process.StartInfo.UseShellExecute = false;
        //process.StartInfo.RedirectStandardOutput = true;

        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = workingDirectory;

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
        //return process.StandardOutput.ReadToEnd();
    }
    public void RunBashScript(string workingDirectory,string scriptPath)
    {
        // pass the path to your bash script as an argument
        process.StartInfo.Arguments = scriptPath;

        // set the working directory for the process
        process.StartInfo.WorkingDirectory = workingDirectory;

        // redirect the output of the process to the console
        process.StartInfo.RedirectStandardOutput = true;

        // start the process
        process.Start();

        // read the output of the process
        string output = process.StandardOutput.ReadToEnd();

        // wait for the process to exit
        process.WaitForExit();

        // print the output to the console
        Console.WriteLine(output);

    }
}