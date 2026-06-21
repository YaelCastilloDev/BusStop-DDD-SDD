var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
  .WithLifetime(ContainerLifetime.Persistent);

var busStopDb = postgres.AddDatabase("busstop");

builder.AddProject<Projects.BusStop_Web>("web")
  .WithReference(busStopDb)
  .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
  .WaitFor(busStopDb);

builder
  .Build()
  .Run();
