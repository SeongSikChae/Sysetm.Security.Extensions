namespace System.Security.OTP
{
	/// <summary>
	/// One Time Password Timer Interface
	/// </summary>
	public interface IOneTimePasswordTimer
	{
		/// <summary>
		/// Timer Time Synchronization
		/// </summary>
		/// <param name="time"></param>
		void Sync(DateTime time);

		/// <summary>
		/// Timer Gets the time.
		/// </summary>
		/// <returns></returns>
		DateTime GetTime();

		/// <summary>
		/// Timer Adds time by a specific number of units.
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="unit"></param>
		void Plus(int factor, TimeGranularityUnit unit);

		/// <summary>
		/// Timer Subtracts time by a specific unit of time.
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="unit"></param>
		void Minus(int factor, TimeGranularityUnit unit);

		/// <summary>
		/// Basic timer object
		/// </summary>
		static readonly IOneTimePasswordTimer Default = new DefaultOneTimePasswordTimer();

		internal sealed class DefaultOneTimePasswordTimer : IOneTimePasswordTimer
		{
			public DateTime GetTime()
			{
				return DateTime.Now;
			}

			public void Minus(int factor, TimeGranularityUnit unit)
			{
				throw new NotImplementedException();
			}

			public void Plus(int factor, TimeGranularityUnit unit)
			{
				throw new NotImplementedException();
			}

			public void Sync(DateTime time)
			{
				throw new NotImplementedException();
			}
		}
	}
}
