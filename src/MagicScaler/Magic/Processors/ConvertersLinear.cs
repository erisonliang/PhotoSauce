﻿using System.Numerics;
using System.Runtime.CompilerServices;

using static PhotoSauce.MagicScaler.MathUtil;

using VectorF = System.Numerics.Vector<float>;

namespace PhotoSauce.MagicScaler
{
	internal sealed class ConverterToLinear<TFrom, TTo> : IConverter<TFrom, TTo> where TFrom : unmanaged where TTo : unmanaged
	{
		public IConversionProcessor<TFrom, TTo> Processor { get; }
		public IConversionProcessor<TFrom, TTo> Processor3A { get; }
		public IConversionProcessor<TFrom, TTo> Processor3X { get; }

		public ConverterToLinear(TTo[] inverseGammaTable)
		{
			Processor = new Converter(inverseGammaTable);
			Processor3A = new Converter3A(inverseGammaTable);
			Processor3X = new Converter3X(inverseGammaTable);
		}

		private sealed class Converter : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] igt;

			public Converter(TTo[] inverseGammaTable) => igt = inverseGammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* igtstart = &igt[0])
				{
					if (typeof(TFrom) == typeof(byte) && typeof(TTo) == typeof(ushort))
						convertUQ15(istart, ostart, (ushort*)igtstart, cb);
					else if (typeof(TFrom) == typeof(byte) && typeof(TTo) == typeof(float))
						convertFloat(istart, ostart, (float*)igtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(float))
						FloatConverter.Interpolating.ConvertFloat(istart, ostart, (float*)igtstart, LookupTables.InverseGammaScale, cb);
				}
			}

			unsafe private static void convertUQ15(byte* ipstart, byte* opstart, ushort* igtstart, int cb)
			{
				byte* ip = ipstart, ipe = ipstart + cb - 8;
				ushort* op = (ushort*)opstart, igt = igtstart;

				while (ip <= ipe)
				{
					ushort o0 = igt[(uint)ip[0]];
					ushort o1 = igt[(uint)ip[1]];
					ushort o2 = igt[(uint)ip[2]];
					ushort o3 = igt[(uint)ip[3]];
					op[0] = o0;
					op[1] = o1;
					op[2] = o2;
					op[3] = o3;

					o0 = igt[(uint)ip[4]];
					o1 = igt[(uint)ip[5]];
					o2 = igt[(uint)ip[6]];
					o3 = igt[(uint)ip[7]];
					op[4] = o0;
					op[5] = o1;
					op[6] = o2;
					op[7] = o3;

					ip += 8;
					op += 8;
				}
				ipe += 8;

				while (ip < ipe)
				{
					op[0] = igt[(uint)ip[0]];
					ip++;
					op++;
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, float* igtstart, int cb)
			{
				byte* ip = ipstart, ipe = ipstart + cb - 8;
				float* op = (float*)opstart, igt = igtstart;

				while (ip <= ipe)
				{
					float o0 = igt[(uint)ip[0]];
					float o1 = igt[(uint)ip[1]];
					float o2 = igt[(uint)ip[2]];
					float o3 = igt[(uint)ip[3]];
					float o4 = igt[(uint)ip[4]];
					float o5 = igt[(uint)ip[5]];
					float o6 = igt[(uint)ip[6]];
					float o7 = igt[(uint)ip[7]];

					op[0] = o0;
					op[1] = o1;
					op[2] = o2;
					op[3] = o3;
					op[4] = o4;
					op[5] = o5;
					op[6] = o6;
					op[7] = o7;

					ip += 8;
					op += 8;
				}
				ipe += 8;

				while (ip < ipe)
				{
					op[0] = igt[(uint)ip[0]];
					ip++;
					op++;
				}
			}
		}

		private sealed class Converter3A : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] igt;

			public Converter3A(TTo[] inverseGammaTable) => igt = inverseGammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* igtstart = &igt[0])
				{
					if (typeof(TFrom) == typeof(byte) && typeof(TTo) == typeof(ushort))
						convertUQ15(istart, ostart, (ushort*)igtstart, cb);
					else if (typeof(TFrom) == typeof(byte) && typeof(TTo) == typeof(float))
						convertFloat(istart, ostart, (float*)igtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(float))
						FloatConverter.Interpolating.ConvertFloat3A(istart, ostart, (float*)igtstart, LookupTables.InverseGammaScale, cb);
				}
			}

			unsafe private static void convertUQ15(byte* ipstart, byte* opstart, ushort* igtstart, int cb)
			{
				byte* ip = ipstart, ipe = ipstart + cb - 4;
				ushort* op = (ushort*)opstart, igt = igtstart;

				while (ip <= ipe)
				{
					uint i0 = igt[(uint)ip[0]];
					uint i1 = igt[(uint)ip[1]];
					uint i2 = igt[(uint)ip[2]];
					uint i3 =     Fix15(ip[3]);

					i0 = UnFix15(i0 * i3);
					i1 = UnFix15(i1 * i3);
					i2 = UnFix15(i2 * i3);

					op[0] = (ushort)i0;
					op[1] = (ushort)i1;
					op[2] = (ushort)i2;
					op[3] = (ushort)i3;

					ip += 4;
					op += 4;
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, float* igtstart, int cb)
			{
				fixed (float* atstart = &LookupTables.Alpha[0])
				{
					byte* ip = ipstart, ipe = ipstart + cb - 4;
					float* op = (float*)opstart, igt = igtstart, at = atstart;

					while (ip <= ipe)
					{
						float o0 = igt[(uint)ip[0]];
						float o1 = igt[(uint)ip[1]];
						float o2 = igt[(uint)ip[2]];
						float o3 =  at[(uint)ip[3]];
						op[0] = o0 * o3;
						op[1] = o1 * o3;
						op[2] = o2 * o3;
						op[3] = o3;

						ip += 4;
						op += 4;
					}
				}
			}
		}

		private sealed class Converter3X : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] igt;

			public Converter3X(TTo[] inverseGammaTable) => igt = inverseGammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* igtstart = &igt[0])
				{
					if (typeof(TFrom) == typeof(byte) && typeof(TTo) == typeof(float))
						convertFloat(istart, ostart, (float*)igtstart, cb);
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, float* igtstart, int cb)
			{
				byte* ip = ipstart, ipe = ipstart + cb - 3;
				float* op = (float*)opstart, igt = igtstart;

				while (ip <= ipe)
				{
					float o0 = igt[(uint)ip[0]];
					float o1 = igt[(uint)ip[1]];
					float o2 = igt[(uint)ip[2]];
					op[0] = o0;
					op[1] = o1;
					op[2] = o2;

					ip += 3;
					op += 4;
				}
			}
		}
	}

	internal class ConverterFromLinear<TFrom, TTo> : IConverter<TFrom, TTo> where TFrom : unmanaged where TTo : unmanaged
	{
		public IConversionProcessor<TFrom, TTo> Processor { get; }
		public IConversionProcessor<TFrom, TTo> Processor3A { get; }
		public IConversionProcessor<TFrom, TTo> Processor3X { get; }

		public ConverterFromLinear(TTo[] gammaTable)
		{
			Processor = new Converter(gammaTable);
			Processor3A = new Converter3A(gammaTable);
			Processor3X = new Converter3X(gammaTable);
		}

		private sealed class Converter : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] gt;

			public Converter(TTo[] gammaTable) => gt = gammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* gtstart = &gt[0])
				{
					if (typeof(TFrom) == typeof(ushort) && typeof(TTo) == typeof(byte))
						convertUQ15(istart, ostart, (byte*)gtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(byte))
						convertFloat(istart, ostart, (byte*)gtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(float))
						FloatConverter.Interpolating.ConvertFloat(istart, ostart, (float*)gtstart, LookupTables.GammaScaleFloat, cb);
				}
			}

			unsafe private static void convertUQ15(byte* ipstart, byte* opstart, byte* gtstart, int cb)
			{
				ushort* ip = (ushort*)ipstart, ipe = (ushort*)(ipstart + cb) - 4;
				byte* op = opstart, gt = gtstart;

				while (ip <= ipe)
				{
					uint i0 = ClampToUQ15One((uint)ip[0]);
					uint i1 = ClampToUQ15One((uint)ip[1]);
					uint i2 = ClampToUQ15One((uint)ip[2]);
					uint i3 = ClampToUQ15One((uint)ip[3]);
					op[0] = gt[i0];
					op[1] = gt[i1];
					op[2] = gt[i2];
					op[3] = gt[i3];

					ip += 4;
					op += 4;
				}
				ipe += 4;

				while (ip < ipe)
				{
					op[0] = gt[(uint)ClampToUQ15One((uint)ip[0])];
					ip++;
					op++;
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, byte* gtstart, int cb)
			{
				float* ip = (float*)ipstart, ipe = (float*)(ipstart + cb) - VectorF.Count; ;
				byte* op = opstart, gt = gtstart;

				var vmin = VectorF.Zero;
				var vmax = new VectorF(UQ15One);
				var vround = new VectorF(0.5f);

				while (ip <= ipe)
				{
					var v = Unsafe.ReadUnaligned<VectorF>(ip) * vmax + vround;
					v = v.Clamp(vmin, vmax);

#if VECTOR_CONVERT
					var vi = Vector.AsVectorUInt32(Vector.ConvertToInt32(v));
#else
					var vi = v;
#endif

					byte o0 = gt[(uint)vi[0]];
					byte o1 = gt[(uint)vi[1]];
					byte o2 = gt[(uint)vi[2]];
					byte o3 = gt[(uint)vi[3]];
					op[0] = o0;
					op[1] = o1;
					op[2] = o2;
					op[3] = o3;

					if (VectorF.Count == 8)
					{
						o0 = gt[(uint)vi[4]];
						o1 = gt[(uint)vi[5]];
						o2 = gt[(uint)vi[6]];
						o3 = gt[(uint)vi[7]];
						op[4] = o0;
						op[5] = o1;
						op[6] = o2;
						op[7] = o3;
					}

					ip += VectorF.Count;
					op += VectorF.Count;
				}
				ipe += VectorF.Count;

				while (ip < ipe)
				{
					op[0] = gt[(uint)FixToUQ15One(ip[0])];
					ip++;
					op++;
				}
			}
		}

		private sealed class Converter3A : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] gt;

			public Converter3A(TTo[] gammaTable) => gt = gammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* gtstart = &gt[0])
				{
					if (typeof(TFrom) == typeof(ushort) && typeof(TTo) == typeof(byte))
						convertUQ15(istart, ostart, (byte*)gtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(byte))
						convertFloat(istart, ostart, (byte*)gtstart, cb);
					else if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(float))
						FloatConverter.Interpolating.ConvertFloat3A(istart, ostart, (float*)gtstart, LookupTables.GammaScaleFloat, cb);
				}
			}

			unsafe private static void convertUQ15(byte* ipstart, byte* opstart, byte* gtstart, int cb)
			{
				ushort* ip = (ushort*)ipstart, ipe = (ushort*)(ipstart + cb) - 4;
				byte* op = opstart, gt = gtstart;

				while (ip <= ipe)
				{
					uint i3 = ip[3];
					byte o3 = UnFix15ToByte(i3 * byte.MaxValue);
					if (o3 == 0)
					{
						*(uint*)op = 0;
					}
					else
					{
						uint o3i = UQ15One * UQ15One / i3;
						uint i0 = ip[0];
						uint i1 = ip[1];
						uint i2 = ip[2];

						byte o0 = gt[(uint)UnFixToUQ15One(i0 * o3i)];
						byte o1 = gt[(uint)UnFixToUQ15One(i1 * o3i)];
						byte o2 = gt[(uint)UnFixToUQ15One(i2 * o3i)];
						op[0] = o0;
						op[1] = o1;
						op[2] = o2;
						op[3] = o3;
					}

					ip += 4;
					op += 4;
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, byte* gtstart, int cb)
			{
				float* ip = (float*)ipstart, ipe = (float*)(ipstart + cb) - 4;
				byte* op = opstart, gt = gtstart;
				float fmax = new Vector4(byte.MaxValue).X, fround = new Vector4(0.5f).X, fmin = fround / fmax;

				while (ip <= ipe)
				{
					float f3 = ip[3];
					if (f3 < fmin)
					{
						*(uint*)op = 0;
					}
					else
					{
						float f3i = UQ15One / f3;
						byte o0 = gt[(uint)ClampToUQ15One((int)(ip[0] * f3i + fround))];
						byte o1 = gt[(uint)ClampToUQ15One((int)(ip[1] * f3i + fround))];
						byte o2 = gt[(uint)ClampToUQ15One((int)(ip[2] * f3i + fround))];
						byte o3 = ClampToByte((int)(f3 * fmax + fround));
						op[0] = o0;
						op[1] = o1;
						op[2] = o2;
						op[3] = o3;
					}

					ip += 4;
					op += 4;
				}
			}
		}

		private sealed class Converter3X : IConversionProcessor<TFrom, TTo>
		{
			private readonly TTo[] gt;

			public Converter3X(TTo[] gammaTable) => gt = gammaTable;

			unsafe void IConversionProcessor.ConvertLine(byte* istart, byte* ostart, int cb)
			{
				fixed (TTo* gtstart = &gt[0])
				{
					if (typeof(TFrom) == typeof(float) && typeof(TTo) == typeof(byte))
						convertFloat(istart, ostart, (byte*)gtstart, cb);
				}
			}

			unsafe private static void convertFloat(byte* ipstart, byte* opstart, byte* gtstart, int cb)
			{
				float* ip = (float*)ipstart, ipe = (float*)(ipstart + cb) - VectorF.Count;
				byte* op = opstart, gt = gtstart;

				var vmin = VectorF.Zero;
				var vmax = new VectorF(UQ15One);
				var vround = new VectorF(0.5f);

				while (ip <= ipe)
				{
					var v = Unsafe.ReadUnaligned<VectorF>(ip) * vmax + vround;
					v = v.Clamp(vmin, vmax);

#if VECTOR_CONVERT
					var vi = Vector.ConvertToInt32(v);
#else
					var vi = v;
#endif

					byte o0 = gt[(uint)vi[0]];
					byte o1 = gt[(uint)vi[1]];
					byte o2 = gt[(uint)vi[2]];
					op[0] = o0;
					op[1] = o1;
					op[2] = o2;

					if (VectorF.Count == 8)
					{
						o0 = gt[(uint)vi[4]];
						o1 = gt[(uint)vi[5]];
						o2 = gt[(uint)vi[6]];
						op[3] = o0;
						op[4] = o1;
						op[5] = o2;
					}

					ip += VectorF.Count;
					op += VectorF.Count - VectorF.Count / 4;
				}

				ipe += VectorF.Count;
				while (ip < ipe)
				{
					op[0] = gt[(uint)FixToUQ15One(ip[0])];
					op[1] = gt[(uint)FixToUQ15One(ip[1])];
					op[2] = gt[(uint)FixToUQ15One(ip[2])];

					ip += 4;
					op += 3;
				}
			}
		}
	}
}