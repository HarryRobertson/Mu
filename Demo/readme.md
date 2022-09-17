This is the demo for Mu (pronounced Moo), which is intended to act as an ASP-esque standard way of building event-driven microservices.
The aim is that you can just call a few boilerplate extension methods in Program.cs to set up the listeners and publishers.
Then the developer just has to worry about writing the handlers for the messages, in the same middleware format as ASP.

You need to run RabbitMQ for this to work, I suggest Docker:
docker run --hostname my-rabbit -p 8080:15672 -p 5672:5672 rabbitmq:3-management

Then you can connect to http://localhost:8080/#/queues/%2F/foo and add a WeatherRequest to the queue:
{ "Days": 5 }

If you F5 the Demo project it should then pick this up and run it through the pipeline.

You should get a response on the other queue http://localhost:8080/#/queues/%2F/bar equivalent to the normal ASP demo WeatherForecast

It's just a start, there's lots I want to add: 
- configurable serialisation (JWT as default to allow sending of headers, payload integrity validation, potential sender validation)
- message Handlers a la Controllers, built in
- Auth
- standard consumers and producers for well used message buses
- integration with ASP for consuming HTTP requests or producing webhooks

Also, it obviously needs unit tests and some focus on performance

When this is fairly stable I want to build a second repo (working title 'Muster') to manage distributed apps built using Mu - possibly using Orleans for the distribution aspect.
