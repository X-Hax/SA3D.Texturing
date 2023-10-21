using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace SA3D.Texturing
{
	/// <summary>
	/// RGBA32 texture.
	/// </summary>
	public sealed class ColorTexture : Texture
	{
		/// <summary>
		/// Creates a new color texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="name">Name of the texture.</param>
		/// <param name="globalIndex">Global index of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ColorTexture(int width, int height, byte[] data, string name, uint globalIndex)
			: base(width, height, data, name, globalIndex) { }

		/// <summary>
		/// Creates a new color texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="globalIndex">Global index of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ColorTexture(int width, int height, byte[] data, uint globalIndex)
			: this(width, height, data, string.Empty, globalIndex) { }

		/// <summary>
		/// Creates a new color texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <param name="name">Name of the texture.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ColorTexture(int width, int height, byte[] data, string name)
			: this(width, height, data, name, 0) { }

		/// <summary>
		/// Creates a new color texture from preexisting data.
		/// </summary>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels.</param>
		/// <param name="data">Raw pixel data to use.</param>
		/// <exception cref="InvalidDataException"></exception>
		public ColorTexture(int width, int height, byte[] data)
			: this(width, height, data, string.Empty, 0) { }

		/// <inheritdoc/>
		protected override int CalculateExpectedDataLength(int width, int height)
		{
			return width * height * 4;
		}

		/// <inheritdoc/>
		public override ReadOnlySpan<byte> GetColorPixels()
		{
			return new(Data);
		}

		/// <inheritdoc/>
		public override bool CheckIsTransparent()
		{
			for(int i = 3; i < Data.Length; i += 4)
			{
				if(Data[i] < 0xFF)
				{
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Read a color texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		public static ColorTexture ReadColoredFromFile(string filepath)
		{
			return ReadColoredFromFile(File.OpenRead(filepath), Path.GetFileNameWithoutExtension(filepath));
		}

		/// <summary>
		/// Read a color texture from file data.
		/// </summary>
		/// <param name="data">File data to read.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static ColorTexture ReadColoredFromFile(byte[] data, string filename)
		{
			using(MemoryStream stream = new(data))
			{
				return ReadColoredFromFile(stream, filename);
			}
		}

		/// <summary>
		/// Read a color texture from a file data stream.
		/// </summary>
		/// <param name="stream">Stream to read the file data from.</param>
		/// <param name="filename">Filename that should be used.</param>
		public static ColorTexture ReadColoredFromFile(Stream stream, string filename)
		{
			long dataStart = stream.Position;

			ImageFileFormat bcFormat = ImageFileFormat.Unknown;
			try
			{
				bcFormat = ImageFile.DetermineImageFormat(stream);
			}
			catch { }

			stream.Seek(dataStart, SeekOrigin.Begin);

			Image<Rgba32> image;
			if(bcFormat != ImageFileFormat.Unknown)
			{
				BcDecoder decoder = new();
				image = decoder.DecodeToImageRgba32(stream);
			}
			else
			{
				image = Image.Load<Rgba32>(stream);
			}

			byte[] data = new byte[image.Width * image.Height * 4];
			image.CopyPixelDataTo(data);

			return new ColorTexture(image.Width, image.Height, data, filename, 0);
		}

	}
}
