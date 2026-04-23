# ODPC

> **Branch `woo-hoo`** — Deze branch integreert de [woo-hoo](https://github.com/infonl/woo-hoo) service voor LLM-gestuurde metadata generatie in de lokale ontwikkelomgeving. Zie [woo-hoo sectie](#woo-hoo-ai-metadata-generatie) voor details.

## Lokaal opstarten

Dit project draait de volledige GPP-publicatie stack lokaal via [.NET Aspire](https://aspire.dev), inclusief:

- GPP-app (ODPC frontend + backend)
- GPP-publicatiebank (ODRC registratiecomponent)
- Open Zaak
- Keycloak (identity provider)
- **woo-hoo** — LLM-gestuurde metadata generatie voor DIWOO-publicaties

### Vereisten

- [dotnet sdk](https://dotnet.microsoft.com/en-us/download)
- [node lts](https://nodejs.org/en/download)
- [aspire cli](https://aspire.dev/get-started/install-cli/#install-the-aspire-cli)
- [just](https://github.com/casey/just#installation) (command runner)
- Docker

### Snel starten

```bash
# 1. Clone de woo-hoo repo naast deze repo (nodig voor AI metadata generatie)
git clone https://github.com/infonl/woo-hoo.git ../woo-hoo

# 2. Installeer frontend dependencies
just npm-install

# 3. Bouw de woo-hoo container image lokaal
just build-woo-hoo

# 4. Stel je LLM API key in (OpenRouter, nodig voor metadata generatie)
just set-llm-key <your-openrouter-api-key>

# 5. Start de stack
just run
```

Na het opstarten opent de Aspire dashboard automatisch. Vanuit daar kun je de app en alle services bereiken.

### Justfile commando's

| Commando | Beschrijving |
| --- | --- |
| `just run` | Start de volledige stack via Aspire |
| `just build` | Bouw de .NET solution |
| `just build-woo-hoo` | Bouw de woo-hoo container image lokaal |
| `just set-llm-key <key>` | Stel de LLM API key in voor metadata generatie |
| `just secrets` | Toon de huidige user secrets |
| `just npm-install` | Installeer frontend dependencies |
| `just typecheck` | Type-check de Vue frontend |
| `just reset-db` | Reset de database (verwijdert postgres volume) |
| `just clean` | Verwijder alle Aspire persistent containers en volumes |

### Devcontainer (alternatief)

1. Installeer de [devcontainers extensie](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) voor vscode
1. Zoek in het command palette (ctrl+shift+p) naar `clone repository in container volume`
1. Vul de url van deze branch in: `https://github.com/GPP-Woo/GPP-app/tree/run-everything`
1. Wacht totdat de devcontainer klaar is
1. Start aspire: `just run`

### woo-hoo (AI metadata generatie)

[woo-hoo](https://github.com/infonl/woo-hoo) is een LLM-gestuurde service die automatisch metadata genereert voor DIWOO-publicaties op basis van geupload documentinhoud. De service draait als container in de Aspire stack.

Om metadata generatie te gebruiken:

1. Zorg dat de woo-hoo image lokaal gebouwd is: `just build-woo-hoo`
2. Stel een [OpenRouter](https://openrouter.ai) API key in: `just set-llm-key <key>`
3. Start de stack: `just run`

In de publicatie-editor verschijnt de knop "Genereer metadata" wanneer er documenten bij een publicatie zijn. De gegenereerde suggesties worden in een preview getoond en kunnen selectief worden toegepast.

## Omgevingsvariabelen

| Variabele                  | Uitleg                                                                                                                                                                                                           |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `POSTGRES_DB`              | Naam van de database bij ODPC                                                                                                                                                                                    |
| `POSTGRES_USER`            | Gebruikersnaam voor toegang van ODPC tot de DB                                                                                                                                                                   |
| `POSTGRES_PASSWORD`        | Wachtwoord van de Postgres user                                                                                                                                                                                  |
| `POSTGRES_HOST`            | Hostnaam van de Postgres database server                                                                                                                                                                         |
| `POSTGRES_PORT`            | Poort om verbinding te maken met de Postgres database server                                                                                                                                                     |
| `OIDC_AUTHORITY`           | URL van de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `https://login.microsoftonline.com/ce1a3f2d-2265-4517-a8b4-3e4f381461ab/v2.0` </details>                  |
| `OIDC_CLIENT_ID`           | Voor toegang tot de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `54f66f54-71e5-45f1-8634-9158c41f602a` </details>                                                |
| `OIDC_CLIENT_SECRET`       | Secret voor de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `VM2B!ccnebNe.M*gxH63*NXc8iTiAGhp` </details>                                                         |
| `OIDC_ROLE_CLAIM_TYPE`     | De naam van de claim in het JWT token van de OpenID Connect Provider waarin de rollen van de ingelogde gebruiker staan. <br/> (default waarde is `roles`)                                                        |
| `OIDC_ADMIN_ROLE`          | De waarde van de role claim in het JWT token van de OpenID Connect Provider voor beheerders <details> <summary>Meer informatie </summary>Bijvoorbeeld: `odpc-admin` </details>                                   |
| `OIDC_ID_CLAIM_TYPE`       | De naam van de claim in het JWT token van de OpenID Connect Provider waarin de unieke identificatie van de ingelogde gebruiker staat. <br/> (default waarde is `preferred_username` met een fallback op `email`) |
| `OIDC_NAME_CLAIM_TYPE`     | De naam van de claim in het JWT token van de OpenID Connect Provider waarin de volledige naam van de ingelogde gebruiker staat <br/> (default waarde is `name`)                                                  |
| `ODRC_BASE_URL`            | De base url van de ODRC (Registratiecomponent) waarmee gekoppeld moet worden. <details> <summary>Meer informatie </summary>Bijvoorbeeld: `https://odrc.mijn-gemeente.nl` </details>                              |
| `ODRC_API_KEY`             | De geheime sleutel voor de ODRC (Registratiecomponent) waarmee gekoppeld moet worden. <details> <summary>Meer informatie </summary>Bijvoorbeeld: `VM2B!ccnebNe.M*gxH63*NXc8iTiAGhp`</details>                    |
| `UPLOAD_TIMEOUT_MINUTES`   | Het aantal minuten dat het uploaden van bestanden maximaal mag duren. <br/> (default waarde is `10`)                                                                                                             |
| `DOWNLOAD_TIMEOUT_MINUTES` | Het aantal minuten dat het downloaden van bestanden maximaal mag duren. <br/> (default waarde is `10`)                                                                                                           |
