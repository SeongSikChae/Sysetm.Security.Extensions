namespace System.Security.OTP
{
	/// <summary>
	/// Interface for implementing One Time Password
	/// </summary>
	public interface IOneTimePassword : IDisposable
	{
		/// <summary>
		/// Generate a One Time Password.
		/// </summary>
		/// <returns>OTP Code</returns>
		string GenerateCode();

		/// <summary>
		/// Verify your One Time Password.
		/// </summary>
		/// <param name="expectedCode">Expected OTP Code</param>
		/// <returns>Validity</returns>
		bool VerifyCode(string expectedCode);
	}

	/// <summary>
	/// Interface for implementing time-based One Time Password
	/// </summary>
	public interface ITimeBasedOneTimePassword : IOneTimePassword
	{
		/// <summary>
		/// Expiration time (seconds)
		/// </summary>
		int ExpireSeconds { get; }

		/// <summary>
		/// Gets the number of seconds remaining until expiration.
		/// </summary>
		/// <param name="dateTime">Standard time</param>
		/// <returns>Time remaining (seconds)</returns>
		int GetRemainingSeconds(DateTime dateTime);
	}

	/// <summary>
	/// Interface for implementing Counter-based One Time Password
	/// </summary>
	public interface ICounterBasedTimePassword : IOneTimePassword
	{
		/// <summary>
		/// Counter
		/// </summary>
		int Counter { get; }
	}
}
