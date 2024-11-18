using System.Runtime.Serialization;

namespace BusinessLogicDomain.API.Entities.Enum
{
    public enum TransactionType
    {
        [EnumMember(Value = "Buy")]
        Buy,
        [EnumMember(Value = "Sell")]
        Sell
    }
}