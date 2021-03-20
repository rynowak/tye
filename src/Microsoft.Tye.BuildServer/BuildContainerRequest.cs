// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Tye.BuildServer
{
    public class BuildContainerRequest
    {
        public string Registry { get; set; } = default!;

        public string ProjectFilePath { get; set; } = default!;
    }
}
