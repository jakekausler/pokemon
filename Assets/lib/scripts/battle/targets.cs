public class Targets {
	public const int SingleNonUser    = 0x00;
	public const int NoTarget         = 0x01;
	public const int RandomOpposing   = 0x02;
	public const int AllOpposing      = 0x04;
	public const int AllNonUsers      = 0x08;
	public const int User             = 0x10;
	public const int BothSides        = 0x20;
	public const int UserSide         = 0x40;
	public const int OpposingSide     = 0x80;
	public const int Partner          = 0x100;
	public const int UserOrPartner    = 0x200;
	public const int SingleOpposing   = 0x400;
	public const int OppositeOpposing = 0x800;

	public static bool HasMultipleTargets(BattleMove move) {
		return move.target == AllOpposing ||
			   move.target == AllNonUsers;
	}

	public static bool TargetsOneOpponent(BattleMove move) {
		return move.target == SingleNonUser ||
			   move.target == RandomOpposing ||
			   move.target == SingleOpposing ||
			   move.target == OppositeOpposing;
	}
}