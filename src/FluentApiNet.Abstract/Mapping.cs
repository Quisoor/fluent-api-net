using System.Linq.Expressions;

namespace FluentApiNet.Abstract
{
    public class Mapping
    {
        public MemberExpression ModelMember { get; set; }
        public MemberExpression EntityMember { get; set; }
    }
}
