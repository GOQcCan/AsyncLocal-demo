# AsyncLocal-demo

Démonstration d’un contexte d’exécution multi-tenant basé sur `AsyncLocal<T>` (tenant, utilisateur, correlation id) propagé de l’API jusqu’aux services applicatifs.

AsyncLocal<T> est une classe située dans le namespace System.Threading qui représente des données ambiantes locales à un flux de contrôle asynchrone donné. Elle permet de stocker des valeurs qui se propagent automatiquement à travers les appels asynchrones (async/await).

Contrairement à ThreadLocal<T> qui stocke des données par thread, AsyncLocal<T> stocke des données par contexte d'exécution asynchrone. Les valeurs sont :
- ✅ Propagées vers le bas du flux asynchrone (aux méthodes appelées)
- ❌ Non propagées vers le haut (les modifications dans les méthodes enfants n'affectent pas le parent)
- ✅ Isolées entre flux parallèles

Différence clé : AsyncLocal<T> vs ThreadLocal<T>

| Critère | AsyncLocal<T> | ThreadLocal<T> |
|----------|-----------|-----------|
| Portée | Flux asynchrone  | Thread physique  |
| Propagation async | ✅ Oui  | ❌ Non  |
| Isolation parallèle | ✅ Oui  | ⚠️ Partiel  |
| Cas d'usage | Applications modernes async  | Code synchrone legacy |

## Objectifs

- Centraliser les informations de contexte (`TenantId`, `UserId`, `CorrelationId`) pour chaque requête.
- Propager ce contexte de manière sûre à travers les appels asynchrones.
- Montrer l’impact sur la logique métier (création/lecture de commandes) et la testabilité.

## Architecture (vue rapide)

- `src/AsyncLocal-demo.Api`
  - API ASP.NET Core, configuration du pipeline, middleware de contexte.
- `src/AsyncLocal-demo.Application`
  - Cas d’usage (ex: `OrderService`) et DTO/commands.
- `src/AsyncLocal-demo.Core`
  - Contrats partagés (ex: `IExecutionContext`).
- `src/AsyncLocal-demo.Infrastructure`
  - Implémentations techniques (ex: `AsyncLocalExecutionContext`, persistence).
- `tests/AsyncLocal-demo.Tests`
  - Tests unitaires et de propagation de contexte.

## Prérequis

- .NET SDK 10
- Visual Studio 2026 (recommandé)

## Démarrage rapide

### 1) Restaurer / compiler
```powershell
dotnet restore dotnet build
```

### 2) Lancer l’API
```powershell
dotnet run --project src/AsyncLocal-demo.Api
```

Swagger est activé en environnement Développement.

### 3) Exécuter les tests
```powershell
dotnet test
```

## Contexte d’exécution

Le contexte d’exécution est exposé via `IExecutionContext` et alimenté côté API par un middleware.

Règles appliquées par `OrderService` :

- `TenantId` est obligatoire → sinon `UnauthorizedAccessException`.
- `CorrelationId` est obligatoire → sinon `InvalidOperationException`.
- `UserId` est optionnel → valeur par défaut `anonymous`.

## Exemple (création de commande)
```console
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: dsf" \
  -H "X-User-Id: drm7348" \
  -d '{
    "items": [
      {"productId": "PORTABLE-001", "productName": "Dell XPS 15", "quantity": 1, "unitPrice": 1499.99},
      {"productId": "SOURIS-001", "productName": "Logitech MX", "quantity": 2, "unitPrice": 79.99}
    ]
  }'
```
## Exemple (Vérifier le contexte)
```console
curl http://localhost:5000/api/orders/context \
  -H "X-Tenant-Id: dsf" \
  -H "X-User-Id: drm7348"
```
## Exemple (Lister les commandes)
```console
curl http://localhost:5000/api/orders \
  -H "X-Tenant-Id: dsf"
```
## Notes

- Les identifiants utilisent `Guid.CreateVersion7()`.
- Le montant total est calculé à partir des lignes de commande.
