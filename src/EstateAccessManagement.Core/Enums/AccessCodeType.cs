using System.ComponentModel;

namespace EstateAccessManagement.Core.Enums
{
    public enum AccessCodeType
    {
        None,
        [Description("TemporaryVisitor")]
        TemporaryVisitor,
        [Description("LongStayVisitor")]
        LongStayVisitor
    }
}
