using System.Diagnostics;
using System.Runtime.InteropServices;
using TrashingService.Simulator;

class Program
{
    static void Main(string[] args)
    {
       
        Console.WriteLine("Please enter basePath,numOfDirectories,numOfSubdirectories,numOfFiles and sizeOfileInGB separated by space eg: /home/chintu/Trash 3 4 5 1 or enter basePath,breadth,depth and totalSizeinGB e.g:/home/chintu/Trash3 3 4 5");
        try
        {
            Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Please enter instruction");
            Start();
        }
      
    }
    static void Start()
    {       
        SimulatorService simulator = new SimulatorService();
        string instruction = Console.ReadLine();
        //string instruction = $"C:\\Users\\Chintu\\Trash 3 4 10";
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                simulator.CreateDirectories(basePath, numOfDirectories, numOfSubdirectories, numOfFiles, sizeOfileInGB);
            }
            else
            {
                WindowsSimulaor ws = new WindowsSimulaor();
                ws.CreateDirectories(basePath, numOfDirectories, numOfSubdirectories, numOfFiles, sizeOfileInGB);
            }
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                simulator.CreateDirectories(basePath, breadth, depth, totalSizeInGB);
            }
            else
            {
                WindowsSimulaor ws = new WindowsSimulaor();
                ws.CreateDirectories(basePath, breadth, depth, totalSizeInGB);
            }
            sw.Stop();
            Console.Write("Total Time stamp:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
        }
        else if(instruction.Trim().ToUpperInvariant()=="EXIT")
        {
            Environment.Exit(0);
        }
        
    }
}


