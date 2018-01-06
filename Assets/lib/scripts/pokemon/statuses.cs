public class Statuses {
	public const int SLEEP     = 1;
    public const int POISON    = 2;
    public const int BURN      = 3;
    public const int PARALYSIS = 4;
    public const int FROZEN    = 5;
    public readonly string[] names = {
    	"healthy",
    	"asleep",
    	"poisoned",
    	"burned",
    	"paralyzed",
    	"frozen"
    };

    public string GetName(int id) {
    	return names[id];
    }
}