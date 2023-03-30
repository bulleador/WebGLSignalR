1. W <head> w index.html musi znaleźć się skrypt SignalR. Testowano na wersji 7.0.4
```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.4/dist/browser/signalr.min.js"></script>
```

2. Przed utworzeniem lub dołączeniem do lobby należy zainicjalizować `LobbyController`
```csharp
var lobbyController = FindObjectOfType<LobbyController>();
lobbyController.Initialise();
```
