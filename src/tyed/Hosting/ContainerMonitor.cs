// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using Microsoft.Extensions.Logging;
using Microsoft.Tye.Resources.Containers;

namespace Microsoft.Tye.Hosting
{
    public class ContainerMonitor
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private readonly ILogger logger;
        private readonly EventBus eventBus;
        private readonly string id;
        private readonly SemaphoreSlim @lock;

        private bool stopping;
        private ReadOnlyMemory<byte> snapshot;
        private ContainerStatusInternal? status;

        public ContainerMonitor(ILogger logger, EventBus eventBus, string id)
        {
            this.logger = logger;
            this.eventBus = eventBus;
            this.id = id;

            this.@lock = new SemaphoreSlim(1, 1);
        }

        public async Task<ContainerStatus> PutAsync(Container container, CancellationToken cancellationToken)
        {
            await this.@lock.WaitAsync(cancellationToken);
            if (this.stopping)
            {
                this.@lock.Release();
                throw new OperationCanceledException("the runtime is shutting down");
            }

            // TODO no rollback for now, just leave it in a failed state

            try
            {
                var oldSnapshot = this.snapshot;
                var snapshot = JsonSerializer.SerializeToUtf8Bytes(container, jsonSerializerOptions);
                if (MemoryExtensions.SequenceEqual(snapshot.AsSpan(), oldSnapshot.Span))
                {
                    return this.status!.ToStatus();
                }

                // stop current container
                var status = this.status;
                this.status = null;
                if (status is object)
                {
                    await StopContainerAsync(status);
                }

                // start new container
                status = await StartContainerAsync(container, cancellationToken);
                await this.eventBus.SendAsync(new ContainerEvent(ResourceGroupLevelResourceId.Parse(this.id), status.Resource, oldSnapshot.Length == 0 ? ContainerEventKind.Added : ContainerEventKind.Updated));
                this.status = status;
                this.snapshot = snapshot;

                return status.ToStatus();
            }
            finally
            {
                this.@lock.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.@lock.WaitAsync(cancellationToken);
            this.stopping = true;

            try
            {
                // stop current container
                var status = this.status;
                this.status = null;
                if (status is object)
                {
                    await StopContainerAsync(status);
                    await this.eventBus.SendAsync(new ContainerEvent(ResourceGroupLevelResourceId.Parse(this.id), status.Resource, ContainerEventKind.Removed));
                }
            }
            finally
            {
                this.@lock.Release();
            }
        }

        private async Task<ContainerStatusInternal> StartContainerAsync(Container container, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting container {ContainerName} with Image {ContainerImage}", container.Name, container.Properties.Image);

            var args = new List<string>()
            {
                "run",
                "-d",
                "--name", container.Name,
                "--restart=unless-stopped",
                container.Properties.Image,
            };

            var command = string.Join(" ", args);
            var result = await ProcessUtil.RunAsync(
                "docker",
                command,
                throwOnError: false,
                cancellationToken: cancellationToken);
            if (result.ExitCode != 0)
            {
                LogCommand("docker", command, result);
                throw new Exception("failed to launch container");
            }

            result = await ProcessUtil.RunAsync("docker", $"ps --no-trunc -f name={container.Name} --format " + "{{.ID}}");
            if (result.ExitCode != 0)
            {
                LogCommand("docker", command, result);
                throw new Exception("failed to launch container");
            }

            var containerId = (string)result.StandardOutput.Trim();
            containerId = containerId.Substring(0, Math.Min(12, containerId.Length));

            this.logger.LogInformation("Running container {ContainerName} with Image {ContainerImage} and Id {ContainerId}", container.Name, container.Properties.Image, containerId);
            return new ContainerStatusInternal()
            {
                ContainerName = container.Name,
                ContainerId = containerId,
                Resource = container,
            };
        }

        private async Task StopContainerAsync(ContainerStatusInternal status)
        {
            this.logger.LogInformation("Stopping container {ContainerName} with ID {ContainerId}", status.ContainerName, status.ContainerId);

            // Docker has a tendency to get stuck so we're going to timeout this shutdown process
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await ProcessUtil.RunAsync("docker", $"stop {status.ContainerId}", throwOnError: false, cancellationToken: cts.Token);
            if (cts.IsCancellationRequested)
            {
                logger.LogWarning($"Failed to stop container due to timeout, container will most likely be running.");
            }

            LogCommand("docker", $"stop {status.ContainerId}", result);

            result = await ProcessUtil.RunAsync("docker", $"rm {status.ContainerId}", throwOnError: false, cancellationToken: cts.Token);
            if (cts.IsCancellationRequested)
            {
                logger.LogWarning($"Failed to remove container due to timeout, container will most likely still exist.");
            }

            LogCommand("docker", $"rm {status.ContainerId}", result);
        }

        private void LogCommand(string process, string args, ProcessResult result)
        {
            logger.LogInformation("Process {CommandLine} completed with exit code: {ExitCode}", process + " " + args, result.ExitCode);
            logger.LogInformation("Process {Process} standard out {StdOut}", process, result.StandardOutput);
            logger.LogInformation("Process {Process} standard error {StdErr}", process, result.StandardError);
        }

        private class ContainerStatusInternal
        {
            public string ContainerName { get; set; } = default!;

            public string ContainerId { get; set; } = default!;

            public Container Resource { get; set; } = default!;

            public ContainerStatus ToStatus()
            {
                return new ContainerStatus();
            }
        }
    }
}
