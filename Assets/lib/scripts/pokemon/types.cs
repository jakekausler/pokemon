using System;

public static class Types {
	public const int NORMAL = 0;
	public const int FIGHTING = 1;
	public const int FLYING = 2;
	public const int POISON = 3;
	public const int GROUND = 4;
	public const int ROCK = 5;
	public const int BUG = 6;
	public const int GHOST = 7;
	public const int STEEL = 8;
	public const int QMARKS = 9;
	public const int FIRE = 10;
	public const int WATER = 11;
	public const int GRASS = 12;
	public const int ELECTRIC = 13;
	public const int PSYCHIC = 14;
	public const int ICE = 15;
	public const int DRAGON = 16;
	public const int DARK = 17;
	public const int FAIRY = 18;

	public static string[] TypeNames = {
		"NORMAL",
		"FIGHTING",
		"FLYING",
		"POISON",
		"GROUND",
		"ROCK",
		"BUG",
		"GHOST",
		"STEEL",
		"QMARKS",
		"FIRE",
		"WATER",
		"GRASS",
		"ELECTRIC",
		"PSYCHIC",
		"ICE",
		"DRAGON",
		"DARK",
		"FAIRY"
	};

	public static int GetValueFromName(string type) {
		for (int i=0; i<TypeNames.Length; i++) {
			if (TypeNames[i] == type) {
				return i;
			}
		}
		return -1;
	}

	private const string path = "Assets/lib/data/types.json";

	public static Type GetType(int id) {
		string json = System.IO.File.ReadAllText(path);
		Type[] types = JsonHelper.FromJson<Type>(json);
		for (int i = 0; i < types.Length; i++) {
			if (types[i].Id == id) {
				return types[id];
			}
		}
		return null;
	}

	public static string GetName(int id) {
		Type t = GetType(id);
		if (t == null) {
			return "";
		}
		return t.Name;
	}

	public static int MaxValue() {
		return 18;
	}

	[Serializable]
	public class Type {
		public string Name;
		public string InternalName;
		public string[] Immunities;
		public string[] Weaknesses;
		public string[] Resistances;
		public int Id;
		public bool IsPseudoType;
		public bool IsSpecialType;
	}

	public static bool IsPseudoType(int type) {
		return GetType(type).IsPseudoType;
	}

	public static bool IsSpecialType(int type) {
		return GetType(type).IsSpecialType;
	}

	public static int[] TypeData = new int[324]{2, 2, 2, 2, 2, 1, 2, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 2, 1, 1, 2, 4, 1, 0, 4, 2, 2, 2, 2, 2, 1, 4, 2, 4, 2, 4, 2, 2, 2, 1, 4, 2, 1, 2, 2, 2, 4, 1, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 0, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 0, 4, 2, 4, 1, 2, 4, 2, 4, 2, 1, 4, 2, 2, 2, 2, 2, 1, 4, 2, 1, 2, 4, 2, 1, 2, 4, 2, 2, 2, 2, 4, 2, 2, 2, 1, 1, 1, 2, 2, 2, 1, 1, 2, 1, 2, 4, 2, 4, 2, 2, 4, 0, 2, 2, 2, 2, 2, 2, 4, 1, 2, 2, 2, 2, 2, 4, 2, 2, 1, 2, 2, 2, 2, 2, 4, 2, 2, 1, 2, 1, 1, 2, 1, 2, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 4, 2, 4, 2, 1, 1, 4, 2, 2, 4, 1, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 4, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1, 1, 4, 4, 1, 2, 1, 2, 1, 4, 1, 2, 2, 2, 1, 2, 2, 2, 4, 2, 0, 2, 2, 2, 2, 2, 2, 4, 1, 1, 2, 2, 1, 2, 2, 4, 2, 4, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 2, 4, 2, 4, 2, 2, 2, 1, 2, 1, 1, 4, 2, 2, 1, 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 4, 2, 2, 1, 2, 2, 2, 2, 2, 4, 1, 2, 2, 2, 2, 2, 4, 2, 2, 1};

	public static int GetEffectiveness(int attackType, int opponentType) {
		if (opponentType < 0) {
			return 2;
		}
		return TypeData[attackType*(MaxValue()+1)+opponentType];
	}

	public static int GetCombinedEffectiveness(int attackType, int opponentType1, int opponentType2=-1, int opponentType3=-1) {
		int mod1 = GetEffectiveness(attackType, opponentType1);
		int mod2 = 2;
		int mod3 = 3;
		if (opponentType2 >= 0 && opponentType1 != opponentType2) {
			mod2 = GetEffectiveness(attackType, opponentType2);
		}
		if (opponentType3 >= 0 && opponentType1 != opponentType3 && opponentType2 != opponentType3) {
			mod3 = GetEffectiveness(attackType, opponentType3);
		}
		return mod1*mod2*mod3;
	}

	public static bool IsIneffective(int attackType, int opponentType1, int opponentType2=-1, int opponentType3=-1) {
		return 0==GetCombinedEffectiveness(attackType, opponentType1, opponentType2, opponentType3);
	}

	public static bool IsNotVeryEffective(int attackType, int opponentType1, int opponentType2=-1, int opponentType3=-1) {
		int e = GetCombinedEffectiveness(attackType, opponentType1, opponentType2, opponentType3);
		return e>0 && e<8;
	}

	public static bool IsNormalEffective(int attackType, int opponentType1, int opponentType2=-1, int opponentType3=-1) {
		return 8==GetCombinedEffectiveness(attackType, opponentType1, opponentType2, opponentType3);
	}

	public static bool IsSuperEffective(int attackType, int opponentType1, int opponentType2=-1, int opponentType3=-1) {
		return 8<GetCombinedEffectiveness(attackType, opponentType1, opponentType2, opponentType3);
	}
}