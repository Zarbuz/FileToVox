namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderColorDenoiser : ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.COLOR_DENOISER;

		public override void ValidateSettings()
		{
		}
	}
}
