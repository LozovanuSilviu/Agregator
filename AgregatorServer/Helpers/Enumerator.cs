namespace AgregatorServer.Helpers;

public static class Enumerator
{
    public static int counter = 0;

    public static int Next()
    {
        counter++;
        return counter;
    }
}