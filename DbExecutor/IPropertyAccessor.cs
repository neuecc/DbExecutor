using System;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Infrastructure
{
    /// <summary>Represents PropertyInfo delegate.</summary>
    [ContractClass(typeof(IPropertyAccessorContract))]
    internal partial interface IPropertyAccessor
    {
        string Name { get; }
        object GetValue(object target);
        void SetValue(object target, object value);
        bool IsReadable { get; }
        bool IsWritable { get; }
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

        public bool IsReadable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWritable
        {
            get { throw new NotImplementedException(); }
        }
    }
}