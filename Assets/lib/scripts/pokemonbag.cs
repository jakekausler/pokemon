public class PokemonBag {
	int lastPocket;
	List<List<int[]>> pockets;
	List<int> registeredItems;
	int[] registerdIndex;

	public string[] PocketNames() {
		return Settings.PocketNames();
	}

	public int NumPockets() {
		return PocketNames().Length;
	}

	public PokemonBag() {
		lastPocket = 1;
		pockets = new List<int>();
		for (int i=0; i < NumPockets(); i++) 
		{
			pockets.Add(new List<int[]>());
			choices.Add(0);
		}
		registeredItems = new List<int>();
		registerdIndex = new int[3]{0,0,1};
	}

	public void Rearrange() {
		if (pockets.Count != NumPockets()) {
			List<List<int>> newPockets = new List<int>();
			for (int i=0; i < NumPockets(); i++) 
			{
				newPockets.Add(new List<int[]>());
				if (choices[i] < 0) {
					choices[i] = 0;
				}
			}
			for (int i=0; i < (int)Math.Min(pockets.Count, NumPockets()); i++) 
			{
				for (int j=0; j < pockets[i].Count; j++) {
					int p = GetPocket(pockets[i][j][0]);
					newPockets[p].Add(pockets[i][j]);
				}
			}
			pockets = newPockets;
		}
	}

	public void Clear() {
		for (int i=0; i < pockets.Count; i++) 
		{
			pockets[i] = new List<int[]>();
		}
	}

	public List<List<int[]>> GetPockets() {
		Rearrange();
		return pockets;
	}

	public int MaxPocketSize(int pocket) {
		int maxsize = Settings.MAX_POCKET_SIZE[pocket];
		if (maxsize < 0) {
			return -1;
		}
		return maxsize;
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