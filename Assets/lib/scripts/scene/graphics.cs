using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

public class Graphics : MonoBehaviour {
	static List<string> Borders;
	static List<string> Speeches;

	static Graphics() {
		Borders = new List<string>();
		Speeches = new List<string>();
		string path = Application.dataPath + "/lib/Pokémon Essentials v17.2 2017-10-15/Graphics/Windowskins";
		string[] files = Directory.GetFiles(path);
		string BorderPattern = @".*choice.*\.png";
		string SpeechPattern = @".*speech.*\.png";
		Regex BorderRegex = new Regex(BorderPattern);
		Regex SpeechRegex = new Regex(SpeechPattern);
		foreach (string file in files) {
			if (BorderRegex.IsMatch(file)) {
				Borders.Add(file);
			}
			if (SpeechRegex.IsMatch(file)) {
				Speeches.Add(file);
			}
		}
	}

	public static List<string> GetBorders() {
		return Borders;
	}
	public static List<string> GetSpeeches() {
		return Speeches;
	}

	public static Texture2D LoadTexture(string path) {
		byte[] fileData = File.ReadAllBytes(path);
		Texture2D retVal = new Texture2D(1,1);
		retVal.LoadImage(fileData);
		return retVal;
	}

	public static void FadeOutInWithMusic(int t, Action f) {
		f();
	}

	public static void FadeOut(int t, Action f) {
		f();
	}

}