# Fork Joint

Fork Joint is a fictional restaurant built during Season 3 of the MassTransit Live Code Video Series. You can [watch the episodes on YouTube](https://youtube.com/playlist?list=PLx8uyNNs1ri2JeyDGFWfCYyAjOB1GP-t1) and follow along by resetting to the various commits in the Git history.



## Docker Setup

The sample application can be run using Docker, however, there are a couple setup tasks required.

- Start all the services `docker compose -f .\docker-compose.services.yml up -d`
- Stopping all the services `docker compose -f .\docker-compose.services.yml down -v`

### Certificate Setup

The `docker-compose.api.yml` maps the local ASP.NET certificate folder into the container so that HTTPS can be used. This is different depending upon your operating system.

> I use a Mac with JetBrains Rider, so my configuration is in the GitHub repository. 

To create the development certificate:

MAC: 

```
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p Passw0rd
dotnet dev-certs https --trust
```

PC: 

```
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p Pass0wrd
dotnet dev-certs https --trust
```

You may need to modify the `docker-compose.api.yml` file to match your path for Windows.

[See this page](https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-6.0) for more information, it was used to get this working on my machine.



## Design Diagrams

### Request Response

![Request Response](https://raw.githubusercontent.com/MassTransit/Sample-ForkJoint/master/assets/requestResponse.svg "Request Response")

### Routing Slip

![Routing Slip](https://raw.githubusercontent.com/MassTransit/Sample-ForkJoint/master/assets/routingSlip.svg "Routing Slip")

### Example SubmitOrder Post
```
POST https://localhost:5001/Order
Content-Type: application/json

{
  "orderId": "{{$guid}}",
  "burgers": [
    {
      "burgerId": "{{$guid}}",
      "weight": 2,
      "lettuce": false,
      "cheese": true,
      "pickle": true,
      "onion": true,
      "ketchup": true,
      "mustard": true,
      "barbecueSauce": true,
      "onionRing": true
    }
  ],
  "fries": [
    {
      "fryId": "{{$guid}}",
      "size": 1
    }
  ],
  "shakes": [
    {
      "shakeId": "{{$guid}}",
      "flavor": "Strawberry",
      "size": 1
    }
  ],
  "fryShakes": [
    {
      "fryShakeId": "{{$guid}}",
      "flavor": "Banna",
      "size": 1
    }
  ]
}
```

### Logging and OpenTelemetry Links

- [Seq](https://datalust.co/) - http://localhost:5341
- [Grafana](https://grafana.com/docs/tempo/latest/) - http://localhost:3001
- [Jaegar](https://www.jaegertracing.io/) - http://localhost:16686