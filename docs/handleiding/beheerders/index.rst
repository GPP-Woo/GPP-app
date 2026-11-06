.. _handleiding_beheerders_index:

Handleiding voor beheerders
===========================

Niet iedere gebruiker mag op álle informatiecategorieën en onderwerpen en namens álle
(geactiveerde) organisaties publicaties aanmaken. In de GPP-App worden daarom
gebruikersgroepen aangemaakt, waaraan autorisaties worden toegekend. Een gebruikersgroep is
gekoppeld aan specifieke organisaties, informatiecategorieën en onderwerpen. Gebruikers
binnen de gebruikersgroep kunnen alleen publicaties aanmaken voor die organisaties, en binnen
die informatiecategorieën en onderwerpen.

Alleen gebruikers met beheerdersrechten kunnen in de GPP-App gebruikersgroepen aanmaken, en
daar gebruikers aan toevoegen.

.. contents:: Op deze pagina
   :local:
   :depth: 2

Hoe krijgt een gebruiker beheerdersrechten?
-------------------------------------------

Om een gebruiker beheerdersrechten te geven, moet deze een specifieke rol krijgen in de
OpenID Connect Identity Provider (bijv. Microsoft Entra ID). De naam van deze rol moet zijn
afgestemd met de beheerders van de Identity Provider, en bij installatie van de App zijn
ingeregeld (zie :ref:`installation_index_oidc`). Neem hiervoor contact op met de beheerders
van de Identity Provider.

Een gebruiker met beheerdersrechten ziet in de menubalk ook het item **Gebruikersgroepen**.
Rollen worden bij het inloggen uitgelezen: na het toekennen of intrekken van de rol moet de
gebruiker opnieuw inloggen.

.. note:: Beheerdersrechten geven toegang tot het beheer van gebruikersgroepen, maar niet
   automatisch tot publicaties. Wil een beheerder zelf publicaties maken of die van collega's
   zien, dan moet hij of zij ook lid zijn van een gebruikersgroep.

.. _handleiding_beheerders_index_gebruikersgroepen:

Gebruikersgroepen
-----------------

Achter het menu-item **Gebruikersgroepen** vind je de bestaande gebruikersgroepen, in
alfabetische volgorde. Let op: de naam van een gebruikersgroep kan best lang zijn. Houd er
rekening mee dat één hele lange naam de hoogte van de rij beïnvloedt.

Bovenaan staan twee knoppen:

* **Nieuwe gebruikersgroep**: opent het scherm om een gebruikersgroep aan te maken.
* **Autorisatieoverzicht downloaden**: zie :ref:`handleiding_beheerders_index_overzicht`.

Klik op een bestaande gebruikersgroep om deze te beheren. Het scherm voor het aanmaken of
beheren van een gebruikersgroep bestaat uit twee delen: de gegevens van de gebruikersgroep en
de waardelijsten.

Gebruikersgroep gegevens
^^^^^^^^^^^^^^^^^^^^^^^^

* ``ID``: Een automatisch toegekende, unieke identificatie (alleen bij een bestaande
  gebruikersgroep).
* ``Naam`` \*: De naam van de gebruikersgroep. Gebruikers zien deze naam als ``Profiel`` bij
  het aanmaken van een publicatie, dus kies een herkenbare naam, bijvoorbeeld "Team
  Vergunningen".
* ``Omschrijving``: Een omschrijving van de gebruikersgroep, bijvoorbeeld voor welke
  afdeling of welk soort publicaties de groep bedoeld is.
* ``Geautoriseerd voor Inzage-procedure``: Vink dit aan als de leden van deze
  gebruikersgroep een :ref:`inzageprocedure <handleiding_gebruikers_index_inzageprocedure>`
  aan hun publicaties mogen koppelen. Een bestaande inzageprocedure kan door de leden altijd
  worden gewijzigd, ook zonder dit vinkje.
* ``Gebruiker toevoegen``: Voeg een gebruiker toe aan deze gebruikersgroep door in dit veld
  de inlognaam van de gebruiker in te vullen en op **Toevoegen** te klikken (of op Enter te
  drukken).
* **Toegevoegde gebruikers**: Hier zie je de inlognamen van de gebruikers die aan deze groep
  zijn toegevoegd. Heeft de gebruiker al eens ingelogd, dan wordt ook de naam getoond die
  uit de Identity Provider is opgehaald; anders staat er een streepje. Door op het
  prullenbakje achter de gebruiker te klikken, verwijder je deze uit de gebruikersgroep.

Gebruikers worden toegevoegd op basis van hun inlognaam in de Identity Provider. Vaak is dit
het e-mailadres. Stem dit af met de beheerders van de Identity Provider. Je kunt een
gebruiker ook al toevoegen voordat deze voor het eerst heeft ingelogd.

.. tip:: Wordt de naam van een gebruiker niet getoond, terwijl je zeker weet dat de
   gebruiker al heeft ingelogd? Controleer dan of de inlognaam exact overeenkomt met de
   identificatie uit de Identity Provider. Hoofd- en kleine letters maken hierbij niet uit.

Waardelijsten
^^^^^^^^^^^^^

Bij **Waardelijsten** zie je voor welke organisaties, informatiecategorieën en onderwerpen
deze gebruikersgroep geautoriseerd is. Dit betekent dat de gebruikers uit deze
gebruikersgroep publicaties kunnen aanmaken voor die organisaties, informatiecategorieën en
onderwerpen. Door één van deze onderdelen open te klappen, zie je welke opties er binnen dat
onderdeel zijn.

* Door een item aan te vinken, wordt de gebruikersgroep geautoriseerd voor dat item.
* Door een item uit te vinken, wordt de autorisatie voor dat item voor de gebruikersgroep
  ingetrokken.
* Met **selecteer alles** vink je in één keer alle items van dat onderdeel aan (of weer uit).
* Achter sommige informatiecategorieën staat een vraagteken met een toelichting.

Klik onderaan op **Opslaan** om de wijzigingen op te slaan, of op **Annuleren** om ze te
verwerpen.

.. warning:: Een gebruiker moet lid zijn van minstens één gebruikersgroep! En iedere
   gebruikersgroep moet geautoriseerd zijn voor minstens één organisatie en minstens één
   informatiecategorie!

.. warning:: Houd er bij het intrekken van autorisaties rekening mee dat gebruikers binnen
   deze gebruikersgroep mogelijk al publicaties hebben aangemaakt binnen die autorisatie. Na
   het intrekken van die autorisatie kunnen deze gebruikers die publicaties niet meer
   aanpassen: ze krijgen de melding "Onder dit profiel heb je niet (meer) de juiste rechten om
   deze publicatie aan te passen. Neem contact op met de beheerder."

.. note:: Het beheren van de items in de waardelijsten gebeurt in het registratiecomponent
   waaraan de GPP-App is gekoppeld, bijvoorbeeld de
   `GPP-Publicatiebank <https://gpp-publicatiebank.readthedocs.io/en/latest/admin/index.html>`_.
   Daar worden bijvoorbeeld organisaties geactiveerd en eigen informatiecategorieën en
   onderwerpen toegevoegd. Alleen geactiveerde organisaties verschijnen in de GPP-App.

Een gebruikersgroep verwijderen
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Open de gebruikersgroep en klik op **Verwijderen**. Na bevestiging wordt de gebruikersgroep
direct verwijderd.

.. warning:: Publicaties die bij de verwijderde gebruikersgroep horen, blijven bestaan, maar
   kunnen in de GPP-App niet meer aangepast, ingetrokken of geclaimd worden. Ze staan ook niet
   meer onder "Publicaties van collega's". De eigenaar ziet ze nog wel onder "Mijn
   publicaties", maar kan ze alleen raadplegen. Verwijder een gebruikersgroep daarom pas als er
   geen lopende publicaties meer onder vallen, of pas de leden en autorisaties aan in plaats van
   de groep te verwijderen.

.. _handleiding_beheerders_index_overzicht:

Autorisatieoverzicht
--------------------

Met de knop **Autorisatieoverzicht downloaden** op het overzicht van de gebruikersgroepen
download je een CSV-bestand met alle gebruikersgroepen. Per gebruikersgroep staan daarin:

* de naam en omschrijving;
* de toegevoegde gebruikers;
* de organisaties, informatiecategorieën en onderwerpen waarvoor de groep geautoriseerd is.

Het bestand gebruikt een puntkomma als scheidingsteken en kan direct in Excel of LibreOffice
Calc geopend worden. Gebruik het bijvoorbeeld om periodiek te controleren of de juiste
mensen de juiste rechten hebben.

Inrichten: een stappenplan
--------------------------

Bij de ingebruikname van de GPP-App kun je de volgende volgorde aanhouden:

1. Zorg dat in de publicatiebank de juiste organisaties zijn geactiveerd en eventuele eigen
   informatiecategorieën en onderwerpen zijn toegevoegd.
2. Bepaal welke teams of afdelingen gaan publiceren, en namens welke organisaties en binnen
   welke informatiecategorieën en onderwerpen elk team mag publiceren.
3. Maak per team een gebruikersgroep aan en vink de bijbehorende waardelijsten aan.
4. Vink ``Geautoriseerd voor Inzage-procedure`` aan bij de teams die ontwerpbesluiten en
   vergelijkbare stukken met een inzagetermijn publiceren.
5. Voeg de gebruikers toe aan de juiste gebruikersgroepen. Een gebruiker mag lid zijn van
   meerdere gebruikersgroepen; hij of zij kiest dan per publicatie het profiel.
6. Laat de gebruikers inloggen en controleer of hun naam verschijnt bij de toegevoegde
   gebruikers.

Wanneer een medewerker vertrekt
-------------------------------

Verwijder de medewerker uit de gebruikersgroepen. Zijn of haar publicaties blijven bestaan.
Een collega uit dezelfde gebruikersgroep kan deze publicaties via "Publicaties van collega's"
:ref:`claimen <handleiding_gebruikers_index_claimen>` en daarna beheren. Laat ook de
toegang in de Identity Provider intrekken.

Problemen oplossen
------------------

Een gebruiker ziet de melding "Er is iets misgegaan bij het ophalen van de gegevens. Neem contact op met de beheerder."
   Controleer of de gebruiker lid is van minstens één gebruikersgroep, en of de inlognaam
   exact overeenkomt met de identificatie uit de Identity Provider.

Een gebruiker ziet bij een nieuwe publicatie "Onder dit profiel heb je niet (meer) de juiste rechten ..."
   Controleer of de gekozen gebruikersgroep geautoriseerd is voor minstens één organisatie
   en één informatiecategorie.

Een gebruiker kan een bestaande publicatie niet meer aanpassen
   Controleer of de gebruikersgroep van de publicatie nog bestaat, of de gebruiker daar nog
   lid van is, en of de groep nog geautoriseerd is voor alle organisaties,
   informatiecategorieën en onderwerpen van de publicatie.

Een gebruiker ziet het onderdeel Inzage-procedure niet
   Vink bij de gebruikersgroep ``Geautoriseerd voor Inzage-procedure`` aan.

Bij een waardelijst verschijnt "Er is iets misgegaan bij het ophalen van de waardelijsten..."
   De GPP-App kan de waardelijsten niet ophalen uit de publicatiebank, of er zijn daar geen
   geactiveerde organisaties of informatiecategorieën. Controleer de koppeling met de
   publicatiebank (zie :ref:`installation_index`) en de waardelijsten in de publicatiebank.

Een gebruiker ziet het menu-item Gebruikersgroepen niet (of krijgt "Geen toegang")
   De gebruiker heeft geen beheerdersrol in de Identity Provider, of moet na het toekennen
   van de rol opnieuw inloggen.
