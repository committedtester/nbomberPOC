# nbomberPOC

This is a very early implementation of https://nbomber.com/. After the removal of support for Visual Studio's load testing, I always felt that there was a missing opportunity for C# Load Test. Nbomber helps to cover this requirement for teams that use C#. 

Naturally communities for Jmeter, Gatling, K6, Locust are larger, but the ability to reuse C# assets if the production code is written in C# shouldn't be underestimated.


# Approach

Load the solution, build and load the test explorer. You'll see one nunit test that accesses a public Star Wars API website. Look to the console output for a nice HTML report of the test

When I have an opportunity I'll see if I can scale this to larger throughput.