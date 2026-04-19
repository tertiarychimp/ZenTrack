using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminGroupSummaryItem
    {
        [DataMember(Name = "groupName")]
        public string GroupName { get; set; }

        [DataMember(Name = "billingReference")]
        public string BillingReference { get; set; }

        [DataMember(Name = "sessionCount")]
        public int SessionCount { get; set; }

        [DataMember(Name = "totalRoundedMinutes")]
        public int TotalRoundedMinutes { get; set; }

        [DataMember(Name = "userCount")]
        public int UserCount { get; set; }

        [DataMember(Name = "canManage")]
        public bool CanManage { get; set; }
    }
}
