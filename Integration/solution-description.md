## Solution Summary:
* **A thread-safe lock mechanism** was implemented as a dedicated classes based on **Single Reponsibility Principle** and **Encapsulation Principles**.
* **ILockProvider** interface is the abstraction of this mechanism, having 2 methods named **AcquireLock** and **ReleaseLock**.
* **SingleServerLockProvider** was implemented for single server scenaria.
* **DistributedLockProvider** was implemented for distributed system scenarios.
* Also another class named **ThreadHelper** was implemented to provide **easy-to-use** and **resuable** mechanism for thread safe invocation of any operation.
* **ThreadHelper** handles ther execution of given action within a thread-safe lock mechanism.
* **ThreadHelper** uses **Strategy Pattern** to get implementation of **ILockProvider** to support injecting different implementation of lock logic.
* **ThreadHelper**, **SingleServerLockProvider** and **DistributedLockProvider** can be injected by using DI.
* **SingleServerLockProvider** and **DistributedLockProvider** also have their own intefaces to support DI.

## 1- Single Server Scenario

* **SingleServerLockProvider** was implemented to support single server scenario.
* It uses **MemoryCache** as in-memory thread safe lock store with expiry time support.
* Expiry time is used to avoid **memory leaks** and **dead locks**.

## 2 - Distributed System Scenario

* **DistributedLockProvider** uses **Redis** as a distributed storage for lock states.
* Redis is proposed to be deployed as a **cluster** for **scalability** and **high availability**.
* Connection to redis database is **on demand**, so it will not cause consume resource upto it is utilized.
* It is better to configure DistributedLockProvider as a **Singleton** on DI to **avoid multiple connections** to Redis and avoid resource exhaustion.


### Weaknesses:
* **Single point of failure:** Redis may turn to single point of failure. if redis is down, the service will not be able to serve any request.
Better solution would be at least we may fallback to local cache if redis is down just to prevent duplicated calls from same app instance.
* **Performance Overhead:** Distributed locking relies on redis over network communication, which introduce extra latency. Having long "content" keys may result in performance issues.
* **Security:** Any security issue on Redis may result in data leakage and also may result in deadlocks.
* **Risk to Loose Lock States:** If the redis service is configured not to persist data on restart, the cache will be empty and the service will let duplicated calls until the cache is filled again.
* **Resource Exhaustion:** Redis may cause resource exhaustion as a result of too connections or many lock data.
* **Potential Deadlock:** If lock mechanism is not implemented properly, it may cause deadlocks.
* **Scalability:** Redis is not scalable as it is single instance. If the load is too high, it may not be able to handle the load.
* **Additional Cost:** Redis will introduce additional cost based on workload.
* **Additional Dependency:** If your stack does not already have Redis, that means additional depenency to manage on your environments.
* **Complexity Overhead:** Distributed locking introduced extra complexity, difficult to reproduce errors etc.

### Solutions for Some Weaknesses:
* Use of hashes instead of whole content texts may solve security and performance issues (if texts are really long)
* Using SingleServerLockProvider as a fallback if redis is down would at least prevents down of the system and still prevents duplicated calls from same app instance.
* Configuring redis to persist data to avoid data loss on restarts
* Configuring good values for expiry time values would help to avoid deadlocks
* Implementing some monitoring and tracing tools would help to debug and fix issues.
* Of course some of those solutions depends on also nature of data and your existing architecture.


It is a tradeoff to choose between distributed vs local locking mechanism based on their weaknesses and costs.
