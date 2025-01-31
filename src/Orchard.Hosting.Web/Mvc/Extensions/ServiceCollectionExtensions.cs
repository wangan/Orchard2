using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.TagHelpers;
using Orchard.Hosting.Mvc.Filters;
using Orchard.Hosting.Mvc.Razor;
using Orchard.Hosting.Web.Mvc.ModelBinding;

namespace Orchard.Hosting.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrchardMvc(this IServiceCollection services)
        {
            services
                .AddMvcCore(options => options.Filters.Add(new ModelBinderAccessorFilter()))
                .AddViews()
                .AddRazorViewEngine();

            services.AddScoped<IModelUpdaterAccessor, LocalModelBinderAccessor>();
            services.AddTransient<IFilterProvider, DependencyFilterProvider>();
            services.AddTransient<IMvcRazorHost, TagHelperMvcRazorHost>();

            services.AddScoped<IAssemblyProvider, OrchardMvcAssemblyProvider>();

            services.AddSingleton<ICompilationService, DefaultRoslynCompilationService>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                var expander = new ModuleViewLocationExpander();
                options.ViewLocationExpanders.Add(expander);
            });
            return services;
        }
    }
}