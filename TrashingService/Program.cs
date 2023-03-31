using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TrashingService;
using TrashingService.Simulator;

class Program
{
    static void Main(string[] args)
    {
        TrashingProcessor trashingProcessor = new TrashingProcessor();
        trashingProcessor.Strat(args);



        //Stopwatch sw = new Stopwatch();
        //sw.Start();
        //trashingProcessor.DeleteUsingLs("/home/chintu/Trash/");
        //sw.Stop();
        //Console.WriteLine("Total Time stamp for deletion:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);

        //trashingProcessor.DeleteUsingFind("/home/chintu/Trash/");
        //string workingDirectory = "/home/chintu/BatchesIn10K/";

        //trashingProcessor.ThrottlingInBatches("/home/chintu/Trash/", workingDirectory);
        //sw.Stop();
        //Console.WriteLine("Total Time stamp for throttling:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
        //sw.Restart();
        //trashingProcessor.DeleteInBatches(workingDirectory);
        //sw.Stop();

        //Console.WriteLine("Total Time stamp for deletion:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);

        //try
        //{
        //    if (args.Length == 0)
        //    {
        //        Console.WriteLine("Please enter basePath,numOfDirectories,numOfSubdirectories,numOfFiles and sizeOfileInGB separated by space eg: /home/chintu/Trash 3 4 5 1 or enter basePath,breadth,depth and totalSizeinGB e.g:/home/chintu/Trash3 3 4 5");
        //        Console.WriteLine("Or If you want to exit then enter exit");
        //    }
        //    Start(args);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //    Console.WriteLine("Please enter valid instruction");
        //    Start(new string[0]);
        //}

    }
    
}


