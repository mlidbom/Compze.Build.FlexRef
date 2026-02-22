using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;

namespace Compze.Build.FlexRef.Specifications;

static class ModuleInitializer
{
   [ModuleInitializer]
   internal static void Initialize()
   {
      if(MSBuildLocator.CanRegister)
         MSBuildLocator.RegisterDefaults();
   }
}