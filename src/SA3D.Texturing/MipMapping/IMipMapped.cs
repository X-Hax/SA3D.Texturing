namespace SA3D.Texturing.MipMapping
{
	/// <summary>
	/// Interface for objects that have mip map data
	/// </summary>
	public interface IMipMapped<T> where T : IMipMapLevel
	{
		/// <summary>
		/// Mip map data
		/// </summary>
		public IMipMapSet<T> MipMaps { get; }
	}
}
