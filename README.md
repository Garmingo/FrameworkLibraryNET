# FiveM Framework Library (.NET implementation)
This library allows you to easily interact with the servers framework, without writing the same code over and over again for each framework.

## Installation

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
