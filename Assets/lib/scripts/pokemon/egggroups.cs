public class EggGroups {
	public const int Undiscovered = 0; // NoEggs, None, NA
	public const int Monster      = 1;
	public const int Water1       = 2;
	public const int Bug          = 3;
	public const int Flying       = 4;
	public const int Field        = 5; // Ground
	public const int Fairy        = 6;
	public const int Grass        = 7; // Plant
	public const int Humanlike    = 8; // Humanoid, Humanshape, Human
	public const int Water3       = 9;
	public const int Mineral      = 10;
	public const int Amorphous    = 11; // Indeterminate
	public const int Water2       = 12;
	public const int Ditto        = 13;
	public const int Dragon       = 14;
	public readonly string[] names = {
		"Undiscovered",
		"Monster",
		"Water 1",
		"Bug",
		"Flying",
		"Field",
		"Fairy",
		"Grass",
		"Human-like",
		"Water 3",
		"Mineral",
		"Amorphous",
		"Water 2",
		"Ditto",
		"Dragon"
	};

	public int MaxValue() {
		return 14;
	}

	public int GetCount() {
		return 15;
	}

	public string GetName(int id) {
		return names[id];
	}
}