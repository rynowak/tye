using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenArm.Resources;

namespace OpenArm
{
    // Decorates the inner model binder provider to set Resource-related properties.
    internal class ResourceTypeModelBinderProvider : IModelBinderProvider
    {
        private readonly BodyModelBinderProvider inner;
        public ResourceTypeModelBinderProvider(BodyModelBinderProvider inner)
        {
            this.inner = inner;
        }
        
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (typeof(Resource).IsAssignableFrom(context.Metadata.ModelType))
            {
                var inner = this.inner.GetBinder(context);
                if (inner == null)
                {
                    return inner;
                }

                return new ResourceTypeModelBinder(inner);
            }

            return default;
        }

        private class ResourceTypeModelBinder : IModelBinder
        {
            private readonly IModelBinder inner;

            public ResourceTypeModelBinder(IModelBinder inner)
            {
                this.inner = inner;
            }
            
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                await this.inner.BindModelAsync(bindingContext);
                if (!bindingContext.Result.IsModelSet )
                {
                    return;
                }

                if (ResourceGroupLevelResourceId.TryParse(bindingContext.HttpContext.Request.Path, out var parsed))
                {
                    var resource = (Resource)bindingContext.Result.Model!;
                    resource.Id = parsed.FullyQualifiedId;
                    resource.NormalizedId = resource.Id.ToLowerInvariant();
                    resource.SubscriptionId = parsed.SubscriptionId;
                    resource.ResourceGroup = parsed.ResourceGroup;
                    resource.Type = parsed.FormatFullyQualifiedType();
                    resource.Name = parsed.FormatName();
                    return;
                }

                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "invalid resource id");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
