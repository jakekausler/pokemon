public class GameMap {
	public int MapID;
	public GameEvent[] events;
	public Dictionary<int, bool> switches;

	public static GetSwitch(int n) {
		return switches[n];
	}

	public class GameEvent {
		public string name;
		public int x;
		public int y;
		public int id;
		public int value;
		public Dictionary<int, bool> switches;
		
		public static GetSwitch(int n) {
			return switches[n];
		}
	}
}