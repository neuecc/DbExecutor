using System;
using System.Diagnostics.Contracts;

namespace Codeplex.Data.Internal
{
    [ContractClass(typeof(IMemberAccessorContract))]
    internal partial interface IMemberAccessor
    {
        string Name { get; }
        Type DelaringType { get; }
        bool IsReadable { get; }
        bool IsWritable { get; }

        object GetValue(object target);
        void SetValue(object target, object value);
    }

    [ContractClassFor(typeof(IMemberAccessor))]
    internal abstract class IMemberAccessorContract : IMemberAccessor
    {
        public string Name
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                throw new NotImplementedException();
            }
        }

        public Type DelaringType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                throw new NotImplementedException();
            }
        }

        public bool IsReadable
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWritable
        {
            get { throw new NotImplementedException(); }
        }

        public object GetValue(object target)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            throw new NotImplementedException();
        }

        public void SetValue(object target, object value)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            throw new NotImplementedException();
        }
    }
}