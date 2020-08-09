using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTWLib.Functions;
using RTWLib.Functions.EDU;
using RTWLib.Objects;
using RTWTools;

namespace RTWUnitCostBalancer
{

    enum commands
    {
        doAll,
        export,
        parse,
        balance,
        refresh,
        filelist,
        stats,
        help
    }

    class Program
    {
        static string import = @"files\import\";
        static string export = @"files\export\";

        static Dictionary<commands, string> help = new Dictionary<commands, string>()
        {

            {commands.doAll, "parse, balance and then export all in one!"},
            {commands.parse, "parse the files in the import folder"},
            {commands.balance, "balance the files that have been parsed" },
            {commands.export, "export the files to the export folder"},
            {commands.refresh, "reload files - use if you've added more files to the import folder"},
            {commands.filelist, "display  list of files loaded in (may not be parsed)"},
            {commands.stats, "displays stats for the given file - cmd format 'stats fileIdx' " },
            {commands.help, "displays the command list" }
        };

        static void Main(string[] args)
        {
            EDU[] edus;
            string[] realNames;
            string[] files = Refresh(out realNames, out edus);

            DisplayTitle();
            DisplayFileList(realNames);

            string input;
            commands command;

            while (true)
            {
                input = Console.ReadLine();
                if (Enum.TryParse(Functions_General.GetFirstWord(input), out command))
                {
                    switch(command)
                    {
                        case commands.doAll: 
                            DoAll(edus, files, realNames);
                            DisplayFileList(realNames);
                            break;
                        case commands.parse: 
                            ParseFiles(edus, files);
                            DisplayFileList(realNames);
                            break;
                        case commands.help:
                            DisplayHelp();
                            break;
                        case commands.refresh:
                            files = Refresh(out realNames, out edus);
                            DisplayFileList(realNames);
                            break;
                        case commands.balance:
                            if (!BalanceFiles(edus))
                                Console.WriteLine("files are not parsed!");
                            DisplayFileList(realNames);
                            break;
                        case commands.filelist: 
                            DisplayFileList(realNames);
                            break;
                        case commands.stats:
                            if (!DisplayStats(input, edus))
                                Console.WriteLine("Error, files may not be parsed or the index is invalid");
                            break;
                        case commands.export:
                            if (!ExportFiles(edus, realNames))
                                Console.WriteLine("Files are null - be sure to use the command 'parse' before using 'export'");
                            else Console.WriteLine("export complete");
                            break;
                    }

                }
                else Console.WriteLine("Invalid Command");
            
            }
        }

        static string[] Refresh(out string[] names, out EDU[] edus)
        {
            string[] files = GetFilesInDirectory(import);
            names = new string[files.Count()];

            for (int i = 0; i < files.Count(); i++)
                names[i] = files[i].Split('\\').Last();

            edus = new EDU[files.Count()];

            for (int i = 0; i < edus.Count(); i++)
                edus[i] = new EDU(true);

            return files;
        }

        static bool DisplayStats(string line, EDU[] edus)
        {

            if (!isParsed(edus))
                return false;

            int indx = 0;
            string[] cmdSplit = line.Split(' ');
            if (cmdSplit.Count() > 1)
            {
                if (!int.TryParse(cmdSplit[1].Trim(), out indx))
                {
                    Console.WriteLine("Invalid index");
                    return false;
                }
            }

            if (indx >= edus.Count() || indx < 0)
                return false;


            Console.WriteLine(GetAnalysis(edus[indx]).Print());

            return true;
        }

        static void DisplayFileList(string[] names)
        {
            Console.WriteLine(
                "###############################" + "\r\n" +
                "########## File List ##########" + "\r\n" +
                "###############################" + "\r\n");
            Console.WriteLine(names.ArrayToString(true, true));
        }


        static void DisplayTitle()
        {
            Console.WriteLine(
                "###############################" + "\r\n" +
                "### RTW Unit Costs Balancer ###" + "\r\n" +
                "##### Type help for help ######" + "\r\n" +
                "###############################");
        }

        static void DisplayHelp()
        {
            Console.WriteLine(
                "############" + "\r\n" +
                "### HELP ###" + "\r\n" +
                "############");
            foreach (var kv in help)
            {
                Console.WriteLine(kv.Key.ToString() + " -- " + kv.Value);
            }
            Console.WriteLine(
                "################" + "\r\n" +
                "### END HELP ###" + "\r\n" +
                "################");
        }

        static string[] GetFilesInDirectory(string directory)
        {
            List<string> files = new List<string>();
            foreach (string str in Directory.EnumerateFiles(directory))
            {
                if (str.EndsWith(".txt") && str.Contains("descr_unit"))
                {
                    files.Add(str);
                }
            }

            return files.ToArray();
        }

        static void DoAll(EDU[] files, string[] names, string[] exportNames)
        {
            ParseFiles(files, names);
            BalanceFiles(files);
            ExportFiles(files, exportNames);
        }

        static AnalysisData GetAnalysis(EDU edu)
        {
            AnalysisData analysisData = new AnalysisData();
            analysisData.Analyse(edu);
            return analysisData;
        }

        static bool BalanceFiles(EDU[] files)
        {
            Console.WriteLine("starting balancer...");

            if (!isParsed(files))
                return false;

            foreach (EDU edu in files)
            {
                AnalysisData ad = GetAnalysis(edu);
                Balancer balancer = new Balancer(ad.atkMin, 4, 4, ad.defMin, 2, 7);
                foreach (Unit unit in edu.units)
                {
                    unit.cost[1] = (int)balancer.CalculateCost(unit);
                    unit.cost[2] = (int)balancer.CalculateUpkeep(unit);
                    unit.cost[3] = (int)balancer.CalculateWepUpgrade(unit);
                    unit.cost[4] = (int)balancer.CalculateArmourUpgradeCost(unit);
                    unit.cost[5] = (int)balancer.CalculateCustomCost(unit);
                }
            }

            Console.WriteLine("balancer complete");
            return true;
        }

        static bool ExportFiles(EDU[] files, string[] names)
        {
            Console.WriteLine("export starting...");

            if (!isParsed(files))
                return false;

            for (int i = 0; i < files.Count(); i++)
            {
                Console.WriteLine("exporting " + names[i]);
                files[i].ToFile(export + names[i]);
            }

            return true;
        }

        static void ParseFiles(EDU[] files, string[] names)
        {
            Console.WriteLine("parsing starting...");
            int i = 0;
            string currentLine;
            int lineNumber;
            foreach (EDU edu in files)
            {
                Console.WriteLine("Attempting parse of: " + names[i]);
                edu.Parse(new string[] { names[i],  names[i] }, out lineNumber, out currentLine);
                Console.WriteLine(names[i] + " -- success");
                i++;
            }
            Console.WriteLine("parsing complete");
        }

        static bool isParsed(EDU[] edus)
        {
            foreach (EDU edu in edus)
            {
                if (edu.units.Count == 0)
                    return false;
            }
            return true;
        }
    }

}
