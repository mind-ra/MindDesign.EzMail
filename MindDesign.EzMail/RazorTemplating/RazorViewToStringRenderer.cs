﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MindDesign.EzMail.RazorTemplating
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>([DisallowNull] string viewName, [DisallowNull] TModel model);
        Task<string> RenderViewToStringAsync<TModel>([DisallowNull] string viewName, [DisallowNull] TModel model, [DisallowNull] ViewDataDictionary viewDataDictionary);
    }

    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor, LinkGenerator linkGenerator)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>([DisallowNull] string viewName, [DisallowNull] TModel model)
        {
            return await RenderViewToStringAsync(viewName, model, new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()));
        }

        public async Task<string> RenderViewToStringAsync<TModel>([DisallowNull] string viewName, [DisallowNull] TModel model, [DisallowNull] ViewDataDictionary viewDataDictionary)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(viewDataDictionary, model),
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new string[] {
                    $"Unable to find view '{viewName}'. The following locations were searched:"
                }.Concat(searchedLocations)
                .Concat(new string[]{
                "Hint:",
                "- Check whether you have added reference to the Razor Class Library that contains the view files.",
                "- Check whether the view file name is correct or exists at the given path."}));

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            var routeData = new RouteData();
            routeData.Routers.Add(new CustomRouter());
            return new ActionContext(httpContext, routeData, new ActionDescriptor());
        }
    }

    internal class CustomRouter : IRouter
    {
        public VirtualPathData? GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            return Task.CompletedTask;
        }
    }
}
