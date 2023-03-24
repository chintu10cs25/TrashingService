﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TrashingService;
using TrashingService.Simulator;

class Program
{
    static void Main(string[] args)
    {
        TrashingProcessor trashingProcessor = new TrashingProcessor();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        // trashingProcessor.DeleteUsingLs("/home/chintu/Trash2/");
        //trashingProcessor.DeleteUsingFind("/home/chintu/Trash/");
        string workingDirectory = "/home/chintu/BatchesIn10K/";

        trashingProcessor.ThrottlingInBatches("/home/chintu/Trash/", workingDirectory);
        sw.Stop();
        Console.WriteLine("Total Time stamp for throttling:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
        sw.Restart();
        trashingProcessor.DeleteInBatches(workingDirectory);
        sw.Stop();

        Console.WriteLine("Total Time stamp for deletion:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);

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
    static void Start(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please enter basePath,numOfDirectories,numOfSubdirectories,numOfFiles and sizeOfileInGB separated by space eg: /home/chintu/Trash 3 4 5 1 or enter basePath,breadth,depth and totalSizeinGB e.g:/home/chintu/Trash3 3 4 5");
            Console.WriteLine("Or If you want to exit then enter exit");
            //string instruction = Console.ReadLine();
            string instruction = "/mnt/d/Trash_1 1 1 10000 1";
            if (instruction.Trim().ToUpperInvariant() == "EXIT")
            {
                Environment.Exit(0);
            }
            args = instruction.Split(" ");
        }
        SimulatorService simulator = new SimulatorService();
        //string instruction = Console.ReadLine();
        //string instruction = $"C:\\Users\\Chintu\\Trash 3 4 10";
        //string[] args = instruction.Split(" ");
        //Named tuple
        //var Person = (FirstName: "Leonardo", LastName: "Gasparini", Age: 26);

        if (args.Count() == 5)
        {
            string basePath = args[0];
            int numOfDirectories = int.Parse(args[1].Trim());
            int numOfSubdirectories = int.Parse(args[2].Trim());
            int numOfFiles = int.Parse(args[3].Trim());
            int sizeOfileInGB = int.Parse(args[4].Trim());
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
        else if (args.Count() == 4)
        {
            string basePath = args[0];
            int breadth = int.Parse(args[1].Trim());
            int depth = int.Parse(args[2].Trim());
            int totalSizeInGB = int.Parse(args[3].Trim());
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
            Console.WriteLine("Total Time stamp:" + sw.Elapsed + "In millisecond:" + sw.ElapsedMilliseconds);
        }
        else
        {
            Console.WriteLine("please enter valid instruction");
            Start(new string[0]);
        }

        

    }
}


