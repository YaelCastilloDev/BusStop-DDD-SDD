var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
  .WithImage("postgis/postgis")
  .WithImageTag("18-3.6")
  .WithLifetime(ContainerLifetime.Persistent);

var busStopDb = postgres.AddDatabase("PostgresConnection");

builder.AddProject<Projects.BusStop_Web>("web")
  .WithEndpoint("https", e => e.Port = 57679)
  .WithReference(busStopDb)
  .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
  .WaitFor(busStopDb);

builder
  .Build()
  .Run();
