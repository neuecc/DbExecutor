using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq.Expressions;

namespace Codeplex.Data.Internal
{
    /// <summary>Delegate accessor created from expression tree.</summary>
    internal class ExpressionAccessor : IMemberAccessor
    {
        public Type DelaringType { get; private set; }
        public string Name { get; private set; }
        public bool IsReadable { get { return getValue != null; } }
        public bool IsWritable { get { return setValue != null; } }

        readonly Func<object, object> getValue;
        readonly Action<object, object> setValue;

        public ExpressionAccessor(PropertyInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            this.Name = info.Name;
            this.DelaringType = info.DeclaringType;
            this.getValue = (info.GetGetMethod(false) != null) ? CreateGetValue(DelaringType, Name) : null;
            this.setValue = (info.GetSetMethod(false) != null) ? CreateSetValue(DelaringType, Name) : null;
        }

        public ExpressionAccessor(FieldInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            this.Name = info.Name;
            this.DelaringType = info.DeclaringType;
            this.getValue = CreateGetValue(DelaringType, Name);
            this.setValue = (!info.IsInitOnly) ? CreateSetValue(DelaringType, Name) : null;
        }

        public object GetValue(object target)
        {
            if (!IsReadable) throw new InvalidOperationException("is not readable member");

            return getValue(target);
        }

        public void SetValue(object target, object value)
        {
            if (!IsWritable) throw new InvalidOperationException("is not writable member");

            setValue(target, value);
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