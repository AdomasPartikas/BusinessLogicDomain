using System.Runtime.Serialization;

namespace BusinessLogicDomain.API.Entities.Enum
{
    public enum SimulationLevel
    {
        [EnumMember(Value = "Easy")]
        Easy,
        [EnumMember(Value = "Normal")]
        Normal,
        [EnumMember(Value = "Hard")]
        Hard
    }
}