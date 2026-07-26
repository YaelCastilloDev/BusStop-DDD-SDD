var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
  .WithImage("postgis/postgis")
  .WithImageTag("18-3.6")
  .WithLifetime(ContainerLifetime.Persistent)
  .WithPgBouncer();

var busStopDb = postgres.AddDatabase("PostgresConnection");

var rabbitmq = builder.AddRabbitMQ("messaging")
  .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.BusStop_Web>("web")
  .WithEndpoint("https", e => e.Port = 57679)
  .WithReference(busStopDb)
  .WithReference(rabbitmq)
  .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
  .WaitFor(busStopDb)
  .WaitFor(rabbitmq);

builder
  .Build()
  .Run();
