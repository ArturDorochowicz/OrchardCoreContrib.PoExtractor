using System;
using System.IO;
using Xunit;

namespace OrchardCoreContrib.PoExtractor.Tests;

public class ConcatStringTests
{
    [Fact]
    public void Main_NoTemplateOption_DetectsConcatenatedRazorString()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), "PoExtractorTests", Guid.NewGuid().ToString("N"));
        var input = Path.Combine(root, "input");
        var output = Path.Combine(root, "output");
        var project = Path.Combine(input, "TestModule");

        Directory.CreateDirectory(project);
        Directory.CreateDirectory(output);

        File.WriteAllText(Path.Combine(project, "TestModule.csproj"), """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
""");

        // This uses concatenation inside the T[...] call — currently not detected.
        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@T["Hello " + "from Razor"]
""");

        var potFileName = "concat.pot";

        try
        {
            // Act
            Program.Main(
            [
                input,
                output,
                "--single", potFileName
            ]);

            // Assert
            var potPath = Path.Combine(output, potFileName);
            Assert.True(File.Exists(potPath));

            var pot = File.ReadAllText(potPath);

            // We expect the extractor to pick up the concatenated literal as "Hello from Razor"
            Assert.Contains("Hello from Razor", pot);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
