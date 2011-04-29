using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Codeplex.Data.Internal
{
    /// <summary>Delegate accessor created from expression tree.</summary>
    internal class ExpressionAccessor : IMemberAccessor
    {
        public Type DeclaringType { get; private set; }
        public string Name { get; private set; }
        public bool IsReadable { get { return GetValueDirect != null; } }
        public bool IsWritable { get { return SetValueDirect != null; } }

        // for performance optimization
        public readonly Func<object, object> GetValueDirect;
        public readonly Action<object, object> SetValueDirect;

        public ExpressionAccessor(PropertyInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            this.Name = info.Name;
            this.DeclaringType = info.DeclaringType;
            this.GetValueDirect = (info.GetGetMethod(false) != null) ? CreateGetValue(DeclaringType, Name) : null;
            this.SetValueDirect = (info.GetSetMethod(false) != null) ? CreateSetValue(DeclaringType, Name) : null;
        }

        public ExpressionAccessor(FieldInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            this.Name = info.Name;
            this.DeclaringType = info.DeclaringType;
            this.GetValueDirect = CreateGetValue(DeclaringType, Name);
            this.SetValueDirect = (!info.IsInitOnly) ? CreateSetValue(DeclaringType, Name) : null;
        }

        public object GetValue(object target)
        {
            return GetValueDirect(target);
        }

        public void SetValue(object target, object value)
        {
            SetValueDirect(target, value);
        }

        // (object x) => (object)((T)x).name
        [ContractVerification(false)]
        static Func<object, object> CreateGetValue(Type type, string name)
        {
            var x = Expression.Parameter(typeof(object), "x");

            var func = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.PropertyOrField(
                        (type.IsValueType ? Expression.Unbox(x, type) : Expression.Convert(x, type)),
                        name),
                    typeof(object)),
                x);

            return func.Compile();
        }

        // (object x, object v) => ((T)x).name = (U)v
        [ContractVerification(false)]
        static Action<object, object> CreateSetValue(Type type, string name)
        {
            var x = Expression.Parameter(typeof(object), "x");
            var v = Expression.Parameter(typeof(object), "v");

            var left = Expression.PropertyOrField(
                (type.IsValueType ? Expression.Unbox(x, type) : Expression.Convert(x, type)),
                name);
            var right = Expression.Convert(v, left.Type);

            var action = Expression.Lambda<Action<object, object>>(
                Expression.Assign(left, right),
                x, v);

            return action.Compile();
        }
    }
}