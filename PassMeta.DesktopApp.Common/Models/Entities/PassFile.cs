namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Extra;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile model.
    /// </summary>
    public class PassFile
    {
        /// <summary>
        /// Passfile identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; init; }

        /// <summary>
        /// Passfile type identifier.
        /// </summary>
        [JsonProperty("type_id")]
        public int TypeId { get; init; }

        /// <summary>
        /// Owner user identifier.
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; init; }

        /// <summary>
        /// Passfile name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Passfile distinctive color.
        /// </summary>
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        /// <summary>
        /// Passfile data version.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Passfile creation date and time.
        /// </summary>
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Passfile information change date and time.
        /// </summary>
        [JsonProperty("info_changed_on")]
        public DateTime InfoChangedOn { get; set; }
        
        /// <summary>
        /// Passfile data change date and time.
        /// </summary>
        [JsonProperty("version_changed_on")]
        public DateTime VersionChangedOn { get; set; }

        [JsonProperty("smth")]
        // ReSharper disable once InconsistentNaming
        private string? _DataEncrypted { set => DataEncrypted = value; }

        /// <summary>
        /// Server identifier.
        /// </summary>
        [JsonProperty("__server_id")]
        public string? ServerId { get; set; }

        /// <summary>
        /// Origin passfile, unchanged from the last download from the server. 
        /// </summary>
        [JsonProperty("__origin")]
        public PassFile? Origin { get; set; }
        
        /// <summary>
        /// Local passfile deletion timestamp.
        /// </summary>
        [JsonProperty("__del")]
        public DateTime? LocalDeletedOn { get; set; }

        /// <summary>
        /// Passfile encrypted data string.
        /// </summary>
        [JsonIgnore]
        public string? DataEncrypted;

        /// <summary>
        /// Passfile passphrase to decrypt <see cref="DataEncrypted"/> and encrypt <see cref="PwdData"/>.
        /// </summary>
        [JsonIgnore]
        public string? PassPhrase;

        /// <summary>
        /// <see cref="PassFileType.Pwd"/> passfile decrypted data.
        /// </summary>
        [JsonIgnore]
        public List<PwdSection>? PwdData;

        /// <summary>
        /// <see cref="PassFileType.Txt"/> passfile decrypted data.
        /// </summary>
        [JsonIgnore]
        public List<TxtSection>? TxtData;  // TODO

        /// <summary>
        /// Passfile local problem.
        /// </summary>
        [JsonIgnore]
        public PassFileProblem? Problem;

        /// <summary>
        /// Passfile marks.
        /// </summary>
        /// <remarks>Relates to data fields.</remarks>
        [JsonIgnore]
        public PassFileMark Marks;

        /// <summary>
        /// Passfile type.
        /// </summary>
        [JsonIgnore]
        public PassFileType Type => (PassFileType)TypeId;

        #region State (one or nothing)

        /// <summary>
        /// Is passfile created locally, but not uploaded to the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalCreated => Id < 1 && Origin is null && LocalDeletedOn is null;

        /// <summary>
        /// Is passfile changed locally, but not updated on the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalChanged => Id > 0 && Origin is not null && LocalDeletedOn is null;

        /// <summary>
        /// Is passfile unchanged locally?
        /// </summary>
        [JsonIgnore]
        public bool LocalNotChanged => Id > 0 && Origin is null && LocalDeletedOn is null;

        /// <summary>
        /// Is passfile deleted locally, but not deleted from the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalDeleted => LocalDeletedOn is not null;

        #endregion

        /// <summary></summary>
        public PassFile()
        {
            Name ??= "?";
        }

        /// <inheritdoc />
        public override string ToString() => $"<Passfile Id={Id}, Name='{Name.Replace("'", "\"")}'>";

        /// <summary>
        /// Deep copy of this passfile.
        /// </summary>
        public PassFile Copy(bool copyData = true)
        {
            var clone = (PassFile)MemberwiseClone();
            clone.Origin = Origin?.Copy(false);
            clone.PwdData = copyData ? clone.PwdData?.Select(section => section.Copy()).ToList() : null;
            clone.TxtData = copyData ? clone.TxtData?.Select(section => section.Copy()).ToList() : null;
            return clone;
        }
    }
}