namespace Client;

internal class Program
{
    static async Task Main()
    {
        
        Console.MainSetup();

        _ = log();

        await Task.Delay(-1);
    }

    static async Task log()
    {
        int it = 1;
        while (it < 40)
        {
            await Task.Delay(500);

            
        
            Console.AddLine($"test {it}");

            it++;
        }
        

    }
}