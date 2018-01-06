public class Natures {
  public const int HARDY   = 0;
  public const int LONELY  = 1;
  public const int BRAVE   = 2;
  public const int ADAMANT = 3;
  public const int NAUGHTY = 4;
  public const int BOLD    = 5;
  public const int DOCILE  = 6;
  public const int RELAXED = 7;
  public const int IMPISH  = 8;
  public const int LAX     = 9;
  public const int TIMID   = 10;
  public const int HASTY   = 11;
  public const int SERIOUS = 12;
  public const int JOLLY   = 13;
  public const int NAIVE   = 14;
  public const int MODEST  = 15;
  public const int MILD    = 16;
  public const int QUIET   = 17;
  public const int BASHFUL = 18;
  public const int RASH    = 19;
  public const int CALM    = 20;
  public const int GENTLE  = 21;
  public const int SASSY   = 22;
  public const int CAREFUL = 23;
  public const int QUIRKY  = 24;
  public readonly string[] names = {
    "Hardy",
    "Lonely",
    "Brave",
    "Adamant",
    "Naughty",
    "Bold",
    "Docile",
    "Relaxed",
    "Impish",
    "Lax",
    "Timid",
    "Hasty",
    "Serious",
    "Jolly",
    "Naive",
    "Modest",
    "Mild",
    "Quiet",
    "Bashful",
    "Rash",
    "Calm",
    "Gentle",
    "Sassy",
    "Careful",
    "Quirky"
  };

  public int MaxValue() {
    return 24;
  }

  public int GetCount() {
    return 25;
  }

  public string GetName(int id) {
    return names[id];
  }
}