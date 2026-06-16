using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
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
		public virtual bool CheckIsTransparent()
		{
			ReadOnlySpan<byte> colorData = GetColorPixels();

			for(int i = 3; i < colorData.Length; i += 4)
			{
				if(colorData[i] < 0xFF)
				{
					return true;
				}
			}

			return false;
		}


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
				return ColorTexture.ReadImage(stream, filename);
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
		/// Encode the colored texture as the given image format
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		/// <param name="format">The format to write as</param>
		/// <exception cref="ArgumentException"></exception>
		public void WriteColorImage(Stream stream, ImageFormat format)
		{
			ReadOnlySpan<byte> colorData = GetColorPixels();
			bool isTransparent = CheckIsTransparent();
			IImageEncoder? encoder;

			switch(format)
			{
				case ImageFormat.PNG:
					encoder = new PngEncoder()
					{
						ColorType = isTransparent ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
						TransparentColorMode = PngTransparentColorMode.Preserve,
						ChunkFilter = PngChunkFilter.ExcludeAll
					};
					break;
				case ImageFormat.BMP:
					encoder = new BmpEncoder()
					{
						BitsPerPixel = isTransparent ? BmpBitsPerPixel.Pixel32 : BmpBitsPerPixel.Pixel24,
						SupportTransparency = isTransparent
					};
					break;
				case ImageFormat.JPEG:
					encoder = new JpegEncoder();
					break;
				case ImageFormat.PBM:
					encoder = new PbmEncoder()
					{
						ColorType = PbmColorType.Rgb,
						ComponentType = PbmComponentType.Byte
					};
					break;
				case ImageFormat.QOI:
					encoder = new QoiEncoder()
					{
						Channels = isTransparent ? QoiChannels.Rgba : QoiChannels.Rgb
					};
					break;
				case ImageFormat.TGA:
					encoder = new TgaEncoder()
					{
						BitsPerPixel = isTransparent ? TgaBitsPerPixel.Pixel32 : TgaBitsPerPixel.Pixel24
					};
					break;
				case ImageFormat.TIFF:
					encoder = new TiffEncoder()
					{
						BitsPerPixel = isTransparent ? TiffBitsPerPixel.Bit32 : TiffBitsPerPixel.Bit24
					};
					break;
				case ImageFormat.WEBP:
					encoder = new WebpEncoder()
					{
						FileFormat = WebpFileFormatType.Lossless,
						TransparentColorMode = isTransparent ? WebpTransparentColorMode.Preserve : WebpTransparentColorMode.Clear
					};
					break;
				case ImageFormat.DDS:
					BcEncoder bcEncoder = new(isTransparent ? CompressionFormat.Bc3 : CompressionFormat.Bc1);
					bcEncoder.EncodeToDds(colorData, Width, Height, PixelFormat.Rgba32).Write(stream);
					return;
				default:
					throw new ArgumentException("Invalid image format", nameof(format));
			}

			Image.LoadPixelData<Rgba32>(colorData, Width, Height).Save(stream, encoder);
		}

		/// <summary>
		/// Encode the colored texture as an image file.
		/// </summary>
		/// <param name="format">The format to write as</param>
		public byte[] WriteColorImageToBytes(ImageFormat format)
		{
			using MemoryStream stream = new();
			WriteColorImage(stream, format);
			return stream.ToArray();
		}

		/// <summary>
		/// Write the colored texture to an image file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <param name="format">The format to write as</param>
		public void WriteColorImageToFile(string filepath, ImageFormat format)
		{
			using FileStream stream = File.Create(filepath);
			WriteColorImage(stream, format);
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GlobalIndex}; {Width}x{Height}";
		}
	}
}
