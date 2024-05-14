using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoniK.Extension.CSharp.Client
{
    public class TaskCreationResponse
    {
        public Dictionary<string, Blob> DataDependenciesDictionary;
        public Dictionary<string, string> ExpectedOutputsDictionary;

        public TaskCreationResponse(Dictionary<string, Blob> dataDependenciesDictionary,
            Dictionary<string, string> expectedOutputsDictionary)
        {
            DataDependenciesDictionary = dataDependenciesDictionary;
            ExpectedOutputsDictionary = expectedOutputsDictionary;
        }
    }
}
