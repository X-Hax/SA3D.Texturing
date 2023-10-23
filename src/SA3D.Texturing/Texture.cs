using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace SA3D.Texturing
{
	/// <summary>
	/// Base texture class.
	/// </summary>
	public abstract class Texture
	{
		/// <summary>
		/// Raw byte data behind the texture.
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// Texture name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Global texture index.
		/// </summary>
		public uint GlobalIndex { get; set; }

		/// <summary>
		/// Width of the texture in pixels.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Height of the texture in pixels.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Texture width expected by the game.
		/// </summary>
		public int OverrideWidth { get; set; }

		/// <summary>
		/// Texture height expected by the game.
		/// </summary>
		public int OverrideHeight { get; set; }

		/// <summary>
		/// Returns <see cref="OverrideWidth"/> if it is > 0. Otherwise returns <see cref="Width"/>
		/// </summary>
		public int ProcessedWidth => OverrideWidth == 0 ? Width : OverrideWidth;

		/// <summary>
		/// Returns <see cref="OverrideHeight"/> if it is > 0. Otherwise returns <see cref="Height"/>
		/// </summary>
		public int ProcessedHeight => OverrideHeight == 0 ? Height : OverrideHeight;

		/// <summary>
		/// Creates a new texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="name">Name of the texture.</param>
		/// <param name="globalIndex">Global index of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public Texture(int width, int height, byte[] data, string name, uint globalIndex)
		{
			if(width < 1 || height < 1)
			{
				throw new ArgumentException("Dimensions invalid! Width and height have to be at least 1!");
			}

			int expectedDataLength = CalculateExpectedDataLength(width, height);
			if(data.Length != expectedDataLength)
			{
				throw new InvalidDataException($"Data length does not match expectations! Is: {data.Length}, should be: {expectedDataLength}");
			}

			Name = name;
			GlobalIndex = globalIndex;
			Width = width;
			Height = height;
			Data = data;

		}

		/// <summary>
		/// Replaces texture dimensions and raw data.
		/// </summary>
		/// <param name="width">New texture width in pixels.</param>
		/// <param name="height">New texture height in pixels.</param>
		/// <param name="data">New raw texture data.</param>
		/// <exception cref="InvalidDataException"></exception>
		public void ReplaceData(int width, int height, byte[] data)
		{
			if(data.Length != CalculateExpectedDataLength(width, height))
			{
				throw new InvalidDataException("Data length does not match expectations!");
			}

			Width = width;
			Height = height;
			Data = data;
		}


		/// <summary>
		/// Calculates the number of bytes that this texture takes up.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <returns>Number of bytes that the texture takes up.</returns>
		protected abstract int CalculateExpectedDataLength(int width, int height);

		/// <summary>
		/// Returns the image in RGBA32 format.
		/// </summary>
		/// <returns>RGBA32 formatted byte array.</returns>
		public abstract ReadOnlySpan<byte> GetColorPixels();

		/// <summary>
		/// Checks whether any pixel has an alpha value below 255.
		/// </summary>
		/// <returns>Whether any pixel is has an alpha value below 255</returns>
		public abstract bool CheckIsTransparent();


		/// <summary>
		/// Read a texture file from a file data stream.
		/// </summary>
		/// <param name="stream">Stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static Texture ReadTexture(Stream stream, string filename)
		{
			long dataStart = stream.Position;

			if(IndexTexture.TryReadIndexed(stream, filename, out IndexTexture? result))
			{
				return result;
			}
			else
			{
				stream.Seek(dataStart, SeekOrigin.Begin);
				return ColorTexture.ReadColored(stream, filename);
			}
		}

		/// <summary>
		/// Read a texture from file data.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static Texture ReadTexture(byte[] data, string filename)
		{
			using(MemoryStream stream = new(data))
			{
				return ReadTexture(stream, filename);
			}
		}

		/// <summary>
		/// Read a texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		public static Texture ReadTextureFromFile(string filepath)
		{
            using(FileStream stream = File.OpenRead(filepath))
            {
			    return ReadTexture(stream, Path.GetFileNameWithoutExtension(filepath));
            }
		}


		/// <summary>
		/// Encode the colored texture as a PNG file and write it to a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteColoredAsPNG(Stream stream)
		{
			ReadOnlySpan<byte> colorData = GetColorPixels();
			PngEncoder encoder = new()
			{
				ColorType = CheckIsTransparent() ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
				TransparentColorMode = PngTransparentColorMode.Preserve,
				ChunkFilter = PngChunkFilter.ExcludeAll
			};
			Image.LoadPixelData<Rgba32>(colorData, Width, Height).SaveAsPng(stream, encoder);
		}

		/// <summary>
		/// Encode the colored texture as a PNG file.
		/// </summary>
		public byte[] WriteColoredAsPNGToBytes()
		{
			byte[] result;

			using(MemoryStream stream = new())
			{
				WriteColoredAsPNG(stream);
				result = stream.ToArray();
			}

			return result;
		}

		/// <summary>
		/// Write the colored texture to a PNG file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		public void WriteColoredAsPNGToFile(string filepath)
		{
            using(FileStream stream = File.Create(filepath))
            {
                WriteColoredAsPNG(stream);
            }
		}


		/// <summary>
		/// Encode the colored texture as a DDS file and write it to a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteColoredAsDDS(Stream stream)
		{
			ReadOnlySpan<byte> colorData = GetColorPixels();
			if(CheckIsTransparent())
			{
				new BcEncoder(CompressionFormat.Bc3).EncodeToDds(colorData, Width, Height, PixelFormat.Rgba32).Write(stream);
			}
			else
			{
				new BcEncoder(CompressionFormat.Bc1).EncodeToDds(colorData, Width, Height, PixelFormat.Rgba32).Write(stream);
			}
		}

		/// <summary>
		/// Encode the colored texture as a DDS file.
		/// </summary>
		public byte[] WriteColoredAsDDSToBytes()
		{
			byte[] result;

			using(MemoryStream stream = new())
			{
				WriteColoredAsDDS(stream);
				result = stream.ToArray();
			}

			return result;
		}

		/// <summary>
		/// Write the colored texture to a DDS file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		public void WriteColoredAsDDSToFile(string filepath)
		{
            using(FileStream stream = File.Create(filepath))
            {
			    WriteColoredAsDDS(stream);
            }
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GlobalIndex}; {Width}x{Height}";
		}
	}
}
