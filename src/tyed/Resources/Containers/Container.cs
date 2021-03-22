// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using OpenArm.Resources;

namespace Microsoft.Tye.Resources.Containers
{
    [ResourceType("Radius.Tye/containers")]
    public class Container : Resource
    {
        public ContainerProperties Properties { get; set; } = new ContainerProperties();
    }

    public class ContainerProperties : ResourceProperties
    {
        [Required]
        public string Image { get; set; } = default!;

        public string? Digest { get; set; }
    }
}
