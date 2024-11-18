using System.Runtime.Serialization;

namespace BusinessLogicDomain.API.Entities.Enum
{
    public enum TransactionStatus
    {
        [EnumMember(Value = "On Hold")]
        OnHold,
        [EnumMember(Value = "Completed")]
        Completed,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
        [EnumMember(Value = "Pending")]
        Pending

    }
}