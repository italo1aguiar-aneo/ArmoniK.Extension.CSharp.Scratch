// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2024. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using System.Runtime.Loader;

using ArmoniK.Extension.CSharp.Common;

namespace ArmoniK.Extension.CSharp.Worker;

public class AppsLoader : IAppsLoader
{
  private Assembly            assembly_;
  public  AssemblyLoadContext UserAssemblyLoadContext { get; }

  public DynamicLibrary DynamicLibrary { get; set; }

  public void Dispose()
  {
  }

  public T GetServiceContainerInstance<T>(DynamicLibrary gridAppNamespace)
  {
    using (UserAssemblyLoadContext.EnterContextualReflection())
    {
      // Create an instance of a class from the assembly.
      var classType = assembly_.GetTypes()
                               .Where(i => i.IsClass && i.IsGenericType)
                               .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IService));
      if (classType != null)
      {
        var serviceContainer = (T)Activator.CreateInstance(classType);

        return serviceContainer;
      }
    }

    throw new Exception($"Cannot find ServiceContainer in: {gridAppNamespace.Name} in dll [{gridAppNamespace.PathToFile}] implementing IService Interface");
  }
}
