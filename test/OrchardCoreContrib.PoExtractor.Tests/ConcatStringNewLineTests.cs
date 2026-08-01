using System;
using System.IO;
using Xunit;

namespace OrchardCoreContrib.PoExtractor.Tests;

public class ConcatStringNewLineTests
{
    [Fact]
    public void Detects_Concatenation_WithNewLineBetweenParts()
    {
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

        // Concatenation with a newline and whitespace between the operands
        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@T["Hello "
    + "from Razor"]
""");

        var potFileName = "newline_concat.pot";

        try
        {
            Program.Main(new[] { input, output, "--single", potFileName });

            var potPath = Path.Combine(output, potFileName);
            Assert.True(File.Exists(potPath));

            var pot = File.ReadAllText(potPath);
            Assert.Contains("Hello from Razor", pot);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
