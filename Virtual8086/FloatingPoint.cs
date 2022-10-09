// Floating Point Example.
// 
// Copyright 2005, Extreme Optimization. (http://www.extremeoptimization.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer. 
//  * Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution. 
//  * Neither the name of Extreme Optimization nor the names of its contributors 
//    may be used to endorse or promote products derived from this software
//    without specific prior written permission. 
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace Extreme.FloatingPoint
{
	/// <summary>
	/// Summary description for Ieee.
	/// </summary>
	public sealed class FloatingPoint
	{
		#region Private instance members
		private FloatingPoint() {}

		private static readonly double NegativeZero = BitConverter.Int64BitsToDouble(unchecked (8*0x1000000000000000));
		private const Double MinDouble = 4.94065645841247e-324;

		private const long _signMask = -1-0x7FFFFFFFFFFFFFFF;
		private const long _signClearMask = 0x7FFFFFFFFFFFFFFF;

		private const long _mantissaMask = 0xFFFFFFFFFFFFF;
		private const long _mantissaClearMask = _signMask | _exponentMask;

		private const long _exponentMask = 0x7FF0000000000000;
		private const long _exponentClearMask = _signMask | _mantissaMask;

		private const int _bias = 1023;
		private const int _mantissaBits = 52;
		#endregion

		#region Methods for getting parts of a double's binary representation.
		/// <summary>
		/// Returns the mantissa part of a <see cref="double"/>.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>An <see cref="long"/> value containing the 53 bits of the mantissa.
		/// </returns>
		/// <remarks>The implicit leading bit is not included.</remarks>
		public static long Mantissa(double x)
		{
			return BitConverter.DoubleToInt64Bits(x) & _mantissaMask;
		}

		/// <summary>
		/// Returns the mantissa of a floating-point number.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>A number between 1 and 2, or zero if <paramref name="x"/> is zero.</returns>
		public static double Significand(double x)
		{
			if (Double.IsNaN(x))
				return x;
			if (Double.IsInfinity(x))
				return x;
			int exponent = BiasedExponent(x);
			long mantissa = Mantissa(x);
			// We must treat 0 and denormalized numbers separately.
			if (exponent == 0)
			{
				// If x is zero, return zero.
				if (mantissa == 0)
					return 0.0;
				// else, shift the mantissa to the left until it is 53 bits long...
				while (mantissa < _mantissaMask)
					mantissa <<= 1;
				// ...and chop of the leading bit:
				mantissa &= _mantissaMask;
			}
			return BitConverter.Int64BitsToDouble((_bias << 52) + mantissa);
		}

		/// <summary>
		/// Returns the unbiased exponent of a floating-point number.
		/// </summary>
		/// <param name="x">A number.</param>
		/// <returns>The unbiased exponent of <paramref name="x"/>.</returns>
		/// <remarks>The unbiased exponent is obtained by subtracting the bias from the biased exponent
		/// taken directly from the binary representation of <paramref name="x"/>. Special values are not
		/// considered.</remarks>
		public static int Exponent(double x)
		{
			return (int)((BitConverter.DoubleToInt64Bits(x) & _exponentMask) >> 52) - _bias;
		}

		/// <summary>
		/// Returns the biased exponent of a floating-point number.
		/// </summary>
		/// <param name="x">A number.</param>
		/// <returns>The biased exponent of <paramref name="x"/>.</returns>
		/// <remarks>The return value is taken directly from the binary representation of <paramref name="x"/>. 
		/// Special values are not considered.</remarks>
		public static int BiasedExponent(double x)
		{
			return (int)((BitConverter.DoubleToInt64Bits(x) & _exponentMask) >> 52);
		}

		/// <summary>
		/// Returns the value of the sign bit of a number.
		/// </summary>
		/// <param name="x">A number.</param>
		/// <returns>The sign bit taken directly from the binary representation of <paramref name="x"/>.</returns>
		public static int SignBit(double x)
		{
			return ((BitConverter.DoubleToInt64Bits(x) & _signMask) != 0) ? 1 : 0;
		}
		#endregion

		#region Implementation of the IEEE-754 "Recommended functions"
		/// <summary>
		/// Returns the IEEE floating-point class of a number.
		/// </summary>
		/// <param name="x">A <<see cref="double"/>.</param>
		/// <returns>An <see cref="IeeeClass"/> value.</returns>
		public static IeeeClass Class(double x)
		{
			long bits = BitConverter.DoubleToInt64Bits(x);
			bool positive = (bits >= 0);
			bits = bits & _signClearMask;
			if (bits >= 0x7ff0000000000000)	// overflow / NAN
			{
				if ((bits & _mantissaMask) == 0) // Infinity
				{
					if (positive)
						return IeeeClass.PositiveInfinity;
					else
						return IeeeClass.NegativeInfinity;
				}
				else
				{
					if ((bits & _mantissaMask) < 0x8000000000000)
						return IeeeClass.QuietNaN;
					else
						return IeeeClass.SignalingNaN;
				}
			}
			else if (bits < 0x0010000000000000) // 0 or denormalized
			{
				if (bits == 0)
				{
					if (positive)
						return IeeeClass.PositiveZero;
					else
						return IeeeClass.NegativeZero;
				}
				else
				{
					if (positive)
						return IeeeClass.PositiveDenormalized;
					else
						return IeeeClass.NegativeDenormalized;
				}				
			}
			else
			{
				if (positive)
					return IeeeClass.PositiveNormalized;
				else
					return IeeeClass.NegativeNormalized;
			}
		}

		/// <summary>
		/// Copies the sign of a number.
		/// </summary>
		/// <param name="sizeValue">The number whose sign to copy.</param>
		/// <param name="signValue">The number whose value to copy.</param>
		/// <returns>A <see cref="double"/> with the magnitude of <paramref name="sizeValue"/>
		/// and the sign of <paramref name="sizeValue"/>.</returns>
		public static double CopySign(double sizeValue, double signValue)
		{
			// This is straightforward bit manipulation. Copy the first bit of signValue to sizeValue.
			return BitConverter.Int64BitsToDouble(
				(BitConverter.DoubleToInt64Bits(sizeValue) & _signClearMask)
				| (BitConverter.DoubleToInt64Bits(signValue) & _signMask));
		}

		/// <summary>
		/// Gets a value that indicates whether a number is finite.
		/// </summary>
		/// <param name="x">A real number.</param>
		/// <returns><see langword="true"/> if the number <paramref name="x"/> is finite;
		/// otherwise <see langword="false"/>.</returns>
		public bool IsFinite(double x)
		{
			// Check the exponent part. If it is all 1's then we have infinity/NaN
			long bits = BitConverter.DoubleToInt64Bits(x);
			return ((bits & _exponentMask) == _exponentMask);
		}

		/// <summary>
		/// Returns the unbiased exponent of a number.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>The unbiased exponent of a number.</returns>
		/// <remarks>This method returns an exponent <c>e</c> such that <c>1 <= 2<sup>-e</sup>x < 2</c>. This is true
		/// even for denormalized numbers. If <paramref name="x"/> is zero, then <see cref="Double.NegativeInfinity"/> is returned.</remarks>
		public static double Logb(double x)
		{
			// Treat special cases first.
			if (double.IsNaN(x))
				return x;
			if (double.IsInfinity(x))
				return double.PositiveInfinity;
			if (x == 0)
				return double.NegativeInfinity;

			int e = BiasedExponent(x);
			// See if the number is denormalized, and take appropriate action.
			if (e == 0)
			{
				e = -1074;
				// Get the mantissa without the sign.
				long bits = BitConverter.DoubleToInt64Bits(x) & _signClearMask;
				// We covered the case where bits = 0 already.
				do
				{
					bits >>= 1;
					e++;
				}
				while (bits > 0);
				return e;
			}

			// e was biased, so subtract the bias to get the unbiased exponent.
			return e - _bias;
		}

		/// <summary>
		/// Gets the next floating-point number in the direction of another number.
		/// </summary>
		/// <param name="fromValue">The starting point.</param>
		/// <param name="toValue">The value indicating the direction in which to find
		/// the next number.</param>
		/// <returns></returns>
		public static double NextAfter(double fromValue, double toValue)
		{
			// If toValue equals fromValue, we have no direction to go in, so we return fromValue.
			if (fromValue == toValue)
				return fromValue;

			// NaN's return themselves.
			if (double.IsNaN(fromValue))
				return fromValue;
			if (double.IsNaN(toValue))
				return toValue;

			// Infinities always remain infinite.
			if (double.IsInfinity(fromValue))
				return fromValue;

			// Handle the special case 0.
			if (fromValue == 0)
				return (toValue > 0) ?  MinDouble : -MinDouble;

			// All other cases are handled by incrementing or decrementing the bits value.
			// Transitions to infinity, to denormalized, and to zero are all taken care of this way.
			long bits = BitConverter.DoubleToInt64Bits(fromValue);
			// A xor here avoids nesting conditionals. We have to increment if
			// fromValue lies between 0 and toValue.
			if ((fromValue > 0) ^ (fromValue > toValue))
				bits++;
			else
				bits--;
			return BitConverter.Int64BitsToDouble(bits);
		}

		/// <summary>
		/// Returns a number scaled by a power of 2.
		/// </summary>
		/// <param name="x">A number.</param>
		/// <param name="n">The integer exponent of the scale factor.</param>
		/// <returns>The value 2<sup><paramref name="n"/></sup><paramref name="x"/>.</returns>
		/// <remarks>.</remarks>
		public static double Scalb(double x, int n)
		{
			// Treat special cases first.
			if (x == 0 || double.IsInfinity(x) || double.IsNaN(x))
				return x;
			if (n == 0)
				return x;

			int e = BiasedExponent(x);
			long mantissa = Mantissa(x);
			long sign = ((x > 0) ? 0 : _signMask);

			// Is x denormalized?
			if (e == 0)
			{
				if (n < 0)
				{
					// n negative means we have to shift the mantissa -n bits to the right.
					mantissa >>= -n;
					return BitConverter.Int64BitsToDouble(sign | mantissa);
				}
				else
				{
					// n positive means we need to shift to the left until we get a normalized number...
					// if we get there, that is.
					while (mantissa <= _mantissaMask && n > 0)
					{
						mantissa <<= 1;
						n--;
					}
					if (mantissa > _mantissaMask)
						n++;
					// The value of n is now the biased exponent.

					// Does the result overflow?
					if (n > 2*_bias)
						return (x > 0) ? double.PositiveInfinity : double.NegativeInfinity;

					// n is the biased exponent of the result.
					return BitConverter.Int64BitsToDouble(sign | ((long)n << 52) | (mantissa & _mantissaMask));
				}
			}

			// If we get here, we know x is normalized.
			// Do scaling. e becomes the biased exponent of the result.
			e = e + n;

			// Check for 0 or denormalized.
			if (e < 0)
			{
				mantissa = ((1L << 52) + mantissa) >> (1-e);
				return BitConverter.Int64BitsToDouble(sign | mantissa);
			}

			// Check for overflow.
			if (e > 2*_bias)
				return (x > 0) ? double.PositiveInfinity : double.NegativeInfinity;

			// If we're here, the result is normalized.
			long bits = sign | ((long)e << 52) | mantissa;
			return BitConverter.Int64BitsToDouble(bits);
		}

		/// <summary>
		/// Returns a value that indicates whether two values are unordered.
		/// </summary>
		/// <param name="x">A number.</param>
		/// <param name="y">A number.</param>
		/// <returns><see langword="true"/> if either <paramref name="x"/> or <paramref name="y"/> is <see cref="Double.NaN"/>;
		/// otherwise <see langword="false"/>.</returns>
		public static bool Unordered(double x, double y)
		{
			return double.IsNaN(x) || double.IsNaN(y);
		}
		#endregion		


	}
}