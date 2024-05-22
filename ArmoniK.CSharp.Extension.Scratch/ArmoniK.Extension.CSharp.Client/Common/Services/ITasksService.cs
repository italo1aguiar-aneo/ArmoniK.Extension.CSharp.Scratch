using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
    Task<IEnumerable<string>> SubmitTasksAsync(IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default);
}