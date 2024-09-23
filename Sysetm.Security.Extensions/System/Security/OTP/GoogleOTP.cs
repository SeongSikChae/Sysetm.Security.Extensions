namespace System.Security.OTP
{
	using Buffers.Binary;
	using Cryptography;
	using Text;

	/// <summary>
	/// Google Authenticator OTP
	/// </summary>
	/// <param name="timer"></param>
	/// <param name="secret"></param>
	/// <param name="expireSeconds"></param>
	public sealed class GoogleOTP(IOneTimePasswordTimer timer, byte[] secret, int expireSeconds = 30) : ITimeBasedOneTimePassword
	{
		/// <summary>
		/// Base32 SecretKey and ExpireSeconds
		/// </summary>
		/// <param name="secret"></param>
		/// <param name="expireSeconds"></param>
		public GoogleOTP(string secret, int expireSeconds = 30) : this(IOneTimePasswordTimer.Default, secret, expireSeconds) { }
		/// <summary>
		/// Custom OneTimePasswordTimer and Base32 SecretKey and ExpireSeconds
		/// </summary>
		/// <param name="timer"></param>
		/// <param name="secret"></param>
		/// <param name="expireSeconds"></param>
		public GoogleOTP(IOneTimePasswordTimer timer, string secret, int expireSeconds = 30) : this(timer, Base32.FromBase32(secret), expireSeconds) { }

		private readonly PasscodeGenerator passcodeGenerator = new PasscodeGenerator(new HMACSHA1(secret));

		/// <summary>
		/// ExpireSeconds Property
		/// </summary>
		public int ExpireSeconds => expireSeconds;

		/// <summary>
		/// OTP Generate Password
		/// </summary>
		/// <returns></returns>
		public string GenerateCode()
		{
			long t = timer.GetTime().ToTimeSeconds() / ExpireSeconds;
			return passcodeGenerator.Generate(t);
		}

		/// <summary>
		/// Get Remaining Seconds
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>

		public int GetRemainingSeconds(DateTime dateTime)
		{
			return (int)(ExpireSeconds - (dateTime.ToTimeSeconds() % ExpireSeconds));
		}

		/// <summary>
		/// Verify Password
		/// </summary>
		/// <param name="expectedCode"></param>
		/// <returns></returns>
		public bool VerifyCode(string expectedCode)
		{
			long t = timer.GetTime().ToTimeSeconds() / ExpireSeconds;
			return passcodeGenerator.VerifyCode(t, expectedCode);
		}

		/// <summary>
		/// Export Url (Import Authenticator App)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="issuer"></param>
		/// <returns></returns>
		public string ExportUrl(string id, string issuer)
		{
			string str = Base32.ToBase32(secret);
			return $"otpauth://totp/{id}?secret={str}&issuer={issuer}";
		}

		private bool disposedValue;

		/// <summary>
		/// OTP Service Dispose
		/// </summary>
		public void Dispose()
		{
            if (!disposedValue)
            {
				passcodeGenerator.Dispose();
				disposedValue = true;
				GC.SuppressFinalize(this);
			}
		}

		private sealed class PasscodeGenerator(HMAC hmac) : IDisposable
		{
			public string Generate(long state)
			{
				Span<byte> buffer = stackalloc byte[sizeof(long)];
				BinaryPrimitives.WriteInt64BigEndian(buffer, state);

				byte[] hash = hmac.ComputeHash(buffer.ToArray());
				int offset = hash[^1] & 0xF;
				long value = (hash[offset] & 0x7f) << 24
								| (hash[offset + 1] & 0xff) << 16
								| (hash[offset + 2] & 0xff) << 8
								| (hash[offset + 3] & 0xff) % 1000000;

				return Digits(value, 6);
			}

			private static string Digits(long intput, int digitCount)
			{
				long truncatedValue = (long)(intput % Math.Pow(10, digitCount));
				return truncatedValue.ToString().PadLeft(digitCount, '0');
			}

			public bool VerifyCode(long t, string expected)
			{
				IEnumerable<long> enumerable = VerificationWindow.RfcSpecifiedNetworkDelay.GetCandidates(t).ToList();
				foreach (long frame in enumerable)
				{
					string actual = Generate(frame);
					if (expected.Equals(actual))
						return true;
				}
				return false;
			}

			private bool disposedValue;

			public void Dispose()
			{
				if (!disposedValue)
				{
					hmac.Dispose();
					disposedValue = true;
					GC.SuppressFinalize(this);
				}
			}
		}

		private sealed class VerificationWindow(int previous, int future)
		{
			public int Previous => previous;
			public int Future => future;

			public IEnumerable<long> GetCandidates(long initialFrame)
			{
				yield return initialFrame;

				for (int i = 1; i <= Previous; i++)
				{
					var val = initialFrame - i;
					if (val < 0)
						break;
					yield return val;
				}

				for (int i = 1; i <= Future; i++)
					yield return initialFrame + i;
			}

			public static readonly VerificationWindow RfcSpecifiedNetworkDelay = new VerificationWindow(1, 1);
		}
	}
}
