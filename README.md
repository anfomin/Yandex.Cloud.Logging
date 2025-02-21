# Yandex.Cloud.Logging

[Yandex.Cloud Logging](https://yandex.cloud/services/logging) provider implementation for Microsoft.Extensions.Logging. Logs are queued and delivered via background service.

## Installation via NuGet [![NuGet](https://img.shields.io/nuget/v/Yandex.Cloud.Logging.svg)](https://www.nuget.org/packages/Yandex.Cloud.Logging)

```
dotnet add package Yandex.Cloud.Logging
```

## How to use with ASP.NET Core

Register services in `Program.cs`:

```C#
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddYandexCloud();
builder.AddSingleton(new Sdk(...)); // register Yandex.Cloud.Sdk with your credentials
```

And add configuration to the `appsettings.json`:

```json
{
    "Logging": {
        "YandexCloud": {
            "FolderId": "<required folder identifier from Yandex.Cloud console>",
            "GroupId": "<required group identifier from Yandex.Cloud console>",
            "ResourceType": "<optional resource type>",
            "ResourceId": "<optional resource identifier>"
        }
    }
}
```

You can configure Yandex.Cloud logger via code as well:

```C#
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddYandexCloud(opt =>
{
    opt.Credentials = new IamJwtCredentialsProvider(...); // specify Yandex.Cloud credentials here
    opt.FolderId = "<required folder identifier from Yandex.Cloud console>";
    opt.ResourceType = "<optional resource type>";
    opt.GroupId = "<required group identifier from Yandex.Cloud console>";
    opt.ResourceId = "<optional resource identifier>";
});
```

## License

This package has MIT license. Refer to the [LICENSE](LICENSE) for detailed information.
