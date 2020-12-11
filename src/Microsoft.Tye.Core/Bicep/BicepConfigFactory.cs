// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bicep.Core.Diagnostics;
using Bicep.Core.Emit;
using Bicep.Core.Extensions;
using Bicep.Core.FileSystem;
using Bicep.Core.SemanticModel;
using Bicep.Core.Syntax;
using Bicep.Core.Text;
using Bicep.Core.TypeSystem.Az;
using Bicep.Core.Workspaces;
using Microsoft.Tye.ConfigModel;
using Microsoft.Tye.Core;
using Microsoft.Tye.Serialization;
using Tye.Serialization;
using YamlDotNet.Core;

namespace Microsoft.Tye.Bicep
{
    public static class BicepConfigFactory
    {
        public static ConfigApplication FromFile(FileInfo file)
        {
            var extension = file.Extension.ToLowerInvariant();
            switch (extension)
            {
                case ".yaml":
                case ".yml":
                    return FromYaml(file);

                case ".bicep":
                    return FromBicep(file);

                case ".csproj":
                case ".fsproj":
                    return FromProject(file);

                case ".sln":
                    return FromSolution(file);

                default:
                    throw new CommandException($"File '{file.FullName}' is not a supported format.");
            }
        }

        private static ConfigApplication FromProject(FileInfo file)
        {
            var application = new ConfigApplication()
            {
                Source = file,
                Name = NameInferer.InferApplicationName(file)
            };

            var service = new ConfigService()
            {
                Name = NormalizeServiceName(Path.GetFileNameWithoutExtension(file.Name)),
                Project = file.FullName.Replace('\\', '/'),
            };

            application.Services.Add(service);

            return application;
        }

        private static ConfigApplication FromSolution(FileInfo file)
        {
            var application = new ConfigApplication()
            {
                Source = file,
                Name = NameInferer.InferApplicationName(file)
            };

            // BE CAREFUL modifying this code. Avoid proliferating MSBuild types
            // throughout the code, because we load them dynamically.
            foreach (var projectFile in ProjectReader.EnumerateProjects(file))
            {
                // Check for the existance of a launchSettings.json as an indication that the project is
                // runnable. This will only apply in the case where tye is being used against a solution
                // like `tye init` or `tye run` without a `tye.yaml`.
                //
                // We want a *fast* heuristic that excludes unit test projects and class libraries without
                // having to load all of the projects. 
                var launchSettings = Path.Combine(projectFile.DirectoryName!, "Properties", "launchSettings.json");
                if (File.Exists(launchSettings) || ContainsOutputTypeExe(projectFile))
                {
                    var service = new ConfigService()
                    {
                        Name = NormalizeServiceName(Path.GetFileNameWithoutExtension(projectFile.Name)),
                        Project = projectFile.FullName.Replace('\\', '/'),
                    };

                    application.Services.Add(service);
                }
            }

            return application;
        }

        private static bool ContainsOutputTypeExe(FileInfo projectFile)
        {
            // Note, this will not work if OutputType is on separate lines.
            // TODO consider a more thorough check with xml reading, but at that point, it may be better just to read the project itself.
            var content = File.ReadAllText(projectFile.FullName);
            return content.Contains("<OutputType>exe</OutputType>", StringComparison.OrdinalIgnoreCase);
        }

        private static ConfigApplication FromYaml(FileInfo file)
        {
            using var parser = new YamlParser(file);
            return parser.ParseConfigApplication();
        }

        private static ConfigApplication FromBicep(FileInfo file)
        {
            var syntaxTreeGrouping = SyntaxTreeGroupingBuilder.Build(new FileResolver(), new Workspace(), PathHelper.FilePathToFileUrl(file.FullName));
            var compilation = new Compilation(new AzResourceTypeProvider(), syntaxTreeGrouping);
            compilation.GetEntrypointSemanticModel();

            static (bool, List<string>) GetDiagnosticsAndCheckSuccess(Compilation compilation)
            {
                var errors = new List<string>();
                var success = true;
                foreach (var (syntaxTree, diagnostics) in compilation.GetAllDiagnosticsBySyntaxTree())
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        var (line, character) = TextCoordinateConverter.GetPosition(syntaxTree.LineStarts, diagnostic.Span.Position);
                        var message = $"{syntaxTree.FileUri.LocalPath}({line + 1},{character + 1}) : {diagnostic.Level} {diagnostic.Code}: {diagnostic.Message}";
                        errors.Add(message);
                        success &= diagnostic.Level != DiagnosticLevel.Error;
                    }
                }

                return (success, errors);
            }

            var (success, errors) = GetDiagnosticsAndCheckSuccess(compilation);
            if (!success)
            {
                throw new TyeYamlException(string.Join(Environment.NewLine, errors));
            }

            var emitter = new TemplateEmitter(compilation.GetEntrypointSemanticModel());

            using var outputStream = new MemoryStream();
            emitter.Emit(outputStream);

            outputStream.Seek(0L, SeekOrigin.Begin);

            var reader = new StreamReader(outputStream, Encoding.UTF8);
            var text = reader.ReadToEnd();
            var template = JsonSerializer.Deserialize<ArmTemplate>(text);

            var applications = new List<ApplicationResource>();
            var components = new Dictionary<string, List<ComponentResource>>();
            var deployments = new Dictionary<string, List<DeploymentResource>>();

            foreach (var resource in template.Resources)
            {
                if (resource.AsApplication() is ApplicationResource application)
                {
                    var name = ArmExpressionParser.Parse(resource.Name!).Eval();
                    var parts = name.Split("/");
                    resource.Name = application.Name = parts[1];
                    applications.Add(application);
                }
                else if (resource.AsComponent() is ComponentResource component)
                {
                    var name = ArmExpressionParser.Parse(resource.Name!).Eval();
                    var parts = name.Split("/");
                    resource.Name = component.Name = parts[2];

                    if (!components.TryGetValue(parts[1], out var items))
                    {
                        items = new List<ComponentResource>();
                        components.Add(parts[1], items);
                    }

                    items.Add(component);
                }
                else if (resource.AsDeployment() is DeploymentResource deployment)
                {
                    var name = ArmExpressionParser.Parse(resource.Name!).Eval();
                    var parts = name.Split("/");
                    resource.Name = deployment.Name = parts[2];

                    if (!deployments.TryGetValue(parts[1], out var items))
                    {
                        items = new List<DeploymentResource>();
                        deployments.Add(parts[1], items);
                    }

                    items.Add(deployment);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported resource type: " + resource.Type);
                }
            }

            if (deployments.SelectMany(m => m.Value).Count() != 1)
            {
                throw new InvalidOperationException("Specify exactly one deployment");
            }

            var applicationName = deployments.Single().Key;
            var dep = deployments.Single().Value.Single();

            var services = new List<ConfigService>();
            foreach (var dc in dep.Properties?.Components ?? Array.Empty<DeploymentComponent>())
            {
                var name = ArmExpressionParser.Parse(dc.ComponentName!).Eval();
                var parts = name.Split("/");
                dc.ComponentName = parts.Length == 1 ? parts[0] : parts[2];
                
                var component = components[applicationName].Where(c => c.Name == dc.ComponentName).Single();
                var service = new ConfigService()
                {
                    Name = dc.ComponentName,
                    Project = component.Properties?.Build?.Dotnet?.Project,
                };

                services.Add(service);
            }

            return new ConfigApplication()
            {
                Source = file,
                Name = applicationName,
                Services = services,
            };
        }

        private static string NormalizeServiceName(string name)
            => Regex.Replace(name.ToLowerInvariant(), "[^0-9A-Za-z-]+", "-");
    }
}
