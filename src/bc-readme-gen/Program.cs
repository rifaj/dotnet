﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

// Test Comment
// Test
namespace bc_readme_gen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Bad argument.");
                Console.WriteLine("You need to specify location of breaking change documents.");
            }
            
            string bcpath = args[0];

            var bcList = new Dictionary<string,List<BreakingChange>>();
            var template = "README-template.md";
            string templateText = null;
            string bcpathREADME = Path.Combine(bcpath, "README.md");
            var bcdir = new DirectoryInfo(bcpath);
            const string versionIntroduced = "### Version Introduced";


            foreach(var changeFile in bcdir.GetFiles("*.md"))
            {
                if (changeFile.Name == "! Template.md" || changeFile.Name == "README.md")
                {
                    continue;
                }

                var change = new BreakingChange();

                using (var reader = changeFile.OpenText())
                {
                    var titleLine = reader.ReadLine();
                    var title = titleLine.Substring(3);
                    change.Title = title;
                    change.Path = changeFile.Name;

                    string versionLine = null;
                    while ((versionLine = reader.ReadLine()) != null)
                    {
                        if (versionLine != versionIntroduced)
                        {
                            continue;
                        }
                        
                        var version = reader.ReadLine();
                        change.Version = version;
                        break;
                    }

                    if (!bcList.ContainsKey(change.Version))
                    {
                        bcList.Add(change.Version,new List<BreakingChange>());
                    }

                    var versionChanges = bcList[change.Version];
                    versionChanges.Add(change);
                }
            }

            using (var templateReader = File.OpenText(template))
            {
                templateText = templateReader.ReadToEnd();
            }

            var keysArrayLength = bcList.Keys.Count;
            var keysArray = new string[keysArrayLength];
            bcList.Keys.CopyTo(keysArray,0);
            
            Array.Sort(keysArray);
            Array.Reverse(keysArray);

            using (var writer = File.CreateText(bcpathREADME))
            {
                writer.Write(templateText);
                writer.WriteLine();

                foreach(var ver in keysArray)
                {
                    var hashVersion = new StringBuilder();
                    foreach (var c in ver)
                    {
                        if (c != '.')
                        {
                            hashVersion.Append(c);
                        }
                    }

                    var hashLink = $"net-framework-{hashVersion.ToString()}";
                    writer.WriteLine($"- [.NET Framework {ver}](#{hashLink})");
                }

                foreach(var ver in keysArray)
                {
                    writer.WriteLine();
                    writer.WriteLine($"## .NET Framework {ver}");
                    writer.WriteLine();

                    var breaks = bcList[ver];

                    breaks.Sort((break1,break2)=>break1.Title.CompareTo(break2.Title));

                    foreach (var b in breaks)
                    {
                        writer.WriteLine($"- [{b.Title}]({b.Path})");
                    }
                }

                writer.WriteLine();
                writer.WriteLine("This file was generated by [Breaking Change Readme Generator](https://github.com/Microsoft/dotnet/blob/master/src/bc-readme-gen).");
            }            
        }

    }
}

public class BreakingChange
{
    public string Title;
    public string Path;
    public string Version;
}
