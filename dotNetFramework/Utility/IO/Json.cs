using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.IO;
	using System.Collections.Generic;
	using System.Reflection;

	using System.Web.Http.ModelBinding;
	using System.Web.Http.ModelBinding.Binders;
	using System.Web.Http.Controllers;
	using System.Web.Http.ValueProviders;

	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Formatting;

	using System.Drawing;
	using System.Drawing.Printing;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using Newtonsoft.Json.Converters;
	using Newtonsoft.Json.Utilities;
	using Newtonsoft.Json.Linq;

	public class ConverterContractResolver : DefaultContractResolver {
		public static readonly ConverterContractResolver Instance = new ConverterContractResolver();
		protected override JsonContract CreateContract(Type objectType) {
			if (typeof(Point) == objectType) return base.CreateObjectContract(objectType);
			if (typeof(Rectangle) == objectType) return base.CreateObjectContract(objectType);
			return base.CreateContract(objectType);
		}
	}

	public class QueZedJson {
		static List<JsonConverter> converters = new List<JsonConverter>() {
			// This hopefully brings consistency and sanity to how these types are serialized and de-serialized
			new PointConverter(),
			new PointFConverter(),
			new RectangleConverter(),
			new RectangleFConverter(),
			new KeyValuePairConverter()
		};
		static JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter() {
			SerializerSettings = new JsonSerializerSettings() {
				Converters = converters,
				ContractResolver = new ConverterContractResolver()
			},
		};

		public static JsonConverter[] Converters { get { return converters.ToArray(); } }
		public static MediaTypeFormatter[] Formatters { get { return new MediaTypeFormatter[] { formatter }; } } 
		public static JsonSerializerSettings Settings {
			get {
				return new JsonSerializerSettings {
					DateTimeZoneHandling = DateTimeZoneHandling.Utc,
					Converters = converters
				};
			}
		}
	}

	public abstract class JsonConverter<T> : JsonConverter {
		protected abstract T Create(Type objectType, JObject jObject);
		public override bool CanConvert(Type objectType) { return typeof(T).IsAssignableFrom(objectType); }
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			JObject jObject = JObject.Load(reader);
			T target = Create(objectType, jObject);
			// This doesn't appear to be needed since in all the cases I've seen target is populated at this point
			// serializer.Populate(jObject.CreateReader(), target);
			return target;
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { throw new NotImplementedException(); }
	}

	public class PointConverter : JsonConverter<Point> {
		// we don't want to allow this type (Point) to convert to string
		// public override bool CanConvert(Type objectType) { return typeof(string) == objectType ? false : base.CanConvert(objectType); }
		protected override Point Create(Type objectType, JObject jObject) { return new Point((int)jObject["X"], (int)jObject["Y"]); }
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			Point point = (Point)value;
			serializer.Serialize(writer, new JObject { { "X", point.X }, { "Y", point.Y } });
		}
	}

	public class PointFConverter : JsonConverter<PointF> {
		protected override PointF Create(Type objectType, JObject jObject) { return new PointF((float)jObject["X"], (float)jObject["Y"]); }
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			PointF pointf = (PointF)value;
			serializer.Serialize(writer, new JObject { { "X", pointf.X }, { "Y", pointf.Y } });
		}
	}

	public class RectangleConverter : JsonConverter<Rectangle> {
		protected override Rectangle Create(Type objectType, JObject jObject) { return new Rectangle((int)jObject["X"], (int)jObject["Y"], (int)jObject["Width"], (int)jObject["Height"]); }
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			Rectangle rectangle = (Rectangle)value;
			serializer.Serialize(writer, new JObject { { "X", rectangle.X }, { "Y", rectangle.Y }, { "Width", rectangle.Width }, { "Height", rectangle.Height } });
		}
	}

	public class RectangleFConverter : JsonConverter<RectangleF> {
		protected override RectangleF Create(Type objectType, JObject jObject) { return new RectangleF((float)jObject["X"], (float)jObject["Y"], (float)jObject["Width"], (float)jObject["Height"]); }
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			RectangleF rectanglef = (RectangleF)value;
			serializer.Serialize(writer, new JObject { { "X", rectanglef.X }, { "Y", rectanglef.Y }, { "Width", rectanglef.Width }, { "Height", rectanglef.Height } });
		}
	}

	//public class KeyValuePairConverter : JsonConverter<KeyValuePair<string, string>> {
	//	protected override KeyValuePair<string, string> Create(Type objectType, JObject jObject) { return new KeyValuePair<string, string>(jObject["Key"].ToString(), jObject["Value"].ToString()); }
	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
	//		KeyValuePair<string, string> kvp = (KeyValuePair<string, string>)value;
	//		serializer.Serialize(writer, new JObject { { "Key", kvp.Key }, { "Value", kvp.Value } });
	//	}
	//}

#region save

	//		public static MediaTypeFormatter[] Formatters {
	//			get {
	//				JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
	//// Making ajax calls from jscript error when json setting are set this way.
	//// There may be a better way to fix this... but just turning it off for now makes things much better.
	////				formatter.SerializerSettings = Defaults;
	////				formatter.SerializerSettings.Converters.Add(new JavaScriptDateTimeConverter());
	//				formatter.SerializerSettings.Converters = converters;
	//				return new MediaTypeFormatter[] { formatter };
	//			}
	//		}

	//public class JsonModelBinder<T> : IModelBinder {
	//	public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext) {
	//		if (bindingContext.ModelType != typeof(T)) return false;
	//		ValueProviderResult vpr = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
	//		if (null != vpr) { bindingContext.Model = JsonConvert.DeserializeObject<T>(vpr.AttemptedValue); return true; }
	//		bindingContext.ModelState.AddModelError(bindingContext.ModelName, string.Format("Cannot convert value to {0}.", bindingContext.ModelName));
	//		return false;
	//	}
	//}

	//	public class ConverterContractResolver : DefaultContractResolver {
	//		public static readonly ConverterContractResolver Instance = new ConverterContractResolver();
	//		protected override JsonContract CreateContract(Type objectType) {
	//			JsonContract contract = base.CreateContract(objectType);
	//			// this will only be called once and then cached

	//			// Convert these types using the JavaScriptDateTimeConverter.
	//			// I don't like the format particularly, but it doesn't include a '+' in the format that was causing difficulties.
	//			// As long as we're the same on both sides all should be well.
	////			if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset)) contract.Converter = new JavaScriptDateTimeConverter();
	//			if (objectType == typeof(Point) || objectType == typeof(Rectangle)) contract = base.CreateObjectContract(objectType);
	//			return contract;
	//		}

	//		public override JsonContract ResolveContract(Type type) {
	//			return base.ResolveContract(type);
	//		}
	//	}

	//// This class is used to replace the DataContractJsonSerializer and use Newtonsoft Json serializer
	//public class JsonNetFormatter : MediaTypeFormatter {
	//	public JsonNetFormatter() { SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")); }
	//	public override bool CanWriteType(Type type) {
	//		// don't serialize JsonValue structure use default for that
	//		//if (type == typeof(JsonValue) || type == typeof(JsonObject) || type == typeof(JsonArray)) return false;
	//		return true;
	//	}
	//	public override bool CanReadType(Type type) {
	//		//if (type == typeof(IKeyValueModel)) return false;
	//		return true;
	//	}

	//	public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger) {
	//		var task = Task<object>.Factory.StartNew(() => {
	//			JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
	//			StreamReader sr = new StreamReader(readStream);
	//			JsonTextReader jreader = new JsonTextReader(sr);
	//			JsonSerializer ser = new JsonSerializer();
	//			ser.Converters.Add(new IsoDateTimeConverter());
	//			object val = ser.Deserialize(jreader, type);
	//			return val;
	//		});
	//		return task;
	//	}

	//	public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext) {
	//		var task = Task.Factory.StartNew(() => {
	//			var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
	//			//string json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonConverter[1] { new IsoDateTimeConverter() });
	//			string json = JsonConvert.SerializeObject(value, Formatting.Indented);
	//			byte[] buf = System.Text.Encoding.Default.GetBytes(json);
	//			writeStream.Write(buf, 0, buf.Length);
	//			writeStream.Flush();
	//		});
	//		return task;
	//	}
	//}

	/*
	How to create and use a json custom converter.
	I didn't end up using it but I think we may want to do in some circumstance.

	 
	class ICloudBlobConverter : JsonConverter<ICloudBlob> {
		protected override ICloudBlob Create(Type objectType, JObject jObject) {
			BlobType bt = jObject["BlobType"].ToObject<BlobType>();
			Uri uri = new Uri(jObject["Uri"].ToObject<string>());
			switch (bt) {
				case BlobType.BlockBlob: return new CloudBlockBlob(uri);
				case BlobType.PageBlob: return new CloudPageBlob(uri);
			}
			throw new ApplicationException(String.Format("The BlobType {0} is not supported.", bt));
		}
	}

	in code...
	JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
	formatter.SerializerSettings.Converters.Add(new ICloudBlobConverter());
	foreach (ICloudBlob icb in (response.Content.ReadAsAsync<IEnumerable<ICloudBlob>>(new MediaTypeFormatter[] { formatter }).Result)) console.WriteLine(string.Format("Blob Type: {0}\nContent Encoding: {1}\nContent Language: {2}\nContent Type: {3}\nETag: {4}\nName: {5}\nSize: {6}\nUri: {7}", icb.BlobType, icb.Properties.ContentEncoding, icb.Properties.ContentLanguage, icb.Properties.ContentType, icb.Properties.ETag, icb.Name, icb.Properties.Length.ToString(), icb.Uri), ConsoleColor.White);
	...
	*/

#endregion

}
