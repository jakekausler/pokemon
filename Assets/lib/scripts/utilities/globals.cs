using System;
using System.Collections.Generic;

public static class Globals {
	public static int FATEFUL_ENCOUNTER_SWITCH = 32;
	public static int NO_MONEY_LOSS = 33;
	/**
	 - Contains global flags
	 - Contains global variables
	**/
	public static Dictionary<int, bool> switches;
	public static Dictionary<int, string> variables;

	/**
	 - Get the value of a switch at position n
	**/
	public static bool getSwitch(int n) {
		return switches[n];
	}

	public static void setSwitch(int n, bool b) {
		switches[n] = b;
	}

	/**
	 - Get the value of a variable at position n
	**/
	public static string getVariable(int n) {
		return variables[n];
	}

	public static void setVariable(int n, string s) {
		variables[n] = s;
	}
}