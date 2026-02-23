using Amicitia.IO.Binary;
using SA3D.Common.IO;

namespace SA3D.Texturing.Texname
{
	/// <summary>
	/// Stores a texture name and its attributes
	/// </summary>
	public class TextureName : IBinarySerializable
	{
		/// <summary>
		/// Size of the struct in bytes.
		/// </summary>
		public const int StructSize = 0xC;

		/// <summary>
		/// The texture name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Attributes.
		/// </summary>
		public uint Attributes { get; private set; }

		/// <summary>
		/// Texture address.
		/// </summary>
		public uint TextureAddress { get; set; }


		/// <summary>
		/// Creates a new texture name.
		/// </summary>
		/// <param name="name">The texture name.</param>
		/// <param name="attributes">Attributes.</param>
		/// <param name="textureAddress">Texture address.</param>
		public TextureName(string? name, uint attributes, uint textureAddress)
		{
			Name = name;
			Attributes = attributes;
			TextureAddress = textureAddress;
		}

		/// <summary>
		/// Creates a new, empty texture name
		/// </summary>
		public TextureName() : this(null, 0, 0) { }


		/// <summary>
		/// Reads a texture name from a <see cref="BinaryObjectReader"/>
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		public void Read(BinaryObjectReader reader)
		{
			Name = reader.ReadStringOffset(StringBinaryFormat.NullTerminated);
			Attributes = reader.ReadUInt32();
			TextureAddress = reader.ReadUInt32();
		}

		/// <summary>
		/// Writes a texture name to a <see cref="BinaryObjectWriter"/>
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		public void Write(BinaryObjectWriter writer)
		{
			writer.WriteStringOffset(StringBinaryFormat.NullTerminated, Name, alignment: 4);
			writer.WriteUInt32(Attributes);
			writer.WriteUInt32(TextureAddress);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Name ?? "!NULL";
		}
	}
}
