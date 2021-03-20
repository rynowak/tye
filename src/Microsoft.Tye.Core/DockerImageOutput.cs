// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Tye
{
    public class DockerImageOutput : ServiceOutput
    {
        public DockerImageOutput(string imageName, string imageTag, string imageDigest)
        {
            if (imageName is null)
            {
                throw new ArgumentNullException(nameof(imageName));
            }

            if (imageTag is null)
            {
                throw new ArgumentNullException(nameof(imageTag));
            }

            if (imageDigest is null)
            {
                throw new ArgumentNullException(nameof(imageDigest));
            }

            ImageName = imageName;
            ImageTag = imageTag;
            ImageDigest = imageDigest;
        }

        public string ImageName { get; }

        public string ImageTag { get; }

        public string ImageDigest { get; }
    }
}
