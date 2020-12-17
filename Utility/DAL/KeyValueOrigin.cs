using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Utility {
   using System.Collections.Specialized;
   using System.Reflection;

   public class KeyValueOrigin {
      public enum Origins { _None, AppConfig, CommandLine, Code, Database }
      private string key = null;
      private string value = null;
      private Origins origin = Origins._None;
      public KeyValueOrigin(KeyValuePair<string, string> kvp, Origins origin) : this(kvp.Key, kvp.Value, origin) { }
      public KeyValueOrigin(string key, string value, Origins origin) {
         if (null == key) throw new ArgumentNullException("key");
         if (null == value) throw new ArgumentNullException("value");
         if (Origins._None == origin) throw new ArgumentException("origin cannot have value of '_None'", "origin");
         this.key = key;
         this.value = value;
         this.origin = origin;
      }
      public string Key { get { return key; } }
      public string Value { get { return value; } }
      public Origins Origin { get { return origin; } }
   }

   public class KeyValueOriginCollection {
      Dictionary<string, KeyValueOrigin> dictionary = null;
      public KeyValueOriginCollection() { dictionary = new Dictionary<string, KeyValueOrigin>(); }

      public void Add(KeyValueOrigin nvo) { dictionary.Add(nvo.Key, nvo); }
      public void Add(string key, string value, KeyValueOrigin.Origins origin, bool replace) { if (replace) dictionary[key] = new KeyValueOrigin(key, value, origin); else dictionary.Add(key, new KeyValueOrigin(key, value, origin)); }
      public void Add(string key, string value, KeyValueOrigin.Origins origin) { dictionary.Add(key, new KeyValueOrigin(key, value, origin)); }
      public void Add(NameValueCollection nvc, KeyValueOrigin.Origins origin) { for (int i = 0; i < nvc.Count; ++i) Add(nvc.GetKey(i), nvc.Get(i), origin); }
      public bool Exists(string key) { return dictionary.ContainsKey(key); }
      public string this[string key] { get { return dictionary[key].Value; } }

      public T GetAs<T>(string key) {
         if (!Exists(key)) throw new ArgumentException(string.Format("No value for key:{0}.", key));
         Type type = typeof(T);
         MethodInfo tryParseMethod = type.GetMethod("TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() });
         if (null != tryParseMethod) {
            object[] parameters = new object[] { this[key], null };
            if ((bool)tryParseMethod.Invoke(type, parameters)) return (T)parameters[1];
            else throw new ArgumentException(string.Format("Cannot convert value for key:{0} to Type:{1}.", key, type.Name));
         }
         MethodInfo parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) });
         if (null != parseMethod) return (T)parseMethod.Invoke(null, new object[] { this[key] });
         throw new ArgumentException(string.Format("No conversion to Type:{0}.", type.Name));
      }

      public KeyValueOrigin[] Settings {
         get {
            KeyValueOrigin[] settings = new KeyValueOrigin[dictionary.Values.Count];
            dictionary.Values.CopyTo(settings, 0);
            return settings;
         }
      }
   }
}
