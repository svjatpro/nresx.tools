using System;
using CommandLine;

namespace nresx.CommandLine.Commands;

[Verb("help", HelpText = "help")]
public class HelpCommand : BaseCommand
{
    [Option('a', "all", HelpText = "List all commands", Required = false)]
    public bool ListAll { get; set; }

    protected override bool IsDryRunAllowed => false;

    protected override void ExecuteCommand()
    {
        //if (ListAll)
        {
            Console.WriteLine("Main Commands:");
            foreach (var t in Context.CommandTypes)
            {
                var verb = t.GetCustomAttributes(typeof(VerbAttribute), false);
                if (verb.Length > 0)
                {
                    var v = (VerbAttribute)verb[0];
                    Console.WriteLine($"  {v.Name,-12} {v.HelpText}");
                }
            }
            return;
        }

        //var help = HelpText.AutoBuild(result, h =>
        //{
        //    h.Heading = "nresx - resource tool";
        //    h.Copyright = "© 2025 nresx";
        //    h.Version = "0.3.0";
        //    h.AddPreOptionsLine("Usage: nresx <command> [options]");
        //    h.AddPostOptionsLine("");
        //    h.AddPostOptionsLine("Available commands:");
        //    foreach (var t in commandTypes)
        //    {
        //        var verb = t.GetCustomAttribute<VerbAttribute>();
        //        if (verb != null)
        //            h.AddPostOptionsLine($"  {verb.Name,-12} {verb.HelpText}");
        //    }
        //    h.AddPostOptionsLine("");
        //    h.AddPostOptionsLine("Use 'nresx <command> -h' for more details.");
        //    return h;
        //}, e => e);

        //Console.WriteLine($@"nresx help");
    }
}