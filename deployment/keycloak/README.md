# Checknote Keycloak Template

This folder holds public-safe Keycloak setup material for the Checknote reference-first authentication baseline.

The production runtime is external to the Checknote build: Keycloak runs directly on Windows with a supported Java runtime and is installed as a Windows service. Docker, Linux containers, and WSL are not part of the Checknote deployment stance. Bazel governs the Checknote-owned templates and tests in this folder, but it does not own the live Keycloak server process.

## Local Shape

- Realm: `checknote`
- Angular public client: `checknote-angular`
- Local browser app origins: `http://127.0.0.1:4200` and `http://localhost:4200`
- Local Keycloak issuer: `http://127.0.0.1:8080/realms/checknote`
- Local Keycloak metadata: `http://127.0.0.1:8080/realms/checknote/.well-known/openid-configuration`
- Local Keycloak health: `http://127.0.0.1:9000/health/`

## Browser Login Runtime

The Angular browser flow needs a browser-reachable Keycloak base URL through `Keycloak__AuthServerUrl`.

The API may validate tokens against a server-local metadata URL such as `http://127.0.0.1:8080/...`, but the Angular app cannot use a server-local URL from a user's browser. Production login needs Keycloak exposed through an appropriate public host or IIS reverse proxy before the sign-in button can complete the browser redirect flow.

## Public Repo Boundary

The repo can contain Keycloak realm templates, client names, local development URLs, API JWT validation code, tests, and docs.

Do not commit live user TOTP secrets, Keycloak admin credentials, Keycloak client secrets, signing keys, database connection strings, recovery codes, logs containing identity/session data, or exported realm files from production.

Copy `keycloak.realm.template.json` to a local ignored file before replacing placeholders such as `__CHECKNOTE_SMTP_HOST__` or `__CHECKNOTE_SMTP_PASSWORD__`.
