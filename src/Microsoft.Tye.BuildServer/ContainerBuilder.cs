// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Tye.BuildServer
{
    public class ContainerBuilder
    {
        private readonly ILogger logger;

        public ContainerBuilder(ILogger<ContainerBuilder> logger)
        {
            this.logger = logger;
        }

        public async Task<BuildContainerResponse> BuildAsync(BuildContainerRequest request)
        {
            var console = new LoggerConsole(this.logger);
            var output = new OutputContext(console, Verbosity.Debug);

            var projectFile = new FileInfo(request.ProjectFilePath);
            if (!projectFile.Exists)
            {
                throw new CommandException("project file does not exist");
            }
            
            var app = await ApplicationFactory.CreateAsync(output, projectFile);
            app.Registry ??= new ContainerRegistry(request.Registry);

            var service = app.Services.OfType<ProjectServiceBuilder>().SingleOrDefault();
            if (service is null)
            {
                throw new CommandException("Cannot find service.");
            }

            var executor = new ApplicationExecutor(output)
            {
                ServiceSteps =
                {
                    new ApplyContainerDefaultsStep(),
                    new PublishProjectStep(),
                    new BuildDockerImageStep(),
                }
            };

            await executor.ExecuteAsync(app);

            var image = service.Outputs.OfType<DockerImageOutput>().SingleOrDefault();
            if (image is null)
            {
                throw new CommandException("No image was produced.");
            }
            
            return new BuildContainerResponse()
            {
                ProjectFilePath = request.ProjectFilePath,
                Output = console.Captured.ToString(),
                Image = image.ImageName,
                Tag = image.ImageTag,
                Digest = image.ImageDigest,
            };
        }
    }
}
