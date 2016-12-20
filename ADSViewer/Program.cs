using System;
using System.IO;
using System.Linq;
using Trinet.Core.IO.Ntfs;

namespace ADSViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: adsviewer -(l|s|d) <folder|file> [streamname]");
                return;
            }
            var selSwitch = ProcessSwitches(args);
            if (selSwitch == null)
                return;
            var parameters = args.Where(a => !a.StartsWith("-"));
            string fileName = ".";
            if (parameters.Count() > 0)
                fileName = parameters.First();
            var streamName = parameters.Skip(1).FirstOrDefault();
            if (File.Exists(fileName))
                ProcessFile(fileName, streamName, selSwitch);
            else if (Directory.Exists(fileName))
                foreach (var file in Directory.GetFiles(fileName))
                    ProcessFile(file, streamName, selSwitch);
            else
            {
                Console.WriteLine("error: file/folder does not exist");
                return;
            }
        }

        private static string ProcessSwitches(string[] args)
        {
            var switches = args.Where(a => a.StartsWith("-"));
            if (switches.Count() > 1)
            {
                Console.WriteLine("error: you can use only one switch at a time (-l, -s, -d)");
                return null;
            }
            var selSwitch = switches.Count() == 0 ? "l" : switches.First().Substring(1);
            if (!"lsd".Contains(selSwitch))
            {
                Console.WriteLine("error: valid switches: -l(ist), -s(how contents), -d(elete)");
                return null;
            }
            return selSwitch;
        }

        private static void ProcessFile(string fileName, string streamName, string selSwitch)
        {
            switch (selSwitch)
            {
                case "l":
                    ListStreams(fileName);
                    break;
                case "s":
                    ShowContents(fileName, streamName);
                    break;
                case "d":
                    DeleteStream(fileName, streamName);
                    break;
            }
        }

        private static void DeleteStream(string fileName, string streamName)
        {
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine($"{file.Name} - {file.Length:n0}");
            if (!string.IsNullOrWhiteSpace(streamName) && file.AlternateDataStreamExists(streamName))
            {
                file.DeleteAlternateDataStream(streamName);
                Console.WriteLine($"    {streamName} deleted");
            }
            else if (string.IsNullOrWhiteSpace(streamName))
                foreach (AlternateDataStreamInfo s in file.ListAlternateDataStreams())
                {
                    s.Delete();
                    Console.WriteLine($"    {s.Name} deleted");
                }
        }

        private static void ShowContents(string fileName, string streamName)
        {
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine($"{file.Name} - {file.Length:n0}");
            if (!string.IsNullOrWhiteSpace(streamName) && file.AlternateDataStreamExists(streamName))
            {
                AlternateDataStreamInfo s = file.GetAlternateDataStream(streamName, FileMode.Open);
                ShowContentsOfStream(file, s);
            }
            else if (string.IsNullOrWhiteSpace(streamName))
                foreach (AlternateDataStreamInfo s in file.ListAlternateDataStreams())
                    ShowContentsOfStream(file, s);
        }

        private static void ShowContentsOfStream(FileInfo file, AlternateDataStreamInfo s)
        {
            Console.WriteLine($"    {s.Name}");
            using (TextReader reader = s.OpenText())
            {
                Console.WriteLine(reader.ReadToEnd());
            }
        }

        private static void ListStreams(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            Console.WriteLine($"{file.Name} - {file.Length:n0}");
            foreach (AlternateDataStreamInfo s in file.ListAlternateDataStreams())
                Console.WriteLine($"    {s.Name} - {s.Size:n0}");
        }
    }
}
