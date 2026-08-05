# Lancer l'application en local

L'application est composée de deux parties à lancer séparément :
le **backend** (.NET 8 / ASP.NET Core, `src/WebApi`) et le **frontend**
(Angular, `frontend/`).

Aujourd'hui tout tourne uniquement en local (base de données LocalDB,
CORS limité à `http://localhost:4200`, clé JWT en clair dans
`appsettings.json`). Voir la section **"Avant de déployer"** en bas de
page pour la liste de ce qu'il faudra changer.

## Prérequis

| Outil | Utilité |
|---|---|
| [.NET 8 SDK](https://dotnet.microsoft.com/download) (ou plus récent) | Compiler/lancer le backend |
| [Node.js](https://nodejs.org/) 18+ et npm | Compiler/lancer le frontend Angular |
| SQL Server LocalDB | Base de données locale (voir ci-dessous) |
| Outil global `dotnet-ef` | Créer/appliquer des migrations EF Core manuellement |
| Visual Studio 2022 (17.8+) **ou** VS Code | Éditeur / IDE |

Installer l'outil EF Core si besoin :
```bash
dotnet tool install --global dotnet-ef
```

## Base de données

Le projet utilise **SQL Server LocalDB**, avec la chaîne de connexion
définie dans `src/WebApi/appsettings.json` :
```
Server=(localdb)\MSSQLLocalDB;Database=TheHiveDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

- Si vous avez installé **Visual Studio** avec la charge de travail
  *"Développement ASP.NET et web"*, LocalDB est déjà installé.
- Sinon, installez-le seul via le
  [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
  (choisir "LocalDB" dans le téléchargement SQL Server Express).
- Aucune installation de SQL Server Management Studio n'est nécessaire,
  mais elle facilite l'inspection de la base si besoin (se connecter à
  `(localdb)\MSSQLLocalDB`).

**Les migrations sont appliquées automatiquement au démarrage de l'API**
(`context.Database.MigrateAsync()` dans `src/WebApi/Program.cs`), et les
utilisateurs de test sont créés dans la foulée (`DatabaseSeeder.cs` —
voir `TEST_USERS.md`). Vous n'avez donc **rien à faire manuellement** au
premier lancement : la base `TheHiveDb` sera créée toute seule.

Si vous devez appliquer les migrations manuellement (ex: hors du
démarrage de l'app) :
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Option A — Lancer avec Visual Studio

1. Ouvrir `TheHive.slnx` à la racine du projet.
2. Clic droit sur le projet **WebApi** → *Définir comme projet de
   démarrage*.
3. Choisir le profil `https` dans la liste déroulante (à côté du bouton
   ▶️) puis lancer (F5 ou Ctrl+F5). Swagger s'ouvre sur
   `https://localhost:7051/swagger` si vous laissez `launchUrl` par défaut,
   sinon naviguez-y manuellement.
4. Le frontend Angular **ne se lance pas depuis Visual Studio** : ouvrez
   un terminal dans `frontend/` et lancez (voir "Frontend" ci-dessous).

## Option B — Lancer avec VS Code

1. Ouvrir le dossier racine du projet dans VS Code.
2. Extensions recommandées : *C# Dev Kit* (ou *C#* d'Anysphere/OmniSharp)
   pour le backend, *Angular Language Service* pour le frontend.
3. Dans un terminal intégré :
   ```bash
   dotnet restore
   dotnet run --project src/WebApi
   ```
   L'API démarre sur `http://localhost:5000` (et `https://localhost:7051`
   si le profil `https` est utilisé — voir
   `src/WebApi/Properties/launchSettings.json`).
4. Dans un **second** terminal intégré (voir "Frontend" ci-dessous).

## Frontend (identique VS Code / Visual Studio)

```bash
cd frontend
npm install       # uniquement la première fois, ou après un changement de dépendances
npm start         # équivalent à `ng serve`, sert sur http://localhost:4200
```

Le frontend est déjà configuré (`frontend/src/environments/environment.ts`)
pour appeler l'API sur `http://localhost:5000/api` et le hub SignalR sur
`http://localhost:5000`. Si vous lancez le backend en HTTPS uniquement,
mettez à jour `apiUrl`/`hubUrl` en conséquence.

Ouvrir ensuite `http://localhost:4200` dans le navigateur. Se connecter
avec un des comptes listés dans `TEST_USERS.md`, ou créer un compte via
le lien *"Créer un compte"* sur la page de connexion.

## Résumé des ports

| Service | URL |
|---|---|
| Frontend Angular | http://localhost:4200 |
| API backend (HTTP) | http://localhost:5000 |
| API backend (HTTPS) | https://localhost:7051 |
| Swagger | `{URL de l'API}/swagger` |
| Hub SignalR | `{URL de l'API}/hubs/checklists` |

## Base de données : SQL Server en local, PostgreSQL en prod

Le projet supporte deux providers EF Core en parallèle, chacun avec son
propre jeu de migrations (les migrations EF Core sont spécifiques à un
provider — impossible de faire tourner les mêmes sur SQL Server et sur
Postgres) :

- `SqlServerApplicationDbContext` — migrations dans
  `src/Infrastructure/Migrations/` (utilisé par défaut, en local).
- `PostgresApplicationDbContext` — migrations dans
  `src/Infrastructure/Migrations/Postgres/` (utilisé en production).

Le provider actif est choisi via la clé de config `Database:Provider`
(`SqlServer` ou `Postgres`, valeur par défaut `SqlServer`) — voir
`appsettings.json` (dev) et `appsettings.Production.json` (qui bascule
sur `Postgres`). Au démarrage, `context.Database.MigrateAsync()`
n'applique que les migrations du provider actif.

Pour ajouter une migration après un changement de modèle, il faut la
générer **pour les deux providers** :
```bash
dotnet ef migrations add <NomMigration> --project src/Infrastructure --startup-project src/WebApi
dotnet ef migrations add <NomMigration> --project src/Infrastructure --startup-project src/Infrastructure --context PostgresApplicationDbContext --output-dir Migrations/Postgres
```
(La deuxième commande utilise `PostgresApplicationDbContextFactory` pour
générer la migration sans connexion Postgres réelle ; le
`--startup-project` pointe sur Infrastructure pour éviter de dépendre de
la config runtime de WebApi.)

## Déploiement avec Docker

Le repo contient tout ce qu'il faut pour déployer via Docker Compose sur
un VPS Linux :

- `src/WebApi/Dockerfile` — build multi-stage du backend (.NET SDK →
  runtime ASP.NET), écoute sur le port 8080 à l'intérieur du conteneur.
- `frontend/Dockerfile` + `frontend/nginx.conf` — build Angular en mode
  production, servi par Nginx, qui fait aussi office de reverse proxy
  vers le backend pour `/api/`, `/images/` (fichiers statiques uploadés)
  et `/hubs/` (WebSocket SignalR).
- `docker-compose.yml` — orchestre `postgres`, `backend` et `frontend`
  (seul `frontend` expose un port sur l'hôte, `80`).

Sur le VPS :
```bash
git clone <votre-repo> && cd TheHiveCheckLists
cp .env.example .env
# éditer .env : POSTGRES_PASSWORD, JWT_KEY (openssl rand -base64 48), APP_ORIGIN
docker compose up -d --build
```
Les migrations PostgreSQL et le seed des utilisateurs de test s'exécutent
automatiquement au démarrage du conteneur `backend` (même logique qu'en
local). Les données Postgres et les images uploadées persistent dans les
volumes nommés `postgres_data` et `backend_images`.

Ce qui **reste à faire manuellement**, non couvert par ce compose file :
- [ ] Mettre en place HTTPS (Let's Encrypt/certbot) devant le port 80 —
      actuellement le trafic est en clair. Un moyen simple est
      d'ajouter Certbot + un renouvellement automatique devant Nginx,
      ou de placer Traefik/Caddy en frontal.
- [ ] Revoir les mots de passe des comptes de test (`TEST_USERS.md`) —
      ne pas les laisser tels quels en prod.
- [ ] Sauvegardes régulières (`pg_dump` planifié pour le volume
      `postgres_data`, sauvegarde du volume `backend_images`).
- [ ] Pare-feu du VPS (`ufw allow 80,443/tcp`, bloquer le reste).
