using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Tye.Bicep
{
    internal class ArmTemplate
    {
        [JsonPropertyName("resources")]
        public ArmResource[] Resources { get; set; } = Array.Empty<ArmResource>();
    }

    internal class ArmResource
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("apiVersion")]
        public string? ApiVersion { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("properties")]
        public object? Properties { get; set; }

        [JsonPropertyName("dependsOn")]
        public string[] DependsOn { get; set; } = Array.Empty<string>();

        public bool IsApplication()
        {
            return Type == "Microsoft.CustomProviders/resourceProviders/Applications";
        }

        public bool IsComponent()
        {
            return Type == "Microsoft.CustomProviders/resourceProviders/Applications/Components";
        }

        public bool IsDeployment()
        {
            return Type == "Microsoft.CustomProviders/resourceProviders/Applications/Deployments";
        }

        public ApplicationResource? AsApplication()
        {
            return IsApplication() ? ApplicationResource.Create(this) : null;
        }

        public ComponentResource? AsComponent()
        {
            return IsComponent() ? ComponentResource.Create(this) : null;
        }

        public DeploymentResource? AsDeployment()
        {
            return IsDeployment() ? DeploymentResource.Create(this) : null;
        }
    }

    internal class ApplicationResource
    {
        public static ApplicationResource Create(ArmResource resource)
        {
            var json = JsonSerializer.Serialize(resource);
            return JsonSerializer.Deserialize<ApplicationResource>(json);
        }

        [JsonPropertyName("application")]
        public string? Application { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("properties")]
        public ApplicationProperties? Properties { get; set; }
    }

    internal class ApplicationProperties
    {
    }

    internal class ComponentResource
    {
        public static ComponentResource Create(ArmResource resource)
        {
            var json = JsonSerializer.Serialize(resource);
            return JsonSerializer.Deserialize<ComponentResource>(json);
        }

        [JsonPropertyName("application")]
        public string? Application { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("properties")]
        public ComponentProperties? Properties { get; set; }
    }

    internal class ComponentProperties
    {
        public static ComponentResource Create(object properties)
        {
            var json = JsonSerializer.Serialize(properties);
            return JsonSerializer.Deserialize<ComponentResource>(json);
        }

        [JsonPropertyName("build")]
        public BuildSection? Build { get; set; }

        [JsonPropertyName("config")]
        public ConfigSection? Config { get; set; }

        [JsonPropertyName("run")]
        public RunSection? Run { get; set; }

        [JsonPropertyName("dependsOn")]
        public DependsOnItem[] DependsOn { get; set; } = Array.Empty<DependsOnItem>();

        [JsonPropertyName("provides")]
        public ProvidesItem[] Provides { get; set; } = Array.Empty<ProvidesItem>();
    }

    internal class BuildSection
    {
        [JsonPropertyName("dotnet")]
        public BuildDotnetSection? Dotnet { get; set; }
    }

    public class BuildDotnetSection
    {
        [JsonPropertyName("project")]
        public string? Project { get; set; }
    }

    internal class ConfigSection
    {
        [JsonExtensionData]
        public Dictionary<string, object> Innards { get; set; } = new Dictionary<string, object>();
    }

    internal class RunSection
    {
        [JsonExtensionData]
        public Dictionary<string, object> Innards { get; set; } = new Dictionary<string, object>();
    }

    internal class DependsOnItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Innards { get; set; } = new Dictionary<string, object>();
    }

    internal class ProvidesItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Innards { get; set; } = new Dictionary<string, object>();
    }

    internal class DeploymentResource
    {
        public static DeploymentResource Create(ArmResource resource)
        {
             var json = JsonSerializer.Serialize(resource);
            return JsonSerializer.Deserialize<DeploymentResource>(json);
        }

        [JsonPropertyName("application")]
        public string? Application { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("properties")]
        public DeploymentProperties? Properties { get; set; }
    }

    internal class DeploymentProperties
    {
        [JsonPropertyName("components")]
        public DeploymentComponent[] Components { get; set; } = Array.Empty<DeploymentComponent>();
    }

    internal class DeploymentComponent
    {
        [JsonPropertyName("componentName")]
        public string? ComponentName { get; set; }
    }
}
