using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;

namespace Compze.Build.FlexRef.Specifications;

static class ModuleInitializer
{
   [ModuleInitializer]
   internal static void Initialize()
   {
      // When real MSBuild DLLs are already in the output directory (copied by the
      // CopyMSBuildRuntimeAssemblies build target), the CLR loads them directly
      // and MSBuildLocator registration is unnecessary. CanRegister is false when
      // Microsoft.Build assemblies are already loaded.
      if(MSBuildLocator.CanRegister)
         MSBuildLocator.RegisterDefaults();
   }
}