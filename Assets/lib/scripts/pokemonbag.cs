using System;
using System.Collections.Generic;

public class PokemonBag {
	int lastPocket;
	int[] choices;
	List<List<int[]>> pockets;
	List<int> registeredItems;
	int registeredItem;
	int[] registerdIndex;

	public string[] PocketNames() {
		return Settings.POCKET_NAMES;
	}

	public int NumPockets() {
		return PocketNames().Length;
	}

	public PokemonBag() {
		lastPocket = 1;
		pockets = new List<List<int[]>>();
		choices = new int[NumPockets()];
		for (int i=0; i < NumPockets(); i++) 
		{
			pockets.Add(new List<int[]>());
			choices[i] = 0;
		}
		registeredItems = new List<int>();
		registeredItem = -1;
		registerdIndex = new int[3]{0,0,1};
	}

	public void Rearrange() {
		if (pockets.Count != NumPockets()) {
			List<List<int[]>> newPockets = new List<List<int[]>>();
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
					int p = Items.GetPocket(pockets[i][j][0]);
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
		if (pocket <= 0 || pocket > NumPockets()) {
			throw new Exception(string.Format("Invalid pocket: {0}", pocket));
		}
		Rearrange();
		return (int)Math.Min(choices[pocket], pockets[pocket].Count-1);
	}

	public void SetChoice(int pocket, int value) {
		if (pocket <= 0 || pocket > NumPockets()) {
			throw new Exception(string.Format("Invalid pocket: {0}", pocket));
		}
		Rearrange();
		if (value < pockets[pocket].Count) {
			choices[pocket] = value;
		}
	}

	public int[] GetAllChoices() {
		int[] ret = new int[NumPockets()];
		for (int i=0; i<ret.Length; i++) 
		{
			ret[i] = choices[i];
			choices[i] = 0;
		}
		return ret;
	}

	public void SetAllChoices(int[] choices) {
		this.choices = choices;
	}

	public int Quantity(int item) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		int pocket = Items.GetPocket(item);
		int maxSize = MaxPocketSize(pocket);
		if (maxSize < 0) {
			maxSize = pockets[pocket].Count;
		}
		return ItemStorageHelper.Quantity(pockets[pocket], maxSize, item);
	}

	public bool HasItem(int item) {
		return Quantity(item) > 0;
	}

	public bool CanStore(int item, int qty=1) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		int pocket = Items.GetPocket(item);
		int maxSize = MaxPocketSize(pocket);
		if (maxSize < 0) {
			maxSize = pockets[pocket].Count;
		}
		return ItemStorageHelper.CanStore(pockets[pocket], maxSize, Settings.BAG_MAX_PER_SLOT, item, qty);
	}

	public bool StoreItem(int item, int qty=1) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		int pocket = Items.GetPocket(item);
		int maxSize = MaxPocketSize(pocket);
		if (maxSize < 0) {
			maxSize = pockets[pocket].Count;
		}
		return ItemStorageHelper.StoreItem(pockets[pocket], maxSize, Settings.BAG_MAX_PER_SLOT, item, qty, true);
	}

	public bool ChangeItem(int oldItem, int newItem) {
		if (oldItem < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", oldItem));
		}
		if (newItem < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", newItem));
		}
		int pocket = Items.GetPocket(oldItem);
		int maxSize = MaxPocketSize(pocket);
		if (maxSize < 0) {
			maxSize = pockets[pocket].Count;
		}
		bool ret = false;
		for (int i=0; i<maxSize; i++) 
		{
			int[] itemSlot = pockets[pocket][i];
			if (itemSlot[0] == oldItem) {
				itemSlot[0] = newItem;
				ret = true;
			}
		}
		return ret;
	}

	public bool ChangeQuantity(int pocket, int index, int newQty=1) {
		if (pocket <= 0 || pocket > NumPockets()-1) {
			return false;
		}
		if (pockets[pocket].Count-1 < index) {
			return false;
		}
		newQty = (int)Math.Min(newQty, MaxPocketSize(pocket));
		pockets[pocket][index][1] = newQty;
		return true;
	}

	public bool DeleteItem(int item, int qty=1) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		int pocket = Items.GetPocket(item);
		int maxSize = MaxPocketSize(pocket);
		if (maxSize < 0) {
			maxSize = pockets[pocket].Count;
		}
		return ItemStorageHelper.DeleteItem(pockets[pocket], maxSize, item, qty);
	}

	public List<int> RegisteredItems() {
		if (registeredItems == null) {
			registeredItems = new List<int>();
		}
		if (registeredItem > 0 && !registeredItems.Contains(registeredItem)) {
			registeredItems.Add(registeredItem);
			registeredItem = -1;
		}
		return registeredItems;
	}

	public bool IsRegistered(int item) {
		return registeredItems.Contains(item);
	}

	public void RegisterItem(int item) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		if (!IsRegistered(item)) {
			registeredItems.Add(item);
		}
	}

	public void UnregisterItem(int item) {
		if (item < 1) {
			throw new Exception(string.Format("Item number {0} is invalid.", item));
		}
		if (IsRegistered(item)) {
			for (int i=0; i<registeredItems.Count; i++) 
			{
				if (registeredItems[i] == item) {
					registeredItems.RemoveAt(i);
					break;
				}
			}
		}
	}

	public int[] RegisteredIndex() {
		if (registerdIndex == null || registerdIndex.Length != 3) {
			registerdIndex = new int[3]{0,0,1};
		}
		return registerdIndex;
	}
}

public class PCItemStorage {
	public const int MAXSIZE = 50;
	public const int MAXPERSLOT = 999;
	List<int[]> items;

	public PCItemStorage() {
		items = new List<int[]>();
	}

	public int[] GetAt(int i) {
		return items[i];
	}

	public void SetAt(int i, int item, int qty=1) {
		items[i] = new int[2]{item, qty};
	}

	public int Length() {
		return items.Count;
	}

	public bool Empty() {
		return Length() == 0;
	}

	public void Clear() {
		items = new List<int[]>();
	}

	public int GetItem(int i) {
		return (i < 0 || i >= Length()) ? 0 : items[i][0];
	}

	public int GetCount(int i) {
		return (i < 0 || i >= Length()) ? 0 : items[i][1];
	}

	public int Quantity(int item) {
		return ItemStorageHelper.Quantity(items, MAXSIZE, item);
	}

	public bool CanStore(int item, int qty=1) {
		return ItemStorageHelper.CanStore(items, MAXSIZE, MAXPERSLOT, item, qty);
	}

	public bool StoreItem(int item, int qty=1) {
		return ItemStorageHelper.StoreItem(items, MAXSIZE, MAXPERSLOT, item, qty);
	}

	public bool DeleteItem(int item, int qty=1) {
		return ItemStorageHelper.DeleteItem(items, MAXSIZE, item, qty);
	}
}

public static class ItemStorageHelper {
	public static int Quantity(List<int[]> items, int maxSize, int item) {
		int ret = 0;
		for (int i=0; i<maxSize; i++) 
		{
			if (i < items.Count && items[i].Length == 2 && items[i][0] == item) {
				int[] itemSlot = items[i];
				ret += itemSlot[1];
			}
		}
		return ret;
	}

	public static bool DeleteItem(List<int[]> items, int maxSize, int item, int qty) {
		if (qty < 0) {
			throw new Exception(string.Format("Invalid value for qty: {0}", qty));
		}
		if (qty == 0) {
			return true;
		}
		bool ret = false;
		List<int> toRemove = new List<int>();
		for (int i=0; i<maxSize; i++) 
		{
			if (i < items.Count && items[i].Length == 2 && items[i][0] == item) {
				int[] itemSlot = items[i];
				int amt = (int)Math.Min(qty, itemSlot[1]);
				itemSlot[1] -= amt;
				qty -= amt;
				if (itemSlot[1] == 0) {
					toRemove.Add(i);
				}
				if (qty == 0) {
					ret = true;
					break;
				}
			}
		}
		for (int i=toRemove.Count-1; i >= 0; i--) 
		{
			items.RemoveAt(toRemove[i]);
		}
		return ret;
	}

	public static bool CanStore(List<int[]> items, int maxSize, int maxPerSlot, int item, int qty) {
		if (qty < 0) {
			throw new Exception(string.Format("Invalid value for qty: {0}", qty));
		}
		if (qty == 0) {
			return true;
		}
		for (int i=0; i<maxSize; i++) 
		{
			if (i >= items.Count || items[i].Length != 2) {
				qty -= (int)Math.Min(qty, maxPerSlot);
				if (qty == 0) {
					return true;
				}
			} else if (items[i][0] == item && items[i][1] < maxPerSlot) {
				int newAmt = items[i][1];
				newAmt = (int)Math.Min(newAmt + qty, maxPerSlot);
				qty -= newAmt - items[i][1];
				if (qty == 0) {
					return true;
				}
			}
		}
		return false;
	}

	public static bool StoreItem(List<int[]> items, int maxSize, int maxPerSlot, int item, int qty, bool sorting=false) {
		if (qty < 0) {
			throw new Exception(string.Format("Invalid value for qty: {0}", qty));
		}
		if (qty == 0) {
			return true;
		}
		for (int i=0; i<maxSize; i++) 
		{
			if (i >= items.Count || items[i].Length != 2) {
				items.Add(new int[2]{item, (int)Math.Min(qty, maxPerSlot)});
				qty -= items[i][1];
				if (sorting) {
					if (Settings.POCKET_AUTO_SORT[Items.GetPocket(item)]) {
						items.Sort();
					}
				}
				if (qty == 0) {
					return true;
				}
			} else if (items[i][0] == item && items[i][1] < maxPerSlot) {
				int newAmt = items[i][1];
				newAmt = (int)Math.Min(newAmt + qty, maxPerSlot);
				qty -= newAmt - items[i][1];
				items[i][1] = newAmt;
				if (qty == 0) {
					return true;
				}
			}
		}
		return false;
	}
}