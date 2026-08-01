using System;
using System.IO;
using Xunit;

namespace OrchardCoreContrib.PoExtractor.Tests;

public class ConcatStringAdditionalTests
{
    [Fact]
    public void Detects_ChainedConcatenation()
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

        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@T["a" + "b" + "c"]
""");

        var potFileName = "chained.pot";

        try
        {
            Program.Main(new[] { input, output, "--single", potFileName });

            var potPath = Path.Combine(output, potFileName);
            Assert.True(File.Exists(potPath));

            var pot = File.ReadAllText(potPath);
            Assert.Contains("abc", pot);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Detects_ParenthesizedConcatenation()
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

        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@T[("Hello " + "from") + " Razor"]
""");

        var potFileName = "parenthesized.pot";

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

    [Fact]
    public void Ignores_NonConstantConcatenation()
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

        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@{
    var part = "World";
}
@T["Hello " + part]
""");

        var potFileName = "nonconstant.pot";

        try
        {
            Program.Main(new[] { input, output, "--single", potFileName });

            var potPath = Path.Combine(output, potFileName);
            Assert.True(File.Exists(potPath));

            var pot = File.ReadAllText(potPath);
            // The extractor should not evaluate concatenation involving variables, so "Hello World" should NOT be present
            Assert.DoesNotContain("Hello World", pot);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Accepts_InterpolatedString_WithNoInterpolations()
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

        // Interpolated string with no interpolation slots
        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@T[$"Hello from Razor"]
""");

        var potFileName = "interpolated_plain.pot";

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

    [Fact]
    public void Ignores_InterpolatedString_WithInterpolations()
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

        File.WriteAllText(Path.Combine(project, "Index.cshtml"), """
@{
    var name = "Razor";
}
@T[$"Hello {name}"]
""");

        var potFileName = "interpolated_with_var.pot";

        try
        {
            Program.Main(new[] { input, output, "--single", potFileName });

            var potPath = Path.Combine(output, potFileName);
            Assert.True(File.Exists(potPath));

            var pot = File.ReadAllText(potPath);
            // Should not contain the evaluated value
            Assert.DoesNotContain("Hello Razor", pot);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
