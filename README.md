# GPP-app

## Lokaal opstarten met Docker Compose

Je kan de GPP-app lokaal opstarten door het volgende commando uit te voeren in de rootmap van de repository: `docker compose up`

### GPP-app als afhankelijkheid in Docker Compose

We hebben ervoor gekozen om de GPP-publicatiebank (voorheen werd deze "ODRC" genoemd) als afhankelijkheid op te nemen in Docker Compose. Dit houdt in dat er een instantie van de GPP-publicatiebank wordt opgestart.

De configuratie en benodigde bestanden zijn overgenomen uit [De github repository van de GPP-publicatiebank](https://github.com/GPP-Woo/GPP-publicatiebank). Als daar iets in wijzigt, zullen we dat hier moeten doorvoeren.

Als je met een separate installatie (bijvoorbeeld een testomgeving) van de GPP-publicatiebank wil koppelen, moet je de variabelen `ODRC_BASE_URL` en `ODRC_API_KEY` aanpassen. Dit kun je ofwel in het docker-compose.yml bestand aanpassen (zorg dat je dit niet incheckt!), ofwel in je [user secrets bestand](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets). In het laatste geval moet je de variabelen verwijderen uit het docker-compose.yml bestand.

Standaard wordt bij het draaien van `docker compose up` de laatste versie van het image van de GPP-publicatiebank opgehaald. Als je dit wil forceren kan je het commando `docker compose pull odrc-django` uitvoeren.

Als je een specifieke versie van het image van de GPP-publicatiebank wil vastpinnen, zet je dit in de enviroment variable `ODRC_VERSION`

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
| `ODRC_BASE_URL`            | De base url van de GPP-publicatiebank (Registratiecomponent) waarmee gekoppeld moet worden. <details> <summary>Meer informatie </summary>Bijvoorbeeld: `https://odrc.mijn-gemeente.nl` </details>                              |
| `ODRC_API_KEY`             | De geheime sleutel voor de GPP-publicatiebank (Registratiecomponent) waarmee gekoppeld moet worden. <details> <summary>Meer informatie </summary>Bijvoorbeeld: `VM2B!ccnebNe.M*gxH63*NXc8iTiAGhp`</details>                    |
| `UPLOAD_TIMEOUT_MINUTES`   | Het aantal minuten dat het uploaden van bestanden maximaal mag duren. <br/> (default waarde is `10`)                                                                                                             |
| `DOWNLOAD_TIMEOUT_MINUTES` | Het aantal minuten dat het downloaden van bestanden maximaal mag duren. <br/> (default waarde is `10`)                                                                                                           |
