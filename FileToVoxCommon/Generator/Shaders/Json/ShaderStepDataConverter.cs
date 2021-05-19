using System;
using FileToVoxCommon.Generator.Shaders.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FileToVoxCommon.Generator.Shaders.Json
{
	public class ShaderStepConcreteClassConverter : DefaultContractResolver
	{
		protected override JsonConverter ResolveContractConverter(Type objectType)
		{
			if (typeof(ShaderStep).IsAssignableFrom(objectType) && !objectType.IsAbstract)
				return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
			return base.ResolveContractConverter(objectType);
		}
	}

	class ShaderStepDataConverter : JsonConverter
	{
		static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new ShaderStepConcreteClassConverter() };
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			Enum.TryParse(jo["shaderType"].ToString(), out ShaderType shaderType);
			switch (shaderType)
			{
				case ShaderType.PATINA:
					return JsonConvert.DeserializeObject<ShaderPatina>(jo.ToString(), SpecifiedSubclassConversion);
				case ShaderType.CASE:
					return JsonConvert.DeserializeObject<ShaderCase>(jo.ToString(), SpecifiedSubclassConversion);
				case ShaderType.FIX_HOLES:
					return JsonConvert.DeserializeObject<ShaderFixHoles>(jo.ToString(), SpecifiedSubclassConversion);
				case ShaderType.FIX_LONELY:
					return JsonConvert.DeserializeObject<ShaderFixLonely>(jo.ToString(), SpecifiedSubclassConversion);
				case ShaderType.COLOR_DENOISER:
					return JsonConvert.DeserializeObject<ShaderColorDenoiser>(jo.ToString(), SpecifiedSubclassConversion);
				default:
					throw new Exception();
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(ShaderStep));
		}
	}
}
