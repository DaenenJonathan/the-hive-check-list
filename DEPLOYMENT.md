# CI/CD & déploiement

## Vue d'ensemble

Un seul workflow (`.github/workflows/ci-cd.yml`) gère tout :

```
push / PR sur main
   ├─ backend-test   (dotnet build + test)
   └─ frontend-test  (npm ci + ng test headless + ng build)
        │  (seulement si push sur main, pas sur une PR)
        ▼
   docker-build-push  (build + push des images vers GHCR, tag :latest et :<sha>)
        ▼
   deploy             (SSH vers le VPS → git reset --hard → docker compose pull/up -d)
```

Les Pull Requests vers `main` déclenchent uniquement les tests (gate avant merge). Seul un push/merge sur `main` déclenche le build des images et le déploiement.

## Secrets GitHub à configurer

Dans le repo GitHub : **Settings → Secrets and variables → Actions → New repository secret**.

| Secret | Description |
|---|---|
| `VPS_HOST` | IP ou nom d'hôte du VPS |
| `VPS_USER` | Utilisateur SSH utilisé pour le déploiement (ex: `deploy`) |
| `VPS_SSH_KEY` | Clé privée SSH dédiée au déploiement (voir ci-dessous) |
| `VPS_PORT` | Port SSH si différent de 22 (optionnel) |
| `VPS_DEPLOY_PATH` | Chemin absolu du clone du repo sur le VPS (ex: `/opt/thehive`) |

`GITHUB_TOKEN` est fourni automatiquement par GitHub Actions — pas besoin de le créer, il sert à publier/pull les images sur GHCR (`ghcr.io`).

## Préparation du VPS (une seule fois)

### 1. Générer une clé SSH dédiée au déploiement

Sur ta machine (pas sur le VPS) :

```bash
ssh-keygen -t ed25519 -f deploy_key -C "github-actions-deploy" -N ""
```

- Copie **`deploy_key.pub`** dans `~/.ssh/authorized_keys` de l'utilisateur `VPS_USER` sur le VPS.
- Colle le contenu de **`deploy_key`** (clé privée) dans le secret GitHub `VPS_SSH_KEY`.
- Supprime les deux fichiers locaux une fois copiés.

### 2. Cloner le repo sur le VPS

```bash
sudo mkdir -p /opt/thehive && sudo chown $USER:$USER /opt/thehive
git clone https://github.com/DaenenJonathan/the-hive-check-list.git /opt/thehive
```

> Si le repo GitHub est **privé**, `git clone`/`git pull` via HTTPS demandera une authentification. Le plus simple : passer le repo en public, ou configurer un [Deploy Key GitHub](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/managing-deploy-keys) en lecture seule sur le VPS et cloner en SSH (`git@github.com:...`) à la place.

### 3. Rendre les packages GHCR accessibles

Par défaut, les images poussées sur `ghcr.io` héritent de la visibilité du repo. Si le repo est privé, le VPS doit s'authentifier avant de faire `docker compose pull` — le workflow le fait déjà automatiquement à chaque déploiement via `docker login` avec le `GITHUB_TOKEN` du run. Aucune action requise de ta part sauf si tu veux aussi pouvoir `pull` manuellement depuis le VPS (dans ce cas, génère un [Personal Access Token `read:packages`](https://github.com/settings/tokens) et fais `docker login ghcr.io -u <user> -p <token>` une fois).

### 4. Créer le fichier `.env` de production

Ce fichier ne quitte **jamais** le VPS (il n'est ni commité, ni transporté par le pipeline) :

```bash
cd /opt/thehive
cp .env.example .env
nano .env   # renseigner POSTGRES_PASSWORD, JWT_KEY (openssl rand -base64 48), APP_ORIGIN, FRONTEND_HTTP_PORT
```

`FRONTEND_HTTP_PORT` (nouveau, défaut `8080`) est le port sur lequel le conteneur frontend écoute en local sur le VPS — c'est ce port que ton reverse proxy existant doit cibler (voir plus bas).

### 5. Premier déploiement manuel

```bash
cd /opt/thehive
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Les déploiements suivants sont automatiques via le pipeline.

## Reverse proxy existant

`docker-compose.prod.yml` publie le frontend sur `127.0.0.1:${FRONTEND_HTTP_PORT:-8080}` (au lieu du port 80 utilisé en local). Le backend n'est jamais exposé directement : Nginx (dans le conteneur frontend) proxifie déjà `/api`, `/images` et `/hubs` vers le backend en interne.

Configure ton reverse proxy (Nginx Proxy Manager / Traefik / Caddy) pour router ton nom de domaine vers `http://<IP_VPS>:8080` (ou `http://127.0.0.1:8080` si le proxy tourne sur le même hôte), avec le certificat TLS géré côté proxy.

## Rollback

Chaque image est aussi taguée avec le SHA du commit (`ghcr.io/daenenjonathan/the-hive-check-list-backend:<sha>`). Pour revenir à une version précédente sur le VPS :

```bash
cd /opt/thehive
git log --oneline -10          # repérer le sha voulu
export IMAGE_TAG=<sha>
docker compose -f docker-compose.yml -f docker-compose.prod.yml pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```
