public class PokemonBag {
	int lastPocket;
	int[] pockets;
	int[] choices;

	public string[] PocketNames() {
		return null;
	}

	public int NumPockets() {
		return 0;
	}

	public PokemonBag() {

	}

	public void Rearrange() {

	}

	public void Clear() {

	}

	public int MaxPocketSize(int pocket) {
		return 0;
	}

	public int GetChoice(int pocket) {
		return 0;
	}

	public void SetChoice(int pocket, int value) {
		
	}

	public int[] GetAllChoices() {
		return null;
	}

	public void SetAllChoices(int[] choices) {
		
	}

	public int Quantity(int item) {
		return 0;
	}

	public bool HasItem(int item) {
		return true;
	}

	public bool CanStore(int item, int qty=1) {
		return true;
	}

	public bool StoreItem(int item, int qty=1) {
		return true;
	}

	public bool ChangeItem(int oldItem, int newItem) {
		return true;
	}

	public bool ChangeQuantity(int pocket, int index, int newQty=1) {
		return true;
	}

	public bool DeleteItem(int item, int qty=1) {
		return true;
	}

	public int[] RegisteredItems() {
		return null;
	}

	public bool IsRegistered(int item) {
		return true;
	}

	public void RegisterItem(int item) {
		
	}

	public void UnregisterItem(int item) {
		
	}

	public int[] RegisteredIndex() {
		return null;
	}
}

public class PCItemStorage {
	public const int MAXSIZE = 50;
	public const int MAXPERSLOT = 999;

	public PCItemStorage() {

	}

	public int GetAt(int i) {
		return 0;
	}

	public void SetAt(int i, int item) {
		
	}

	public int Length() {
		return 0;
	}

	public bool Empty() {
		return true;
	}

	public void Clear() {
		
	}

	public int GetItem(int i) {
		return 0;
	}

	public int GetCount(int i) {
		return 0;
	}

	public int Quantity(int item) {
		return 0;
	}

	public bool CanStore(int item, int qty=1) {
		return true;
	}

	public bool StoreItem(int item, int qty=1) {
		return true;
	}

	public bool DeleteItem(int item, int qty=1) {
		return true;
	}
}

public static class ItemStorageHelper {
	public static int Quantity(int[] items, int maxSize, int item) {
		return 0;
	}

	public static bool DeleteItem(int[] items, int maxSize, int item, int qty) {
		return true;
	}

	public static bool CanStore(int[] items, int maxSize, int maxPerSlot, int item, int qty) {
		return true;
	}

	public static bool StoreItem(int[] items, int maxSize, int maxPerSlot, int item, int qty, bool sorting=false) {
		return true;
	}
}