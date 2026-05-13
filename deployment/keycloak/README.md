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

## Login Theme

The `themes/checknote/login` folder contains the Checknote Keycloak login theme. It is served by Keycloak from `auth.checknote.io`, not by the Angular app from `www.checknote.io`.

The theme inherits from Keycloak `base`, vendors Normalize.css and Skeleton 2.0.4 as local CSS baselines, then layers `checknote-login.css` on top for the Checknote-specific layout and palette. The live auth host should not depend on a public CSS CDN. Keycloak still owns the auth flow/templates, while Checknote owns the visible auth page styling.

The first theme pass is CSS-first and follows the current task-list palette:

- page background: `#f5f7fb`
- primary text: `#182033`
- muted text: `#53617a` and `#63718a`
- card background: `#ffffff`
- card border: `#d9e0ec`
- input border: `#c6cfdd`
- primary action: `#234b78`
- focus outline: `#9db8df`
- success/accent green: `#e8f3ee` and `#1f6f54`

To install the theme on a Windows Keycloak ZIP runtime, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\install-checknote-theme.ps1 -KeycloakHome "C:\Services\keycloak\keycloak-26.6.1"
```

Then select `checknote` as the login theme for the `checknote` realm in Keycloak Realm settings, or import a realm template that sets `"loginTheme": "checknote"`.

## Public Repo Boundary

The repo can contain Keycloak realm templates, client names, local development URLs, API JWT validation code, tests, and docs.

Do not commit live user TOTP secrets, Keycloak admin credentials, Keycloak client secrets, signing keys, database connection strings, recovery codes, logs containing identity/session data, or exported realm files from production.

Copy `keycloak.realm.template.json` to a local ignored file before replacing placeholders such as `__CHECKNOTE_SMTP_HOST__` or `__CHECKNOTE_SMTP_PASSWORD__`.
