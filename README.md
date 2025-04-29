## Overview

There are two NuGets on this repository:

- Simple.Redis.Cache.Distributed: contains abstractions for caching objects using keys.
- Simple.Redis.Cache.Memory: in-memory implementation.

### Usage

The Get and GetAsync methods uses Func<Task<T>> to encapsulate an asyncronous method that returns a value of the type specified:

```
var users = await _cachingService.GetAsync("users", async () => await _usersRepository.Get(companyId));
```

We can also pass a TimeSpan as an expiration from now:

```
var users = await _cachingService.GetAsync("users", async () => await _usersRepository.Get(companyId), TimeSpan.FromHous(1));
```

We can also use the forceFetch parameter to enforce the execution of the delegate to ensure we retrieve the latest values:

```
var deals = await _cachingService.GetAsync("deals", async () => await _dealsRepository.Get(userId), forceFetch: true);
```
