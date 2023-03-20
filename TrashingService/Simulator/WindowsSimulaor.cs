using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TrashingService.Common;

namespace TrashingService.Simulator
{
    internal class WindowsSimulaor
    {
        private Terminal terminal;
        public WindowsSimulaor() 
        {
            terminal = new Terminal();
        }
        public void CreateDirectories(string basePath, int numOfDirectories, int numOfSubdirectories, int numOfFiles, int fileSizeInGB)
        {
            IEnumerable<string> directoryList = Enumerable.Range(1, numOfDirectories).Select(num => Path.GetRandomFileName());
            Dictionary<string, List<string>> subdirectoriesByDirectory = directoryList.ToDictionary
            (
                directory => directory,
                directory => Enumerable.Range(1, numOfSubdirectories)
                    .Select(num => Path.GetRandomFileName())
                    .ToList()
            );
            int numOfFilesByDirectory = directoryList.Count();
            int countOfAllSubdirectories = subdirectoriesByDirectory.Values.SelectMany(subdirList => subdirList).Count();
            int totalNumOfFiles = numOfFilesByDirectory + countOfAllSubdirectories;
            foreach (var kv in subdirectoriesByDirectory)
            {
                string directoryPath = Path.Combine(basePath, kv.Key);
                Directory.CreateDirectory(directoryPath);
                CreateFile(directoryPath, fileSizeInGB, "GB", ".txt");
                foreach (var subdirectory in kv.Value)
                {
                    string subdirectoryPath = Path.Combine(directoryPath, subdirectory);
                    Directory.CreateDirectory(subdirectoryPath);
                }
            }
        }
        public void CreateDirectories(string basePath, int numOfDirectories, int numOfSubdirectories, string storageUnit, int fileSize,int numOfFiles, string fileType)
        {
            IEnumerable<string> directoryList = Enumerable.Range(1, numOfDirectories).Select(num => Path.GetRandomFileName());
            Dictionary<string, List<string>> subdirectoriesByDirectory = directoryList.ToDictionary
            (
                directory => directory,
                directory => Enumerable.Range(1, numOfSubdirectories)
                    .Select(num => Path.GetRandomFileName())
                    .ToList()
            );
            int numOfFilesByDirectory = directoryList.Count();
            int countOfAllSubdirectories = subdirectoriesByDirectory.Values.SelectMany(subdirList => subdirList).Count();
            int totalNumOfFiles = numOfFilesByDirectory + countOfAllSubdirectories;
            foreach (var kv in subdirectoriesByDirectory)
            {
                string directoryPath = Path.Combine(basePath, kv.Key);
                Directory.CreateDirectory(directoryPath);
                CreateFile(directoryPath, fileSize,storageUnit,fileType);
                foreach (var subdirectory in kv.Value)
                {
                    string subdirectoryPath = Path.Combine(directoryPath, subdirectory);
                    Directory.CreateDirectory(subdirectoryPath);
                }
            } 
        }
        public void CreateDirectories(string basePath, int breadth, int depth, long totalSizeInGB)
        {
            int bufferSize = 100;
            long fileSize = 0;
            // Get the available space on the file system containing the directory
            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(basePath));
            long availableSpaceInBytes = driveInfo.AvailableFreeSpace;
            long availableSpaceInGB = availableSpaceInBytes / (1024 * 1024 * 1024);
            // Check if there is enough space to create the file
            long totalInBytes = totalSizeInGB * 1024 * 1024 * 1024;
            if (availableSpaceInGB < totalSizeInGB)
            {
                Console.WriteLine($"Not enough space to create the file. Available space:{availableSpaceInGB}");
                Console.WriteLine($"If you want to continue with available space then press y then enter y or prss any other key to exit");
                string input = Console.ReadLine();
                if (input.Trim().ToUpperInvariant() != "Y")
                {
                    return;
                }
                totalSizeInGB = availableSpaceInGB - bufferSize;
            }


            long numOfFiles = GetNumOfFiles(breadth, depth);
            long sizePerfileInBytes = totalInBytes / numOfFiles;
            Console.WriteLine($"Directories:{numOfFiles + 1}");
            Console.WriteLine($"Files:{numOfFiles}");

            string blockSize = "1G";
            if (numOfFiles < totalSizeInGB)
            {
                fileSize = totalSizeInGB / numOfFiles;
                blockSize = "1G";
            }
            else if (numOfFiles > totalSizeInGB)
            {
                if (numOfFiles < (totalSizeInGB * 1024) / numOfFiles)
                {
                    fileSize = totalSizeInGB * 1024 / numOfFiles;
                    blockSize = "1M";
                }
                else if (numOfFiles < (totalSizeInGB * 1024 * 1024) / numOfFiles)
                {
                    fileSize = totalSizeInGB * 1024 * 1024 / numOfFiles;
                    blockSize = "1K";
                }
                else
                {
                    fileSize = totalSizeInGB * 1024 * 1024 * 1024 / numOfFiles;
                    blockSize = "1B";
                }

            }
            LoadData(basePath, breadth, depth, sizePerfileInBytes);

        }
        private void LoadData(string basePath, int breadth, int depth, long sizePerfileInBytes)
        {
            if (depth == 0)
            {
                return;
            }

            for (int i = 1; i <= breadth; i++)
            {
                string directoryPath = Path.Combine(basePath, Path.GetRandomFileName());
                string fileName = Path.GetRandomFileName();
                DirectoryInfo dirInfo = Directory.CreateDirectory(directoryPath);
                CreateFile(directoryPath, sizePerfileInBytes, ".txt");
                LoadData(directoryPath, breadth, depth - 1, sizePerfileInBytes);
            }
        }

        private void CreateFile(string directoryPath, long fileSizeInBytes, string fileExtension)
        {
            string fileName = Path.GetRandomFileName() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            // Create random data buffer
            byte[] buffer = new byte[4096]; //4KB

            // Create file and write random data
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                long remainingBytes = fileSizeInBytes;
                while (remainingBytes > 0)
                {
                    int bytesToWrite = (int)Math.Min(buffer.Length, remainingBytes);
                    RandomNumberGenerator.Fill(buffer);
                    fs.Write(buffer, 0, bytesToWrite);
                    remainingBytes -= bytesToWrite;
                }
            }
            Console.WriteLine($"File created: {filePath}");

        }

        private void CreateFile(string directory, long fileSize, string storageunit, string fileExtension)
        {
            long fileSizeInBytes = fileSize;
            if (storageunit.ToUpperInvariant() == "GB")
            {
                fileSizeInBytes = fileSize * 1024 * 1024 * 1024; // 500 MB
            }
            else if (storageunit.ToUpperInvariant() == "MB")
            {
                fileSizeInBytes = fileSize * 1024 * 1024; // 500 MB
            }
            else if (storageunit.ToUpperInvariant() == "KB")
            {
                fileSizeInBytes = fileSize * 1024; // 500 KB
            }

            CreateFile(directory, fileSizeInBytes, fileExtension);
        }
        private long GetNumOfFiles(int breadh, int depth)
        {
            long numOfDirectories = breadh;
            while (depth > 1)
            {
                numOfDirectories = (numOfDirectories + 1) * breadh;
                depth--;
            }

            return numOfDirectories;
        }
    }
}
