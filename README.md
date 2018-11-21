# fluent-api-net

[![Build status](https://aurelienbretin.visualstudio.com/FluentApiNet/_apis/build/status/FluentApiNet_develop)](https://aurelienbretin.visualstudio.com/FluentApiNet/_build/latest?definitionId=3)

## Description

It's a library with you can construct your business part quickly optimized by mapping, lambda expression and pagination.

Use ServiceBase class like :
````
var service = new MyService(mydbcontext);
var result = service.Get(x => x.Id == 1);
````
