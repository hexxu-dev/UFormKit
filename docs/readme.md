# UForm Kit Overview
UForm Kit is a package for Umbraco which enables you to create and manage multiple contact forms on your site,  and customize the form and the mail contents flexibly with simple markup.

UForm Kit is heavily inspired and influenced by the ContactForm7 plugin for WordPress - https://wordpress.org/plugins/contact-form-7/ and we feel that the Umbraco community would benefit from similar solution.
# System requirements
* Umbraco 10.0+, 
* Microsoft SQL Server database
# Installation
## Command line
UForm Kit can be installed using the NuGet Package Manager, by running the following command at the command line prompt in your web project folder.
```
dotnet add package UFormKit
```
## Visual Studio
In Visual Studio, you can use the NuGet Package Manager GUI from the Tools menu, by selecting Tools > NuGet Package Manager > Manage NuGet Packages for Solution.
Alternatively, you can run the following command from the NuGet Package Manager Console:
```
Install-Package UFormKit
```
# Upgrading
## Command line
UForm Kit can be upgraded to the latest version by running the following command at the command line prompt from your web project folder.
```
dotnet add package UFormKit
```
## Visual Studio
In Visual Studio, you can use the NuGet Package Manager GUI from the Tools menu, by selecting Tools > NuGet Package Manager > Manage NuGet Packages for Solution.
# Telemetry statistics
Since version 1.0.2, our UForm Kit package has been collecting telemetry data.
This provides us with insights to which Umbraco and package versions being used, so that we can make informed decisions on how to focus our future development efforts. The data is sent anonymously, no personal or sensitive data is collected.
## What type of data is being captured?
An example of the data captured is as follows.
```
{
    "umbraco_id": "0403E47E-EFE7-4CF2-8E97-148681DAFC10",
    "umbraco_version": "13.0.0",
    "package_id": "UForm Kit",
    "package_version": "1.0.2"
}
```
## How to disable telemetry?
If you would prefer to opt-out and disable the telemetry feature, add this option in your appsettings.json file:
```
{
  "UFormKit": {
    "DisableTelemetry": true
  }
}
```
# Documentation and Related links
* UForm Kit Getting Started Guide: [Getting Started](https://hexxu-services-ltd.gitbook.io/uform-kit-documentation/)
* UForm Kit Technical Documentation: [Technical Documentation](https://hexxu-services-ltd.gitbook.io/uform-kit-documentation/technical-documentation)