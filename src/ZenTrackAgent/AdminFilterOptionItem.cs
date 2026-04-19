using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminFilterOptionItem
    {
        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }
    }
}
