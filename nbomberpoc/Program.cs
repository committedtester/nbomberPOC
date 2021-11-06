using System;

using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using NBomber.Plugins.Network.Ping;

namespace NBomberTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var getTattoine = Step.Create("Fetch Tattoine",
                     clientFactory: HttpClientFactory.Create(),
                     execute: context =>
                     {
                         var request = Http.CreateRequest("GET", "http://swapi.dev/api/planets/1/")
                                                       .WithHeader("Accept", "text/html");

                         return Http.Send(request, context);
                     });


            var getFirstResident = Step.Create("Fetch First Resident",
                     clientFactory: HttpClientFactory.Create(),
                     execute: context =>
                     {
                         var firstResident = context.GetPreviousStepResponse<string>();
                         var request = Http.CreateRequest("GET", firstResident)
                                                       .WithHeader("Accept", "text/html");

                         return Http.Send(request, context);
                     });


            var scenario = ScenarioBuilder
                  .CreateScenario("simple_http", getTattoine)
                  .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                  .WithLoadSimulations(
                      Simulation.InjectPerSec(rate: 2, during: TimeSpan.FromSeconds(30))
                  );

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}