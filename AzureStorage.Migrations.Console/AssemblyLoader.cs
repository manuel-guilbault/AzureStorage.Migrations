using System;
using System.IO;
using System.Reflection;

namespace AzureStorage.Migrations.Console
{
    public class AssemblyLoader
    {
        public static Assembly Load(FileInfo file)
            => Load(file, AppDomain.CurrentDomain);

        public static Assembly Load(FileInfo file, AppDomain appDomain)
        {
            var loader = new AssemblyLoader(file);
            var assembly = loader.LoadFrom(appDomain);
            return assembly;
        }

        private readonly FileInfo file;

        private AssemblyLoader(FileInfo file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
        }

        private Assembly LoadFrom(AppDomain appDomain)
        {
            appDomain.AssemblyResolve += LoadDependency;
            appDomain.TypeResolve += LoadDependency;

            var assembly = appDomain.Load(AssemblyName.GetAssemblyName(file.FullName));
            return assembly;
        }

        private Assembly LoadDependency(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            try
            {
                var filePath = GetFilePathFromAssemblyName(args.Name);
                assembly = Assembly.LoadFile(filePath);
            }
            catch { }

            return assembly;
        }

        private string GetFilePathFromAssemblyName(string name)
            => Path.Combine(file.DirectoryName, name.Split(',')[0] + ".dll");
    }
}
