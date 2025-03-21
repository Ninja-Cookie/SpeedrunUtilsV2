using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpeedrunUtilsV2
{
    internal static class EasyReflection
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
        
        private readonly static Dictionary<int, (MemberInfo, object[])> Members = new Dictionary<int, (MemberInfo, object[])>();

        private const string EXCEPTION_BadMember        = "Type \"{0}\" does not contain matching member \"{1}\" of the given parameters.";
        private const string EXCEPTION_BadType          = "The return type \"{0}\" is not assignable from \"{1}\" in \"{2}\".";
        private const string EXCEPTION_BadMemberType    = "The member \"{0}\" in \"{1}\" is not a Field type or assignable Property.";
        private const string EXCEPTION_BadInvokeType    = "The member \"{0}\" in \"{1}\" is not a Method type.";

        /// <summary>
        /// Retrieves the value of a specified member (including methods) from an object.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="type">The object instance from which to retrieve the member value.</param>
        /// <param name="member">The name of the member to retrieve.</param>
        /// <param name="memberParams">Optional or required parameters for the member.</param>
        /// <returns>The value of the specified member cast to type <typeparamref name="T"/>.</returns>
        internal static T GetValue<T>(this object obj, string member, params object[] memberParams)
        {
            MemberInfo memberInfo = obj.GetMember(member, ref memberParams);

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Method:
                    if (typeof(T).IsAssignableFrom((memberInfo as MethodInfo).ReturnType))
                        return (T)(memberInfo as MethodInfo).Invoke(obj, memberParams);
                break;

                case MemberTypes.Field:
                    if (typeof(T).IsAssignableFrom((memberInfo as FieldInfo).FieldType))
                        return (T)(memberInfo as FieldInfo).GetValue(obj);
                break;

                case MemberTypes.Property:
                    if (typeof(T).IsAssignableFrom((memberInfo as PropertyInfo).PropertyType))
                        return (T)(memberInfo as PropertyInfo).GetValue(obj, null);
                break;

                case MemberTypes.NestedType:
                    if (typeof(T).IsAssignableFrom(typeof(TypeInfo)))
                        return (T)(object)(memberInfo as TypeInfo);
                break;
            }
            throw new Exception(string.Format(EXCEPTION_BadType, typeof(T), member, obj.GetType()));
        }

        /// <summary>
        /// Retrieves the nested type information of a specified member from an object.
        /// </summary>
        /// <param name="obj">The object instance from which to retrieve the nested type.</param>
        /// <param name="member">The name of the member to retrieve.</param>
        /// <returns>The nested type information of the specified member.</returns>
        internal static TypeInfo GetNestedType(this object obj, string member)
        {
            return obj.GetValue<TypeInfo>(member);
        }

        /// <summary>
        /// Sets the value of a specified field or property member on an object.
        /// </summary>
        /// <param name="obj">The object instance on which to set the member value.</param>
        /// <param name="member">The name of the member to set.</param>
        /// <param name="value">The value to set to the member.</param>
        internal static void SetValue(this object obj, string member, object value)
        {
            object[] emptyArray = Array.Empty<object>();
            MemberInfo memberInfo = obj.GetMember(member, ref emptyArray);

            if (memberInfo.MemberType == MemberTypes.Field)
                (memberInfo as FieldInfo).SetValue(obj, value);
            else if (memberInfo.MemberType == MemberTypes.Property && (memberInfo as PropertyInfo).SetMethod != null)
                (memberInfo as PropertyInfo).SetValue(obj, value);
            else
                throw new Exception(string.Format(EXCEPTION_BadMemberType, member, obj.GetType()));
        }

        /// <summary>
        /// Invokes a specified method on an object with the given parameters.
        /// </summary>
        /// <param name="obj">The object instance on which to invoke the method.</param>
        /// <param name="member">The name of the method to invoke.</param>
        /// <param name="memberParams">The parameters to pass to the method.</param>
        internal static void InvokeMethod(this object obj, string member, params object[] memberParams)
        {
            MemberInfo memberInfo = obj.GetMember(member, ref memberParams);
            if (memberInfo.MemberType == MemberTypes.Method)
                (memberInfo as MethodInfo).Invoke(obj, memberParams);
            else
                throw new Exception(string.Format(EXCEPTION_BadInvokeType, member, obj.GetType()));
        }

        /// <summary>
        /// Retrieves the <see cref="MemberInfo"/> of a specified member from an object, using the provided parameters, if any.
        /// </summary>
        /// <param name="obj">The object instance from which to retrieve the member information.</param>
        /// <param name="member">The name of the member to retrieve.</param>
        /// <param name="memberParams">Optional or required parameters for the member.</param>
        /// <returns>The <see cref="MemberInfo"/> of the specified member.</returns>
        internal static MemberInfo GetMember(this object obj, string member, params object[] memberParams)
        {
            return GetMember(obj, member, ref memberParams);
        }

        private static MemberInfo GetMember(this object obj, string member, ref object[] memberParams)
        {
            int hash = (obj.GetType(), member, HashedObjects(memberParams)).GetHashCode();
            if (Members.TryGetValue(hash, out var value))
            {
                memberParams = value.Item2;
                return value.Item1;
            }

            MemberInfo memberInfo = GetMemberFinal(obj, member, ref memberParams);
            if (memberInfo == null)
                throw new Exception(string.Format(EXCEPTION_BadMember, obj.GetType(), member));

            Members.Add(hash, (memberInfo, memberParams));
            return memberInfo;
        }

        private static int HashedObjects(object[] memberParams)
        {
            unchecked
            {
                int hash = 17;
                foreach (var obj in memberParams)
                    hash = hash * 31 + (obj?.GetHashCode() ?? 0);
                return hash;
            }
        }

        private static MemberInfo GetMemberFinal(this object obj, string member, ref object[] memberParams)
        {
            MemberInfo[] members = obj.GetType().GetMember(member, Flags);

            if (members.Length == 0)
                return null;

            if (members.Length == 1)
                return members.First();

            int paramLength = memberParams.Length;
            MethodInfo[] methods = members.Where(x => x.MemberType == MemberTypes.Method && (x as MethodInfo).GetParameters().Length >= paramLength).Cast<MethodInfo>().ToArray();

            if (methods.Length == 1)
                return methods.First();

            List<object> memberParamsFianl = new List<object>();
            memberParamsFianl.AddRange(memberParams);

            foreach (var method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                bool isMatch = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    bool flag = memberParams.Length - 1 >= i;
                    if (!flag && parameters[i].HasDefaultValue)
                    {
                        memberParamsFianl.Add(parameters[i].DefaultValue);
                    }
                    else if (!(flag && memberParams[i].GetType() == parameters[i].ParameterType))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    memberParams = memberParamsFianl.ToArray();
                    return method;
                }
            }
            return null;
        }
    }
}
