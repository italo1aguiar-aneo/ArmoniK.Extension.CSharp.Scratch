# ArmoniK C# Client Extension

## Overview
This project contains the new ArmoniK C# client extension. It proposes a new approach to ArmoniK usage of ArmoniK API. It enables users to detail their task graphs in a better way managing data dependencies and enabling better data management between tasks. It also allows users to make use of the async programming functions which where not available on the actual SDK. 

## Table of Contents
- [Overview](#overview)
- [Concepts](#concepts)
- [Usage](#usage)

## Concepts

### Blobs

In ArmoniK, data is managed through entities called "Results," which encompass all input and output data. To enhance intuitiveness, we propose using the term "blob" to describe any data entity. Blobs simplify data handling and provide a clear and consistent way to manage data throughout the SDK.

A blob is described as follows:

- **Name:** `string` - A user-defined name for the blob. If not provided, the SDK can assign a default name.
- **ID:** `string` - A unique identifier for the blob in ArmoniK. This ID is generated by the API, with the SDK abstracting the creation process for ease of use.
- **Content:** `ReadOnlyMemory<byte>` - The actual data content of the blob. This content can be downloaded from or uploaded to ArmoniK.

This naming convention and structure ensure that data management is straightforward and intuitive, enabling developers to interact with data seamlessly.

Blobs are created only by the SDK, in order to create a blob, users must use BlobService provided by ArmoniK Client class.

```csharp
var blobService = await client.GetBlobService();
// CreateBlobAsync(string name, ReadOnlyMemory<byte> content, Session session = null, CancellationToken cancellationToken = default)
// Other overloads are available
var newBlob = await blobService.CreateBlobAsync("Payload", Encoding.ASCII.GetBytes("Hello"), session);
```

### TaskNodes

ArmoniK is a graph-oriented orchestrator that allows users to define complex workflows and dependencies between tasks. To enable more intuitive graph definitions, we propose the abstraction of TaskNodes. TaskNodes are fundamental building blocks that represent individual tasks within the orchestrated workflow.

A TaskNode is defined by the following properties:

- **ExpectedOutputs:** `IEnumerable<BlobInfo>` - This property lists the expected outputs of the task. Each output is represented as a `BlobInfo` object, which contains metadata about the data produced by the task. These outputs can be used as inputs for other tasks, enabling the construction of a dependency graph.

- **DataDependencies:** `IEnumerable<BlobInfo>` - This property lists the data dependencies required by the task to execute. Each dependency is represented as a `BlobInfo` object, which contains metadata about the data required. These dependencies ensure that tasks are executed in the correct order, based on the availability of their input data.

- **Payload:** `BlobInfo` - This property represents the primary data payload for the task. The payload is a `BlobInfo` object that contains the data necessary for the task's execution. 

- **TaskOptions:** `TaskOptions` - This property specifies the configuration options for the task. `TaskOptions` can include various settings such as partition, execution parameters, priority levels, and resource requirements. These options allow for fine-tuned control over the task's execution within the orchestrated workflow.

### Services

In order segregate responsabilites, we will enable multiple services which are responsible for dealing with the API:

To segregate responsibilities and provide a clear structure, ArmoniK SDK will enable multiple services responsible for interacting with the API. Each service is focused on a specific aspect of the API, ensuring modularity and ease of use.

- **SessionService:** `ISessionService` - This service is responsible for managing sessions providing an interface for session-related operations.

- **BlobService:** `IBlobService` - This service deals with blobs, which represent data entities in ArmoniK. It provides methods for creating, uploading and downloading blobs. 

- **TasksService:** `ITasksService` - This service is responsible for managing tasks. It provides functionalities for submitting tasks.

- **EventsService:** `IEventsService` - This service handles events within the ArmoniK ecosystem. This allows for real-time notifications and reactions to various events occurring during the execution of tasks and workflows.

### ArmoniKClient

Users will exchange with ArmoniK by ArmoniKClient, which manages all services and channels pools. ArmoniKClient exposes a set of functions to enable users to access services:

- **SessionService:** `Task<ISessionService> GetSessionService()`
  - Retrieves the service responsible for managing sessions, including creating and handling session lifecycles.

- **BlobService:** `Task<IBlobService> GetBlobService()`
  - Retrieves the service responsible for managing blobs, including creating, uploading, downloading, and managing data blobs.

- **TasksService:** `Task<ITasksService> GetTasksService()`
  - Retrieves the service responsible for managing tasks, including submitting tasks, monitoring their status, and retrieving results.

- **EventsService:** `Task<IEventsService> GetEventsService()`
  - Retrieves the service responsible for managing events, including subscribing to, publishing, and handling events within the ArmoniK ecosystem.

## Usage

A project with a simple use case is available.

## Next Step

- End-to-end tests
- Delagate functions as parameters
- Send Tasks in chunk