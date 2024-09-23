namespace System.Text
{
	/// <summary>
	/// Base32 Encoder and Decoder
	/// </summary>
	public static class Base32
	{
		private const string _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		/// <summary>
		/// binary to Base32 String Encoder
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ToBase32(byte[] input)
		{
			ArgumentNullException.ThrowIfNull(input);
			StringBuilder sb = new StringBuilder();
			for (int offset = 0; offset < input.Length;)
			{
				int numCharsToOutput = GetNextGroup(input, ref offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h);
				sb.Append((numCharsToOutput >= 1) ? _base32Chars[a] : '=');
				sb.Append((numCharsToOutput >= 2) ? _base32Chars[b] : '=');
				sb.Append((numCharsToOutput >= 3) ? _base32Chars[c] : '=');
				sb.Append((numCharsToOutput >= 4) ? _base32Chars[d] : '=');
				sb.Append((numCharsToOutput >= 5) ? _base32Chars[e] : '=');
				sb.Append((numCharsToOutput >= 6) ? _base32Chars[f] : '=');
				sb.Append((numCharsToOutput >= 7) ? _base32Chars[g] : '=');
				sb.Append((numCharsToOutput >= 8) ? _base32Chars[h] : '=');
			}
			return sb.ToString();
		}

		/// <summary>
		/// Base32 String to Binary Decoder
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		/// <exception cref="FormatException"></exception>
		public static byte[] FromBase32(string input)
		{
			ArgumentNullException.ThrowIfNull(input);
			ReadOnlySpan<char> trimmedInput = input.AsSpan().TrimEnd('=');
			if (trimmedInput.Length == 0)
				return [];
			byte[] output = new byte[trimmedInput.Length * 5 / 8];
			int bitIndex = 0;
			int inputIndex = 0;
			int outputBits = 0;
			int outputIndex = 0;
			while (outputIndex < output.Length)
			{
				int byteIndex = _base32Chars.IndexOf(char.ToUpperInvariant(trimmedInput[inputIndex]));
				if (byteIndex < 0)
					throw new FormatException();

				int bits = Math.Min(5 - bitIndex, 8 - outputBits);
				output[outputIndex] <<= bits;
				output[outputIndex] |= (byte)(byteIndex >> (5 - (bitIndex + bits)));

				bitIndex += bits;
				if (bitIndex >= 5)
				{
					inputIndex++;
					bitIndex = 0;
				}

				outputBits += bits;
				if (outputBits >= 8)
				{
					outputIndex++;
					outputBits = 0;
				}
			}
			return output;
		}

		private static int GetNextGroup(Span<byte> input, ref int offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h)
		{
			uint b1, b2, b3, b4, b5;
			int retVal = (input.Length - offset) switch
			{
				1 => 2,
				2 => 4,
				3 => 5,
				4 => 7,
				_ => 8,
			};
			b1 = (offset < input.Length) ? input[offset++] : 0U;
			b2 = (offset < input.Length) ? input[offset++] : 0U;
			b3 = (offset < input.Length) ? input[offset++] : 0U;
			b4 = (offset < input.Length) ? input[offset++] : 0U;
			b5 = (offset < input.Length) ? input[offset++] : 0U;

			a = (byte)(b1 >> 3);
			b = (byte)(((b1 & 0x07) << 2) | (b2 >> 6));
			c = (byte)((b2 >> 1) & 0x1f);
			d = (byte)(((b2 & 0x01) << 4) | (b3 >> 4));
			e = (byte)(((b3 & 0x0f) << 1) | (b4 >> 7));
			f = (byte)((b4 >> 2) & 0x1f);
			g = (byte)(((b4 & 0x3) << 3) | (b5 >> 5));
			h = (byte)(b5 & 0x1f);

			return retVal;
		}
	}
}
