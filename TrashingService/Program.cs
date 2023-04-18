using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TrashingService;
using TrashingService.Simulator;

class Program
{
    static void Main(string[] args)
    {

        SimulatorStarter simulatorStarter = new SimulatorStarter();
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter basePath,numOfDirectories,numOfSubdirectories,numOfFiles and sizeOfileInGB separated by space eg: /home/chintu/Trash 3 4 5 1 or enter basePath,breadth,depth and totalSizeinGB e.g:/home/chintu/Trash3 3 4 5");
                Console.WriteLine("Or If you want to exit then enter exit");
            }
            simulatorStarter.Start(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Please enter valid instruction");
            simulatorStarter.Start(new string[0]);
        }

    }
    
}


