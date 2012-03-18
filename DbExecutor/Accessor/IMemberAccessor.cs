using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Codeplex.Data.Internal
{
    [ContractClass(typeof(IMemberAccessorContract))]
    internal interface IMemberAccessor
    {
        string Name { get; }
        string MemberName { get; }
        Type DeclaringType { get; }
        bool IsDataContractedType { get; }
        bool IsDataContractedMember { get; }
        bool IsIgnoreSerialize { get; }
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
                return default(string);
            }
        }

        public string MemberName
        {
            get
            {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return default(string);
            }
        }

        public Type DeclaringType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return default(Type);
            }
        }

        public bool IsIgnoreSerialize
        {
            get { return default(bool); }
        }


        public bool IsDataContractedType
        {
            get { return default(bool); }
        }

        public bool IsDataContractedMember
        {
            get { return default(bool); }
        }

        public bool IsReadable
        {
            get { return default(bool); }
        }

        public bool IsWritable
        {
            get { return default(bool); }
        }

        public object GetValue(object target)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<InvalidOperationException>(IsReadable, "is not readable member");
            return default(object);
        }

        public void SetValue(object target, object value)
        {
            Contract.Requires<ArgumentNullException>(target != null);
            Contract.Requires<InvalidOperationException>(IsWritable, "is not writable member");
        }
    }
}