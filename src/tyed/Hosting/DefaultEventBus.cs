// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Tye.Hosting
{
    public class DefaultEventBus : EventBus, IHostedService
    {
        private readonly ILogger logger;
        private readonly Channel<HostingEvent> channel;
        private readonly EventSink[] sinks;

        public DefaultEventBus(ILogger<DefaultEventBus> logger, IEnumerable<EventSink> sinks)
        {
            this.logger = logger;
            this.sinks = sinks.ToArray();

            this.channel = Channel.CreateBounded<HostingEvent>(new BoundedChannelOptions(capacity: 1000));
        }

        public override async Task SendAsync(HostingEvent @event)
        {
            try
            {
                await this.channel.Writer.WriteAsync(@event);
            }
            catch (ChannelClosedException)
            {
                // ignore these - we don't want crash during shutdown.
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => { _ = ConsumeEvents(); });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.channel.Writer.Complete();
            return Task.CompletedTask;
        }

        private async Task ConsumeEvents()
        {
            await foreach (var @event in this.channel.Reader.ReadAllAsync())
            {
                for (var i = 0; i < this.sinks.Length; i++)
                {
                    var sink = sinks[i];
                    try
                    {
                        await sink.OnEventAsync(@event);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Exception writing to sink {Sink}", sink);
                    }
                }
            }
        }
    }
}
