using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class ApiStatusResponse
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
