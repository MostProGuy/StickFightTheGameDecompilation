namespace InControl.NativeProfile
{
	public class MicrosoftXbox360ControllerMacProfile : Xbox360DriverMacProfile
	{
		public MicrosoftXbox360ControllerMacProfile()
		{
			base.Name = "Microsoft Xbox 360 Controller";
			base.Meta = "Microsoft Xbox 360 Controller on Mac";
			Matchers = new NativeInputDeviceMatcher[4]
			{
				new NativeInputDeviceMatcher
				{
					VendorID = 1118,
					ProductID = 654
				},
				new NativeInputDeviceMatcher
				{
					VendorID = 1118,
					ProductID = 655
				},
				new NativeInputDeviceMatcher
				{
					VendorID = 3695,
					ProductID = 672
				},
				new NativeInputDeviceMatcher
				{
					VendorID = 1118,
					ProductID = 672
				}
			};
		}
	}
}
