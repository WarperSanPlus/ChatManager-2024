using Newtonsoft.Json;
using System;

namespace Models
{
    public class Entry : BaseModel
    {
        public int Id { get; set; } = 0;
        public int IdUser { get; set; } = 0;
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? End { get; set; } = null;

        [JsonIgnore]
        public User User { get; set; }

        /// <returns>Is this entry still active?</returns>
        public bool IsFinised() => this.End != null;

        public bool IsValid() => this.Start != this.End;
    }
}