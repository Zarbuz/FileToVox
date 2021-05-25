namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderFixLonely : ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.FIX_LONELY;

		public override void ValidateSettings()
		{
		}
	}
}
