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

            var clientFactory = HttpClientFactory.Create();


            var getStarWarsPlanet = Step.Create("GET a Star Wars Planet",
                timeout: TimeSpan.FromMilliseconds(2000), //default is 1000ms
                      clientFactory: clientFactory,
                      execute: async context =>
                     {
                         var threadNumber = context.ScenarioInfo.ThreadNumber + 1; //Threads start with a zero
                         var request = Http.CreateRequest("GET", $"http://swapi.dev/api/planets/{threadNumber}/")
                         .WithCheck(async response =>
                         {
                             var json = await response.Content.ReadAsStringAsync();
                             var starWarsPlanet = JsonConvert.DeserializeObject<StarWarsPlanet>(json);
                             Response stepResponse;

                             if (starWarsPlanet != null)
                             {
                                 stepResponse = Response.Ok(starWarsPlanet, statusCode: ((int)response.StatusCode)); //  pass user object response to the next step
                             }
                             else
                             {
                                 context.Logger.Information("Did not find this star wars planet");
                                 stepResponse = Response.Fail(error:$"Did not find this star wars planet", statusCode: (int)response.StatusCode);
                             }
                             return stepResponse;

                         });
                         var response = await Http.Send(request, context);
                         return response;
                     });

            var getFirstFilm = Step.Create("GET first film for planet",
            timeout: TimeSpan.FromMilliseconds(2000), //default is 1000ms
          clientFactory: clientFactory,
          execute: async context =>
          {
              try
              {
                  var starWarsPlanet = context.GetPreviousStepResponse<StarWarsPlanet>();

				  if (starWarsPlanet!=null)
                  {
                      if (starWarsPlanet.films.Count > 0)
					  {
						  var firstFilm = starWarsPlanet.films[0];

						  var response = await context.Client.GetAsync(firstFilm);

						  return response.IsSuccessStatusCode
							  ? Response.Ok(statusCode: (int)response.StatusCode, sizeBytes: (int)response.Content.Headers.ContentLength)
							  : Response.Fail(statusCode: (int)response.StatusCode, sizeBytes: (int)response.Content.Headers.ContentLength);
					  }
                  }
				  return Response.Ok(statusCode: 200);
              }
              catch (Exception e)
              {
                  return Response.Fail(error: e, statusCode: 404);
              }


          });

            return ScenarioBuilder.CreateScenario("Star Wars Scenario", getStarWarsPlanet, getFirstFilm);
        }


        [Test]
        public void nbomberStarwars()
        {
            var scenario = BuildScenario()
                            .WithoutWarmUp()
                            .WithLoadSimulations(new[]
                            {
					//Simulation.KeepConstant(copies: 1, during: TimeSpan.FromSeconds(2)) //useful for debugging
					Simulation.RampPerSec(rate: 50, during: TimeSpan.FromSeconds(10)),
                            });

            var nodeStats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithLoggerConfig(() => new LoggerConfiguration().MinimumLevel.Verbose()) // set log to verbose
                .Run();



            var scenarioStats = nodeStats.ScenarioStats[0];
            Assert.Greater(scenarioStats.StepStats[0].Ok.Request.Count, 0, $"{scenarioStats.StepStats[0].StepName}: Confirm that a successful request has been made");
            Assert.AreEqual(0, scenarioStats.StepStats[0].Fail.Request.Count, $"{scenarioStats.StepStats[0].StepName}: Confirm that no failed requests have occurred");

            Assert.Greater(scenarioStats.StepStats[1].Ok.Request.Count, 0, $"{scenarioStats.StepStats[1].StepName}: Confirm that a successful request has been made");
            Assert.AreEqual(0, scenarioStats.StepStats[1].Fail.Request.Count, $"{scenarioStats.StepStats[1].StepName}: Confirm that no failed requests have occurred");

            /* Following is using Fluent Assertions, need to convert to nunit
			stepStats.Ok.Request.RPS.Should().BeGreaterThan(8);
			stepStats.Ok.Latency.Percent75.Should().BeGreaterOrEqualTo(100);
			stepStats.Ok.DataTransfer.MinBytes.Should().Be(1024);
			stepStats.Ok.DataTransfer.AllBytes.Should().BeGreaterOrEqualTo(17408L);
			
			Data that can be extracted includes

			{{ ScenarioName = "Star Wars Scenario"
  RequestCount = 48
  OkCount = 48
  FailCount = 0
  AllBytes = 72640L
  StepStats =
			 [|{ StepName = "GET a Star Wars Planet"
				 Ok = { Request = { Count = 24
									RPS = 2.4 }
						Latency = { MinMs = 502.46
									MeanMs = 757.07
									MaxMs = 1711.71
									Percent50 = 573.44
									Percent75 = 605.7
									Percent95 = 1580.03
									Percent99 = 1712.13
									StdDev = 385.99
									LatencyCount = { LessOrEq800 = 19
													 More800Less1200 = 0
													 MoreOrEq1200 = 5 } }
						DataTransfer = { MinBytes = 639
										 MeanBytes = 762
										 MaxBytes = 1086
										 Percent50 = 642
										 Percent75 = 773
										 Percent95 = 1086
										 Percent99 = 1086
										 StdDev = 173.77
										 AllBytes = 18299L }
						StatusCodes = [|{ StatusCode = 200
										  IsError = false
										  Message = ""
										  Count = 24 }|] }
				 Fail = { Request = { Count = 0
									  RPS = 0.0 }
						  Latency = { MinMs = 0.0
									  MeanMs = 0.0
									  MaxMs = 0.0
									  Percent50 = 0.0
									  Percent75 = 0.0
									  Percent95 = 0.0
									  Percent99 = 0.0
									  StdDev = 0.0
									  LatencyCount = { LessOrEq800 = 0
													   More800Less1200 = 0
													   MoreOrEq1200 = 0 } }
						  DataTransfer = { MinBytes = 0
										   MeanBytes = 0
										   MaxBytes = 0
										   Percent50 = 0
										   Percent75 = 0
										   Percent95 = 0
										   Percent99 = 0
										   StdDev = 0.0
										   AllBytes = 0L }
						  StatusCodes = [||] } };
			   { StepName = "GET first film for planet"
				 Ok = { Request = { Count = 24
									RPS = 2.4 }
						Latency = { MinMs = 260.59
									MeanMs = 308.03
									MaxMs = 365.66
									Percent50 = 303.62
									Percent75 = 338.18
									Percent95 = 364.54
									Percent99 = 365.82
									StdDev = 34.3
									LatencyCount = { LessOrEq800 = 24
													 More800Less1200 = 0
													 MoreOrEq1200 = 0 } }
						DataTransfer = { MinBytes = 2240
										 MeanBytes = 2264
										 MaxBytes = 2323
										 Percent50 = 2241
										 Percent75 = 2323
										 Percent95 = 2323
										 Percent99 = 2323
										 StdDev = 37.27
										 AllBytes = 54341L }
						StatusCodes = [|{ StatusCode = 200
										  IsError = false
										  Message = ""
										  Count = 24 }|] }
				 Fail = { Request = { Count = 0
									  RPS = 0.0 }
						  Latency = { MinMs = 0.0
									  MeanMs = 0.0
									  MaxMs = 0.0
									  Percent50 = 0.0
									  Percent75 = 0.0
									  Percent95 = 0.0
									  Percent99 = 0.0
									  StdDev = 0.0
									  LatencyCount = { LessOrEq800 = 0
													   More800Less1200 = 0
													   MoreOrEq1200 = 0 } }
						  DataTransfer = { MinBytes = 0
										   MeanBytes = 0
										   MaxBytes = 0
										   Percent50 = 0
										   Percent75 = 0
										   Percent95 = 0
										   Percent99 = 0
										   StdDev = 0.0
										   AllBytes = 0L }
						  StatusCodes = [||] } }|]
  LatencyCount = { LessOrEq800 = 43
				   More800Less1200 = 0
				   MoreOrEq1200 = 5 }
  LoadSimulationStats = { SimulationName = "ramp_per_sec"
						  Value = 5 }
  StatusCodes = [|{ StatusCode = 200
					IsError = false
					Message = ""
					Count = 48 }|]
  CurrentOperation = Complete
  Duration = 00:00:10 }}
			 
			 
			 */






        }
    }
}
