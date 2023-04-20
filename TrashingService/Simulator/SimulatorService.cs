using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TrashingService.Common;

namespace TrashingService.Simulator;
public class SimulatorService
{
    private Terminal terminal;
    public SimulatorService()
    {
        //terminal = new Terminal();
    }

    public void CreateDirectories(string basePath,int breadth,int depth,long totalSizeInGB)
    {
        int bufferSize = 100;
        long fileSize=0;
        // Get the available space on the file system containing the directory
        DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(basePath));
        long availableSpaceInBytes = driveInfo.AvailableFreeSpace;
        long availableSpaceInGB = availableSpaceInBytes/(1024 * 1024 * 1024);
        // Check if there is enough space to create the file
        long totalSizeInBytes = totalSizeInGB * 1024 * 1024 * 1024;
        if (availableSpaceInGB < totalSizeInGB)
        {
            Console.WriteLine($"Not enough space to create the file. Available space:{availableSpaceInGB}");
            Console.WriteLine($"If you want to continue with available space then press y then enter y or prss any other key to exit");
            string input = Console.ReadLine();
            if(input.Trim().ToUpperInvariant()!="Y")
            {
                return;
            }
            totalSizeInGB= availableSpaceInGB-bufferSize;
        }

        long numOfFiles=GetNumOfFiles(breadth,depth);
        Console.WriteLine($"Directories:{numOfFiles+1}");
        Console.WriteLine($"Files:{numOfFiles}");

        long sizePerfileInBytes = totalSizeInBytes / numOfFiles;


        string blockSize="1G";
        if(numOfFiles<=totalSizeInGB)
        {
            fileSize=totalSizeInGB/numOfFiles;
            blockSize="1G";
        }
        else if(numOfFiles>totalSizeInGB)
        {
            if(numOfFiles<(totalSizeInGB*1024)/numOfFiles)
            {
                fileSize=totalSizeInGB*1024/numOfFiles;
                blockSize="1M";
            }
            else if(numOfFiles<(totalSizeInGB*1024*1024)/numOfFiles)
            {
                fileSize=totalSizeInGB*1024*1024/numOfFiles;
                blockSize="1K";
            }
            else
            {
                fileSize=totalSizeInGB*1024*1024*1024/numOfFiles;
                blockSize="1B";
            }
           
        }
        LoadData(basePath, breadth, depth, fileSize,blockSize);
                 
    }
    private void LoadData(string basePath, int breadth, int depth, long count,string blockSize)
    {
        if (depth == 0)
        {
            return;
        }
        //Parallel.For(1, breadth, x => 
        //{
        //    string directoryPath = Path.Combine(basePath, Path.GetRandomFileName());
        //    string fileName = Path.GetRandomFileName();
        //    string mkdirCommand = $"mkdir -p \"{directoryPath}\"";
        //    string ddCommand = $"dd if=/dev/urandom of=\"{directoryPath}/{fileName}.txt\" bs={blockSize} count={count}";
        //    //The bs (block size) option is set to 1G, indicating that each block of data should be 1GB in size, and the count option is set to (fileSizeInGB * 1024)
        //    string command = $"{mkdirCommand} && {ddCommand}";
        //    terminal.Enter(command);
        //    LoadData(directoryPath, breadth, depth - 1, count, blockSize);
        //});

        for (int i = 1; i <= breadth; i++)
        {
            string directoryPath = Path.Combine(basePath, Path.GetRandomFileName());
            string fileName = Path.GetRandomFileName();
            string mkdirCommand = $"mkdir -p \"{directoryPath}\"";
            string ddCommand = $"dd if=/dev/urandom of=\"{directoryPath}/{fileName}.txt\" bs={blockSize} count={count}";
            //The bs (block size) option is set to 1G, indicating that each block of data should be 1GB in size, and the count option is set to (fileSizeInGB * 1024)
            string command = $"{mkdirCommand} && {ddCommand}";
            terminal.Enter(command);
            LoadData(directoryPath, breadth, depth - 1, count, blockSize);
        }
    }
    public void CreateDirectoriesWithFileSize(string basePath, int breadth, int depth, long fileSizeInGB)
    {
        if (depth == 0)
        {
            return;
        }

        for (int i = 1; i <= breadth; i++)
        {
            string directoryPath = Path.Combine(basePath, Path.GetRandomFileName());
            string fileName = Path.GetRandomFileName();
            string mkdirCommand = $"mkdir -p \"{directoryPath}\"";
            string ddCommand = $"dd if=/dev/urandom of=\"{directoryPath}/{fileName}.txt\" bs=1M count={fileSizeInGB * 1024}";
            //The bs (block size) option is set to 1G, indicating that each block of data should be 1GB in size, and the count option is set to (fileSizeInGB * 1024)
            string command = $"{mkdirCommand} && {ddCommand}";
            terminal.Enter(command);
            CreateDirectoriesWithFileSize(directoryPath, breadth, depth - 1, fileSizeInGB);
        }
    }

    private long GetNumOfFiles(int breadh,int depth)
    {
        long numOfDirectories=breadh;
        while(depth>1)
        {
            numOfDirectories=(numOfDirectories+1)*breadh;
            depth--;
        }

        return numOfDirectories;
    }

    //public void BuildTreee(TreeNode<string> treeNode,int breadth,int depth)
    //{
        
    //        IEnumerable<string> folders = Enumerable.Range(1, breadth).Select(num => Path.GetRandomFileName());
    //        foreach(string folder in folders)
    //        {
    //            int updatedDepth=depth;
    //            TreeNode<string> breadthTreeNode = new TreeNode<string>(folder);
    //            while(updatedDepth>0)
    //            {
    //                BuildTreee(breadthTreeNode,breadth,--updatedDepth);
    //            }
    //            treeNode.AddChild(breadthTreeNode);
    //        }
      
    //}
    public void CreateDirectories(string basePath,int numOfDirectories,int numOfSubdirectories,int numOfFiles,int fileSizeInKB)
    {
        IEnumerable<string> directoryList = Enumerable.Range(1, numOfDirectories).Select(num => Path.GetRandomFileName());
        Dictionary<string, List<string>> subdirectoriesByDirectory =directoryList.ToDictionary
        (
            directory => directory,
            directory => Enumerable.Range(1, numOfSubdirectories)
                .Select(num => Path.GetRandomFileName())
                .ToList()
        );
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
           // string command = string.Join(" && ",
           //directoryList.Select(directory => $"mkdir -p \"{basePath}/{directory}\"")
           //.Concat(subdirectoriesByDirectory.SelectMany(
           //    kvp => kvp.Value.Select(subdirectory => $"mkdir -p \"{basePath}/{kvp.Key}/{subdirectory}\"")))
           //.Concat(subdirectoriesByDirectory.SelectMany(
           //    kvp => kvp.Value.SelectMany(subdir => Enumerable.Range(1, numOfFiles).Select((i) =>
           //    {
           //        string fileName = Path.GetRandomFileName();
           //        string ddCommand = $"dd if=/dev/urandom of=\"{basePath}/{kvp.Key}/{subdir}/{fileName}.bin\" bs=1B count={fileSizeInKB * 1024}";
           //        return ddCommand;
           //    }))
           //    )));
            foreach (var directory in subdirectoriesByDirectory)
            {
               
               foreach (var subdirectory in directory.Value)
                {
                    string completeDirectory=Path.Combine(basePath, directory.Key, subdirectory);
                    string mkdirCommand = $"mkdir -p \"{completeDirectory}\"";
                    terminal.Enter(mkdirCommand);
                    for (int i = 0; i < numOfFiles; i++)
                    {
                        string fileName = Path.GetRandomFileName();
                        string filePath = Path.Combine(completeDirectory, fileName);
                        string ddCommand = $"dd if=/dev/urandom of=\"{filePath}.bin\" bs=1B count={fileSizeInKB * 1024}";
                        terminal.Enter(ddCommand);
                    }
                }
            }
            

            //WriteDirectoriesToConsole(subdirectoriesByDirectory); 
            //terminal.Enter(command);
        }      
    }
    public void CreateDirectoriesWithPayloadUsingCP(string basePath,string payLoadPath,string prefix,int directories,int subdirectories)
    {

        IEnumerable<string> directoryList = Enumerable.Range(1, directories).Select(num => prefix + "_" + num);
        Dictionary<string, List<string>> subdirectoriesByDirectory = GetAllDirectories(subdirectories, directoryList);

        string command = string.Join
        (" && ",
            directoryList.Select(directory => $"mkdir -p \"{basePath}/{directory}\" && cp -r \"{payLoadPath}\" \"{basePath}/{directory}/\"")
            .Concat(subdirectoriesByDirectory.SelectMany(
                kvp => kvp.Value.Select(subdirectory => $"mkdir -p \"{basePath}/{kvp.Key}/{subdirectory}\" && cp -r \"{payLoadPath}\" \"{basePath}/{kvp.Key}/{subdirectory}/\"")
            ))
        );

         WriteDirectoriesToConsole(subdirectoriesByDirectory); 
         terminal.Enter(command);        

    }
    public void CreateDirectoriesWithPayloadUsingSync(string basePath,string payLoadPath,string prefix,int directories,int subdirectories)
    {
        IEnumerable<string> directoryList = Enumerable.Range(1, directories).Select(num => prefix + "_" + num);
        Dictionary<string, List<string>> subdirectoriesByDirectory = GetAllDirectories(subdirectories, directoryList);

        string command = string.Join
        (" && ",
            directoryList.Select
            (directory => $"mkdir -p \"{basePath}/{directory}\" && rsync -av \"{payLoadPath}/\" \"{basePath}/{directory}/\"")
            .Concat
            (subdirectoriesByDirectory.SelectMany
                (
                    kvp => kvp.Value.Select(subdirectory => $"mkdir -p \"{basePath}/{kvp.Key}/{subdirectory}\" && rsync -av \"{payLoadPath}/\" \"{basePath}/{kvp.Key}/{subdirectory}/\"")
                )
            )
        );

         WriteDirectoriesToConsole(subdirectoriesByDirectory); 
         terminal.Enter(command);        

    }
    private static Dictionary<string, List<string>> GetAllDirectories(int subdirectories, IEnumerable<string> directoryList)
    {
        return directoryList.ToDictionary
        (
            directory => directory,
            directory => Enumerable.Range(1, subdirectories)
                .Select(num => directory + "_" + num)
                .ToList()
        );
    }
    public void CreateDirectoriesWithoutPayload(string basePath,string prefix,int directories,int subdirectories)
    {
        IEnumerable<string> directoryList = Enumerable.Range(1, directories).Select(num => prefix + "_" + num);
        Dictionary<string, List<string>> subdirectoriesByDirectory =directoryList.ToDictionary
        (
            directory => directory,
            directory => Enumerable.Range(1, subdirectories)
                .Select(num => directory + "_" + num)
                .ToList()
        );

        string command = string.Join(" && ",
        directoryList.Select(directory => $"mkdir -p \"{basePath}/{directory}\"")
        .Concat(subdirectoriesByDirectory.SelectMany(
            kvp => kvp.Value.Select(subdirectory => $"mkdir -p \"{basePath}/{kvp.Key}/{subdirectory}\"")
        ))
        );

        WriteDirectoriesToConsole(subdirectoriesByDirectory);   
        terminal.Enter(command);
        
    }
    private static void WriteDirectoriesToConsole(Dictionary<string, List<string>> subdirectoriesByDirectory)
    {
        foreach (var kvp in subdirectoriesByDirectory)
        {
            Console.WriteLine("Directory: " + kvp.Key);

            foreach (var subdirectory in kvp.Value)
            {
                Console.WriteLine("Subdirectory: " + subdirectory);
            }
        }
    }
   
}