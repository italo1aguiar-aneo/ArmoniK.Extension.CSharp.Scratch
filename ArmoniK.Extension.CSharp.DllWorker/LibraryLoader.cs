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

using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.DllCommon;
using ArmoniK.Extension.CSharp.Worker;

using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.DllWorker;

public class LibraryLoader : ILibraryLoader
{
  private ConcurrentDictionary<string, (Assembly, AssemblyLoadContext, string)> assemblyLoadContexts_ = new();

  public ILogger Logger;

  public LibraryLoader(ILoggerFactory loggerFactory)
    => Logger = loggerFactory.CreateLogger<LibraryLoader>();

  public void Dispose()
    => assemblyLoadContexts_ = new ConcurrentDictionary<string, (Assembly, AssemblyLoadContext, string)>();

  public T GetClassInstance<T>(DynamicLibrary dynamicLibrary)
  {
    try
    {
      assemblyLoadContexts_.TryGetValue(dynamicLibrary.ToString(),
                                        out var assembly);
      using (assembly.Item2.EnterContextualReflection())
      {
        // Create an instance of a class from the assembly.
        var classType = assembly.Item1.GetType($"{dynamicLibrary.Namespace}.{dynamicLibrary.Service}");
        Logger.LogInformation($"Types inside the assembly: {string.Join(",", assembly.Item1.GetTypes().Select(x => x.ToString()))}");
        Logger.LogInformation($"Getting type {dynamicLibrary.Namespace}.{dynamicLibrary.Service}: {classType}");
        if (classType == null)
        {
          Logger.LogError($"Error finding class {dynamicLibrary.Namespace}.{dynamicLibrary.Service}");
          throw new WorkerApiException($"Error finding class {dynamicLibrary.Namespace}.{dynamicLibrary.Service}");
        }

        var serviceContainer = (T)Activator.CreateInstance(classType);

        if (serviceContainer == null)
        {
          Logger.LogError("Couldn't load class instance");
          throw new WorkerApiException("Couldn't load class instance");
        }

        return serviceContainer;
      }
    }
    catch (Exception e)
    {
      Logger.LogError($"Error loading class instance: {e.Message}");
      throw new WorkerApiException(e);
    }
  }

  public AssemblyLoadContext GetAssemblyLoadContext(string libraryContextKey)
  {
    var exists = assemblyLoadContexts_.TryGetValue(libraryContextKey,
                                                   out var value);
    if (!exists)
    {
      Logger.LogError($"AssemblyLoadContexts does not have key {libraryContextKey}");
      throw new WorkerApiException("No key found on AssemblyLoadContexts dictionary");
    }

    return value.Item2;
  }

  public void ResetService()
    => Dispose();

  public async Task<string> LoadLibrary(ITaskHandler      taskHandler,
                                        CancellationToken cancellationToken)
  {
    try
    {
      Logger.LogInformation("Starting to LoadLibrary");
      Logger.LogInformation($"Nb of current loaded assemblies: {assemblyLoadContexts_.Count}");

      var taskLibrary = taskHandler.TaskOptions.GetServiceLibrary();

      //Get the data about the dynamic library
      var dynamicLibrary = taskHandler.TaskOptions.GetDynamicLibrary(taskLibrary);
      var filename       = $"{dynamicLibrary}.zip";

      var filePath = @"/tmp/zip";

      var destinationPath = @"/tmp/assemblies";

      var pathToDllFile = dynamicLibrary.PathToFile;

      var dllFileName = dynamicLibrary.DllFileName;

      Logger.LogInformation($"Starting Dynamic loading - TaskLibrary: {taskLibrary}, FileName: {filename}, FilePath: {filePath}, DestinationToUnZip:{destinationPath}, PathToDllFile:{pathToDllFile}, DllFileName: {dllFileName}, Namespace: {dynamicLibrary.Namespace}, Service: {dynamicLibrary.Service}");

      //if the context is already loaded
      if (assemblyLoadContexts_.ContainsKey(dynamicLibrary.ToString()))
      {
        return dynamicLibrary.ToString();
      }

      var loadContext = new AssemblyLoadContext(dynamicLibrary.ToString());

      var dllExists = taskHandler.DataDependencies.TryGetValue(dynamicLibrary.LibraryBlobId,
                                                               out var libraryBytes);
      if (!dllExists || libraryBytes == null)
      {
        throw new WorkerApiException("No library found on data dependencies.");
      }

      try
      {
        Directory.CreateDirectory(filePath);

        await using var fs = new FileStream(Path.Join(filePath,
                                                      filename),
                                            FileMode.Create,
                                            FileAccess.ReadWrite,
                                            FileShare.Read,
                                            libraryBytes.Length);

        fs.Seek(0,
                SeekOrigin.Begin);
        await fs.WriteAsync(libraryBytes,
                            0,
                            libraryBytes.Length,
                            cancellationToken);
      }
      catch (Exception ex)
      {
        throw new WorkerApiException(ex);
      }

      Logger.LogInformation("Extracting from archive {localZip}",
                            Path.Join(filePath,
                                      filename));

      var extractedPath = ExtractArchive(filename,
                                         filePath,
                                         destinationPath,
                                         pathToDllFile,
                                         dllFileName);

      var zipFile = Path.Join(filePath,
                              filename);

      File.Delete(zipFile);

      Logger.LogInformation("Package {dynamicLibrary} successfully extracted from {localAssembly}",
                            dynamicLibrary,
                            extractedPath);

      Logger.LogInformation($"Trying to load: {Path.Join(extractedPath,
                                                         dllFileName)}");

      var assembly = loadContext.LoadFromAssemblyPath(Path.Join(extractedPath,
                                                                dllFileName));

      var added = assemblyLoadContexts_.TryAdd(dynamicLibrary.ToString(),
                                               (assembly, loadContext, Path.Join(extractedPath,
                                                                                 dllFileName)));

      if (!added)
      {
        throw new WorkerApiException($"Unable to add load context {dynamicLibrary}");
      }

      Logger.LogInformation($"Nb of current loaded assemblies: {assemblyLoadContexts_.Count}");

      return dynamicLibrary.ToString();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw new WorkerApiException(ex);
    }
  }

  public static bool IsZipFile(string assemblyNameFilePath)
  {
    var extension = Path.GetExtension(assemblyNameFilePath);
    return extension?.ToLower() == ".zip";
  }

  public string ExtractArchive(string filename,
                               string filePath,
                               string destinationPath,
                               string pathToDllFile,
                               string dllFileName,
                               bool   overwrite = false)
  {
    if (!IsZipFile(filename))
    {
      throw new WorkerApiException("Cannot yet extract or manage raw data other than zip archive");
    }

    var originFile = Path.Join(filePath,
                               filename);

    if (!Directory.Exists(destinationPath))
    {
      Directory.CreateDirectory(destinationPath);
    }

    var dllFile = Path.Join(destinationPath,
                            pathToDllFile,
                            dllFileName);

    Logger.LogInformation($"Dll should in the following folder {dllFile}");

    var lockFileName = Path.Combine(destinationPath,
                                    $"{filename}.lock");

    using (var spinLock = new FileSpinLock(lockFileName,
                                           timeoutMs: 60000))
    {
      if (spinLock.HasLock)
      {
        if (overwrite || !File.Exists(dllFile))
        {
          try
          {
            ZipFile.ExtractToDirectory(originFile,
                                       destinationPath);
          }
          catch (Exception e)
          {
            throw new WorkerApiException(e);
          }
        }
      }
      else
      {
        throw new WorkerApiException($"Could not lock file to extract zip {filename}");
      }
    }

    //Check now if the assembly is present
    if (!File.Exists(dllFile))
    {
      Logger.LogError($"Dll should in the following folder {dllFile}");
      throw new WorkerApiException($"Fail to find assembly {dllFile}");
    }

    return Path.Join(destinationPath,
                     pathToDllFile);
  }

  /// <summary>
  ///   Spin lock on a file
  /// </summary>
  [PublicAPI]
  public sealed class FileSpinLock : IDisposable
  {
    private static readonly byte[] LockBytes = Encoding.ASCII.GetBytes("locked");

    [CanBeNull]
    private readonly FileStream fileStream_;

    /// <summary>
    ///   Creates a spinlock on the file. Sets <see cref="HasLock" /> to true if it successfully locked the file, false
    ///   otherwise
    /// </summary>
    /// <param name="lockFile">File to lock</param>
    /// <param name="deleteOnUnlock">Delete the lockfile when this object is disposed</param>
    /// <param name="timeoutMs">Maximum time to wait for the lock to be acquired</param>
    /// <param name="spinIntervalMs">Interval between lock tries</param>
    public FileSpinLock(string lockFile,
                        bool   deleteOnUnlock = true,
                        int    timeoutMs      = 30000,
                        int    spinIntervalMs = 250)
    {
      HasLock = false;
      var currentSpinTime = 0;
      spinIntervalMs = Math.Min(spinIntervalMs,
                                timeoutMs);
      do
      {
        try
        {
          fileStream_ ??= new FileStream(lockFile,
                                         FileMode.OpenOrCreate,
                                         FileAccess.ReadWrite,
                                         FileShare.None,
                                         1,
                                         FileOptions.WriteThrough | (deleteOnUnlock
                                                                       ? FileOptions.DeleteOnClose
                                                                       : FileOptions.None));
          if (fileStream_.Seek(0,
                               SeekOrigin.End) == 0)
          {
            fileStream_.Write(LockBytes,
                              0,
                              LockBytes.Length);
            fileStream_.Flush();
          }

          fileStream_.Lock(0,
                           LockBytes.Length);

          HasLock = true;
        }
        catch (IOException)
        {
          Thread.Sleep(spinIntervalMs);
          currentSpinTime += spinIntervalMs;
        }
        catch (UnauthorizedAccessException)
        {
          Thread.Sleep(spinIntervalMs);
          currentSpinTime += spinIntervalMs;
        }
      } while (!HasLock && currentSpinTime < timeoutMs + spinIntervalMs);
    }

    /// <summary>
    ///   True if the file is locked by the current class, false otherwise
    /// </summary>
    public bool HasLock { get; }

    /// <inheritdoc />
    public void Dispose()
    {
      if (HasLock)
      {
        fileStream_?.Unlock(0,
                            LockBytes.Length);
      }

      fileStream_?.Dispose();
    }
  }
}
