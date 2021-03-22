// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Tye.Hosting;
using OpenArm;
using OpenArm.Repositories;

namespace Microsoft.Tye.Resources.Containers
{
    [ApiController]
    [ResourceRoute("Radius.Tye", "containers")]
    public class ContainerController : ControllerBase
    {
        private readonly Runtime runtime;
        private readonly IResourceRepository repository;

        public ContainerController(IResourceRepository repository, Runtime runtime)
        {
            this.repository = repository;
            this.runtime = runtime;
        }

        [HttpGet("")]
        public async Task<ActionResult<ListResponse<Container>>> List([FromRoute] string subscriptionId, [FromRoute] string resourceGroup)
        {
            var values = await this.repository.List<Container>(subscriptionId, resourceGroup);
            return new ListResponse<Container>(){ Value = values.ToImmutableArray()};
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Container>> Put([FromBody] Container container)
        {
            var status = await this.runtime.PutAsync(container);
            await this.repository.Upsert<Container>(container);
            return container;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Container>> Get([FromRoute] ResourceGroupLevelResourceId id)
        {
            var value = await this.repository.Get<Container>(id);
            if (value == null)
            {
                return NotFound();
            }

            return value;
        }

        [HttpDelete("{id}")]
        public async Task Delete([FromRoute] ResourceGroupLevelResourceId id)
        {
            await this.runtime.DeleteAsync(id.FullyQualifiedId.ToLowerInvariant());
            await this.repository.Delete(id);
        }
    }
}
