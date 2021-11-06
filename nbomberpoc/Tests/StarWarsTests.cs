namespace nbomberpoc.Tests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using NBomber;
    using NUnit.Framework;
    using NBomber.Contracts;
    using NBomber.CSharp;
    using NBomber.Plugins.Http.CSharp;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Serilog;

    public class StarWarsPlanet
    {
        public string name { get; set; }
        public string rotation_period { get; set; }
        public string orbital_period { get; set; }
        public string diameter { get; set; }
        public string climate { get; set; }
        public string gravity { get; set; }
        public string terrain { get; set; }
        public string surface_water { get; set; }
        public string population { get; set; }
        public List<string> residents { get; set; }
        public List<string> films { get; set; }
        public DateTime created { get; set; }
        public DateTime edited { get; set; }
        public string url { get; set; }
    }




    [TestFixture]
    public class StarWarsTests
    {

        Scenario BuildScenario()
        {
            var getTattoine = Step.Create("GET Tattoine",
                timeout: TimeSpan.FromMilliseconds(2000), //default is 1000ms
                      clientFactory: HttpClientFactory.Create(),
                      execute: async context =>
                     {
                         var request = Http.CreateRequest("GET", "http://swapi.dev/api/planets/1/")
                         .WithCheck(async response =>
                         {
                             var json = await response.Content.ReadAsStringAsync();
                             var starWarsPlanet = JsonConvert.DeserializeObject<StarWarsPlanet>(json);
                             Response stepResponse;

                             if (starWarsPlanet != null)
                             {
                                 stepResponse = Response.Ok(starWarsPlanet,statusCode: ((int)response.StatusCode)); //  pass user object response to the next step
                              }
                             else
                             {
                                 stepResponse = Response.Fail($"Did not find this star wars planet");
                             }
                             return stepResponse;

                         });
                         var response = await Http.Send(request, context);
                         return response;
                     });

            return ScenarioBuilder.CreateScenario("Star Wars Scenario", getTattoine);
        }


        [Test]
        public void nbomberTattoine()
        {
            var scenario = BuildScenario()
                            .WithoutWarmUp()
                            .WithLoadSimulations(new[]
                            {
                    Simulation.KeepConstant(copies: 1, during: TimeSpan.FromSeconds(2))
                            });

            var nodeStats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithLoggerConfig(() => new LoggerConfiguration().MinimumLevel.Verbose()) // set log to verbose
                .Run();



            var stepStats = nodeStats.ScenarioStats[0].StepStats[0];
            Assert.Greater(stepStats.Ok.Request.Count, 0);

            /* Following is using Fluent Assertions, need to convert to nunit
            stepStats.Ok.Request.RPS.Should().BeGreaterThan(8);
            stepStats.Ok.Latency.Percent75.Should().BeGreaterOrEqualTo(100);
            stepStats.Ok.DataTransfer.MinBytes.Should().Be(1024);
            stepStats.Ok.DataTransfer.AllBytes.Should().BeGreaterOrEqualTo(17408L);
            */
        }
    }
}
