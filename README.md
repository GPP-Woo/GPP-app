# ODPC

## Lokaal opstarten met Aspire

1. Installeer de devcontainers extensie voor vscode
1. Kies in het command palette voor ...
1. Vul de url van deze branch in
1. Wacht totdat de devcontainer klaar is
1. Start aspire: `aspire run`

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
