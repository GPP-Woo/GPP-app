.. _installation_index:

Installatie
============

Deze pagina is bedoeld voor technisch beheerders die de GPP-App installeren en koppelen aan
de overige componenten.

De GPP-App kan in een Kubernetes-cluster geïnstalleerd worden met behulp van
`Helm <https://helm.sh/>`_. In een aparte repository houden de ontwikkelaars
`de Helm charts bij <https://github.com/GPP-Woo/charts>`_. Daar zijn ook verdere
installatie-instructies te vinden, zowel voor de GPP-App los als voor het volledige
GPP-Woo-platform.

Voor lokaal testen kan de GPP-App met Docker Compose opgestart worden, zie de
`README in de Github-repository <https://github.com/GPP-Woo/GPP-app>`_. Gebruik die
installatie niet voor productie.

.. contents:: Op deze pagina
   :local:
   :depth: 2

Benodigdheden
-------------

De GPP-App bestaat uit één container (de webapplicatie inclusief de API die de
gebruikersinterface gebruikt) en heeft daarnaast nodig:

* **Een PostgreSQL-database.** Hierin slaat de GPP-App de gebruikersgroepen, hun leden en
  autorisaties op, en de namen van gebruikers die ingelogd zijn geweest. De publicaties en
  documenten zelf staan niet in deze database, maar in de publicatiebank. De
  databasemigraties worden bij het starten automatisch uitgevoerd.
* **Een GPP-Publicatiebank** (of passend alternatief) waarin de publicaties, documenten en
  waardelijsten opgeslagen worden. Zie :ref:`installation_index_publicatiebank`.
* **Een OpenID Connect Identity Provider** voor het inloggen, bijvoorbeeld Microsoft Entra
  ID of Keycloak. Zie :ref:`installation_index_oidc`.
* Voor de knop **Bekijk online** bij gepubliceerde publicaties: een burgerportaal, zoals het
  `GPP-Burgerportaal <https://github.com/GPP-Woo/GPP-burgerportaal>`_. De link daarnaartoe
  wordt ingesteld in de publicatiebank.

.. _installation_index_omgevingsvariabelen:

Omgevingsvariabelen
--------------------

Bij de installatie heeft de GPP-App de volgende omgevingsvariabelen ("environment
variables") nodig. Ze staan ook in de
`README van de Github-repository <https://github.com/GPP-Woo/GPP-APP?tab=readme-ov-file#omgevingsvariabelen>`_.

**Database**

* ``POSTGRES_HOST``: Hostnaam van de PostgreSQL-databaseserver.
* ``POSTGRES_PORT``: Poort van de PostgreSQL-databaseserver.
* ``POSTGRES_DB``: Naam van de database.
* ``POSTGRES_USER``: Gebruikersnaam waarmee de GPP-App verbinding maakt met de database.
* ``POSTGRES_PASSWORD``: Wachtwoord van die gebruiker.

**Koppeling met de publicatiebank**

* ``ODRC_BASE_URL``: Het basisadres van de publicatiebank, zonder ``/api/...``, bijvoorbeeld
  ``https://gpp-publicatiebank.mijn-gemeente.nl``. ("ODRC" is de oude naam van de
  publicatiebank.)
* ``ODRC_API_KEY``: De API-key waarmee de GPP-App toegang krijgt tot de publicatiebank.
* ``UPLOAD_TIMEOUT_MINUTES``: Het aantal minuten dat het uploaden van een bestand naar de
  publicatiebank maximaal mag duren. Standaard ``10``.
* ``DOWNLOAD_TIMEOUT_MINUTES``: Het aantal minuten dat het downloaden van een bestand uit de
  publicatiebank maximaal mag duren. Standaard ``10``.

**Inloggen met OpenID Connect**

* ``OIDC_AUTHORITY``: Het adres van de OpenID Connect Identity Provider, bijvoorbeeld
  ``https://login.microsoftonline.com/<tenant-id>/v2.0``.
* ``OIDC_CLIENT_ID``: De client-ID van de GPP-App bij de Identity Provider.
* ``OIDC_CLIENT_SECRET``: Het bijbehorende geheim (secret).
* ``OIDC_ADMIN_ROLE``: De naam van de rol die beheerdersrechten geeft, bijvoorbeeld
  ``odpc-admin``.
* ``OIDC_ROLE_CLAIM_TYPE``: De naam van de claim waarin de rollen van de gebruiker staan.
  Standaard ``roles``.
* ``OIDC_ID_CLAIM_TYPE``: De naam van de claim met de unieke identificatie (inlognaam) van de
  gebruiker. Standaard ``preferred_username``, met ``email`` als terugvaloptie.
* ``OIDC_NAME_CLAIM_TYPE``: De naam van de claim met de volledige naam van de gebruiker.
  Standaard ``name``.

.. warning:: Behandel ``POSTGRES_PASSWORD``, ``ODRC_API_KEY`` en ``OIDC_CLIENT_SECRET`` als
   wachtwoorden. Sla ze op als secret (bijvoorbeeld een Kubernetes Secret) en niet in
   platte tekst in een configuratiebestand.

.. _installation_index_publicatiebank:

Koppeling met de GPP-Publicatiebank
-----------------------------------

De GPP-App leest en schrijft alle publicaties, documenten en inzageprocedures via de API
van de publicatiebank, en haalt daar ook de waardelijsten (organisaties,
informatiecategorieën en onderwerpen) op.

1. Maak in de beheeromgeving van de publicatiebank een API-key aan voor de GPP-App, onder
   **API-toegang** > **Applicatie-API-keys**. Geef de API-key zowel lees- als
   schrijfrechten.
2. Vul het basisadres van de publicatiebank in bij ``ODRC_BASE_URL`` en de API-key bij
   ``ODRC_API_KEY``.
3. Stel in de algemene instellingen van de publicatiebank de URL-sjablonen in voor de
   GPP-App en het burgerportaal. De GPP-App gebruikt het sjabloon van het burgerportaal voor
   de knop **Bekijk online**.

Bij elke wijziging geeft de GPP-App de inlognaam en naam van de ingelogde gebruiker mee aan
de publicatiebank. Zo is in de audit trail van de publicatiebank te zien wie wat heeft
gedaan, en wordt de juiste eigenaar van een publicatie vastgelegd.

.. note:: Deze versie van de GPP-App gebruikt het ``/api/v2``-endpoint voor
   inzageprocedures van de publicatiebank. Dat is beschikbaar vanaf GPP-Publicatiebank
   3.0.0. Gebruik daarom GPP-Publicatiebank 3.0.0 of nieuwer.

.. _installation_index_oidc:

Configuratie voor de OpenID Connect Identity Provider
-----------------------------------------------------

De GPP-App ondersteunt Single Sign On (SSO) met behulp van OpenID Connect Identity Providers.
Dit betekent dat de organisatie bestaande organisatieaccounts kan gebruiken als er
gecentraliseerd accountbeheer is, zoals Microsoft Entra ID. Het gebruik van OIDC stelt de
organisatie ook in staat om gebruikers automatisch de juiste rol mee te geven op basis van
hun rollen die beheerd worden in de oplossing voor Identiteits- en Toegangsbeheer (IAM).
Hiervoor moeten specifieke zaken ingeregeld worden in de eigen Identity Provider, en moeten
de omgevingsvariabelen die beginnen met ``OIDC_`` worden ingevuld.

Allereerst moeten de IAM-beheerders in je organisatie een set referenties creëren voor de
GPP-App. Gewoonlijk vereist dit dat ze een Client of een App registreren in een
beheerportaal. Daarbij is het volgende van belang:

* **Redirect URI**: ``https://<adres-van-de-gpp-app>/signin-oidc``. Dit is het adres waar
  de Identity Provider de gebruiker na het inloggen naar terugstuurt.
* **Post-logout redirect URI**: ``https://<adres-van-de-gpp-app>/signout-callback-oidc``.
  Dit is het adres waar de gebruiker na het uitloggen naar terugkeert.
* **Flow**: authorization code flow met PKCE, met een client secret.
* **Scopes**: ``openid`` en ``profile``.
* **Claims**: de claims voor de inlognaam, de volledige naam en de rollen moeten in het
  ID-token of via het userinfo-endpoint beschikbaar zijn (zie de ``OIDC_..._CLAIM_TYPE``
  variabelen hierboven).

Daarnaast geldt:

* Alle gebruikers die gekoppeld zijn aan de hierboven genoemde Client of App, kunnen
  inloggen op de GPP-App. Om publicaties te kunnen maken, moeten ze daarna nog door een
  beheerder aan een :ref:`gebruikersgroep <handleiding_beheerders_index_gebruikersgroepen>`
  worden toegevoegd.
* Voor gebruikers die ook beheeractiviteiten moeten uitvoeren, moet er een specifieke rol
  meekomen uit de Identity Provider. De naam van deze rol, bijvoorbeeld ``odpc-admin``, moet
  opgenomen worden in ``OIDC_ADMIN_ROLE``. De naam van de claim waarin deze rol wordt
  meegegeven, stel je in met ``OIDC_ROLE_CLAIM_TYPE``.
* De waarde uit de claim ``OIDC_ID_CLAIM_TYPE`` is de inlognaam die beheerders invullen bij
  het toevoegen van gebruikers aan een gebruikersgroep. Kies een waarde die uniek is en niet
  verandert, bijvoorbeeld het e-mailadres of de UPN, en communiceer aan de beheerders welke
  waarde dat is.

Een gebruiker blijft ingelogd zolang hij of zij actief is; na ongeveer een uur zonder
activiteit moet de gebruiker opnieuw inloggen.

Controle na installatie
-----------------------

* Open de GPP-App en log in. Je komt uit op "Mijn publicaties".
* Log in met een gebruiker met de beheerdersrol en controleer of het menu-item
  **Gebruikersgroepen** zichtbaar is.
* Maak een gebruikersgroep aan. Worden de organisaties en informatiecategorieën getoond, dan
  werkt de koppeling met de publicatiebank.
* Maak een testpublicatie met een document aan en sla deze op als concept. Verwijder de
  testpublicatie daarna weer.
* Het versienummer van de geïnstalleerde GPP-App is op te vragen via
  ``https://<adres-van-de-gpp-app>/api/environment/version``.
