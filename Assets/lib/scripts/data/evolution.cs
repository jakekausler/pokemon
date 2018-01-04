public static class Evolution {
	public static int Unknown = 0; // Do not use
	public static int Happiness = 1;
	public static int HappinessDay = 2;
	public static int HappinessNight = 3;
	public static int Level = 4;
	public static int Trade = 5;
	public static int TradeItem = 6;
	public static int Item = 7;
	public static int AttackGreater = 8;
	public static int AtkDefEqual = 9;
	public static int DefenseGreater = 10;
	public static int Silcoon = 11;
	public static int Cascoon = 12;
	public static int Ninjask = 13;
	public static int Shedinja = 14;
	public static int Beauty = 15;
	public static int ItemMale = 16;
	public static int ItemFemale = 17;
	public static int DayHoldItem = 18;
	public static int NightHoldItem = 19;
	public static int HasMove = 20;
	public static int HasInParty = 21;
	public static int LevelMale = 22;
	public static int LevelFemale = 23;
	public static int Location = 24;
	public static int TradeSpecies = 25;
	public static int LevelDay = 26;
	public static int LevelNight = 27;
	public static int LevelDarkInParty	= 28;
	public static int LevelRain = 29;
	public static int HappinessMoveType = 30;
	public static int Custom1 = 31;
	public static int Custom2 = 32;
	public static int Custom3 = 33;
	public static int Custom4 = 34;
	public static int Custom5 = 35;

	public static string[] EVONAMES = new string[36]{"Unknown","Happiness","HappinessDay","HappinessNight","Level","Trade","TradeItem","Item","AttackGreater","AtkDefEqual","DefenseGreater","Silcoon","Cascoon","Ninjask","Shedinja","Beauty","ItemMale","ItemFemale","DayHoldItem","NightHoldItem","HasMove","HasInParty","LevelMale","LevelFemale","Location","TradeSpecies","LevelDay","LevelNight","LevelDarkInParty","LevelRain","HappinessMoveType","Custom1","Custom2","Custom3","Custom4","Custom5"};

	// 0 = no parameter
	// 1 = Positive integer
	// 2 = Item internal name
	// 3 = Move internal name
	// 4 = Species internal name
	// 5 = Type internal name
	public static int[] EVOPARAMS = new int[36]{0, // Unknown (do not use)
		0,0,0,1,0, // Happiness, HappinessDay, HappinessNight, Level, Trade
		2,2,1,1,1, // TradeItem, Item, AttackGreater, AtkDefEqual, DefenseGreater
		1,1,1,1,1, // Silcoon, Cascoon, Ninjask, Shedinja, Beauty
		2,2,2,2,3, // ItemMale, ItemFemale, DayHoldItem, NightHoldItem, HasMove
		4,1,1,1,4, // HasInParty, LevelMale, LevelFemale, Location, TradeSpecies
		1,1,1,1,5, // LevelDay, LevelNight, LevelDarkInParty, LevelRain, HappinessMoveType
		1,1,1,1,1  // Custom 1-5;
	};

	public static int[][] GetEvolvedFormData(int species) {
		return null;
	}

	public static int GetPreviousForm(int species) {
		return 0;
	}

	public static int GetBabySpecies(int species, int item1=-1, int item2=-1) {
		return 0;
	}

	public static int GetMinimumLevel(int species) {
		return 0;
	}

	public static void EvoDebug() {
		
	}

	public static int MiniCheckEvolution(Pokemon pokemon, int evonib, int level, int poke) {
		return 0;
	}

	public static int MiniCheckEvolutionItem(Pokemon pokemon, int evonib, int level, int poke, int item) {
		return 0;
	}

	public static int CheckEvolutionEx(Pokemon pokemon) {
		return 0;
	}

	public static int CheckEvolution(Pokemon pokemon, int item=0) {
		return 0;
	}
}