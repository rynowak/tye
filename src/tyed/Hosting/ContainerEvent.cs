// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Azure.Deployments.Core.Definitions.Identifiers;
using Microsoft.Tye.Resources.Containers;

namespace Microsoft.Tye.Hosting
{
    public sealed class ContainerEvent : HostingEvent
    {
        public ContainerEvent(ResourceGroupLevelResourceId id, Container? resource, ContainerEventKind kind)
        {
            this.Id = id;
            this.Kind = kind;
            this.Resource = resource;
        }

        public ResourceGroupLevelResourceId Id { get; }

        public ContainerEventKind Kind { get; }

        public Container? Resource { get; }
    }

    public enum ContainerEventKind
    {
        Added,
        Updated,
        Removed,
    }
}
