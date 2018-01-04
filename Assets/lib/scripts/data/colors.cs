public static class Colors {
  public const int Red    = 0;
  public const int Blue   = 1;
  public const int Yellow = 2;
  public const int Green  = 3;
  public const int Black  = 4;
  public const int Brown  = 5;
  public const int Purple = 6;
  public const int Gray   = 7;
  public const int White  = 8;
  public const int Pink   = 9;
  
  private static string[] names = {
    "Red",
    "Blue",
    "Yellow",
    "Green",
    "Black",
    "Brown",
    "Purple",
    "Gray",
    "White",
    "Pink"
  };

  public static int maxValue() {
    return 9;
  }

  public static int getCount() {
    return 10;
  }

  public static string GetName(int id) {
    return names[id];
  }
}