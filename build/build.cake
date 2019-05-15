#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=NUnit.Extension.NUnitV2ResultWriter"
#addin "Cake.Git"
#addin "Cake.FileHelpers"
#addin "nuget:http://6pak.opten.ch/nuget/nuget-v2/?package=Opten.Cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

string feedUrl = "https://www.nuget.org/api/v2/package";
string version = null;

var dest = Directory("./artifacts");

// Cleanup

Task("Clean")
	.Does(() =>
{
	if (DirectoryExists(dest))
	{
		CleanDirectory(dest);
		DeleteDirectory(dest, recursive: true);
	}
});

// Versioning

Task("Version")
	.IsDependentOn("Clean") 
	.Does(() =>
{
	if (DirectoryExists(dest) == false)
	{
		CreateDirectory(dest);
	}

	version = GitDescribe("../", false, GitDescribeStrategy.Tags, 0);
	
	PatchAssemblyInfo("../src/Opten.Core/Properties/AssemblyInfo.cs", version);
	FileWriteText(dest + File("Opten.Core.variables.txt"), "version=" + version);
});

// Building

Task("Restore-NuGet-Packages")
	.IsDependentOn("Version") 
	.Does(() =>
{ 
	NuGetRestore("../Opten.Core.sln", new NuGetRestoreSettings {
		NoCache = true
	});
});

Task("Run-Unit-Tests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
{
	var results = dest + Directory("tests");

	if (DirectoryExists(results) == false)
	{
		CreateDirectory(results);
	}

	DotNetCoreTest("../tests/Opten.Core.Test/Opten.Core.Test.csproj", new DotNetCoreTestSettings
    {
        Configuration = configuration,
		ResultsDirectory = results
    });
});

Task("Pack")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
{
     DotNetCorePack("../src/Opten.Core/Opten.Core.csproj", new DotNetCorePackSettings
     {
         Configuration = configuration,
         OutputDirectory = dest
     });
});

// Deploying

Task("Deploy")
	.Does(() =>
{
	// This is from the Bamboo's Script Environment variables
	string packageId = "Opten.Core";

	// Get the Version from the .txt file
	string version = EnvironmentVariable("bamboo_inject_" + packageId.Replace(".", "_") + "_version");

	if (string.IsNullOrWhiteSpace(version))
	{
		throw new Exception("Version is missing for " + packageId + ".");
	}

	// Get the path to the package
	var package = File(packageId + "." + version + ".nupkg");
            
	// Push the package
	NuGetPush(package, new NuGetPushSettings {
		Source = feedUrl,
		ApiKey = EnvironmentVariable("NUGET_API_KEY")
	});

	// Notifications
	Slack(new SlackSettings {
		ProjectName = "Opten.Core"
	});
});

Task("Default")
	.IsDependentOn("Pack");

RunTarget(target);