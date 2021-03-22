// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Tye.Hosting;

namespace Microsoft.Tye.Dashboard.Models
{
    public class RuntimeEventSink : EventSink
    {
        public event EventHandler<RuntimeModel>? Changed;

        public RuntimeEventSink()
        {
            this.Current = new RuntimeModel(ImmutableArray<ContainerModel>.Empty);
        }

        public RuntimeModel Current { get; private set; }

        public override Task OnEventAsync(HostingEvent @event)
        {
            var current = this.Current;

            current = @event switch
            {
                ContainerEvent containerEvent => ProcessContainerEvent(containerEvent, current),
                _ => current,
            };

            Update(current);
            return Task.CompletedTask;
        }

        private static RuntimeModel ProcessContainerEvent(ContainerEvent @event, RuntimeModel current)
        {
            for (var i = 0; i < current.Containers.Length; i++)
            {
                var container = current.Containers[i];
                if (string.Equals(container.Id, @event.Id.FullyQualifiedId.ToLowerInvariant(), StringComparison.Ordinal))
                {
                    if (@event.Kind == ContainerEventKind.Removed)
                    {
                        return new RuntimeModel(current.Containers.RemoveAt(i));
                    }
                    else
                    {
                        // treat add and update the same in case of double-delivery
                        container = new ContainerModel(@event.Id.FullyQualifiedId.ToLowerInvariant(), @event.Id.FormatName(), @event.Resource!);
                        return new RuntimeModel(current.Containers.SetItem(i, container));
                    }
                }
            }

            if (@event.Kind == ContainerEventKind.Added)
            {
                var container = new ContainerModel(@event.Id.FullyQualifiedId.ToLowerInvariant(), @event.Id.FormatName(), @event.Resource!);
                return new RuntimeModel(current.Containers.Add(container).Sort((x, y) => x.Name.CompareTo(y.Name)));
            }

            // can't understand :(
            return current;
        }

        private void Update(RuntimeModel current)
        {
            if (object.ReferenceEquals(this.Current, current))
            {
                return;
            }
            
            this.Current = current;
            if (this.Changed is object)
            {
                this.Changed.Invoke(this, current);
            }
        }
    }
}
