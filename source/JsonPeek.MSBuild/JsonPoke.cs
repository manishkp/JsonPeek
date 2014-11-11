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
        /// Gets or sets JSON full file path
        /// </summary>
        [Required]
        public string JsonInputPath { get; set; }     
   
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

            if (string.IsNullOrEmpty(this.JPath) || string.IsNullOrEmpty(this.Value))
            {
                this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Skipping json replacement, no 'JPath' or 'Value' specified"),
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
    }
}