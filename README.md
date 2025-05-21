# SSMSJsonViewer

SSMS will make a hyperlink in the results grid for any data cast to the 
XML data type or when the query result is JSON, and if you click the hyperlink, it opens a new text document 
populated with the data from that grid cell.

This extension simply parses and pretty-prints the data when it can be parsed as JSON.

## Features
 - [x] Pretty-print and formatting of JSON results
 - [x] Seamless integration with SSMS

## Installation

Grab the latest build and run the VSIX

## Manual Installation

To manually install the SSMSJsonViewer extension, follow these steps:

1. **Build the Project**
   - Open the solution in Visual Studio.
   - Build the solution to generate the extension artifacts (DLLs, etc.).

2. **Locate the Build Artifacts**
   - After building, locate the output files (typically in the `bin\Debug` or `bin\Release` folder of the `SSMSJsonViewer` project).

3. **Copy Artifacts to SSMS Extensions Folder**
   - Manually copy the build artifacts to the SSMS extensions directory. The default path is usually:
     - For SSMS 21: `C:\Program Files\Microsoft SQL Server Management Studio 21\Release\Common7\IDE\Extensions\SSMSJsonViewer`

4. **Restart SSMS**
   - Close and reopen SSMS to load the extension.

## Notes
- The post-build task in this project automates the copying of artifacts to the SSMS extensions folder. If you prefer manual installation, follow the steps above.
- Ensure SSMS is closed before copying files to avoid file access issues.

