using System.Diagnostics;

namespace System.Security.OTP.Tests
{
	[TestClass]
	public class GoogleOTPTests
	{
		[TestMethod]
        public void GoogleOTPTest()
		{
			MockOneTimePasswordTimer timer = new MockOneTimePasswordTimer(new DateTime(2024, 07, 12, 14, 05, 00));
			using GoogleOTP otp = new GoogleOTP(timer, "AAAAAAAAAAAAAAAA");
			Assert.AreEqual("095428", otp.GenerateCode());
			Assert.AreEqual(30, otp.GetRemainingSeconds(timer.GetTime()));
			timer.Plus(10, TimeGranularityUnit.SECONDS);
			Assert.AreEqual(20, otp.GetRemainingSeconds(timer.GetTime()));
			Assert.IsTrue(otp.VerifyCode("095428"));
			Assert.IsFalse(otp.VerifyCode("000000"));
			Trace.WriteLine(otp.ExportUrl("TEST", "TEST"));
			timer.Plus(21, TimeGranularityUnit.SECONDS);
			Assert.AreNotEqual("095428", otp.GenerateCode());
			Assert.IsTrue(otp.VerifyCode("095428"));
			timer.Plus(31, TimeGranularityUnit.SECONDS);
			Assert.AreNotEqual("095428", otp.GenerateCode());
			Assert.IsFalse(otp.VerifyCode("095428"));
		}

		private sealed class MockOneTimePasswordTimer(DateTime time) : IOneTimePasswordTimer
		{
			private DateTime time = time;

			public DateTime GetTime()
			{
				return time;
			}

			public void Minus(int factor, TimeGranularityUnit unit)
			{
				time = time.Previous(unit, factor);
			}

			public void Plus(int factor, TimeGranularityUnit unit)
			{
				time = time.Next(unit, factor);
			}

			public void Sync(DateTime time)
			{
				this.time = time;
			}
		}
	}
}