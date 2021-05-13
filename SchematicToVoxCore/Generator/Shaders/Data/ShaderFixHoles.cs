namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderFixHoles :ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.FIX_HOLES;

		public override void ValidateSettings()
		{

		}
	}
}
