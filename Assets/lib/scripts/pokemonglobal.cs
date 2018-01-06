public static class PokemonGlobal {
	public static int playerID;
	public static BattleTrainer Trainer;
	public static PokemonStorage Storage;
	public static PokemonBag Bag;
	public static List<Mail> Mailbox;
	public static PokemonTemp PkmnTemp;
	public static GameMap Map;
	public static bool surfing;
	public static bool bicycle;
	public static int playerX;
	public static int playerY;
	public static int direction;
	public static int[] escapePoint;
	public static int repel;
	public static bool INTERNAL;

	public static void UnlockWallpaper(int idx) {
		// TODO
	}

	public static void LockWallpaper(int idx) {
		// TODO
	}

	public static int GetEnvironment() {
		// TODO
		return 0;
	}

	public static int GetTerrainTag() {
		// TODO
		return 0;
	}

	public static int FacingTerrainTag() {
		// TODO
		return 0;
	}

	public static bool HasDependentEvents() {
		// TODO
		return false;
	}

	public static bool Passable(int x, int y, int direction) {
		// TODO
		return false;
	}

	public static void TransferPlayer() {
		// TODO
	}

	public static void MountBike() {
		// TODO
	}

	public static void DismountBike() {
		// TODO
	}
}