# fluent-api-net

|**Branche**|**Build**|
|--|--|
| **master** | [![Build status](https://aurelienbretin.visualstudio.com/FluentApiNet/_apis/build/status/FluentApiNet_master)](https://aurelienbretin.visualstudio.com/FluentApiNet/_build/latest?definitionId=2) |
|**develop**|[![Build status](https://aurelienbretin.visualstudio.com/FluentApiNet/_apis/build/status/FluentApiNet_develop)](https://aurelienbretin.visualstudio.com/FluentApiNet/_build/latest?definitionId=3) |

|**Version**|**Downloads**|
|--|--|
| [![nuget](https://img.shields.io/nuget/v/FluentApiNet.svg)](https://www.nuget.org/packages/FluentApiNet/) | [![nuget](https://img.shields.io/nuget/dt/FluentApiNet.svg)](https://www.nuget.org/packages/FluentApiNet/) |
| [![nuget](https://img.shields.io/nuget/vpre/FluentApiNet.svg)](https://www.nuget.org/packages/FluentApiNet/) |  |

## Description

It's a library with you can construct your business part quickly optimized by mapping, lambda expression and pagination.

Use ServiceBase class like :
````
var service = new MyService(mydbcontext);
var result = service.Get(x => x.Id == 1);
````
