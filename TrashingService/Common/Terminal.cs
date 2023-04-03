using System;
using System.Diagnostics;
using System.Text;

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
    public string EnterCmd(string command)
    {
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}: {command}");
        }
        return process.StandardOutput.ReadToEnd();
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
        // create a new process
        Process process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
        // pass the path to your bash script as an argument
        process.StartInfo.Arguments = scriptPath;

        // set the working directory for the process
        process.StartInfo.WorkingDirectory = workingDirectory;

        // redirect the output of the process to the console
        process.StartInfo.RedirectStandardOutput = true;

        // start the process
        process.Start();

        // read the output of the process
        StreamReader reader = process.StandardOutput;

        Task.Run(() =>
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // print the output to the console
                Console.WriteLine(line);
            }
        });
        Task.Run(() =>
        {
            string line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                Console.WriteLine("Error output: " + line);
            }
        });

        // wait for the process to exit
        process.WaitForExit();

    }
    public void RunBashScript_old(string workingDirectory, string scriptPath, string trashingDirectory, string batchingDirectory, int batchsize)
    {
        // create a new process
        Process process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
        // pass the path to your bash script as an argument
        process.StartInfo.Arguments = $"{scriptPath} {trashingDirectory} {batchingDirectory} {batchsize}";
        // set the working directory for the process
        process.StartInfo.WorkingDirectory = workingDirectory;
        // redirect the output of the process to the console
        process.StartInfo.RedirectStandardOutput = true;
        // redirect the error output of the process to the console
        process.StartInfo.RedirectStandardError = true;
        // start the process
        process.Start();
        // read the output of the process
        StreamReader reader = process.StandardOutput;

        Task.Run(() =>
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // print the output to the console
                Console.WriteLine(line);
            }
        });
        Task.Run(() =>
        {
            string line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                Console.WriteLine("Error output: " + line);
            }
        });
        // wait for the process to exit
        process.WaitForExit();
        
    }
    public Process AssignProcessToBashScript(string workingDirectory, params string[] parameters)
    {
        // create a new process
        Process process = new Process();
        // specify the bash executable as the process to start
        process.StartInfo.FileName = "/bin/bash";
        // pass the path to your bash script as an argument
        string args=string.Join(" ", parameters);
        process.StartInfo.Arguments = args;
         //process.StartInfo.Arguments = $"{scriptPath} {batchingDirectory} {trashingDirectory} {deletionOption}";
         // set the working directory for the process
         process.StartInfo.WorkingDirectory = workingDirectory;
        // redirect the output of the process to the console
        process.StartInfo.RedirectStandardOutput = true;
        // redirect the error output of the process to the console
        process.StartInfo.RedirectStandardError = true;
        //// start the process
        //process.Start();
        //// read the output of the process
        //StreamReader reader = process.StandardOutput;

        //Task.Run(() =>
        //{
        //    string line;
        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        // print the output to the console
        //        Console.WriteLine(line);
        //    }
        //});
        //Task.Run(() =>
        //{
        //    string line;
        //    while ((line = process.StandardError.ReadLine()) != null)
        //    {
        //        Console.WriteLine("Error output: " + line);
        //    }
        //});
        //// wait for the process to exit
        //process.WaitForExit();
        return process;
    }
    public void StartProcess(Process process)
    {
        process.Start();
        StreamReader reader = process.StandardOutput;

        Task.Run(() =>
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        });

        Task.Run(() =>
        {
            string line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                Console.WriteLine("Error output: " + line);
            }
        });

        process.WaitForExit();
    }

}