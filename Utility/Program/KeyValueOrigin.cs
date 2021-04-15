using System;
using System.Collections.Generic;
using System.Linq;

namespace QueZed.Utility.Program {
    using System.Collections.Specialized;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;

    public static partial class TypeExtensions {
        public static T IsNullThrow<T>(this T t, string name = "") where T : class {
            if (null == t) throw new ArgumentNullException(name);
            return t;
        }
    }

    public class KeyValueOrigin {
        public enum Source { none, MachineConfig, AppConfig, CommandLine, Code, Database, Environment }
        private string key = null;
        private string value = null;
        private Source origin = Source.none;
        public KeyValueOrigin(KeyValuePair<string, string> kvp, Source origin) : this(kvp.Key, kvp.Value, origin) { }
        public KeyValueOrigin(string key, string value, Source origin) {
            key.IsNullThrow(nameof(key));
            value.IsNullThrow(nameof(value));
            if (Source.none == origin) throw new ArgumentException($"origin cannot have value of {nameof(origin)}");
            this.key = key;
            this.value = value;
            this.origin = origin;
        }
        public string Key { get { return key; } }
        public string Value { get { return value; } }
        public Source Origin { get { return origin; } }
    }

    public class KeyValueOriginCollection {
        Dictionary<string, KeyValueOrigin> dictionary = null;
        public KeyValueOriginCollection() { dictionary = new Dictionary<string, KeyValueOrigin>(); }

        public void Add(KeyValueOrigin nvo) {
            nvo.IsNullThrow(nameof(nvo));
            dictionary.Add(nvo.Key, nvo);
        }
        public void Add(string key, string value, KeyValueOrigin.Source origin, bool replace = false) { if (replace) dictionary[key] = new KeyValueOrigin(key, value, origin); else dictionary.Add(key, new KeyValueOrigin(key, value, origin)); }
        public void Add(NameValueCollection nvc, KeyValueOrigin.Source origin) {
            nvc.IsNullThrow(nameof(nvc));
            for (int i = 0; i < nvc.Count; ++i) Add(nvc.GetKey(i), nvc.Get(i), origin);
        }
        public bool Exists(string key) { return dictionary.ContainsKey(key); }
        public string this[string key] { get { return dictionary[key].Value; } }

        /// <summary>
        /// Return value for key cast to type T
        /// If key doesn't exist return default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetAsDefault<T>(string key, T defaultValue = default(T)) {
            if (!Exists(key)) return defaultValue;
            return getAs<T>(key);
        }

        /// <summary>
        /// Return value for key cast to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetAs<T>(string key) {
            if (!Exists(key)) throw new ArgumentException($"No value for key:{key}.");
            return getAs<T>(key);
        }

        private T getAs<T>(string key) {
            Type type = typeof(T);
            if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(this[key], typeof(T), null);
            MethodInfo tryParseMethod = type.GetMethod("TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() });
            if (null != tryParseMethod) {
                object[] parameters = new object[] { this[key], null };
                if ((bool)tryParseMethod.Invoke(type, parameters)) return (T)parameters[1];
                else throw new ArgumentException($"Cannot convert value for key:{key} to Type:{type.Name}.");
            }
            MethodInfo parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) });
            if (null != parseMethod) return (T)parseMethod.Invoke(null, new object[] { this[key] });
            throw new ArgumentException($"No conversion to Type:{type.Name}.");
        }

        public ReadOnlyCollection<KeyValueOrigin> Settings {
            get {
                return new ReadOnlyCollection<KeyValueOrigin>(dictionary.Values.ToList());
            }
        }

        public IConfiguration AsConfiguration {
            get {
                return new ConfigurationBuilder().AddInMemoryCollection(dictionary.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Value))).Build();
            }
        }
    }
}
