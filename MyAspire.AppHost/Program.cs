
using MyAspire.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis", 5379)
    .WithHealthCheck()
    .WithImageTag("latest")
    .WithVolume("redis_data", "/data", false)
    .WithOtlpExporter();

var postgreSql = builder.AddPostgres("postgres")//, port: 4432)
                                                //.WithEnvironment("POSTGRES_PASSWORD", "password")
                                                //.WithEnvironment("POSTGRES_USER", "postgres")
    .WithImageTag("latest")
    .WithImage("postgres")
    .WithVolume("postgres_data", "/var/lib/postgresql/data")
    .WithHealthCheck()
    .WithPgAdmin()
    ;
//    .WithDataVolume();
//    .WithOtlpExporter()
//    .WithContainerRunArgs();

var db = postgreSql.AddDatabase("fx-rates", "fx-rates");

//var solace = builder.AddContainer("pubSubStandardSingleNode", "solace/solace-pubsub-standard", "latest")
//    .WithVolume("storage-group", "/var/lib/solace")
//    .WithContainerRunArgs("--shm-size", "1g", "--ulimit", "core=-1", "--ulimit", "nofile=2448:6592")
//    .WithEnvironment("system_scaling_maxconnectioncount", "100")
//    .WithEnvironment("username_admin_password", "admin")
//    .WithEnvironment("username_admin_globalaccesslevel", "admin")
//    .WithHttpEndpoint(8080, 8080, "admin");

var apiService = builder.AddProject<Projects.MyAspire_ApiService>("apiservice")
    .WithHealthCheck()
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.MyAspire_Web>("webfrontend")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
