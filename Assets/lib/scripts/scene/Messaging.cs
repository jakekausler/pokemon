using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Messaging : MonoBehaviour {

	public static void Message(string msg) {
		// TODO
	}

	public static string GetMessage(int type, object o) {
		// TODO
		return "";
	}

	public static void TopRightWindow(string msg) {
		// TODO
	}

	public static bool ConfirmMessage(string msg) {
		// TODO
		return false;
	}

	public static string MessageFreeText(string msg, bool startMsg, int maxLength, int width, Action a) {
		// TODO
		return "";
	}

	public static int Commands(string msg, string[] commands, int cancelVal) {
		// TODO
		return 0;
	}
}

public class MessageTypes {
	public static int FormNames = 0;
	// TODO
}
