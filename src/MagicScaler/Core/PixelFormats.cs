﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PhotoSauce.Interop.Wic;

namespace PhotoSauce.MagicScaler
{
	internal enum PixelNumericRepresentation
	{
		Unspecified = 0,
		Indexed = 1,
		UnsignedInteger = 2,
		SignedInteger = 3,
		Fixed = 4,
		Float = 5
	}

	internal enum PixelColorRepresentation
	{
		Unspecified,
		Grey,
		Bgr,
		Rgb,
		Cmyk
	}

	internal enum PixelAlphaRepresentation
	{
		None,
		Associated,
		Unassociated
	}

	internal enum PixelValueEncoding
	{
		Unspecified,
		Companded,
		Linear,
		scRgb
	}

	internal class PixelFormat : IEquatable<PixelFormat>
	{
		public readonly Guid FormatGuid;
		public readonly string Name;
		public readonly bool IsWicNative;
		public readonly int BitsPerPixel;
		public readonly int ChannelCount;
		public readonly PixelNumericRepresentation NumericRepresentation;
		public readonly PixelColorRepresentation ColorRepresentation;
		public readonly PixelAlphaRepresentation AlphaRepresentation;
		public readonly PixelValueEncoding Encoding;

		public bool Equals(PixelFormat other) => FormatGuid == other.FormatGuid;

		public static bool operator ==(PixelFormat left, PixelFormat right) => left.Equals(right);
		public static bool operator !=(PixelFormat left, PixelFormat right) => !left.Equals(right);

		public override bool Equals(object? o) => o is PixelFormat pf ? Equals(pf) : false;
		public override int GetHashCode() => FormatGuid.GetHashCode();

		public int BytesPerPixel => MathUtil.DivCeiling(BitsPerPixel, 8);

		public bool IsBinaryCompatibleWith(PixelFormat other) =>
			BitsPerPixel == other.BitsPerPixel &&
			ChannelCount == other.ChannelCount &&
			NumericRepresentation == other.NumericRepresentation &&
			ColorRepresentation == other.ColorRepresentation &&
			AlphaRepresentation == other.AlphaRepresentation &&
			Encoding == other.Encoding;

		private PixelFormat(Guid guid, string name, int bpp, int channels, PixelNumericRepresentation numericRepresentation,
			PixelColorRepresentation colorRepresentation = PixelColorRepresentation.Unspecified, PixelAlphaRepresentation alphaRepresentation = PixelAlphaRepresentation.None,
			PixelValueEncoding encoding = PixelValueEncoding.Unspecified, bool isWic = false
		)
		{
			FormatGuid = guid;
			Name = name;
			IsWicNative = isWic;
			BitsPerPixel = bpp;
			ChannelCount = channels;
			NumericRepresentation = numericRepresentation;
			ColorRepresentation = colorRepresentation;
			AlphaRepresentation = alphaRepresentation;
			Encoding = encoding;
		}

		private static readonly Lazy<ReadOnlyDictionary<Guid, PixelFormat>> cache = new Lazy<ReadOnlyDictionary<Guid, PixelFormat>>(getFormatCache);

		public static readonly PixelFormat Grey16BppUQ15 = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0x9F),
			name: "16bpp Grey UQ15",
			bpp: 16,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Fixed,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Grey16BppLinearUQ15 = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA0),
			name: "16bpp Grey UQ15 Linear",
			bpp: 16,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Fixed,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Grey32BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0x9E),
			name: "32bpp Grey Float",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Grey32BppLinearFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA1),
			name: "32bpp Grey Float Linear",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Bgr48BppLinearUQ15 = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA2),
			name: "48bpp BGR UQ15 Linear",
			bpp: 48,
			channels: 3,
			numericRepresentation: PixelNumericRepresentation.Fixed,
			colorRepresentation: PixelColorRepresentation.Bgr,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Bgr96BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA3),
			name: "96bpp BGR Float",
			bpp: 96,
			channels: 3,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Bgr96BppLinearFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA4),
			name: "96bpp BGR Float Linear",
			bpp: 96,
			channels: 3,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Pbgra64BppLinearUQ15 = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA6),
			name: "64bpp pBGRA UQ15 Linear",
			bpp: 64,
			channels: 4,
			numericRepresentation: PixelNumericRepresentation.Fixed,
			colorRepresentation: PixelColorRepresentation.Bgr,
			alphaRepresentation: PixelAlphaRepresentation.Associated,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Pbgra128BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA7),
			name: "128bpp pBGRA Float",
			bpp: 128,
			channels: 4,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			alphaRepresentation: PixelAlphaRepresentation.Associated,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Pbgra128BppLinearFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA8),
			name: "128bpp pBGRA Float Linear",
			bpp: 128,
			channels: 4,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			alphaRepresentation: PixelAlphaRepresentation.Associated,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Y16BppLinearUQ15 = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xA9),
			name: "16bpp Y UQ15 Linear",
			bpp: 16,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Fixed,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat Y32BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAA),
			name: "32bpp Y Float",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Y32BppLinearFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAB),
			name: "32bpp Y Float Linear",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Grey,
			encoding: PixelValueEncoding.Linear
		);

		public static readonly PixelFormat CbCr64BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAC),
			name: "64bpp CbCr Float",
			bpp: 64,
			channels: 2,
			numericRepresentation: PixelNumericRepresentation.Float
		);

		public static readonly PixelFormat Cb32BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAF),
			name: "32bpp Cb Float",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float
		);

		public static readonly PixelFormat Cr32BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xB0),
			name: "32bpp Cr Float",
			bpp: 32,
			channels: 1,
			numericRepresentation: PixelNumericRepresentation.Float
		);

		public static readonly PixelFormat Bgrx128BppFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAD),
			name: "128bpp BGRX Float",
			bpp: 128,
			channels: 4,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			alphaRepresentation: PixelAlphaRepresentation.None,
			encoding: PixelValueEncoding.Companded
		);

		public static readonly PixelFormat Bgrx128BppLinearFloat = new PixelFormat(
			guid: new Guid(0xC175220D, 0x375B, 0x48C9, 0x8D, 0xD9, 0x1D, 0x28, 0x24, 0xFE, 0x88, 0xAE),
			name: "128bpp BGRX Float Linear",
			bpp: 128,
			channels: 4,
			numericRepresentation: PixelNumericRepresentation.Float,
			colorRepresentation: PixelColorRepresentation.Bgr,
			alphaRepresentation: PixelAlphaRepresentation.None,
			encoding: PixelValueEncoding.Linear
		);

		public static PixelFormat FromGuid(Guid guid) => cache.Value.TryGetValue(guid, out var pf) ? pf : throw new NotSupportedException("Unsupported pixel format.");

		private static ReadOnlyDictionary<Guid, PixelFormat> getFormatCache()
		{
			var dic = new Dictionary<Guid, PixelFormat> {
				[Grey16BppUQ15.FormatGuid]          = Grey16BppUQ15,
				[Grey16BppLinearUQ15.FormatGuid]    = Grey16BppLinearUQ15,
				[Grey32BppFloat.FormatGuid]         = Grey32BppFloat,
				[Grey32BppLinearFloat.FormatGuid]   = Grey32BppLinearFloat,
				[Bgr48BppLinearUQ15.FormatGuid]     = Bgr48BppLinearUQ15,
				[Bgr96BppFloat.FormatGuid]          = Bgr96BppFloat,
				[Bgr96BppLinearFloat.FormatGuid]    = Bgr96BppLinearFloat,
				[Pbgra64BppLinearUQ15.FormatGuid]   = Pbgra64BppLinearUQ15,
				[Pbgra128BppFloat.FormatGuid]       = Pbgra128BppFloat,
				[Pbgra128BppLinearFloat.FormatGuid] = Pbgra128BppLinearFloat,
				[Y16BppLinearUQ15.FormatGuid]       = Y16BppLinearUQ15,
				[Y32BppFloat.FormatGuid]            = Y32BppFloat,
				[Y32BppLinearFloat.FormatGuid]      = Y32BppLinearFloat,
				[CbCr64BppFloat.FormatGuid]         = CbCr64BppFloat,
				[Cb32BppFloat.FormatGuid]           = Cb32BppFloat,
				[Cr32BppFloat.FormatGuid]           = Cr32BppFloat,
				[Bgrx128BppFloat.FormatGuid]        = Bgrx128BppFloat,
				[Bgrx128BppLinearFloat.FormatGuid]  = Bgrx128BppLinearFloat
			};

			uint count = 10u;
			var formats = new object[count];
			using var cenum = new ComHandle<IEnumUnknown>(Wic.Factory.CreateComponentEnumerator(WICComponentType.WICPixelFormat, WICComponentEnumerateOptions.WICComponentEnumerateDefault));

			do
			{
				count = cenum.ComObject.Next(count, formats);
				for (int i = 0; i < count; i++)
				{
					using var pixh = new ComHandle<IWICPixelFormatInfo2>(formats[i]);
					var pix = pixh.ComObject;

					uint cch = pix.GetFriendlyName(0, null);
					var sbn = new StringBuilder((int)cch);
					pix.GetFriendlyName(cch, sbn);
					string pfn = sbn.ToString();

					var numericRep = (PixelNumericRepresentation)pix.GetNumericRepresentation();
					var colorRep =
						pfn.Contains("BGR") ? PixelColorRepresentation.Bgr :
						pfn.Contains("RGB") ? PixelColorRepresentation.Rgb :
						pfn.Contains("CMYK") ? PixelColorRepresentation.Cmyk :
						pfn.Contains("Gray") || pfn.EndsWith(" Y") ? PixelColorRepresentation.Grey :
						PixelColorRepresentation.Unspecified;
					var valEncoding = colorRep == PixelColorRepresentation.Grey || colorRep == PixelColorRepresentation.Bgr || colorRep == PixelColorRepresentation.Rgb ?
						numericRep == PixelNumericRepresentation.Fixed || numericRep == PixelNumericRepresentation.Float ? PixelValueEncoding.scRgb :
						PixelValueEncoding.Companded :
						PixelValueEncoding.Unspecified;

					var fmt = new PixelFormat(
						guid: pix.GetFormatGUID(),
						name: pfn,
						bpp: (int)pix.GetBitsPerPixel(),
						channels: (int)pix.GetChannelCount(),
						numericRepresentation: numericRep,
						colorRepresentation: colorRep,
						alphaRepresentation: pfn.Contains("pBGRA") || pfn.Contains("pRGBA") ? PixelAlphaRepresentation.Associated :
							pix.SupportsTransparency() ? PixelAlphaRepresentation.Unassociated :
							PixelAlphaRepresentation.None,
						encoding: valEncoding,
						isWic: true
					);

					dic.Add(fmt.FormatGuid, fmt);
				}
			} while (count > 0);

			return new ReadOnlyDictionary<Guid, PixelFormat>(dic);
		}
	}

	/// <summary>Contains standard pixel formats available as output from an <see cref="IPixelSource" />.</summary>
	public static class PixelFormats
	{
		/// <summary>Greyscale data with 1 byte per pixel.</summary>
		/// <value>6FDDC324-4E03-4BFE-B185-3D77768DC908</value>
		public static readonly Guid Grey8bpp = Consts.GUID_WICPixelFormat8bppGray;
		/// <summary>RGB data with 1 byte per channel in BGR byte order.</summary>
		/// <value>6FDDC324-4E03-4BFE-B185-3D77768DC90C</value>
		public static readonly Guid Bgr24bpp = Consts.GUID_WICPixelFormat24bppBGR;
		/// <summary>RGBA data with 1 byte per channel in BGRA byte order.</summary>
		/// <value>6FDDC324-4E03-4BFE-B185-3D77768DC90F</value>
		public static readonly Guid Bgra32bpp = Consts.GUID_WICPixelFormat32bppBGRA;

		/// <summary>Contains standard pixel formats for <see cref="IYccImageFrame"/> implementations.</summary>
		public static class Planar
		{
			/// <summary>Planar luma data with 1 byte per pixel.</summary>
			/// <value>91B4DB54-2DF9-42F0-B449-2909BB3DF88E</value>
			public static readonly Guid Y8bpp = Consts.GUID_WICPixelFormat8bppY;
			/// <summary>Planar blue-yellow chroma data with 1 byte per pixel.</summary>
			/// <value>1339F224-6BFE-4C3E-9302E4F3A6D0CA2A</value>
			public static readonly Guid Cb8bpp = Consts.GUID_WICPixelFormat8bppCb;
			/// <summary>Planar red-green chroma data with 1 byte per pixel.</summary>
			/// <value>B8145053-2116-49F0-8835ED844B205C51</value>
			public static readonly Guid Cr8bpp = Consts.GUID_WICPixelFormat8bppCr;
		}
	}
}
