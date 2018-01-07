using System;
using System.Collections.Generic;

public class GameMap {
	public int MapID;
	public GameEvent[] events;
	public Dictionary<int, bool> switches;
	public bool whiteFluteUsed;
	public bool blackFluteUsed;

	public bool GetSwitch(int n) {
		return switches[n];
	}

	public void AutoPlay() {
		// TODO
	}

	public void Refresh() {
		// TODO
	}

	public class GameEvent {
		public string name;
		public int x;
		public int y;
		public int id;
		public int value;
		public Dictionary<int, bool> switches;
		
		public bool GetSwitch(int n) {
			return switches[n];
		}
	}
}