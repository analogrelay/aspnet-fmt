using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AspNetFormatter.Rules;
using Microsoft.Extensions.CommandLineUtils;
using System.Diagnostics;

namespace AspNetFormatter
{
    public class Program
    {
        private static readonly HashSet<string> ExcludedDirectories = new HashSet<string>()
        {
            ".git",
            "bin",
            "obj"
        };

        public static int Main(string[] args)
        {
            if (args.Any(a => a == "--debug"))
            {
                Console.WriteLine($"Waiting for debugger. Process ID: {Process.GetCurrentProcess().Id}");
                Console.WriteLine("Press ENTER to resume execution");
                Console.ReadLine();
                args = args.Where(a => a != "--debug").ToArray();
            }

            var app = new CommandLineApplication();
            app.Name = "aspnet-fmt";
            app.FullName = "ASP.NET Code Formatter";
            app.Description = "Checks code against ASP.NET Code style guidelines";
            app.HelpOption("-h|-?|--help");

            var directoriesArgument = app.Argument("DIRECTORIES", "The directories to check", multipleValues: true);
            var fixOption = app.Option("-f|--fix", "Fix the issues, rather than just reporting them", CommandOptionType.NoValue);

            app.OnExecute(() => Run(directoriesArgument.Values.ToList(), fixOption.HasValue()));

            return app.Execute(args);
        }

        private static int Run(IList<string> directories, bool fix)
        {
            var rules = new List<FormattingRule>()
            {
                new LicenseHeaderRule()
            };

            if (directories.Count == 0)
            {
                directories.Add(Directory.GetCurrentDirectory());
            }

            Console.WriteLine("Processing files in the following directories: ");
            foreach (var dir in directories)
            {
                Console.WriteLine($"* {dir}");
            }

            Console.WriteLine("Starting run...");
            var success = true;
            foreach (var dir in directories)
            {
                success &= ProcessDir(dir, fix, rules);
            }

            return success ? 0 : 1;
        }

        private static bool ProcessDir(string path, bool fix, IEnumerable<FormattingRule> rules)
        {
            if (ExcludedDirectories.Contains(Path.GetFileName(path)))
            {
                Console.WriteLine($"* Excluded directory {path}.");
                return true;
            }

            var success = true;
            foreach (var file in Directory.GetFiles(path))
            {
                success &= ProcessFile(file, fix, rules);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                success &= ProcessDir(dir, fix, rules);
            }
            return success;
        }

        private static bool ProcessFile(string file, bool fix, IEnumerable<FormattingRule> rules)
        {
            // Load up the file
            var success = true;
            var content = File.ReadAllText(file);

            // Check rules
            foreach (var rule in rules)
            {
                if (!rule.Validate(file, content))
                {
                    success = false;
                    if (fix)
                    {
                        content = rule.Fix(file, content);
                    }
                    else
                    {
                        Console.WriteLine($"* {file} does not satisfy rule: {rule.GetType().Name}");
                    }
                }
            }

            if (fix && !success)
            {
                // Write the new content to the file
                File.WriteAllText(file, content);
            }
            return success;
        }
    }
}
