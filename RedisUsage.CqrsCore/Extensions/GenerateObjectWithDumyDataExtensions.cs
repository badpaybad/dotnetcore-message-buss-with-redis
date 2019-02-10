using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RedisUsage.CqrsCore.Extensions
{
    public static class GenerateObjectWithDumyDataExtensions
    {
        static Random _rnd = new Random();
        static ConcurrentDictionary<string, int> _mapped = new ConcurrentDictionary<string, int>();

        static Type _currentInstanceType;

        public static T GenerateData<T>() where T : class, new()
        {
            var instanceType = typeof(T);
            var instance = new T();

            _mapped.Clear();

            _currentInstanceType = instanceType;

            var instanceProperties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            foreach (var pi in instanceProperties)
            {
                if (IsIgnoreAttribute(pi)) continue;

                var val = BuildDumyDataForProp(pi.PropertyType, instance);
                if (pi.GetSetMethod() != null)
                {
                    if (val == null) { val = Activator.CreateInstance(pi.PropertyType); }

                    pi.SetValue(instance, val);
                }
            }

            return instance;
        }

        static object BuildInstance(Type instanceType, object objOrigin)
        {
            var instance = Activator.CreateInstance(instanceType);
            var key = objOrigin.GetType() + "__" + instanceType;

            if (_mapped.ContainsKey(key))
            {
                if (_mapped[key] > 0)
                {
                    return null;
                }

                _mapped[key] = _mapped[key] + 1;
            }
            else
            {
                _mapped[key] = 0;
            }

            var instanceProperties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            foreach (var pi in instanceProperties)
            {
                if (IsIgnoreAttribute(pi)) continue;

                var val = BuildDumyDataForProp(pi.PropertyType, instance);
                if (val != null)
                {
                    if (pi.GetSetMethod() != null)
                    {
                        if (val == null) { val = Activator.CreateInstance(pi.PropertyType); }

                        pi.SetValue(instance, val);
                    }
                }               
            }

            return instance;
        }

        static bool IsIgnoreAttribute(PropertyInfo pi)
        {
            var piType = pi.PropertyType;

            object[] attrs = pi.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                Newtonsoft.Json.JsonIgnoreAttribute jsonIgnoreAttr = attr as Newtonsoft.Json.JsonIgnoreAttribute;
                if (jsonIgnoreAttr != null)
                {
                    return true;
                }

                System.Xml.Serialization.XmlIgnoreAttribute xmlIgnore = attr as System.Xml.Serialization.XmlIgnoreAttribute;
                if (xmlIgnore != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static object BuildDumyDataForProp(Type piType, object objOrigin)
        {
            if (piType == null || piType.IsAbstract || piType.IsInterface)
            {
                return null;
            }
            var enumNullType = Nullable.GetUnderlyingType(piType);

            if (piType.IsEnum || (enumNullType != null && enumNullType.IsEnum))
            {
                foreach (var e in Enum.GetValues(piType))
                {
                    return e;
                }
            }

            if (piType == typeof(string) || piType == typeof(String))
            {
                return "String_" + _rnd.Next();
            }
            if (piType == typeof(char) || piType == typeof(Char))
            {
                return (char)_rnd.Next(1, 128);
            }
            if (piType == typeof(Int16) || piType == typeof(Int16?))
            {
                return (Int16)_rnd.Next(1, 128);
            }
            if (piType == typeof(Int32) || piType == typeof(Int32?))
            {
                return _rnd.Next(1, 128);
            }
            if (piType == typeof(Int64) || piType == typeof(Int64?))
            {
                return (Int64)_rnd.Next(1, 128);
            }
            if (piType == typeof(DateTime) || piType == typeof(DateTime?))
            {
                return DateTime.Now;
            }
            if (piType == typeof(float) || piType == typeof(float?))
            {
                return (float)_rnd.Next(1, 128);
            }
            if (piType == typeof(double) || piType == typeof(double?))
            {
                return _rnd.NextDouble();
            }
            if (piType == typeof(decimal) || piType == typeof(decimal?))
            {
                return (decimal)_rnd.Next(1, 128);
            }

            if (piType == typeof(Guid) || piType == typeof(Guid?))
            {
                return Guid.NewGuid();
            }

            if (piType == typeof(bool) || piType == typeof(bool?))
            {
                return _rnd.Next(0, 1) == 0;
            }

            if (piType.IsArray)
            {
                ArrayList arr = new ArrayList();
                var elementType = piType.GetElementType();

                var item = BuildDumyDataForProp(elementType, objOrigin);
                arr.Add(item);

                return arr.ToArray(elementType);
            }

            if (typeof(IEnumerable).IsAssignableFrom(piType))
            {
                var elementType = piType.GenericTypeArguments[0];
                 
                if (typeof(IList).IsAssignableFrom(piType))
                {
                    var list = (IList)Activator.CreateInstance(piType);
                    list.Add(Activator.CreateInstance(elementType));
                    return list;
                }
            }          

            if (piType.IsClass)
            {
                var proInstance = BuildInstance(piType, objOrigin);
                return proInstance;
            }

            return null;
        }

        static IEnumerable GenerateItem(Type elementType)
        {
            yield return Activator.CreateInstance(elementType);
        }
    }
}
