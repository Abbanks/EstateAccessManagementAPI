using System.ComponentModel;

namespace EstateAccessManagement.Core.Enums
{
    public enum AccessCodeType
    {
        None = 0,
        [Description("TemporaryVisitor")]
        TemporaryVisitor = 1,
        [Description("LongStayVisitor")]
        LongStayVisitor = 2
    }
}
