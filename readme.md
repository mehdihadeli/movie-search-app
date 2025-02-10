# 🎬 Movie Search Application

[![build-dot-net](https://github.com/mehdihadeli/movie-search-application/actions/workflows/build-dot-net.yml/badge.svg)](https://github.com/mehdihadeli/movie-search-application/actions/workflows/build-dot-net.yml)

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/mehdihadeli/movie-search-application)
[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/mehdihadeli/movie-search-application)
[![Open with CodeSandbox](https://assets.codesandbox.io/github/button-edit-lime.svg)](https://codesandbox.io/p/sandbox/github/mehdihadeli/movie-search-application)
[![Open in Codeanywhere](https://codeanywhere.com/img/open-in-codeanywhere-btn.svg)](https://app.codeanywhere.com/#https://github.com/mehdihadeli/movie-search-application)

Movies Search Application is a search engine for searching movies and tv-shows with using two popular apis [TMDB](https://developers.themoviedb.org/3) and [Youtube API](https://developers.google.com/youtube/v3) for get all information about the movies and its related trailers with some other features.

This app is based on .Net core and vertical slices architecture and I wrote some level of tests like [Unit Testing](./tests/MovieSearch.UnitTests), [Integration Testing](./tests/MovieSearch.IntegrationTests) and [End-To-End Testing](./tests/MovieSearch.EndToEndTests) for this application.

## Application Structure

In this application I used a [mediator pattern](https://dotnetcoretutorials.com/2019/04/30/the-mediator-pattern-in-net-core-part-1-whats-a-mediator/) with using [MediatR](https://github.com/jbogard/MediatR) library in my controllers for a clean and [thin controller](https://codeopinion.com/thin-controllers-cqrs-mediatr/), also instead of using a `application service` class because after some times our controller will depends to different services and this breaks single responsibility principle. We use mediator pattern to manage the delivery of messages to handlers. One of the advantages behind the [mediator pattern](https://lostechies.com/jimmybogard/2014/09/09/tackling-cross-cutting-concerns-with-a-mediator-pipeline/) is that it allows the application code to define a pipeline of activities for requests . For example in our controllers we create a command and send it to mediator and mediator will route our command to a specific command handler in application layer.

To support [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle) and [Don't Repeat Yourself principles](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself), the implementation of cross-cutting concerns is done using the mediatr [pipeline behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors) or creating a [mediatr decorators](https://lostechies.com/jimmybogard/2014/09/09/tackling-cross-cutting-concerns-with-a-mediator-pipeline/).

Also in this project I used [clean architecture](https://www.youtube.com/watch?v=dK4Yb6-LxAk) and [vertical slice architecture](https://jimmybogard.com/vertical-slice-architecture/) or [Restructuring to a Vertical Slice Architecture](https://codeopinion.com/restructuring-to-a-vertical-slice-architecture/) also I used [feature folder structure](http://www.kamilgrzybek.com/design/feature-folders/) in this project.

I treat each request as a distinct use case or slice, encapsulating and grouping all concerns from front-end to back.
When adding or changing a feature in an application in n-tire architecture, we are typically touching many different "layers" in an application. we are changing the user interface, adding fields to models, modifying validation, and so on. Instead of coupling across a layer, we couple vertically along a slice. we `Minimize coupling` `between slices`, and `maximize coupling` `in a slice`.

With this approach, each of our vertical slices can decide for itself how to best fulfill the request. New features only add code, we're not changing shared code and worrying about side effects. For implementing vertical slice architecture using cqrs pattern is a good match.

![](./assets/Vertical-Slice-Architecture.jpg)

Also here I used cqrs for decompose my features to very small parts that make our application

- maximize performance, scalability and simplicity.
- adding new feature to this mechanism is very easy without any breaking change in other part of our codes. New features only add code, we're not changing shared code and worrying about side effects.
- easy to maintain and any changes only affect on one command or query (or a slice) and avoid any breaking changes on other parts
- it gives us better separation of concerns and cross cutting concern (with help of mediatr behavior pipelines) in our code instead of a big service class for doing a lot of things.

With using CQRS pattern, we cut each business functionality into some vertical slices, and inner each of this slices we have [technical folders structure](http://www.kamilgrzybek.com/design/feature-folders/) specific to that feature (command, handlers, infrastructure, repository, controllers, ...). In Our CQRS pattern each command/query handler is a separate slice. This is where you can reduce coupling between layers. Each handler can be a separated code unit, even copy/pasted. Thanks to that, we can tune down the specific method to not follow general conventions (e.g. use custom SQL query or even different storage). In a traditional layered architecture, when we change the core generic mechanism in one layer, it can impact all methods.

For checking `validation rules` we use two type of validation:

- [Data Validation](http://www.kamilgrzybek.com/design/rest-api-data-validation/): Data validation verify data items which are coming to our application from external sources and check if theirs values are acceptable but Business rules validation is a more broad concept and more close to how business works and behaves. So it is mainly focused on behavior For implementing data validation I used [FluentValidation](https://github.com/FluentValidation/FluentValidation) library for cleaner validation also better separation of concern in my handlers for preventing mixing validation logic with orchestration logic in my handlers.
- [Business Rules validation](http://www.kamilgrzybek.com/design/domain-model-validation/): I explicitly check all of the our business rules, inner my handlers and I will throw a customized exception based on the error that these errors should inherits from [AppException](./src/BuildingBlocks/BulidingBlocks/Exception/AppException.cs) class, because of these exceptions, occurs in application layer and we catch this exceptions in api layer with using [Hellang.Middleware.ProblemDetails](https://www.nuget.org/packages/Hellang.Middleware.ProblemDetails/) middleware and pass a correct status code to client.

Examples of `data validation` :

1- Input Validation

- We want to ensure our Id is greater than zero.

In bellow validator for our query as request we check that Id is greater than zero

```csharp
public class FindMovieByIdQueryValidator : AbstractValidator<FindMovieByIdQuery>
{
    public FindMovieByIdQueryValidator()
    {
        RuleFor(query => query.Id).GreaterThan(0).WithMessage("id should be greater than zero.");
    }
}
```

2- Business Rule Validation

- We want to check our database contains a movie with specific Id and if there is no movie with this Id, we throw a [MovieNotFoundException](./src/MovieSearch.Application/Movies/Exceptions/MovieNotFoundException.cs).

```csharp
 var movie = await _movieDbServiceClient.GetMovieByIdAsync(query.Id, cancellationToken);

if (movie is null)
    throw new MovieNotFoundException(query.Id);
```

Also for handling exceptions and correct status codes in our web api response, I Used [Hellang.Middleware.ProblemDetails](https://www.nuget.org/packages/Hellang.Middleware.ProblemDetails/) package and I config and map all our needed exceptions and their corresponding status code in our Infrastructure layer and [AddInfrastructure](./src/MovieSearch.Infrastructure/Extensions.cs) method.

```csharp
services.AddProblemDetails(x =>
    {
        // Control when an exception is included
        x.IncludeExceptionDetails = (ctx, _) =>
        {
            // Fetch services from HttpContext.RequestServices
            var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
            return env.IsDevelopment() || env.IsStaging();
        };
        x.Map<AppException>(ex => new ProblemDetails
        {
            Title = "Application rule broken",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Type = "https://somedomain/application-rule-validation-error",
        });
        // Exception will produce and returns from our FluentValidation RequestValidationBehavior
        x.Map<ValidationException>(ex => new ProblemDetails
        {
            Title = "input validation rules broken",
            Status = StatusCodes.Status400BadRequest,
            Detail = JsonConvert.SerializeObject(ex.ValidationResultModel.Errors),
            Type = "https://somedomain/input-validation-rules-error",
        });
        ////
        ////
    });
```

In this Project I covered most of important tests like `Unit Testing`, `Integration Testing` and `End To End` testing. For naming tests, I used [vladimir khorikov](https://enterprisecraftsmanship.com/posts/you-naming-tests-wrong/) naming convention in his article and it makes our tests more readable like a documentation for our developers.

In this app for increasing performance we could use caching mechanism simply with implementing a interface `ICachePolicy<,>` and our object for caching, this will handle with a chancing pipeline on mediateR as cross cutting concern with name of `CachingBehavior`. For example for caching our `FindMovieByIdQuery` query we could use bellow code:

```csharp
public class FindMovieByIdQuery : IQuery<FindMovieByIdQueryResult>
{
    public int Id { get; init; }

    public class CachePolicy : ICachePolicy<FindMovieByIdQuery, FindMovieByIdQueryResult>
    {
        public DateTimeOffset? AbsoluteExpirationRelativeToNow => DateTimeOffset.Now.AddMinutes(15);

        public string GetCacheKey(FindMovieByIdQuery query)
        {
            return CacheKey.With(query.GetType(), query.Id.ToString());
        }
    }
}
```

## Communication with External APIs

In this application for communicating with external apis like [TMDB Apis](https://developers.themoviedb.org/3) and [Youtube Apis](https://developers.google.com/youtube/v3) I used a [Anti Corruption Layer](https://deviq.com/domain-driven-design/anti-corruption-layer) as a [mediator](https://dev.to/asarnaout/the-anti-corruption-layer-pattern-pcd) between our system domain model and external systems domain model. The reason why you might use an anti corruption layer is to create a little padding between subsystems so that they do not `leak into each other` too much.

An Anti-Corruption Layer (ACL) is a set of patterns placed between the domain model and other bounded contexts or third party dependencies. The intent of this layer is to prevent the intrusion of foreign concepts and models into the domain model. The patterns are used to map between foreign domain models and APIs to types and interfaces that are part of the domain model.
This layer translates requests that one subsystem makes to the other subsystem. Use this pattern to ensure that an application's design is not limited by dependencies on outside subsystems.

![](./assets/diagram.png)

In our application I create an anti corruption class for `TMDB api` with name of [TMDBServiceClient](./src/MovieSearch.Infrastructure/Services/Clients/MovieDb/TMDBServiceClient.cs) and an anti corruption class for `youtube api` with name of [YoutubeVideoServiceClient](./src/MovieSearch.Infrastructure/Services/Clients/Video/YoutubeVideoServiceClient.cs).

In this anti corruption classes we also used some [resiliency mechanisms](https://medium.com/@emanuele.bucarelli/improve-resilience-in-the-net-application-80adda2c7710) like `Retry`, `Circuit-breaker`,`Timeout` and`Bulkhead` patterns for increasing our resiliency in calling third party apis and for doing this I used [Polly](https://github.com/App-vNext/Polly) library.

## Technologies - Libraries

- ✔️ **[`.NET 8`](https://dotnet.microsoft.com/download)** - .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
- ✔️ **[`Newtonsoft.Json`](https://github.com/JamesNK/Newtonsoft.Json)** - Json.NET is a popular high-performance JSON framework for .NET
- ✔️ **[`MVC Versioning API`](https://github.com/microsoft/aspnet-api-versioning)** - Set of libraries which add service API versioning to ASP.NET Web API, OData with ASP.NET Web API, and ASP.NET Core
- ✔️ **[`FluentValidation`](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library for building strongly-typed validation rules
- ✔️ **[`Swagger & Swagger UI`](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** - Swagger tools for documenting API's built on ASP.NET Core
- ✔️ **[`Serilog`](https://github.com/serilog/serilog)** - Simple .NET logging with fully-structured events
- ✔️ **[`Polly`](https://github.com/App-vNext/Polly)** - Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner
- ✔️ **[`Scrutor`](https://github.com/khellang/Scrutor)** - Assembly scanning and decoration extensions for Microsoft.Extensions.DependencyInjection
- ✔️ **[`EasyCaching`](https://github.com/dotnetcore/EasyCaching)** - Open source caching library that contains basic usages and some advanced usages of caching which can help us to handle caching more easier.
- ✔️ **[`AutoMapper`](https://github.com/AutoMapper/AutoMapper)** - Convention-based object-object mapper in .NET.
- ✔️ **[`MediatR`](https://github.com/jbogard/MediatR)** - Simple, unambitious mediator implementation in .NET.
- ✔️ **[`Hellang.Middleware.ProblemDetails`](https://github.com/khellang/Middleware/tree/master/src/ProblemDetails)** - A middleware for handling exception in .Net Core
- ✔️ **[`GuardClauses`](https://github.com/ardalis/GuardClauses)** - A simple package with guard clause extensions.
- ✔️ **[`AspNetCore.HealthChecks.UI`](https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks)** - Enterprise HealthChecks for ASP.NET Core Diagnostics Package
- ✔️ **[`NSubstitute`](https://nsubstitute.github.io/)** - A friendly substitute for .NET mocking libraries.
- ✔️ **[`FluentAssertions`](https://github.com/fluentassertions/fluentassertions)** - A very extensive set of extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style unit tests.

## Configs

For using this app we need a [YouTube ApiKey](https://developers.google.com/youtube/v3/getting-started) also for using TMDB api we need a [TMDB Api Key](https://www.themoviedb.org/settings/api) (I put a api key for TMDB in setting file for test purpose). you should set your api keys in [appsettings.json](./src/MovieSearch.Api/appsettings.json) file in bellow section:

```json
  "TMDBOptions": {
    "BaseApiAddress": "https://api.themoviedb.org/3",
    "ApiKey": "122f0c50ae02fa84601c07025cb6d2f1",
    "Language": "en-US",
    "Region": "US"
  },
  "YoutubeVideoOptions": {
    "ApiKey": "", //your youtube api key
    "SearchPart": "snippet",
    "SearchType": "video",
    "Order": 4
  },
```

We could also set this YouTube ApiKey setting with using OS environments with using bellow command in cmd:

```cmd
`setx YoutubeVideoOptions__ApiKey "your youtube api key"`
```

It automatically will replace this env `APiKey` with our appsettings `APIKey`.

Also for security purpose of our Apis I used [API key Authentication](https://codingsonata.com/secure-asp-net-core-web-api-using-api-key-authentication/) and you have to pas a API key in header of request or in query string of your api url with this key name `X-Api-Key`.
For example for url It will be like this:

```bash
http://localhost:5000/api/v1/movies/150/with-trailers?trailersCount=10&X-Api-Key=C5BFF7F0-B4DF-475E-A331-F737424F013C
```

For setup valid api key there is a class with name [InMemoryGetApiKeyQuery](./src/MovieSearch.Infrastructure/Security/InMemoryGetApiKeyQuery.cs), that this class is a in-memory registry for all valid Api Key. this class implemented `IGetApiKeyQuery` class. you can implement this interface and store your keys in your favorite provider like EF Core Sql Server or a Json file, ...
Some of valid keys for test are:

```bash
C5BFF7F0-B4DF-475E-A331-F737424F013C
5908D47C-85D3-4024-8C2B-6EC9464398AD
06795D9D-A770-44B9-9B27-03C6ABDB1BAE
```

Also our swagger is fully compatible with this Api key and you can authenticate in swagger UI before making any request. Use one of the above keys in this text box.

![](./assets/API-Key-Auth.png)

## How to Run

### CMD

For running our Apis we need to run bellow command in shell in root of the project.

```bash
.\scripts\api.bat
```

Then after this,our application will be up and running. our API service will be host on http://localhost:5000.

### Docker Compose

We can run this app on docker with this [docker-compose.yaml](./deployments/docker-compose/docker-compose.yaml) file with bellow command in root of application:

```bash
docker-compose -f ./deployments/docker-compose/docker-compose.yaml up
```

Also docker image is available on the docker hub in this address: [https://hub.docker.com/r/mehdihadeli/movie.api](https://hub.docker.com/r/mehdihadeli/movie.api)

### Kubernetes

For setup your local environment for using kubernetes you can use different approuch but I personally perfer to use [K3s](https://k3s.io/) from rancher team.

For running our app on kubernetes cluster we should apply [movie-search-api.yaml](./deployments/k8s/movie-search-api.yaml) file with
