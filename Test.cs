namespace app;

public class Test
{
    public static void Tester()
    {
        string path = "truc";
        bool is_admin = true;
        switch ((path, is_admin)) 
        {
            case ("truc", true):
                Console.WriteLine("truc, true");
                break;
            case ("", true):
                Console.WriteLine("'', true");
                break;
            case ("truc", false):
                Console.WriteLine("truc, false");
                break;
            case ("", false):
                Console.WriteLine("'', false");
                break;
        }
    }
}