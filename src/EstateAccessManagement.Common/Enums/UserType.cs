using System.ComponentModel;

namespace EstateAccessManagement.Common.Enums
{
    public enum UserType
    {
        None = 0,
        [Description("Admin")]
        Admin,
        [Description("Resident")]
        Resident,
        [Description("Security")]
        Security
    }
}