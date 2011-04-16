using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Codeplex.Data
{
    #region IPropertyAccessor contract binding
    /// <summary>Represents PropertyInfo delegate.</summary>
    [ContractClass(typeof(IPropertyAccessorContract))]
    internal partial interface IPropertyAccessor
    {
        string Name { get; }
        object GetValue(object target);
        void SetValue(object target, object value);
    }

    [ContractClassFor(typeof(IPropertyAccessor))]
    abstract class IPropertyAccessorContract : IPropertyAccessor
    {
        [Pure]
        public string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                throw new NotImplementedException();
            }
        }

        public object GetValue(object target)
        {
            throw new NotImplementedException();
        }

        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    /// <summary>Represents PropertyInfo delegate.</summary>
    internal class PropertyAccessor<TTarget, TProperty> : IPropertyAccessor
    {
        readonly string name;
        readonly Func<TTarget, TProperty> getter;
        readonly Action<TTarget, TProperty> setter;

        public PropertyAccessor(string name, Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");
            Contract.EndContractBlock();

            this.name = name;
            this.getter = getter;
            this.setter = setter;
        }

        public string Name
        {
            get { return name; }
        }

        public object GetValue(object target)
        {
            return this.getter((TTarget)target);
        }

        public void SetValue(object target, object value)
        {
            this.setter((TTarget)target, (TProperty)value);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!String.IsNullOrEmpty(name));
        }
    }
}