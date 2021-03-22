// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Tye.Resources.Containers;

namespace Microsoft.Tye.Dashboard.Models
{
    public class ContainerModel
    {
        public ContainerModel(string id, string name, Container resource)
        {
            this.Id = id;
            this.Name = name;
            this.Resource = resource;
        }

        public string Id { get; }

        public string Name { get; }

        public Container Resource { get; }
    }
}
