using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminGroupSummaryResponse
    {
        [DataMember(Name = "totalSessions")]
        public int TotalSessions { get; set; }

        [DataMember(Name = "totalRoundedMinutes")]
        public int TotalRoundedMinutes { get; set; }

        [DataMember(Name = "totalUsers")]
        public int TotalUsers { get; set; }

        [DataMember(Name = "items")]
        public List<AdminGroupSummaryItem> Items { get; set; }
    }
}
