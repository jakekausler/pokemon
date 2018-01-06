using System;

public class Seasons {
	public static int GetSeason() {
		DateTime now = Utilities.GetTimeNow();
		if (now.Date < (new DateTime(now.Year, 3, 21).Date) || now.Date > (new DateTime(now.Year, 12, 21)).Date) {
			return 3;
		}
		if (now.Date < (new DateTime(now.Year, 6, 21)).Date) {
			return 0;
		}
		if (now.Date < (new DateTime(now.Year, 9, 21)).Date) {
			return 1;
		}
		return 2;
	}
}