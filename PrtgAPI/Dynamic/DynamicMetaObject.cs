using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;

namespace PrtgAPI.Dynamic
{  
    class DynamicMetaObject<T> : DynamicMetaObject
    {
        private new T Value => (T)base.Value;

        public Type ValueType => Value.GetType();

        internal DynamicProxy<T> Proxy { get; }

        public DynamicMetaObject(Expression expression, T value, DynamicProxy<T> proxy) : base(expression, BindingRestrictions.Empty, value)
        {
            Proxy = proxy;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var builder = new DynamicExpressionBuilder<T>(this, binder, DynamicExpressionBuilder<T>.NoArgs, e => binder.FallbackGetMember(this, e));

            return builder.CallMethodWithResult(DynamicProxy<T>.GetMember);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var builder = new DynamicExpressionBuilder<T>(this, binder, DynamicExpressionBuilder<T>.NoArgs, e => binder.FallbackSetMember(this, value, e));

            return builder.CallMethodReturnLast(DynamicProxy<T>.SetMember, value.Expression);
        }

        [ExcludeFromCodeCoverage]
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Proxy.GetDynamicMemberNames(Value);
        }
    }
}
