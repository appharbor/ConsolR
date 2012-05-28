# ConsolR

ConsolR enables you to execute C# code againt a running .NET 4.0 web application's app domain through the browser.
Once installed ConsolR will automatically configure itself during application start and is accessible through the 
"/consolr" path. This enables an interactive console session against for instance your production environment.

It can be used for a number of things, but is mostly intended for debugging purposes. Possible use cases include:

* Reading and setting static variables.
* Access application configuration.
* Executing one-off code (sending e-mails, invalidating cache etc).
* Access databases, logging tools etc. that are only accessible from production instances.

While this tool is very powerful some caution should be excersised when using it.
Always remember that you're running the code inside of your application's appdomain.
This means that you can access and modify the application's state and run any code you want, 
which could render the application useless or harmful.

Future versions may allow for the execution of code inside a 
"cloned" application domain that reduce the risks associated with 

ConsolR is based on the excellent [Compilify project](https://github.com/Compilify/Compilify) by Justin Rusbatch.

## Installing with NuGet

Open your package manager console and install the package:

    Install-Package ConsolR

## Running ConsolR in a website

Simply install the ConsolR NuGet package in your .NET 4.0 web application and access the ConsolR interface on "http://example.com/consolr".
The default username and password is "foo" and "bar" (make sure to change these before deploying to your production environment).

The NuGet package will add all dependencies along with required assets and configuration:

* Dependencies: Roslyn, Nancy, SignalR.AspNet.Hosting, ConsolR.Core, ConsolR.Hosting
* Assets: All assets files are located in the "/assets/consolr" directory of your web application. You can modify these as you like although the "index.html" file is required.
* Configuration: A number of appSettings that ConsolR relies on are added during installation.

## Configuring ConsolR

When the ConsolR nuget package is installed three application settings will be added to your web.config in the `<appSettings>` section:

* `consolr.executiontimeout`: The maximum amount of time (in seconds) before the connection and code execution will time out.
* `consolr.username`: The username used for accessing ConsolR.
* `consolr.password`: The password used for accessing ConsolR.

Make sure to change these username and password before deploying the application to production servers.
