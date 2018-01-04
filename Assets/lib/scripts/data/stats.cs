public static class Stats {
	public const int HP       = 0;
    public const int ATTACK   = 1;
    public const int DEFENSE  = 2;
    public const int SPEED    = 3;
    public const int SPATK    = 4;
    public const int SPDEF    = 5;
    public const int ACCURACY = 6;
    public const int EVASION  = 7;
    public static string[] names = {
		"HP",
		"Attack",
		"Defense",
		"Speed",
		"Special Attack",
		"Special Defense",
		"accuracy",
		"evasiveness"
    };
    public static string[] briefNames = {
		"HP",
		"Atk",
		"Def",
		"Spd",
		"SpAtk",
		"SpDef",
		"acc",
		"eva"
    };

    public static string GetName(int id) {
    	return names[id];
    }

    public static string GetNameBrief(int id) {
    	return briefNames[id];
    }
}