using VigilAgent.Api.Services;

await NewMethod();

static async Task NewMethod()
{
    var agent = new VigilAgent.Api.Services.VigilAgent(); // Ensure VigilAgent is a class, not just a namespace  

    while (true)
    {
        Console.Write("User: ");
        string input = Console.ReadLine();

        string response = await agent.HandleUserInput(input);

        Console.WriteLine("Agent: " + response);
    }
}
