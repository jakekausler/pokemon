public class DamageState {
	public int HPLost; // HP lost by opponent, inc. HP lost by a substitute
	public bool Critical; // Critical hit flag
	public int CalculatedDamage; // Calculated damage
	public int TypeModifier; // Type effectiveness
	public bool Substitute; // A substitute took the damage
	public bool FocusBand; // Focus Band used
	public bool FocusSash; // Focus Sash used
	public bool Sturdy; // Sturdy ability used
	public bool Endured; // Damage was endured
	public bool BerryWeakened; // A type-resisting berry was used

	public DamageState() {
		Reset();
	}

	public void Reset() {
		HPLost = 0;
		Critical = false;
		CalculatedDamage = 0;
		TypeModifier = 0;
		Substitute = false;
		FocusBand = false;
		FocusSash = false;
		Sturdy = false;
		Endured = false;
		BerryWeakened = false;
	}
}