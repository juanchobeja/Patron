namespace WorkerPatron
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddWorkerServices();                    

                })
                .Build()
                .RunAsync();
        }
    }
}
