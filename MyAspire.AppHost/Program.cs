using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

if (!Directory.Exists("../jaeger"))
{
    Directory.CreateDirectory("../jaeger");
}

if (!Directory.Exists("../seq"))
{
    Directory.CreateDirectory("../seq");
}


//metrics should be sent to prometheus
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithHttpEndpoint(targetPort: 3000, name: "http");

builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
       .WithBindMount("../prometheus_data", "/prometheus")
       .WithHttpEndpoint(/* This port is fixed as it's referenced from the Grafana config */ port: 9090, targetPort: 9090);


/*
 https://www.jaegertracing.io/docs/1.57/deployment/
 */

//traces should be sent to jaeger
builder.AddContainer("jaeger", "jaegertracing/all-in-one", "1.57")
    .WithEnvironment("SPAN_STORAGE_TYPE", "badger")
    .WithEnvironment("BADGER_EPHEMERAL", "false")
    .WithEnvironment("BADGER_DIRECTORY_VALUE", "/badger/data")
    .WithEnvironment("BADGER_DIRECTORY_KEY", "/badger/key")
    .WithBindMount("../jaeger", "/badger")
    .WithHttpEndpoint(14317, 4317, "grpc")
    .WithHttpEndpoint(16686, 16686);


//logs should be sent to seq
builder.AddContainer("seq", "datalust/seq")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithBindMount("../seq", "/data")
    .WithHttpEndpoint(5341, 80);


var apiService = builder.AddProject<Projects.MyAspire_ApiService>("apiservice");


builder.AddProject<Projects.MyAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
