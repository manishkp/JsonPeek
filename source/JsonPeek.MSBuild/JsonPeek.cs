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
        /// Gets or sets JSON full file path
        /// </summary>
        public string JsonInputPath { get; set; }

        /// <summary>
        /// Gets or sets JSON content
        /// </summary>
        public string JsonContent { get; set; }

        /// <summary>
        /// Gets list of Results found for the JPath
        /// </summary>
        [Output]
        public ITaskItem[] Result { get; private set; }

        /// <summary>
        /// Gets or sets JPath
        /// This is current JPath supported by Newtonsoft.Json
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        [Required]
        public string JPath { get; set; }

        /// <summary>
        /// Executes the Task
        /// </summary>
        /// <returns>True if success</returns>
        public bool Execute()
        {
            if ((string.IsNullOrEmpty(this.JsonInputPath) && string.IsNullOrEmpty(this.JsonContent)) ||
                (!string.IsNullOrEmpty(this.JsonInputPath) && !string.IsNullOrEmpty(this.JsonContent)))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                    string.Format(
                    "Skipping json peek, as both 'JsonInputPath' and 'JsonContent' are empty (or both are defined)",
                    this.JsonInputPath),
                    string.Empty,
                    "JsonPeek",
                    MessageImportance.Normal));

                return false;
            }

            if (!File.Exists(this.JsonInputPath) && string.IsNullOrEmpty(this.JsonContent))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json peek, as there are no json files found at {0}",
                            this.JsonInputPath),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                return false;
            }

            if (string.IsNullOrEmpty(this.JPath))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Skipping json peek, no JPath specified"),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                return false;
            }

            var returnValue = new List<ITaskItem>();

            string content;

            if (string.IsNullOrEmpty(this.JsonContent))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format("Started json peek for file {0}", this.JsonInputPath),
                        string.Empty,
                        "JsonPeek",
                        MessageImportance.Normal));

                using (var sr = new StreamReader(this.JsonInputPath))
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

                returnValue.AddRange(GetTaskItem(currentNode));                        
            }

            this.Result = returnValue.ToArray();
            return true;
        }

        /// <summary>
        /// The get task item.
        /// </summary>
        /// <param name="jsonObject">
        /// The json object.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>. of TaskItems
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static IEnumerable<ITaskItem> GetTaskItem(JToken jsonObject)
        {
            var returnValue = new List<ITaskItem>();
            if (jsonObject.Type == JTokenType.Array)
            {
                foreach (var arrayNode in jsonObject)
                {
                    returnValue.AddRange(GetTaskItem(arrayNode));
                }               
            }
            else if (jsonObject.Type == JTokenType.Object)
            {
                var taskItem = new TaskItem(jsonObject.ToString());               
                foreach (var prop in jsonObject.OfType<JProperty>())
                {
                    // we only support 1 level of complex objects
                    taskItem.SetMetadata(prop.Name, prop.Value.ToString());
                }

                returnValue.Add(taskItem);
            }
            else
            {
                var taskItem = new TaskItem(jsonObject.ToString());
                // this helps get metadata batching scenario work %(array.Value)
                taskItem.SetMetadata("Value", jsonObject.ToString());
                returnValue.Add(taskItem);
            }

            return returnValue;
        }
    }
}