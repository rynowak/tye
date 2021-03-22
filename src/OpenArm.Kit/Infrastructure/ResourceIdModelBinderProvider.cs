using System.Threading.Tasks;
using Azure.Deployments.Core.Definitions.Identifiers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OpenArm
{
    internal class ResourceIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ResourceId))
            {
                return new ResourceIdBinder();
            }
            else if (context.Metadata.ModelType == typeof(TenantLevelResourceId))
            {
                return new TenantLevelResourceIdBinder();
            }
            else if (context.Metadata.ModelType == typeof(SubscriptionLevelResourceId))
            {
                return new SubscriptionLevelResourceIdBinder();
            }
            else if (context.Metadata.ModelType == typeof(ResourceGroupLevelResourceId))
            {
                return new ResourceGroupLevelResourceIdBinder();
            }

            return null;
        }

        private class ResourceIdBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (ResourceId.TryParse(bindingContext.HttpContext.Request.Path, out var parsed))
                {
                    bindingContext.Result = ModelBindingResult.Success(parsed);
                    return Task.CompletedTask;
                }

                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "invalid resource id");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
        }

        private class TenantLevelResourceIdBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (TenantLevelResourceId.TryParse(bindingContext.HttpContext.Request.Path, out var parsed))
                {
                    bindingContext.Result = ModelBindingResult.Success(parsed);
                    return Task.CompletedTask;
                }

                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "invalid resource id");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
        }

        private class SubscriptionLevelResourceIdBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (SubscriptionLevelResourceId.TryParse(bindingContext.HttpContext.Request.Path, out var parsed))
                {
                    bindingContext.Result = ModelBindingResult.Success(parsed);
                    return Task.CompletedTask;
                }

                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "invalid resource id");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
        }

        private class ResourceGroupLevelResourceIdBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (ResourceGroupLevelResourceId.TryParse(bindingContext.HttpContext.Request.Path, out var parsed))
                {
                    bindingContext.Result = ModelBindingResult.Success(parsed);
                    return Task.CompletedTask;
                }

                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "invalid resource id");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
        }
    }
}