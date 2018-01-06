using System;
using System.Collections.Generic;

public class PokemonBox {
	public Pokemon[] pokemon;
	public string name;
	public int background;

	public PokemonBox(string name, int maxPokemon=30) {
		pokemon = new Pokemon[maxPokemon];
		this.name = name;
		background = 0;
	}

	public int Length() {
		return pokemon.Length;
	}

	public int NonNullItems() {
		int t = 0;
		for (int i=0; i<pokemon.Length; i++) {
			if (pokemon[i] != null) {
				t++;
			}
		}
		return t;
	}

	public bool Full() {
		return Length() == NonNullItems();
	}

	public bool Empty() {
		return NonNullItems() == 0;
	}

	public Pokemon GetAt(int i) {
		return pokemon[i];
	}

	public void SetAt(int i, Pokemon p) {
		pokemon[i] = p;
	}

	public void Clear() {
		for (int i=0; i<pokemon.Length; i++) {
			pokemon[i] = null;
		}
	}
}

public class PokemonStorage {
	public List<PokemonBox> boxes;
	public int currentBox;
	public Dictionary<int, bool> unlockedWallpapers;
	public const int BASIC_WALLPAPER = 16;

	public PokemonStorage(int maxBoxes=Settings.STORAGE_BOXES, int maxPokemon=30) {

	}

	public string[] AllWallpapers() {
		return null;
	}

	public int[] UnlockedWallpapers() {
		return null;
	}

	public Tuple<string, int> AvailableWallpapers() {
		return null;
	}

	public bool IsAvailableWallpaper(int i) {
		return true;
	}

	public Pokemon[] Party() {
		return null;
	}

	public int MaxBoxes() {
		return 0;
	}

	public bool Full() {
		return true;
	}

	public int FirstFreePos(int box) {
		return 0;
	}

	public PokemonBox GetBoxAt(int x) {
		return null;
	}

	public Pokemon GetPokemonAt(int x, int y=-1) {
		return null;
	}

	public void SetPokemonAt(int x, int y, Pokemon value) {

	}

	public bool Copy(int boxDst, int indexDst, int boxSrc, int indexSrc) {
		return true;
	}

	public bool Move(int boxDst, int indexDst, int boxSrc, int indexSrc) {
		return true;
	}

	public bool MoveCaughtToParty(Pokemon pkmn) {
		return true;
	}

	public bool MoveCaughtToBox(Pokemon pkmn, int box) {
		return true;
	}

	public int StoreCaught(Pokemon pkmn) {
		return 0;
	}

	public void Delete(int box, int inx) {

	}

	public void Clear() {

	}
}

public class RegionalStorage {
	public PokemonStorage[] storages;
	public int lastMap;
	public int rgnMap;

	public RegionalStorage() {

	}

	public PokemonStorage GetCurrentStorage() {
		return null;
	}

	public string[] AllWallpapers() {
		return null;
	}

	public int[] UnlockedWallpapers() {
		return null;
	}

	public Tuple<string, int> AvailableWallpapers() {
		return null;
	}

	public PokemonBox[] Boxes() {
		return null;
	}

	public Pokemon[] Party() {
		return null;
	}

	public int MaxBoxes() {
		return 0;
	}

	public int maxPokemon(PokemonBox box) {
		return 0;
	}

	public bool Full() {
		return true;
	}

	public int GetCurrentBox() {
		return 0;
	}

	public void SetCurrentBox(int v) {
		
	}

	public int FirstFreePos(int box) {
		return 0;
	}

	public PokemonBox GetBoxAt(int x) {
		return null;
	}

	public Pokemon GetPokemonAt(int x, int y=-1) {
		return null;
	}

	public void SetPokemonAt(int x, int y, Pokemon value) {

	}

	public bool Copy(int boxDst, int indexDst, int boxSrc, int indexSrc) {
		return true;
	}

	public bool Move(int boxDst, int indexDst, int boxSrc, int indexSrc) {
		return true;
	}

	public bool MoveCaughtToParty(Pokemon pkmn) {
		return true;
	}

	public bool MoveCaughtToBox(Pokemon pkmn, int box) {
		return true;
	}

	public int StoreCaught(Pokemon pkmn) {
		return 0;
	}

	public void Delete(int box, int inx) {

	}

	public void Clear() {

	}
}