using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Codeplex.Data.Internal
{
    /// <summary>Delegate accessor created from expression tree.</summary>
    internal class CompiledAccessor : IMemberAccessor
    {
        public Type DeclaringType { get; private set; }
        [Obsolete("use MemberName instead of Name. This is only for monitoring")]
        public string Name { get; private set; }
        public string MemberName { get; private set; }
        public bool IsIgnoreSerialize { get; private set; }
        public bool IsDataContractedType { get; private set; }
        public bool IsDataContractedMember { get; private set; }
        public bool IsReadable { get { return GetValueDirect != null; } }
        public bool IsWritable { get { return SetValueDirect != null; } }

        // for performance optimization
        public readonly Func<object, object> GetValueDirect;
        public readonly Action<object, object> SetValueDirect;

        public CompiledAccessor(PropertyInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            InitializeFieldByDataContract(info);
            if (!IsIgnoreSerialize)
            {
                var allowNonPublic = this.IsDataContractedType;
                this.GetValueDirect = (info.GetGetMethod(allowNonPublic) != null) ? CreateGetValue(DeclaringType, MemberName) : null;
                this.SetValueDirect = (info.GetSetMethod(allowNonPublic) != null) ? CreateSetValue(DeclaringType, MemberName) : null;
            }
        }

        public CompiledAccessor(FieldInfo info)
        {
            Contract.Requires<ArgumentNullException>(info != null);

            InitializeFieldByDataContract(info);
            if (!IsIgnoreSerialize)
            {
                this.GetValueDirect = CreateGetValue(DeclaringType, MemberName);
                this.SetValueDirect = (!info.IsInitOnly || this.IsDataContractedType) ? CreateSetValue(DeclaringType, MemberName) : null;
            }
        }

        /// <summary>
        /// set DeclaringType, IsIgnoreSerialize, IsDataContractedType, IsDataContractedMember, Name and MemberName
        /// </summary>
        protected void InitializeFieldByDataContract(MemberInfo info)
        {
#pragma warning disable 612, 618

            this.DeclaringType = info.DeclaringType;
            this.Name = this.MemberName = info.Name;
            this.IsIgnoreSerialize = info.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false).Any();

            this.IsDataContractedType = info.DeclaringType.GetCustomAttributes(typeof(DataContractAttribute), false).Any();
            if (this.IsDataContractedType)
            {
                var dataMember = info.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
                if (dataMember != null)
                {
                    this.IsDataContractedMember = true;
                    this.MemberName = dataMember.Name ?? this.Name;
                }
                else
                {
                    this.IsIgnoreSerialize = true;
                }
            }

#pragma warning restore 612, 618
        }

        [Obsolete("use GetValueDirect instead of GetValue for the better performance.")]
        public object GetValue(object target)
        {
            return GetValueDirect(target);
        }

        [Obsolete("use SetValueDirect instead of SetValue for the better performance.")]
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