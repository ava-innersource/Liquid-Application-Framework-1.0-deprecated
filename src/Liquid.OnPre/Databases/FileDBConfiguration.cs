using FluentValidation; 
using Liquid.Runtime.Configuration;

namespace Liquid.OnPre
{
    /// <summary>
    /// The Configuration for FileConfiguration
    /// </summary>
    public class FileDBConfiguration : LightConfig<FileDBConfiguration>
    {
        /// <summary>
        /// Path to create files
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The necessary validation to create
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(Path))
            {
                Path = @"AppData\";
            }
        }
    }
}