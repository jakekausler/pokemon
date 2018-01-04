// Success State used for Battle Arena
public class SuccessState {
	public int TypeModifier;
	public int UseState; // 0 - Not Used, 1 - Failed, 2 - Succeeded
	public bool Protected;
	public int Skill;

	public SuccessState() {
		Clear();
	}

	public void Clear() {
		TypeModifier = 4;
		UseState = 0;
		Protected = false;
		Skill = 0;
	}

	public void UpdateSkill() {
		if (UseState == 1 && !Protected) {
			Skill -= 2;
		} else if (UseState == 2) {
			if (TypeModifier > 4) {
				Skill += 2; // Super Effective
			} else if (TypeModifier == 4) {
				Skill += 1; // Effective
			} else if (TypeModifier >= 1) {
				Skill -= 1; // Not Very Effective
			} else {
				Skill -= 2; // Ineffective
			}
		}
		TypeModifier = 4;
		UseState = 0;
		Protected = false;
	}
}