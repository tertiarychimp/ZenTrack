using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminFilterOptionsResponse
    {
        [DataMember(Name = "users")]
        public List<AdminFilterOptionItem> Users { get; set; }

        [DataMember(Name = "groups")]
        public List<AdminFilterOptionItem> Groups { get; set; }
    }
}
