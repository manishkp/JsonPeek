// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonPoke.cs">
//   Copyright belongs to Manish Kumar
// </copyright>
// <summary>
//   Build task to replace value at Jpath
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonPeek.MSBuild
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Build.Framework;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Build task to convert Resource file to Java script Object Notation file
    /// </summary>
    public class JsonPoke : ITask
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
        [Required]
        public string JsonInputPath { get; set; }     
   
        /// <summary>
        /// Gets or sets JValue
        /// </summary>
        public string JValue { get; set; }

        /// <summary>
        /// Gets or sets Array
        /// </summary>
        public ITaskItem[] JArray { get; set; }

        /// <summary>
        /// Gets or sets Object
        /// </summary>
        public ITaskItem JObject { get; set; }

        /// <summary>
        /// List of Metadata Values to include in for objects
        /// </summary>
        public string[] Metadata { get; set; }

        
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
            if (!File.Exists(this.JsonInputPath))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json replacement, as there are no json files found at {0}", this.JsonInputPath),
                        string.Empty,
                        "JsonPoke",
                        MessageImportance.Normal));

                return false;
            }

            if (string.IsNullOrEmpty(this.JPath) || (string.IsNullOrEmpty(this.JValue) && this.JArray == null && this.JObject == null))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json replacement, no 'JPath' or 'JValue'/'JArray'/'JObject' specified"),
                        string.Empty,
                        "JsonPoke",
                        MessageImportance.Normal));

                return false;
            }

            this.BuildEngine.LogMessageEvent(
                new BuildMessageEventArgs(
                    string.Format("Started json poke for file {0}", this.JsonInputPath),
                    string.Empty,
                    "JsonPoke",
                    MessageImportance.Normal));

            try
            {
                // Replacing the value 
                JObject root = null;
                using (var sr = new StreamReader(this.JsonInputPath))
                {
                    var content = sr.ReadToEnd();
                    root = Newtonsoft.Json.Linq.JObject.Parse(content);

                    var currentNodes = root.SelectTokens(this.JPath, false);

                    foreach (var currentNode in currentNodes)
                    {
                        if (!string.IsNullOrEmpty(this.JValue))
                        {
                            this.BuildEngine.LogMessageEvent(
                                new BuildMessageEventArgs(
                                    string.Format("Replacing value : {0} with {1}", currentNode.ToString(), this.JValue),
                                    string.Empty,
                                    "JsonPoke",
                                    MessageImportance.Normal));
                            currentNode.Replace(new JValue(this.JValue));
                        }
                        else if (this.JArray != null)
                        {
                            this.BuildEngine.LogMessageEvent(
                                new BuildMessageEventArgs(
                                    string.Format("Replacing array value for {0} ", currentNode.ToString()),
                                    string.Empty,
                                    "JsonPoke",
                                    MessageImportance.Normal));
                            currentNode.Replace(new JArray(this.JArray.Select(this.GetObject)));
                        }
                        else if (this.JObject != null)
                        {
                            this.BuildEngine.LogMessageEvent(
                                new BuildMessageEventArgs(
                                    string.Format("Replacing object value for {0}", currentNode.ToString()),
                                    string.Empty,
                                    "JsonPoke",
                                    MessageImportance.Normal));
                            currentNode.Replace(this.GetObject(this.JObject));
                        }
                    }
                }

                using (var fs = File.Open(this.JsonInputPath, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                using (var jw = new JsonTextWriter(sw))
                {
                    // Trying to fix file not being closed issue
                    jw.CloseOutput = true;
                    jw.Formatting = Formatting.Indented;
                    root.WriteTo(jw);
                }
            }
            catch (Exception)
            {
                // Adding information about jpath and full filepath for debugging purposes 
                this.BuildEngine.LogErrorEvent(
                    new BuildErrorEventArgs(
                        string.Empty,
                        string.Empty,
                        this.JsonInputPath,
                        0,
                        0,
                        0,
                        0,
                        string.Format("Failed to replace JPath:{0} in file:{1}", this.JPath, this.JsonInputPath),
                        string.Empty,
                        "JsonPoke"));
                throw;
            }

            return true;
        }

        /// <summary>
        /// The get object.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private JToken GetObject(ITaskItem value)
        {
            if (this.Metadata == null
                || this.Metadata.Length <= 0)
            {
                return new JValue(value.ToString());
            }

            var jsonObject = new JObject();
            foreach (var metadataName in this.Metadata)
            {
                jsonObject[metadataName] = value.GetMetadata(metadataName);
            }

            return jsonObject;
        }
    }
}