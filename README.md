# Fork Joint

Fork Joint is a fictional restaurant built during Season 3 of the MassTransit Live Code Video Series.



## Docker Setup

The sample application can be run using Docker, however, there are a couple setup tasks required.

### Certificate Setup

The `docker-compose.yml` maps the local ASP.NET certificate folder into the container so that HTTPS can be used. This is different depending upon your operating system.

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

You may need to modify the `docker-compose.yml` file to match your path for Windows.

[See this page](https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-5.0) for more information, it was used to get this working on my machine.

