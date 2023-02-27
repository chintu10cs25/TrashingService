using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        SimulatorService simulator = new SimulatorService();
        Console.WriteLine("Please enter basePath,numOfDirectories,numOfSubdirectories,numOfFiles and sizeOfileInGB separated by space eg: /home/chintu/Trash 3 4 5 1 or enter basePath,breadth,depth and totalSizeinGB e.g:/home/chintu/Trash3 3 4 5");
        string instruction = Console.ReadLine();
        string[] tokens = instruction.Split(" ");
        if (tokens.Count() == 5)
        {
            string basePath = tokens[0];
            int numOfDirectories = int.Parse(tokens[1].Trim());
            int numOfSubdirectories = int.Parse(tokens[2].Trim());
            int numOfFiles = int.Parse(tokens[3].Trim());
            int sizeOfileInGB = int.Parse(tokens[4].Trim());
            Stopwatch sw = new Stopwatch();
            sw.Start();
            simulator.CreateDirectories(basePath, numOfDirectories, numOfSubdirectories, numOfFiles, sizeOfileInGB);
            sw.Stop();
            Console.Write("Total Time stamp:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
            // CreteDirectories();
        }
        else if (tokens.Count() == 4)
        {
            string basePath = tokens[0];
            int breadth = int.Parse(tokens[1].Trim());
            int depth = int.Parse(tokens[2].Trim());
            int totalSizeInGB = int.Parse(tokens[3].Trim());
            Stopwatch sw = new Stopwatch();
            sw.Start();
            simulator.CreateDirectories(basePath, breadth, depth, totalSizeInGB);
            sw.Stop();
            Console.Write("Total Time stamp:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
        }

        //input: /home/chintu/Trash3 3 4 5 1
        //string basePath = "/home/chintu/Trash33";
        string Payload = "/home/chintu/vscode";//Files with data

        //simulator.CreateDirectoriesWithoutPayload(basePath,"Trash",1,2);
        //simulator.CreateDirectoriesWithPayloadUsingSync(basePath,Payload,"Trash",1,2);



    }

    private static void CreateDirectories()
    {
        string basePath = "/home/chintu/mydirectory";
        string[] subdirectories = { "subdir1", "subdir2", "subdir3" };
        // string cmd = $"mkdir -p /home/chintu/mydirectory/{{articles/{{new,rewrites}},images,notes,done}}";
        // RunCommand(cmd);
        foreach (string subdirectory in subdirectories)
        {
            string fullPath = $"{basePath}/{subdirectory}";
            string command = $"mkdir -p {fullPath}";
            RunCommand(command);
            Console.WriteLine($"Created directory: {fullPath}");

            // Create subdirectories
            string[] subSubdirectories = { "subsubdir1", "subsubdir2", "subsubdir3" };
            foreach (string subSubdirectory in subSubdirectories)
            {
                string subFullPath = $"{fullPath}/{subSubdirectory}";
                string subCommand = $"mkdir -p {subFullPath}";
                RunCommand(subCommand);
                Console.WriteLine($"Created subdirectory: {subFullPath}");
            }
        }
    }

    static void RunCommand(string command)
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
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


