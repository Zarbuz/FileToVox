using FileToVoxCommon.Generator.Heightmap.Data;
using FileToVoxCommon.Generator.Shaders.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

namespace FileToVoxCommon.Json
{
	public class JsonBaseImportConcreteClassConverter : DefaultContractResolver
	{
		protected override JsonConverter ResolveContractConverter(Type objectType)
		{
			if (typeof(JsonBaseImportData).IsAssignableFrom(objectType) && !objectType.IsAbstract)
				return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
			return base.ResolveContractConverter(objectType);
		}
	}

	public class JsonBaseImportDataConverter : JsonConverter
	{
		static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new JsonBaseImportConcreteClassConverter() };

		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(JsonBaseImportData));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			Enum.TryParse(jo["GeneratorType"].ToString(), out GeneratorType generatorType);
			switch (generatorType)
			{
				//case GeneratorType.Terrain:
				//	return JsonConvert.DeserializeObject<WorldTerrainData>(jo.ToString(), SpecifiedSubclassConversion);
				case GeneratorType.Heightmap:
					return JsonConvert.DeserializeObject<HeightmapData>(jo.ToString(), SpecifiedSubclassConversion);
				case GeneratorType.Shader:
					return JsonConvert.DeserializeObject<ShaderData>(jo.ToString(), SpecifiedSubclassConversion);
				default:
					throw new Exception();
			}
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException(); // won't be called because CanWrite returns false
		}
	}
}
