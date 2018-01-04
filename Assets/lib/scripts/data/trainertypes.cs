using System;

public class TrainerTypes {
	private const string path = "Assets/lib/data/trainertypes.json";

	public static TrainerType GetTrainerType(int id) {
		string json = System.IO.File.ReadAllText(path);
		TrainerType[] types = JsonHelper.FromJson<TrainerType>(json);
		for (int i = 0; i < types.Length; i++) {
			if (types[i].Id == id) {
				return types[i];
			}
		}
		return null;
	}

	public static string GetName(int id) {
		TrainerType i = GetTrainerType(id);
		if (i == null) {
			return "";
		}
		return i.Type;
	}

	[Serializable]
	public class TrainerType {
		public int Id;
		public string InternalName;
		public string Type;
		public int BaseMoney;
		public string BattleBGM;
		public string VictoryBGM;
		public string IntroME;
		public string Gender;
		public int SkillLevel;
		public string SkillCode;
	}
}