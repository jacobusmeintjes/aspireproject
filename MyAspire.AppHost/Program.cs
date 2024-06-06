var builder = DistributedApplication.CreateBuilder(args);


//metrics should be sent to prometheus
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithHttpEndpoint(targetPort: 3000, name: "http");

builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
       .WithBindMount("../prometheus_data", "/prometheus")
       .WithHttpEndpoint(/* This port is fixed as it's referenced from the Grafana config */ port: 9090, targetPort: 9090);


//traces should be sent to jaeger
builder.AddContainer("jaeger", "jaegertracing/all-in-one", "1.57")
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
