// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Tye.Resources.Containers;

namespace Microsoft.Tye.Hosting
{
    public class DefaultRuntime : Runtime, IHostedService
    {
        private readonly ILogger logger;
        private readonly EventBus eventBus;
        private readonly object @lock;
        private bool stopping;
        private bool stopped;

        private TaskCompletionSource<object?> start;
        private readonly ConcurrentDictionary<string, ContainerMonitor> monitors;

        public DefaultRuntime(ILogger<Runtime> logger, EventBus eventBus)
        {
            this.logger = logger;
            this.eventBus = eventBus;

            this.@lock = new object();
            this.start = new TaskCompletionSource<object?>();
            this.monitors = new ConcurrentDictionary<string, ContainerMonitor>();
        }

        public override async Task<ContainerStatus> PutAsync(Container container, CancellationToken cancellationToken)
        {
            await AwaitWithCancellation(start.Task, cancellationToken);
            lock (this.@lock)
            {
                if (this.stopping)
                {
                    throw new OperationCanceledException("The runtime is shutting down.");
                }
            }

            var monitor = this.monitors.GetOrAdd(container.NormalizedId, id => new ContainerMonitor(this.logger, this.eventBus, id));
            return await monitor.PutAsync(container, cancellationToken);
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            await AwaitWithCancellation(start.Task, cancellationToken);
            lock (this.@lock)
            {
                if (this.stopping)
                {
                    throw new OperationCanceledException("The runtime is shutting down.");
                }
            }

            if (this.monitors.TryRemove(id, out var monitor))
            {
                await monitor.StopAsync(cancellationToken);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting runtime");
            this.start.SetResult(null);
            this.logger.LogInformation("Started runtime");

            return this.start.Task;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping runtime");

            lock (this.@lock)
            {
                this.stopping = true;
            }

            foreach (var kvp in this.monitors)
            {
                try
                {
                    await kvp.Value.StopAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to stop container with Id {ResourceId}", kvp.Key);
                }
            }

            lock (this.@lock)
            {
                this.stopped = true;
            }

            this.logger.LogInformation("Stopped runtime");
        }

        private async Task AwaitWithCancellation(Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object?>();
            using var t = cancellationToken.UnsafeRegister(t => { ((TaskCompletionSource<object?>)t!).SetResult(null); }, tcs);

            var result = await Task.WhenAny(task, tcs.Task);
            if (result == tcs.Task && cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }


        }
    }
}
