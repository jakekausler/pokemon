using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Messaging : MonoBehaviour {
	string message = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
	bool enableMessage = false;

	Texture2D backgroundTexture;

	GUIStyle style;

	public void OnGUI() {
		if (enableMessage) {
			if (style == null) {
				setStyle();
			}
			if (backgroundTexture == null) {
				changeBackground(0);
			}
			int width = 1000;
			int height = 200;
			Rect boxLocation = new Rect((Screen.width-width)/2, Screen.height-height, width, height);
			Rect textLocation = new Rect(boxLocation.x + Screen.width/25*2, boxLocation.y + Screen.height/25, width-Screen.width/25*4, height-Screen.width/25*2);
			GUI.DrawTexture(boxLocation, backgroundTexture);
			GUI.TextArea(textLocation, message, style);
		}
	}

	public void showMessage() {
		enableMessage = true;
	}

	public void hideMessage() {
		enableMessage = false;
	}

	public bool canScrollUp() {
		return false;
	}

	public void scrollUp() {

	}

	public bool canScrollDown() {
		return false;
	}

	public void scrollDown() {

	}

	public void changeMessage(string message) {
		this.message = message;
	}

	void changeBackground(int id) {
		if (id >=0 && id < Graphics.GetBorders().Count) {
			backgroundTexture = Graphics.LoadTexture(Graphics.GetBorders()[id]);
		} else {
			backgroundTexture = EditorGUIUtility.whiteTexture;
		}
	}

	void setStyle() {
		style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = 40;
		style.wordWrap = true;
	}
}
