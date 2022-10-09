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

namespace Extreme.FloatingPoint
{
	/// <summary>
	/// Enumerates the possible values for the class of a floating-point number.
	/// </summary>
	public enum IeeeClass
	{
		/// <summary>
		/// The value is a signaling NaN (Not a Number).
		/// </summary>
		SignalingNaN,
		/// <summary>
		/// The value is a quiet (non-signaling) NaN (Not a Number).
		/// </summary>
		QuietNaN,
		/// <summary>
		/// The value is positive infinity.
		/// </summary>
		PositiveInfinity,
		/// <summary>
		/// The value is negative infinity.
		/// </summary>
		NegativeInfinity,
		/// <summary>
		/// The value is a normal, positive number.
		/// </summary>
		PositiveNormalized,
		/// <summary>
		/// The value is a normal, negative number.
		/// </summary>
		NegativeNormalized,
		/// <summary>
		/// The value is a denormalized positive number.
		/// </summary>
		PositiveDenormalized,
		/// <summary>
		/// The value is a denormalized negative number.
		/// </summary>
		NegativeDenormalized,
		/// <summary>
		/// The value is positive zero.
		/// </summary>
		PositiveZero,
		/// <summary>
		/// The value is negative zero.
		/// </summary>
		NegativeZero
	}
}