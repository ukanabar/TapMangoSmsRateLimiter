using NBomber.CSharp;
using NBomber.Contracts;

/*
 * Randomized test 
 * Select account id from 1,2,5 and 7. So basically some valid and some invalids
 * Select phonumber of random length between 8 to 12
 * Send random number of request per second between 5 and 10
 */

public class Program
{
    private const string BaseUrl = "http://localhost:5077/api/Sms/can-send";

    public static void Main(string[] args)
    {
        var random = new Random();
        var httpClient = new HttpClient();
        var accountIds = new[] { 1, 2 , 5 ,7 };

        var step = Step.Create("Randomized_Test", async context =>
        {
            var accountId = accountIds[random.Next(accountIds.Length)];

            long phoneNumber;
            int phoneNumberLength = random.Next(8, 12);
            if (phoneNumberLength == 10)
            {
                phoneNumber = random.Next(1000000000, int.MaxValue);
            }
            else if (phoneNumberLength < 10)
            {
                phoneNumber = random.Next((int)Math.Pow(10, phoneNumberLength - 1), (int)Math.Pow(10, phoneNumberLength) - 1);
            }
            else
            {
                phoneNumber = (long)(random.Next(1000000000, int.MaxValue) * Math.Pow(10, phoneNumberLength - 10));
            }

            var response = await httpClient.GetAsync($"{BaseUrl}?accountId={accountId}&phoneNumber={phoneNumber}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == (System.Net.HttpStatusCode)429 ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return Response.Ok();
            }
            else
            {
                return Response.Fail(response.StatusCode.ToString());
            }
        });

        int minRequestsPerSec = 5;
        int maxRequestsPerSec = 10;
        int randomRequestsPerSec = random.Next(minRequestsPerSec, maxRequestsPerSec + 1);

        var scenario = ScenarioBuilder.CreateScenario("RandomizedLoadTest", step)
            .WithLoadSimulations(
                Simulation.InjectPerSec(randomRequestsPerSec, TimeSpan.FromSeconds(10))
            );

        NBomberRunner.RegisterScenarios(scenario).Run();
    }
}
