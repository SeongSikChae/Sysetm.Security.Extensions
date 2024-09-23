namespace System.Security.OTP
{
	using Text;

	internal class SamsungServiceModeOTP(string secret, IOneTimePasswordTimer timer) : ICounterBasedTimePassword
	{
		public int Counter { get; private set; } = 4;

		public string GenerateCode()
		{
			if (Counter <= -1)
				throw new Exception("Counter Expired.");
			StringBuilder builder = new StringBuilder(secret);
			builder.Append(GetDateString(Counter--));
			return MakeHashCode(builder).ToString();
		}

		private string GetDateString(int min)
		{
			DateTime utcNow = timer.GetTime().ToUniversalTime();
			utcNow = utcNow.AddMinutes(-min);
			StringBuilder builder = new StringBuilder($"{(utcNow.Year - 2000):D2}");
			builder = builder.Append($"{utcNow.Month:D2}");
			builder = builder.Append($"{utcNow.Minute:D2}");
			builder = builder.Append($"{utcNow.Day:D2}");
			builder = builder.Append($"{utcNow.Hour:D2}");
			return builder.ToString();
		}

		private static int MakeHashCode(StringBuilder builder)
		{
			int nHashValue = 0;
			for (int i = 0; i < builder.Length; i++)
				nHashValue = (nHashValue << 5) + nHashValue + builder[i];
			if (nHashValue < 0)
				return nHashValue * -1;
			return nHashValue;
		}

		public bool VerifyCode(string expectedCode)
		{
			for (int min = 4; min > -1; min--)
			{
				StringBuilder builder = new StringBuilder(secret);
				builder.Append(GetDateString(min));
				string actualCode = MakeHashCode(builder).ToString();
				if (expectedCode.Equals(actualCode))
					return true;
			}
			return false;
		}

		public void Dispose()
		{
		}
	}
}
