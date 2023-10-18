using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System;

namespace SA3D.Texturing.Texname
{
	/// <summary>
	/// Stores a texture name and its attributes
	/// </summary>
	public class TextureName
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
		/// Reads a texture from a reader.
		/// </summary>
		/// <param name="reader">Reader to read the data from.</param>
		/// <param name="address">The address at which to read the texture name struct.</param>
		/// <returns>The read texture name.</returns>
		public static TextureName Read(EndianStackReader reader, uint address)
		{
			uint nameAddr = reader.ReadPointer(address);
			string? name = nameAddr == 0 ? null : reader.ReadNullterminatedString(nameAddr);
			uint attributes = reader.ReadUInt(address + 4);
			uint textureAddr = reader.ReadUInt(address + 8);

			return new(name, attributes, textureAddr);
		}

		/// <summary>
		/// Writes the texture name struct to a writer and obtains the name string from labels.
		/// </summary>
		/// <param name="writer">Writer to write the struct to.</param>
		/// <param name="labels">Label dictionary to obtain the string address from.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Write(EndianStackWriter writer, LabelDictionary labels)
		{
			uint nameAddress = 0;
			if(Name != null)
			{
				if(!labels.TryGetAddress(Name, out uint? tmpNameAddress))
				{
					throw new InvalidOperationException("Name has not been written yet!");
				}

				nameAddress = tmpNameAddress!.Value;
			}

			writer.WriteUInt(nameAddress);
			writer.WriteUInt(Attributes);
			writer.WriteUInt(TextureAddress);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Name ?? "!NULL";
		}
	}
}
