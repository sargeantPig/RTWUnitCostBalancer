using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTWLib.Functions.EDU;
using RTWLib.Objects;

namespace RTWUnitCostBalancer
{

    enum commands
    {
        doAll,
        export,
        parse,
        balance,
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
            {commands.help, "displays the command list" }
        };

        static void Main(string[] args)
        {
            string[] files = GetFilesInDirectory(import);
            string[] realNames = new string[files.Count()];

            for(int i = 0; i < files.Count(); i++)
                realNames[i] = files[i].Split('\\').Last();


            EDU[] edus = new EDU[files.Count()];

            for (int i = 0; i < edus.Count(); i++)
                edus[i] = new EDU(true);


            DisplayTitle();

            string input;
            commands command;

            while (true)
            {
                input = Console.ReadLine();
                if (Enum.TryParse(input, out command))
                {
                    switch(command)
                    {
                        case commands.doAll: DoAll(edus, files, realNames);
                            break;
                        case commands.parse: 
                            ParseFiles(edus, files);
                            break;
                        case commands.help:
                            DisplayHelp();
                            break;
                        case commands.balance: 
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

        static void DisplayTitle()
        {
            Console.WriteLine(
                "###############################" + "r\n" +
                "### RTW Unit Costs Balancer ###" + "\r\n" +
                "##### Type help for help ######" + "\r\n" +
                "###############################");
        }

        static void DisplayHelp()
        {
            Console.WriteLine(
                "############" + "r\n" +
                "### HELP ###" + "\r\n" +
                "############");
            foreach (var kv in help)
            {
                Console.WriteLine(kv.Key.ToString() + " -- " + kv.Value);
            }
            Console.WriteLine(
                "################" + "r\n" +
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

        static void BalanceFiles(EDU[] files)
        {
            Console.WriteLine("starting balancer...");

            foreach (EDU edu in files)
            {
                foreach (Unit unit in edu.units)
                {
                    unit.cost[4] = (unit.primaryArmour.stat_pri_armour[1] - 4) * (50 + 50);
                    unit.cost[5] = (int)Math.Round(unit.cost[1] / 4.0);
                }
            }

            Console.WriteLine("balancer complete");

        }

        static bool ExportFiles(EDU[] files, string[] names)
        {
            Console.WriteLine("export starting...");
            for (int i = 0; i < files.Count(); i++)
            {
                if (files[i] == null)
                    return false;

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
        }
    }

}
