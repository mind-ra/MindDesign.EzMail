using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MindDesign.EzMail.RazorTemplating;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

namespace MindDesign.EzMail.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEzMailServices(this IServiceCollection services)
        {
            //ref: https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file#api-incompatibility
            var assembliesBaseDirectory = AppContext.BaseDirectory;

            //in .net 5, RCL assemblies are located next the main executable even if /p:IncludeAllContentForSelfExtract=true is provided while publishing
            //also when .net core 3.1 project is published using .net 5 sdk, above scenario happens
            //so, additionally look for RCL assemblies at the main executable directory as well
            var mainExecutableDirectory = GetMainExecutableDirectory();

            //To add support for MVC application
            var webRootDirectory = GetWebRootDirectory(assembliesBaseDirectory);

            var fileProvider = new PhysicalFileProvider(assembliesBaseDirectory);

            services.TryAddSingleton<IWebHostEnvironment>(new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? Constants.LibraryIdentifier,
                ContentRootPath = assembliesBaseDirectory,
                ContentRootFileProvider = fileProvider,
                WebRootPath = webRootDirectory,
                WebRootFileProvider = new PhysicalFileProvider(webRootDirectory)
            });
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<DiagnosticSource>(new DiagnosticListener(Constants.LibraryIdentifier));
            services.TryAddSingleton<DiagnosticListener>(new DiagnosticListener(Constants.LibraryIdentifier));
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddLogging();
            services.AddHttpContextAccessor();
            var builder = services.AddMvcCore().AddRazorViewEngine();

            var viewAssemblyFiles = GetRazorClassLibraryAssemblyFilesPath(assembliesBaseDirectory, mainExecutableDirectory);
            //ref: https://stackoverflow.com/questions/52041011/aspnet-core-2-1-correct-way-to-load-precompiled-views
            //load view assembly application parts to find the view from shared libraries
            builder.AddViewAssemblyApplicationParts(viewAssemblyFiles);

            services.Configure<MvcRazorRuntimeCompilationOptions>(o =>
            {
                o.FileProviders.Add(fileProvider);
            });

            services.TryAddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.TryAddScoped<IEzMailService, EzMailService>();
        }

        /// <summary>
        /// Returns the path of the main executable file using which the application is started
        /// </summary>
        /// <returns></returns>
        private static string? GetMainExecutableDirectory()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }

        /// <summary>
        /// Looks for Razor Class Library(RCL) assemblies at the given directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>Absolute paths of the RCL assemblies</returns>
        private static List<string> GetRazorClassLibraryAssemblyFilesPath(string directory)
        {
            return Directory.GetFiles(directory, "*.Views.dll").ToList();
        }

        /// <summary>
        /// Get the all the RCL assembly file paths from all possible locations
        /// </summary>
        /// <param name="assembliesBaseDirectory"></param>
        /// <param name="mainExecutableDirectory"></param>
        /// <returns></returns>
        private static List<string> GetRazorClassLibraryAssemblyFilesPath(string assembliesBaseDirectory, string? mainExecutableDirectory)
        {
            var viewAssemblyFiles = GetRazorClassLibraryAssemblyFilesPath(assembliesBaseDirectory);

            // if RCL assemblies are found at the main executable directory, add them as well.
            if (mainExecutableDirectory?.Length > 0 && Directory.Exists(mainExecutableDirectory) && !mainExecutableDirectory.Equals(assembliesBaseDirectory))
            {
                viewAssemblyFiles.AddRange(GetRazorClassLibraryAssemblyFilesPath(mainExecutableDirectory));
            }
            return viewAssemblyFiles.Distinct().ToList();
        }

        /// <summary>
        /// Get the web root directory where the static content resides. This is to add support for MVC applications
        /// If the webroot directory doesn't exist, set the path to assembly base directory.
        /// </summary>
        /// <param name="assembliesBaseDirectory"></param>
        /// <returns></returns>
        private static string GetWebRootDirectory(string assembliesBaseDirectory)
        {
            var webRootDirectory = Path.Combine(assembliesBaseDirectory, "wwwroot");
            if (!Directory.Exists(webRootDirectory))
            {
                webRootDirectory = assembliesBaseDirectory;
            }

            return webRootDirectory;
        }


        /// <summary>
        /// Loads the RCL assemblies to the application parts.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="viewAssemblyFiles"></param>
        private static void AddViewAssemblyApplicationParts(this IMvcCoreBuilder builder, List<string> viewAssemblyFiles)
        {
            foreach (var assemblyFile in viewAssemblyFiles)
            {
                var viewAssembly = Assembly.LoadFile(assemblyFile);

                builder.PartManager.ApplicationParts.Add(new CompiledRazorAssemblyPart(viewAssembly));
            }
        }


        internal class HostingEnvironment : IWebHostEnvironment
        {
            public HostingEnvironment()
            {
            }

            public string EnvironmentName { get; set; } = default!;
            public string ApplicationName { get; set; } = default!;
            public string WebRootPath { get; set; } = default!;
            public IFileProvider WebRootFileProvider { get; set; } = default!;
            public string ContentRootPath { get; set; } = default!;
            public IFileProvider ContentRootFileProvider { get; set; } = default!;
        }
    }
}
