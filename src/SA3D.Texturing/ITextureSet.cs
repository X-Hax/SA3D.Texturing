using System.Collections.Generic;

namespace SA3D.Texturing
{
	/// <summary>
	/// Texture set interface
	/// </summary>
	public interface ITextureSet
	{
		/// <summary>
		/// Textures of the texture set
		/// </summary>
		public IReadOnlyList<ITexture> Textures { get; }
	}
}
