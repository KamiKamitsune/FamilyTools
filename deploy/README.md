# Déploiement FamilyTools (VM `family-tools`)

Déploiement **sans registre** et **sans build sur la VM** : la CI produit des images et
les transfère par SSH, la VM ne fait que `docker load` + `docker compose up`.

## Topologie

| Service | Image | Exposé | Rôle |
|---|---|---|---|
| `familytools` | `mssql/server:2022` | interne | SQL Server (volume `familytools-sql-data`) |
| `cache` | `redis:8.6` | interne | Cache Redis |
| `messaging` | `rabbitmq:4.3` | interne | Bus de messages |
| `keycloak` | `keycloak:26.6` | **8080** | SSO/OIDC (volume `familytools-keycloak-data`) |
| `migrations` | `familytools/migrations` | — | Migrations EF (run-once) |
| `easycomptaapi` | `familytools/easycomptaapi` | interne | API EasyCompta |
| `accounts` | `familytools/accounts` | interne | API "Mon compte" |
| `csv-worker` | `familytools/csv-worker` | — | Worker d'import CSV |
| `webfrontend` | `nginx:1.27` | **4200** | SPA Angular + reverse-proxy `/api` `/hubs` `/identity` |
| `familytools-compose-dashboard` | `aspire-dashboard` | 18888 | Observabilité OTEL |

## Chaîne CI → VM (job `deploy`, branche `main`)

1. `dotnet publish -t:PublishContainer` → 4 tarballs OCI (`bundle/images/*.tar`), **sans démon Docker**.
2. Réutilise le dist Angular (artefact de `build-frontend`) → `bundle/web/`.
3. Embarque `docker-compose.yaml`, `nginx.conf`, le `.env` (secret `DEPLOY_ENV_FILE`),
   les thèmes et le realm Keycloak.
4. Transfert du bundle → `/opt/familytools` sur la VM (`tar` over SSH ; pas de rsync requis sur la VM).
5. SSH : `docker load` des tarballs puis `docker compose up -d`.

## Variables CI/CD requises (Settings → CI/CD → Variables, **Protected**)

| Variable | Type | Contenu |
|---|---|---|
| `DEPLOY_HOST` | Variable | IP de la VM (`192.168.8.210`) |
| `DEPLOY_USER` | Variable | utilisateur SSH (`debian`) |
| `DEPLOY_SSH_KEY` | File | clé privée de déploiement (ed25519) |
| `DEPLOY_ENV_FILE` | File | secrets stables (cf. `.env.example`) |

## Déploiement manuel (debug, depuis la VM)

```bash
cd /opt/familytools
for t in images/*.tar; do docker load -i "$t"; done
docker compose --env-file .env up -d
```

## À faire côté hôte/réseau (hors scope CI)

- **Authentification** : le SPA (navigateur) joint Keycloak sur `:8080`. Les `redirectUris`
  du realm `familytools` doivent inclure l'URL publique réelle de la VM (pas `localhost`).
- Mettre un reverse-proxy TLS devant `:4200` (front) et `:8080` (Keycloak) pour la prod.
