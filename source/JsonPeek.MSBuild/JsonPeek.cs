// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonPeek.cs">
//   Copyright belongs to Manish Kumar
// </copyright>
// <summary>
//   Build task to return Jpath value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonPeek.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using Microsoft.Build.Framework;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Build task to convert Resource file to Java script Object Notation file
    /// </summary>
    public class JsonPeek : ITask
    {
        /// <summary>
        /// Gets or sets Build Engine
        /// </summary>
        public IBuildEngine BuildEngine { get; set; }

        /// <summary>
        /// Gets or sets Host Object
        /// </summary>
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// Gets or sets JSON Full Path
        /// </summary>
        [Required]
        public string FileFullPath { get; set; }

        /// <summary>
        /// Gets or sets JSON File Name
        /// </summary>
        [Required]
        public string JsonFile { get; set; }     
   
        /// <summary>
        /// Gets or sets Object Value
        /// </summary>
        [Output]
        public string Value { get; set; }   
  
        /// <summary>
        /// Gets or sets JPath
        /// This is current JPath supported by Newtonsoft.Json 
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."),Required]
        public string JPath { get; set; }

        /// <summary>
        /// Executes the Task
        /// </summary>
        /// <returns>True if success</returns>
        public bool Execute()
        {
            var jsonFullPath = Path.Combine(this.FileFullPath, this.JsonFile);

            if (!File.Exists(jsonFullPath))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json peek, as there are no json files found at {0}",
                            jsonFullPath),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));
            }

            if (string.IsNullOrEmpty(this.JPath)
                || string.IsNullOrEmpty(this.Value))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Skipping json peek, no xpath or value specified"),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));
            }

            this.BuildEngine.LogMessageEvent(
                new BuildMessageEventArgs(
                    string.Format("Started json peek for file {0}", jsonFullPath),
                    string.Empty,
                    "JsonPeek",
                    MessageImportance.Normal));

            var returnValue = new List<string>();

            using (var sr = new StreamReader(jsonFullPath))
            {
                var content = sr.ReadToEnd();
                var root = JObject.Parse(content);
                var currentNodes = root.SelectTokens(this.JPath, false);
                foreach (var currentNode in currentNodes)
                {
                    this.BuildEngine.LogMessageEvent(
                        new BuildMessageEventArgs(
                            string.Format("Found value : {0}", currentNode.ToString()),
                            string.Empty,
                            "JsonPeek",
                            MessageImportance.Normal));
                    returnValue.Add(currentNode.ToString());
                }
            }

            if (returnValue.Count == 1)
            {
                this.Value = returnValue[0];
            }
            else if (returnValue.Count > 1)
            {
                this.Value = JsonConvert.SerializeObject(returnValue);
            }

            return true;
        }
    }
}