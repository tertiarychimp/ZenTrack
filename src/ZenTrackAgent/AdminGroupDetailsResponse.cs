using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminGroupDetailsResponse
    {
        [DataMember(Name = "groupName")]
        public string GroupName { get; set; }

        [DataMember(Name = "billingReference")]
        public string BillingReference { get; set; }

        [DataMember(Name = "currentMembers")]
        public List<AdminFilterOptionItem> CurrentMembers { get; set; }

        [DataMember(Name = "unassignedUsers")]
        public List<AdminFilterOptionItem> UnassignedUsers { get; set; }
    }
}
