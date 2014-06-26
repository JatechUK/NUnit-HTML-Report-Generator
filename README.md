NUnit HTML Report Generator
===========================

This project is used to convert NUnit generated XML result files into a self-contained Bootstrap based HTML page.

##Example
* [Click here to view the example output](http://htmlpreview.github.io/?https://github.com/JatechUK/NUnit-HTML-Report-Generator/blob/master/ExampleResults.html)
* [Click here to see the input that generated the output](https://raw.githubusercontent.com/JatechUK/NUnit-HTML-Report-Generator/master/NUnit%20HTML%20Report%20Generator/ExampleResults.xml)

##Building 
1. Open in Visual Studio 2012
2. Click Build > Build Solution

##Running
Included in the project is an example NUnit XML results file originally found at [vincentp-tests playground](https://github.com/vincentp-test/playground/blob/master/test-results.xml). The project has a debug command line arguement set in the project settings that will pass the filename of the example file into the software to generate a HTML file. Clicking the Start Debugging/F5 button in visual studio will run the software and generate a HTML file based on the example XML file. The output file will be found in the same directory as your executable (bin).

##Usage
NUnitHTMLReportGenerator.exe [input-path] [output-path]

##Disclaimer
This project is maintained by [Luke Browning](http://github.com/luke-browning) on behalf of [Jatech](http://github.com/JatechUK) and is released under the [GPLv2.0](https://raw.githubusercontent.com/JatechUK/NUnit-HTML-Report-Generator/master/LICENSE).
