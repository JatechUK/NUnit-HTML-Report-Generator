using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Jatech.NUnit
{
    public class Options
    {
        [Option("input",HelpText = "input file",Required = true)]
        public string Input { get; set; }

        [Option("output",HelpText = "output file",Required = false)]
        public string Output { get; set; }

        [Option("summary", HelpText = "json file containing all addition info you want to dispaly in the test runner",Required = false)]
        public string Summary { get; set; }

        [HelpOption]
        public string GetCorrectSytax()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
