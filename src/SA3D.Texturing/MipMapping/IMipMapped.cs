namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Interface for objects that have mip map data
	/// </summary>
	public interface IMipMapped
	{
		/// <summary>
		/// Mip map data
		/// </summary>
		public IMipMapSet MipMaps { get; }
	}
}
