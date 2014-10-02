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
        [Required]
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
                            "Skipping json replacement, as there are no json files found at {0}", jsonFullPath),
                        string.Empty,
                        "JsonPoke",
                        MessageImportance.Normal));
            }

            if (string.IsNullOrEmpty(this.JPath) || string.IsNullOrEmpty(this.Value))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json replacement, no xpath or value specified"),
                        string.Empty,
                        "JsonPoke",
                        MessageImportance.Normal));
            }
          
            this.BuildEngine.LogMessageEvent(
                new BuildMessageEventArgs(
                    string.Format("Started json poke for file {0}", jsonFullPath),
                    string.Empty,
                    "JsonPoke",
                    MessageImportance.Normal));


            JObject root = null;

            try
            {
                // Replacing the value 
                using (var sr = new StreamReader(jsonFullPath))
                {
                    var content = sr.ReadToEnd();
                    root = JObject.Parse(content);

                    var currentNodes = root.SelectTokens(this.JPath, false);

                    foreach (var currentNode in currentNodes)
                    {
                        this.BuildEngine.LogMessageEvent(
                            new BuildMessageEventArgs(
                                string.Format("Replacing value : {0} with {1}", currentNode.ToString(), this.Value),
                                string.Empty,
                                "JsonPoke",
                                MessageImportance.Normal));
                        currentNode.Replace(new JValue(this.Value));
                    }
                }

                if (root != null)
                {
                    using (FileStream fs = File.Open(jsonFullPath, FileMode.Create))
                    using (var sw = new StreamWriter(fs))
                    using (var jw = new JsonTextWriter(sw))
                    {
                        // Trying to fix file not being closed issue
                        jw.CloseOutput = true;
                        jw.Formatting = Formatting.Indented;
                        root.WriteTo(jw);
                    }
                }
            }
            catch (Exception)
            {
                // Adding information about jpath and full filepath for debugging purposes 
                this.BuildEngine.LogErrorEvent(
                    new BuildErrorEventArgs(
                        string.Empty,
                        string.Empty,
                        this.JsonFile,
                        0,
                        0,
                        0,
                        0,
                        string.Format("Failed to replace jpath:{0} in file:{1}", this.JPath, jsonFullPath),
                        string.Empty,
                        "JsonPoke"));
                throw;
            }

            return true;
        }
    }
}