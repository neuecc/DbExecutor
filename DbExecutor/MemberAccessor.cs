using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq.Expressions;

namespace Codeplex.Data.Infrastructure
{
    // define ref delegate
    public delegate void ActionRef<T1, T2>(ref T1 t1, T2 t2);
    public delegate TR FuncRef<T1, TR>(ref T1 t1);

    public class MemberAccessor
    {
        public Type DelaringType { get; private set; }
        public string Name { get; private set; }
        public bool IsReadable { get { return getValue != null; } }
        public bool IsWritable { get { return setValue != null; } }

        readonly FuncRef<object, object> getValue;
        readonly ActionRef<object, object> setValue;

        public MemberAccessor(PropertyInfo info)
        {
            Contract.Requires(info != null);

            this.Name = info.Name;
            this.DelaringType = info.DeclaringType;
            this.getValue = info.CanRead ? CreateGetValue(DelaringType, Name) : null;
            this.setValue = info.CanWrite ? CreateSetValue(DelaringType, Name) : null;
        }

        public MemberAccessor(FieldInfo info)
        {
            Contract.Requires(info != null);

            this.Name = info.Name;
            this.DelaringType = info.DeclaringType;
            this.getValue = CreateGetValue(DelaringType, Name);
            this.setValue = CreateSetValue(DelaringType, Name);
        }

        public object GetValue(ref object target)
        {
            Contract.Requires(target != null);
            if (!IsReadable) throw new InvalidOperationException("is not readable member");

            return getValue(ref target);
        }

        public void SetValue(ref object target, object value)
        {
            Contract.Requires(target != null);
            if (!IsWritable) throw new InvalidOperationException("is not writable member");

            setValue(ref target, value);
        }

        // (ref object x) => (object)((T)x).name
        [ContractVerification(false)]
        static FuncRef<object, object> CreateGetValue(Type type, string name)
        {
            var x = Expression.Parameter(typeof(object).MakeByRefType(), "x");

            var func = Expression.Lambda<FuncRef<object, object>>(
                Expression.Convert(
                    Expression.PropertyOrField(
                        (type.IsValueType ? Expression.Unbox(x, type) : Expression.Convert(x, type)),
                        name),
                    typeof(object)),
                x);

            return func.Compile();
        }

        // (ref object x, object v) => ((T)x).name = (U)v
        [ContractVerification(false)]
        static ActionRef<object, object> CreateSetValue(Type type, string name)
        {
            var x = Expression.Parameter(typeof(object).MakeByRefType(), "x");
            var v = Expression.Parameter(typeof(object), "v");

            var left = Expression.PropertyOrField(
                (type.IsValueType ? Expression.Unbox(x, type) : Expression.Convert(x, type)),
                name);
            var right = Expression.Convert(v, left.Type);

            var action = Expression.Lambda<ActionRef<object, object>>(
                Expression.Assign(left, right),
                x, v);

            return action.Compile();
        }
    }
}