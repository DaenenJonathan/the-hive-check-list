# Utilisateurs de test

Ces comptes sont créés **automatiquement** au démarrage de l'API par
`src/Infrastructure/Persistence/DatabaseSeeder.cs` (appelé depuis
`Program.cs` juste après l'application des migrations EF Core). Si un
compte existe déjà (même email), il n'est pas recréé — vous pouvez donc
changer son mot de passe en base sans qu'il soit écrasé au prochain démarrage.

⚠️ **Ces comptes et mots de passe sont uniquement destinés au développement
local.** Ne jamais les utiliser tels quels dans un environnement de
production — voir `RUNNING_THE_APP.md` pour la checklist de déploiement.

## Comptes disponibles

| Nom | Email (= identifiant de connexion) | Mot de passe | Rôle |
|---|---|---|---|
| Jonathan Daenen | `daenen@thehive.local` | `Daenen@2026!` | **Admin** |
| Admin TheHive | `admin@thehive.local` | `Admin123!` | Admin |
| Manager Demo | `manager@thehive.local` | `Manager123!` | Manager |
| Jean Dupont | `warehouse@thehive.local` | `Warehouse123!` | WarehouseUser |

## Rôles applicatifs

- `Admin` — accès complet
- `Manager` — gestion des checklists et des imports Excel
- `WarehouseUser` — mise à jour du statut des articles en entrepôt
- `Viewer` — lecture seule (aucun compte de test seedé avec ce rôle pour l'instant)

## Créer d'autres comptes de test

Depuis le frontend, la page **Créer un compte** (`/login/register` ou
`/register`) permet de créer un nouvel utilisateur. Les comptes créés via
cette page reçoivent automatiquement le rôle `WarehouseUser` (le
changement de rôle vers `Admin`/`Manager` doit se faire manuellement en
base de données ou via un outil d'admin — un utilisateur ne peut pas
s'auto-attribuer un rôle élevé, c'est volontaire pour la sécurité).

Politique de mot de passe actuelle (`src/Infrastructure/DependencyInjection.cs`) :
- 6 caractères minimum
- au moins une majuscule, une minuscule et un chiffre
- caractère spécial non obligatoire

Après 5 échecs de connexion consécutifs, le compte est verrouillé
15 minutes (protection contre le brute-force).
