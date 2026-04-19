using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminUsageSessionPage
    {
        [DataMember(Name = "page")]
        public int Page { get; set; }

        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }

        [DataMember(Name = "totalRows")]
        public int TotalRows { get; set; }

        [DataMember(Name = "totalPages")]
        public int TotalPages { get; set; }

        [DataMember(Name = "items")]
        public List<AdminUsageSessionListItem> Items { get; set; }
    }
}
