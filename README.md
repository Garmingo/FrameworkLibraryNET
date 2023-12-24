# FiveM Framework Library (.NET implementation)
This library allows you to easily interact with the servers framework, without writing the same code over and over again for each framework.

## Supported Frameworks
 * ESX Legacy
 * ESX Infinity
 * QBCore
 * Custom implementations

## Usage (Server)
```typescript
using FrameworkLibraryServer;
private Framework framework = new Framework();

framework.getPlayerWalletMoney(playerId);
framework.addPlayerWalletMoney(playerId, 500);
...
```

## Usage (Client)
```typescript
using FrameworkLibraryClient;
private Framework framework = new Framework();

framework.getPlayerJobName();
framework.getPlayerJobGrade();
...
```

## Docs
https://docs.garmingo.com/purchase-and-installation/frameworks

## Other packages

https://github.com/Garmingo/framework-js-client

https://github.com/Garmingo/framework-js-server

https://github.com/Garmingo/framework-lua
