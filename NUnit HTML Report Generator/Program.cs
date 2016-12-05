#region File Header
// <copyright application="NUnit HTML Report Generator" file="Program.cs" company="Jatech Limited">
// Copyright (c) 2014 Jatech Limited. All rights reserved.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
// </copyright>
// <author>luke-browning</author>
// <date>26/06/2014</date>
// <summary>
// Console application to convert NUnit XML results file to
// a standalone HTML page based on Bootstrap 3
// </summary>
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Jatech.NUnit
{
    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        #region Private Constants

        /// <summary>
        /// Usage example.
        /// </summary>
        private const string Usage = "Usage: NUnitHTMLReportGenerator.exe [input-path] [output-path]";

        /// <summary>
        /// Regular expression for acceptable characters in html id.
        /// </summary>
        private static readonly Regex Regex = new Regex("[^a-zA-Z0-9 -]");

        /// <summary>
        /// Switches for displaying the basic help when executed.
        /// </summary>
        private static readonly List<string> HelpParameters = new List<string>() { "?", "/?", "help" };

        #endregion

        #region Main

        /// <summary>
        /// Main entry-point for this application.
        /// </summary>
        /// <param name="args">Array of command-line argument strings.</param>
        static void Main(string[] args)
        {
            StringBuilder html = new StringBuilder();
            bool ok = false;
            string input = string.Empty, output = string.Empty;

            if (args.Length == 1)
            {
                input = args[0];

                // Check if the user wants help, otherwise assume its a
                // filename that needs to be processed
                if (HelpParameters.Contains(input))
                {
                    Console.WriteLine(Usage);
                }
                else
                {
                    // Output file with the same name in the same folder
                    // with a html extension
                    output = Path.ChangeExtension(input, "html");

                    // Check input file exists and output file doesn't
                    ok = CheckInputAndOutputFile(input, output);
                }
            }
            else if (args.Length == 2)
            {
                // If two parameters are passed, assume the first is 
                // the input path and the second the output path
                input = args[0];
                output = args[1];

                // Check input file exists and output file doesn't
                ok = CheckInputAndOutputFile(input, output);
            }
            else
            {
                // Display the usage message
                Console.WriteLine(Usage);
            }

            // If input file exists and output doesn't exist
            if (ok)
            {
                // Generate the HTML page
                html.Append(GetHTML5Header("Results"));
                html.Append(ProcessFile(input));
                html.Append(GetHTML5Footer());

                // Save HTML to the output file
                File.WriteAllText(output, html.ToString());
            }
        }

        #endregion

        #region Private Methods

        #region File Access

        /// <summary>
        /// Check input and output file existence
        /// Input file should exist, output file should not
        /// </summary>
        /// <param name="input">The input file name</param>
        /// <param name="output">The output name</param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private static bool CheckInputAndOutputFile(string input, string output)
        {
            bool ok = false;

            if (File.Exists(input))
            {
                if (!File.Exists(output))
                {
                    ok = true;
                }
                else
                {
                    Console.WriteLine(string.Format("Output file '{0}' already exists", output));
                }
            }
            else
            {
                Console.WriteLine("File does not exist");
            }

            return ok;
        }

        #endregion

        #region Processing

        /// <summary>
        /// Process the results file.
        /// </summary>
        /// <param name="file">The filename of the XML results file to parse.</param>
        /// <returns>
        /// HTML page content.
        /// </returns>
        private static string ProcessFile(string file)
        {
            StringBuilder html = new StringBuilder();
            XElement doc = XElement.Load(file);

            // Load summary values
            string testName = doc.Attribute("name").Value;
            int testTests = doc.Attribute("total") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("total").Value) ? doc.Attribute("total").Value : "0");
            int testErrors = doc.Attribute("errors") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("errors").Value) ? doc.Attribute("errors").Value : "0");
            int testFailures = doc.Attribute("failures") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("failures").Value) ? doc.Attribute("failures").Value : "0");
            int testNotRun = doc.Attribute("not-run") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("not-run").Value) ? doc.Attribute("not-run").Value : "0");
            int testInconclusive = doc.Attribute("inconclusive") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("inconclusive").Value) ? doc.Attribute("inconclusive").Value : "0");
            int testIgnored = doc.Attribute("ignored") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("ignored").Value) ? doc.Attribute("ignored").Value : "0");
            int testSkipped = doc.Attribute("skipped") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("skipped").Value) ? doc.Attribute("skipped").Value : "0");
            int testInvalid = doc.Attribute("invalid") == null ? 0 : int.Parse(!string.IsNullOrEmpty(doc.Attribute("invalid").Value) ? doc.Attribute("invalid").Value : "0");
            DateTime testDate = DateTime.Parse(string.Format("{0} {1}", doc.Attribute("date").Value, doc.Attribute("time").Value));
            string testPlatform = doc.Element("environment").Attribute("platform").Value;

            // Calculate the success rate
            decimal percentage = 0;
            if (testTests > 0)
            {
                int failures = testErrors + testFailures;
                percentage = decimal.Round(decimal.Divide(failures, testTests) * 100, 1);
            }

            // Container
            html.AppendLine("<div class=\"container-fluid page\">");

            // Summary panel
            html.AppendLine("<div class=\"row\">");
            html.AppendLine("<div class=\"col-md-12\">");
            html.AppendLine("<div class=\"panel panel-default\">");
            html.AppendLine(string.Format("<div class=\"panel-heading\">Summary - <small>{0}</small></div>", testName));
            html.AppendLine("<div class=\"panel-body\">");

            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Tests</div><div class=\"val ignore-val\">{0}</div></div>", testTests));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Failures</div><div class=\"val {1}\">{0}</div></div>", testFailures, testFailures > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Errors</div><div class=\"val {1}\">{0}</div></div>", testErrors, testErrors > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Not Run</div><div class=\"val {1}\">{0}</div></div>", testNotRun, testNotRun > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Inconclusive</div><div class=\"val {1}\">{0}</div></div>", testInconclusive, testInconclusive > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Ignored</div><div class=\"val {1}\">{0}</div></div>", testIgnored, testIgnored > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Skipped</div><div class=\"val {1}\">{0}</div></div>", testSkipped, testSkipped > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Invalid</div><div class=\"val {1}\">{0}</div></div>", testInvalid, testInvalid > 0 ? "text-danger" : string.Empty));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Date</div><div class=\"val\">{0}</div></div>", testDate.ToString("d MMM")));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Time</div><div class=\"val\">{0}</div></div>", testDate.ToShortTimeString()));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Platform</div><div class=\"val\">{0}</div></div>", testPlatform));
            html.AppendLine(string.Format("<div class=\"col-md-2 col-sm-4 col-xs-6 text-center\"><div class=\"stat\">Success Rate</div><div class=\"val\">{0}%</div></div>", 100 - percentage));

            // End summary panel
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Process test fixtures
            html.Append(ProcessFixtures(doc.Descendants("test-suite").Where(x => x.Attribute("type") != null && x.Attribute("type").Value == "TestFixture")));

            // End container
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        /// <summary>
        /// Process the test fixtures.
        /// </summary>
        /// <param name="fixtures">The test-fixture elements.</param>
        /// <returns>
        /// Fixtures as HTML.
        /// </returns>
        private static string ProcessFixtures(IEnumerable<XElement> fixtures)
        {
            StringBuilder html = new StringBuilder();
            int index = 0;
            string fixtureName, fixtureNamespace, fixtureTime, fixtureResult, fixtureReason;

            // Loop through all of the fixtures
            foreach (var fixture in fixtures)
            {
                // Load fixture details
                fixtureName = fixture.Attribute("name").Value;
                fixtureNamespace = GetElementNamespace(fixture);
                fixtureTime = fixture.Attribute("time") != null ? fixture.Attribute("time").Value : string.Empty;
                fixtureResult = fixture.Attribute("result").Value;
                fixtureReason = fixture.Element("reason") != null ? fixture.Element("reason").Element("message").Value : string.Empty;

                html.AppendLine("<div class=\"col-md-3\">");
                html.AppendLine("<div class=\"panel ");

                // Colour code panels
                switch (fixtureResult.ToLower())
                {
                    case "success":
                        html.Append("panel-success");
                        break;
                    case "ignored":
                        html.Append("panel-info");
                        break;
                    case "failure":
                    case "error":
                        html.Append("panel-danger");
                        break;
                    default:
                        html.Append("panel-default");
                        break;
                }

                html.Append("\">");
                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine(string.Format("{0} - <small>{1}</small><small class=\"pull-right\">{2}</small>", fixtureName, fixtureNamespace, fixtureTime));

                // If the fixture has a reason, display an icon 
                // on the top of the panel with a tooltip containing 
                // the reason
                if (!string.IsNullOrEmpty(fixtureReason))
                {
                    html.AppendLine(string.Format("<span class=\"glyphicon glyphicon-info-sign pull-right info hidden-print\" data-toggle=\"tooltip\" title=\"{0}\"></span>", fixtureReason));
                }

                html.AppendLine("</div>");
                html.AppendLine("<div class=\"panel-body\">");

                // Generate a unique id for the modal dialog
                string modalId = string.Format("modal-{0}-{1}", Regex.Replace(HttpUtility.UrlEncode(fixtureName), string.Empty), index++);

                html.AppendLine("<div class=\"text-center\" style=\"font-size: 1.5em;\">");

                // Add a colour coded link to the modal dialog
                switch (fixtureResult.ToLower())
                {
                    case "success":
                        html.AppendLine(string.Format("<a href=\"#{0}\" role=\"button\" data-toggle=\"modal\" class=\"text-success no-underline\">", modalId));
                        html.AppendLine("<span class=\"glyphicon glyphicon-ok-sign\"></span>");
                        html.AppendLine("<span class=\"test-result\">Success</span>");
                        html.AppendLine("</a>");
                        break;
                    case "ignored":
                        html.AppendLine(string.Format("<a href=\"#{0}\" role=\"button\" data-toggle=\"modal\" class=\"text-info no-underline\">", modalId));
                        html.AppendLine("<span class=\"glyphicon glyphicon-info-sign\"></span>");
                        html.AppendLine("<span class=\"test-result\">Ignored</span>");
                        html.AppendLine("</a>");
                        break;
                    case "notrunnable":
                        html.AppendLine(string.Format("<a href=\"#{0}\" role=\"button\" data-toggle=\"modal\" class=\"text-default no-underline\">", modalId));
                        html.AppendLine("<span class=\"glyphicon glyphicon-remove-sign\"></span>");
                        html.AppendLine("<span class=\"test-result\">Not Runnable</span>");
                        html.AppendLine("</a>");
                        break;
                    case "failure":
                    case "error":
                        html.AppendLine(string.Format("<a href=\"#{0}\" role=\"button\" data-toggle=\"modal\" class=\"text-danger no-underline\">", modalId));
                        html.AppendLine("<span class=\"glyphicon glyphicon-exclamation-sign\"></span>");
                        html.AppendLine("<span class=\"test-result\">Failed</span>");
                        html.AppendLine("</a>");
                        break;
                    default:
                        break;
                }

                html.AppendLine("</div>");

                // Generate a printable view of the fixtures
                html.AppendLine(GeneratePrintableView(fixture, fixtureReason));

                // Generate the modal dialog that will be shown
                // if the user clicks on the test-fixtures
                html.AppendLine(GenerateFixtureModal(fixture, modalId, fixtureName, fixtureReason));

                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Gets an elements namespace.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>
        /// The element namespace.
        /// </returns>
        private static string GetElementNamespace(XElement element)
        {
            // Move up the tree to get the parent elements
            var namespaces = element.Ancestors("test-suite").Where(x => x.Attribute("type").Value.ToLower() == "namespace");

            // Get the namespace
            return string.Join(".", namespaces.Select(x => x.Attribute("name").Value));
        }

        #endregion

        #region HTML Helpers

        /// <summary>
        /// Generates a printable view of test cases.
        /// </summary>
        /// <param name="fixture">The test fixture.</param>
        /// <param name="warningMessage">Warning message to display.</param>
        /// <returns>
        /// The printable view as HTML.
        /// </returns>
        private static string GeneratePrintableView(XElement fixture, string warningMessage)
        {
            StringBuilder html = new StringBuilder();

            string name, result;
            html.AppendLine("<div class=\"visible-print printed-test-result\">");

            // Display a warning message if set
            if (!string.IsNullOrEmpty(warningMessage))
            {
                html.AppendLine(string.Format("<div class=\"alert alert-warning\"><strong>Warning:</strong> {0}</div>", warningMessage));
            }

            // Loop through test cases in the fixture
            foreach (var testCase in fixture.Descendants("test-case"))
            {
                // Get test case properties
                name = testCase.Attribute("name").Value;
                result = testCase.Attribute("result").Value;

                // Remove namespace if in name
                name = name.Substring(name.LastIndexOf('.') + 1, name.Length - name.LastIndexOf('.') - 1);

                // Create colour coded panel based on result
                html.AppendLine("<div class=\"panel ");

                switch (result.ToLower())
                {
                    case "success":
                        html.Append("panel-success");
                        break;
                    case "ignored":
                        html.Append("panel-info");
                        break;
                    case "failure":
                    case "error":
                        html.Append("panel-danger");
                        break;
                    default:
                        html.Append("panel-default");
                        break;
                }

                html.Append("\">");

                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendLine(name);
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendLine("<div class=\"panel-body\">");
                html.AppendLine(string.Format("<div><strong>Result:</strong> {0}</div>", result));

                // Add failure messages if available
                if (testCase.Elements("failure").Count() == 1)
                {
                    html.AppendLine(string.Format("<div><strong>Message:</strong> {0}</div>", testCase.Element("failure").Element("message").Value));
                    html.AppendLine(string.Format("<div><strong>Stack Trace:</strong> <pre>{0}</pre></div>", testCase.Element("failure").Element("stack-trace").Value));
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");

            return html.ToString();
        }

        /// <summary>
        /// Generates a modal dialog to display the test-cases in a fixture.
        /// </summary>
        /// <param name="fixture">The fixture element.</param>
        /// <param name="modalId">Identifier for the modal dialog</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns>
        /// The dialogs HTML.
        /// </returns>
        private static string GenerateFixtureModal(XElement fixture, string modalId, string title, string warningMessage)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine(string.Format("<div class=\"modal fade\" id=\"{0}\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">", modalId));
            html.AppendLine("<div class=\"modal-dialog\">");
            html.AppendLine("<div class=\"modal-content\">");
            html.AppendLine("<div class=\"modal-header\">");
            html.AppendLine("<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-hidden=\"true\">&times;</button>");
            html.AppendLine(string.Format("<h4 class=\"modal-title\" id=\"myModalLabel\">{0}</h4>", title));
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"modal-body\">");

            int i = 0;
            string name, result;
            html.AppendLine(string.Format("<div class=\"panel-group no-bottom-margin\" id=\"{0}-accordion\">", modalId));

            if (!string.IsNullOrEmpty(warningMessage))
            {
                html.AppendLine(string.Format("<div class=\"alert alert-warning\"><strong>Warning:</strong> {0}</div>", warningMessage));
            }

            // Add each test case to the dialog, colour 
            // coded based on the result
            foreach (var testCase in fixture.Descendants("test-case"))
            {
                // Get properties
                name = testCase.Attribute("name").Value;
                result = testCase.Attribute("result").Value;

                // Remove namespace if included
                name = name.Substring(name.LastIndexOf('.') + 1, name.Length - name.LastIndexOf('.') - 1);

                html.AppendLine("<div class=\"panel ");

                switch (result.ToLower())
                {
                    case "success":
                        html.Append("panel-success");
                        break;
                    case "ignored":
                        html.Append("panel-info");
                        break;
                    case "failure":
                    case "error":
                        html.Append("panel-danger");
                        break;
                    default:
                        html.Append("panel-default");
                        break;
                }

                html.Append("\">");

                html.AppendLine("<div class=\"panel-heading\">");
                html.AppendLine("<h4 class=\"panel-title\">");
                html.AppendLine(string.Format("<a data-toggle=\"collapse\" data-parent=\"#{1}\" href=\"#{1}-accordion-{2}\">{0}</a>", name, modalId, i));
                html.AppendLine("</h4>");
                html.AppendLine("</div>");
                html.AppendLine(string.Format("<div id=\"{0}-accordion-{1}\" class=\"panel-collapse collapse\">", modalId, i++));
                html.AppendLine("<div class=\"panel-body\">");
                html.AppendLine(string.Format("<div><strong>Result:</strong> {0}</div>", result));

                // Add failure messages if available
                if (testCase.Elements("failure").Count() == 1)
                {
                    html.AppendLine(string.Format("<div><strong>Message:</strong> {0}</div>", testCase.Element("failure").Element("message").Value));
                    html.AppendLine(string.Format("<div><strong>Stack Trace:</strong> <pre>{0}</pre></div>", testCase.Element("failure").Element("stack-trace").Value));
                }

                html.AppendLine("</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"modal-footer\">");
            html.AppendLine("<button type=\"button\" class=\"btn btn-primary\" data-dismiss=\"modal\">Close</button>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        #region HTML5 Template

        /// <summary>
        /// Gets the HTML 5 header.
        /// </summary>
        /// <param name="title">The title for the header.</param>
        /// <returns>
        /// The HTML 5 header.
        /// </returns>
        private static string GetHTML5Header(string title)
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine("<!doctype html>");
            header.AppendLine("<html lang=\"en\">");
            header.AppendLine("  <head>");
            header.AppendLine("    <meta charset=\"utf-8\">");
            header.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1\" />"); // Set for mobile
            header.AppendLine(string.Format("    <title>{0}</title>", title));

            // Add custom scripts
            header.AppendLine("    <script>");

            // Include jQuery in the page
            header.AppendLine(Properties.Resources.jQuery);
            header.AppendLine("    </script>");
            header.AppendLine("    <script>");

            // Include Bootstrap in the page
            header.AppendLine(Properties.Resources.BootstrapJS);
            header.AppendLine("    </script>");
            header.AppendLine("    <script type=\"text/javascript\">");
            header.AppendLine("    $(document).ready(function() { ");
            header.AppendLine("        $('[data-toggle=\"tooltip\"]').tooltip({'placement': 'bottom'});");
            header.AppendLine("    });");
            header.AppendLine("    </script>");

            // Add custom styles
            header.AppendLine("    <style>");

            // Include Bootstrap CSS in the page
            header.AppendLine(Properties.Resources.BootstrapCSS);
            header.AppendLine("    .page { margin: 15px 0; }");
            header.AppendLine("    .no-bottom-margin { margin-bottom: 0; }");
            header.AppendLine("    .printed-test-result { margin-top: 15px; }");
            header.AppendLine("    .reason-text { margin-top: 15px; }");
            header.AppendLine("    .scroller { overflow: scroll; }");
            header.AppendLine("    @media print { .panel-collapse { display: block !important; } }");
            header.AppendLine("    .val { font-size: 38px; font-weight: bold; margin-top: -10px; }");
            header.AppendLine("    .stat { font-weight: 800; text-transform: uppercase; font-size: 0.85em; color: #6F6F6F; }");
            header.AppendLine("    .test-result { display: block; }");
            header.AppendLine("    .no-underline:hover { text-decoration: none; }");
            header.AppendLine("    .text-default { color: #555; }");
            header.AppendLine("    .text-default:hover { color: #000; }");
            header.AppendLine("    .info { color: #888; }");
            header.AppendLine("    </style>");
            header.AppendLine("  </head>");
            header.AppendLine("  <body>");

            return header.ToString();
        }

        /// <summary>
        /// Gets the HTML 5 footer.
        /// </summary>
        /// <returns>
        /// A HTML 5 footer.
        /// </returns>
        private static string GetHTML5Footer()
        {
            StringBuilder footer = new StringBuilder();
            footer.AppendLine("  </body>");
            footer.AppendLine("</html>");

            return footer.ToString();
        }

        #endregion

        #endregion

        #endregion
    }
}
