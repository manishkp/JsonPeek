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
    using System.Linq;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

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
        /// Gets or sets JSON File Name
        /// </summary>
        public string JsonInputPath { get; set; }

        /// <summary>
        /// Gets or sets JSON content
        /// </summary>
        public string JsonContent { get; set; }

        /// <summary>
        /// Gets or sets Object Value
        /// </summary>
        [Output]
        public ITaskItem[] Result { get; private set; }

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
            if (string.IsNullOrEmpty(this.JsonInputPath) && string.IsNullOrEmpty(this.JsonContent) ||
                (!string.IsNullOrEmpty(this.JsonInputPath) && !string.IsNullOrEmpty(this.JsonContent)))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                    string.Format(
                    "Skipping json peek, as both 'JsonInputPath' and 'JsonContent' are empty (or both are defined)",
                    JsonInputPath),
                    string.Empty,
                    "JsonPeek",
                    MessageImportance.Normal));

                return false;
            }

            if (!File.Exists(JsonInputPath) && string.IsNullOrEmpty(this.JsonContent))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json peek, as there are no json files found at {0}",
                            JsonInputPath),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                return false;
            }

            if (string.IsNullOrEmpty(this.JPath))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Skipping json peek, no xpath or value specified"),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                return false;
            }

            var returnValue = new List<string>();

            string content;

            if (string.IsNullOrEmpty(this.JsonContent))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Started json peek for file {0}", JsonInputPath),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                using (var sr = new StreamReader(JsonInputPath))
                {
                    content = sr.ReadToEnd();
                }
            }
            else
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Started json peek"),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));


                content = this.JsonContent;
            }


            var root = JObject.Parse(content);
            var currentNodes = root.SelectTokens(this.JPath, false);
            foreach (var currentNode in currentNodes)
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Found value : {0}", currentNode.ToString()),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Low));
                returnValue.Add(currentNode.ToString());
            }

            this.Result = returnValue.Select(outputVal => (ITaskItem)new TaskItem(outputVal)).ToArray();
            return true;
        }
    }
}