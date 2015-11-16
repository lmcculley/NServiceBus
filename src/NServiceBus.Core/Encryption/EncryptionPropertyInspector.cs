namespace NServiceBus.Encryption
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Logging;
    using Utils.Reflection;

    class EncryptionPropertyInspector
    {
        Conventions conventions;

        public EncryptionPropertyInspector(Conventions conventions)
        {
            this.conventions = conventions;
        }


        static bool IsIndexedProperty(MemberInfo member)
        {
            var propertyInfo = member as PropertyInfo;

            return propertyInfo?.GetIndexParameters().Length > 0;
        }

        bool IsEncryptedMember(MemberInfo arg)
        {
            var propertyInfo = arg as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    if (conventions.IsEncryptedProperty(propertyInfo))
                    {
                        throw new Exception("Cannot encrypt or decrypt indexed properties that return a WireEncryptedString.");
                    }

                    return false;
                }

                return conventions.IsEncryptedProperty(propertyInfo);
            }

            var fieldInfo = arg as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType == typeof(WireEncryptedString);
            }

            return false;
        }

        public void ForEachMember(object root, Action<object, MemberInfo> action)
        {
            if (root == null || visitedMembers.Contains(root))
            {
                return;
            }

            visitedMembers.Add(root);

            var members = GetFieldsAndProperties(root);

            foreach (var member in members)
            {
                if (IsEncryptedMember(member))
                {
                    action(root, member);
                }

                //don't recurse over primitives or system types
                if (member.ReflectedType.IsPrimitive || member.ReflectedType.IsSystemType())
                {
                    continue;
                }

                // don't try to recurse over members of WireEncryptedString
                if (member.DeclaringType == typeof(WireEncryptedString))
                {
                    continue;
                }

                if (IsIndexedProperty(member))
                {
                    continue;
                }

                var child = member.GetValue(root);

                var items = child as IEnumerable;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        //don't recurse over primitives or system types
                        if (item.GetType().IsPrimitive || item.GetType().IsSystemType())
                        {
                            break;
                        }

                        ForEachMember(item, action);
                    }
                }
                else
                {
                    ForEachMember(child, action);
                }
            }
        }


        static IEnumerable<MemberInfo> GetFieldsAndProperties(object target)
        {
            if (target == null)
            {
                return new List<MemberInfo>();
            }

            var messageType = target.GetType();

            IEnumerable<MemberInfo> members;
            if (!cache.TryGetValue(messageType.TypeHandle, out members))
            {
                cache[messageType.TypeHandle] = members = messageType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m =>
                    {
                        var fieldInfo = m as FieldInfo;
                        if (fieldInfo != null)
                        {
                            return !fieldInfo.IsInitOnly;
                        }

                        var propInfo = m as PropertyInfo;
                        if (propInfo != null)
                        {
                            return propInfo.CanWrite;
                        }

                        return false;
                    })
                    .ToList();
            }

            return members;
        }

        HashSet<object> visitedMembers = new HashSet<object>();

        static ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<MemberInfo>> cache = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<MemberInfo>>();

        static ILog Log = LogManager.GetLogger<IEncryptionService>();
    }
}
