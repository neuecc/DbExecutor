using System;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Infrastructure
{
    /// <summary>Represents PropertyInfo delegate.</summary>
    internal class PropertyAccessor<TTarget, TProperty> : IPropertyAccessor
    {
        readonly Type type;
        readonly string name;
        readonly Func<TTarget, TProperty> getter;
        readonly Action<TTarget, TProperty> setter;

        public PropertyAccessor(Type declaringType, string name, Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(declaringType != null);

            this.type = declaringType;
            this.name = name;
            this.getter = getter;
            this.setter = setter;
        }

        public string Name
        {
            get { return name; }
        }

        public Type DeclaringType
        {
            get { return type; }
        }

        public object GetValue(object target)
        {
            if (getter == null) throw new InvalidOperationException("not readable");

            return this.getter((TTarget)target);
        }

        public void SetValue(object target, object value)
        {
            if (setter == null) throw new InvalidOperationException("not writable");

            this.setter((TTarget)target, (TProperty)value);
        }

        public bool IsReadable
        {
            get { return getter != null; }
        }

        public bool IsWritable
        {
            get { return setter != null; }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!String.IsNullOrEmpty(name));
            Contract.Invariant(type != null);
        }
    }
}