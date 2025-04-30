# Yandex.Cloud.Logging

[Yandex.Cloud Logging](https://yandex.cloud/services/logging) provider implementation for Microsoft.Extensions.Logging. Library features:

- Supports .NET 8 and .NET 9.
- Supports Blazor.
- Can log HTTP request URL, method IP and username.
- Logs are queued and delivered via background service.

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

If you want to log HTTP request URL, method, IP and username then add package `Yandex.Cloud.Logging.AspNetCore` and use method 'AddYandexCloudWithHttp':

```C#
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddYandexCloudWithHttp();
builder.AddSingleton(new Sdk(...)); // register Yandex.Cloud.Sdk with your credentials
```

Then add logger configuration to the `appsettings.json`:

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
    opt.Credentials = new IamJwtCredentialsProvider(...); // you can specify Yandex.Cloud credentials here if Yandex.Cloud.Sdk is not registered as service
    opt.FolderId = "<required folder identifier from Yandex.Cloud console>";
    opt.ResourceType = "<optional resource type>";
    opt.GroupId = "<required group identifier from Yandex.Cloud console>";
    opt.ResourceId = "<optional resource identifier>";
});
builder.Logging.AddYandexCloudWithHttp(); // optional
```

## Customization

You can add custom information to the log payload by implementing `IYandexCloudLogEntryProvider`. For example:

```C#
public class CustomLogEntryProvider : IYandexCloudLogEntryProvider
{
    public void ApplyPayload(Struct payload)
    {
        payload.Fields["custom"] = Value.ForString("custom string value");
    }
}
```

Then register new payload provider:

```C#
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddYandexCloud()
    .AddYandexCloudLogEntryProvider<CustomLogEntryProvider>();
```

## License

This package has MIT license. Refer to the [LICENSE](LICENSE) for detailed information.