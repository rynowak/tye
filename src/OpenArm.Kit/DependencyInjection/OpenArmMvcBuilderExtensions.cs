using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenArm;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenArmMvcBuilderExtensions
    {
        public static IMvcBuilder AddResourceIdModelBinder(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new ResourceIdModelBinderProvider());
            });
        }

        public static IMvcBuilder AddResourceTypeModelBinder(this IMvcBuilder builder)
        {
            return builder.DecorateBodyModelBinder(inner => new ResourceTypeModelBinderProvider(inner));
        }

        public static IMvcBuilder DecorateBodyModelBinder(this IMvcBuilder builder, Func<BodyModelBinderProvider, IModelBinderProvider> factory)
        {
            return builder.AddMvcOptions(options =>
            {
                var provider = options.ModelBinderProviders.OfType<BodyModelBinderProvider>().First();
                var index = options.ModelBinderProviders.IndexOf(provider);
                var decorated = factory(provider);
                options.ModelBinderProviders.Insert(index, decorated);
            });
        }
    }
}