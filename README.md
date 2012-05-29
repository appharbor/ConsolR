# ConsolR

ConsolR enables you to execute C# code againt a running .NET 4.0 web application's app domain through a browser.
Once installed, ConsolR will automatically configure itself during application start and is accessible through the 
"/consolr" path. This enables an interactive console session against for instance an application's production environment.

It can be used for a number of things, but is mostly intended for debugging purposes. Possible use cases include:

* Reading and setting static variables.
* Access application configuration.
* Executing one-off code (sending e-mails, invalidating cache etc).
* Access databases, logging tools etc. that are only accessible from production instances.

While this tool is very powerful some caution should be excersised when using it. Your use of it is at your own risk.
Always remember that you're running the code inside of your application's appdomain.
This means that you can access and modify the application's state and run any code you want, 
which could render the application useless or harmful. Also make sure to always use HTTPS for transferring sensitive information.

ConsolR is based on the excellent [Compilify project](https://github.com/Compilify/Compilify) by Justin Rusbatch.

## Installing with NuGet

Open your package manager console and install the package:

    Install-Package ConsolR

## Running ConsolR in a website

Simply install the ConsolR NuGet package in your .NET 4.0 web application and access the ConsolR interface on "http://example.com/consolr".
The default username and password is "foo" and "bar" (make sure to change these before deploying to your production environment).

The NuGet package will add all dependencies along with required assets and configuration:

* Dependencies: Roslyn, SignalR.
* Assets: All assets files are located in the "/assets/consolr" directory of the project you install ConsolR in. You can modify these as you please although the "index.html" must remain in it's current location.
* Configuration: A number of appSettings that ConsolR relies on are added during installation.

## Configuring ConsolR

When the ConsolR nuget package is installed three application settings will be added to your web.config in the `<appSettings>` section:

* `consolr.executiontimeout`: The maximum amount of time (in seconds) before the connection and code execution will time out (default: 30).
* `consolr.rootPath`: The root path under which ConsolR will be accessible (default: "consolr")
* `consolr.username`: The username used for accessing ConsolR (default: "foo").
* `consolr.password`: The password used for accessing ConsolR (default: "bar").

Make sure to change the username and password before deploying the application to production servers.
We also recommend changing the root path so users won't be able to detect whether you're using ConsolR.

### AppHarbor

If you're running your ConsolR-enabled application on AppHarbor you can configure these setting with "Configuration Variables". This way you can avoid including
sensitive information in your configuration files, which in turn might be tracked in a version control system.

## Helpers

A couple of handy helper methods are readily available in the [ConsolR.Hosting.Helper class](https://github.com/appharbor/ConsolR/blob/master/Hosting/Helper.cs).
Currently you can call `ReadFile(string filename)` and `Log(string message)`. Feel free to add more as you see fit and send us a pull request so we can merge it in.

The Log method enables you to broadcast messages back to your browser window by using the SignalR connection that is established when executing code.
This can be useful if you want to follow the execution of your code and is also an example of using features avaiable in the AppDomain to create a more interactive experience. 

##License
[MIT License](https://github.com/appharbor/ConsolR/blob/master/LICENSE.md)
