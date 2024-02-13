# HackerNewsCachingService

HackerNewsCachingServer is an ASP.NET Core application developed using C# 12.0,
ASP.NET Core and targeting the net8.0 framework. It presents simple caching server which takes advantage of polling and in memory cache for serving n best stories from [ Hacker New API ](https://github.com/HackerNews/API?tab=readme-ov-file)

## Prerequisites
- .NET 8.0 SDK

## Getting Started

1. **Clone the repository**

   `git clone []`

2. **Navigate to the project directory**

   `cd 'path'`

3. **Restore the packages**

   `dotnet restore`

4. **Build the project**

   `dotnet build`

5. **Run the application**

   `dotnet run`
6. **Example Load test ([Artilerry](https://www.artillery.io/))**

   `artillery run loadTest.yml`

## Know Issues
* the biggest downside is caused by implementation of polling that requires fetching at the worst case 200 stories one by one. In addition to that
  there is no way to fetch best stories by passing collection of ids within one round-trip request.
* lack of unit tests / integration tests / load tests
* scaling is limited by single process
* no more than 200 best stories handled due to _Hacker News_ api limitations
* No good client library for handling _EventSource / Server-Sent Events protocol_.
## Possible improvements
* replacing part of polling implementation with event based communication.In theory it;s supported by firebase Rest API however Firesharp library seems to have issue with implementation of this feature.
* adding circuit breaker could prevent from further overloading  the _HackerNews_ service in case it already struggles with handling requests
* adding more logs and metric would definitely show clearer picture of current bottlenecks and problems.
* reducing deserialization and serialization overhead might be worth investigating (other libraries, fewer mapping operations)
* Scaling horizontally although more complex for maintenance might give much better throughput, for example with some external
  caching service (Redis) and LB
* low level improvements like replacing string manipulation with span if possible.
