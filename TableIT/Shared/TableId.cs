namespace TableIT.Shared;

public static class TableId
{
    private const string IdLetters = "ACDEFGHJKMNPRSTWXYZ1234679";
    private static Random Random { get; } = new Random();

    public static string Generate(int legnth = 6)
    {
        var letters = new char[legnth];
        for (int i = 0; i < letters.Length; i++)
        {
            letters[i] = IdLetters[Random.Next(IdLetters.Length)];
        }
        return new string(letters);
    }
}
