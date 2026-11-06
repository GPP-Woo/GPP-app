.. _handleiding_gebruikers_index:

Handleiding voor gebruikers
===========================

Deze handleiding beschrijft hoe je in de GPP-App publicaties en documenten aanmaakt,
publiceert, wijzigt en intrekt. Lees voor de gebruikte begrippen en statussen eerst
:ref:`handleiding_index_begrippen`.

.. contents:: Op deze pagina
   :local:
   :depth: 2

.. _handleiding_gebruikers_index_toegang:

Toegang tot de GPP-App
----------------------

Voordat je kunt gaan werken in de GPP-App, moeten er twee dingen gebeuren:

1. Je moet als gebruiker worden geautoriseerd voor de GPP-App, in de Identity Provider van
   je organisatie. Dit moet gebeuren door de beheerders van Identity en Access Management
   (IAM).
2. Je moet door de beheerder van de GPP-App aan minstens één
   :ref:`gebruikersgroep <handleiding_beheerders_index_gebruikersgroepen>` worden
   toegevoegd. Die gebruikersgroep bepaalt voor welke organisaties, informatiecategorieën en
   onderwerpen je publicaties mag maken.

.. warning:: Een gebruiker moet lid zijn van minstens één gebruikersgroep! En iedere
   gebruikersgroep moet geautoriseerd zijn voor minstens één organisatie en minstens één
   informatiecategorie! Ben je geen lid van een gebruikersgroep, dan zie je bij het
   aanmaken van een publicatie de melding "Er is iets misgegaan bij het ophalen van de
   gegevens. Neem contact op met de beheerder." Is je gebruikersgroep niet geautoriseerd
   voor een organisatie en een informatiecategorie, dan zie je na het kiezen van het profiel
   de melding "Onder dit profiel heb je niet (meer) de juiste rechten om deze publicatie aan
   te passen."

Inloggen en uitloggen
^^^^^^^^^^^^^^^^^^^^^

Ga naar het webadres van de GPP-App van je organisatie. Ben je nog niet ingelogd, dan zie
je de melding "Niet ingelogd" en een knop **Inloggen**. Na het klikken word je doorgestuurd
naar de inlogpagina van je organisatie (Single Sign On). Ben je daar al ingelogd, dan kom je
direct in de GPP-App.

Je blijft ingelogd zolang je actief bent. Werk je ongeveer een uur niet in de GPP-App, dan
moet je opnieuw inloggen. Sla daarom een publicatie waar je lang mee bezig bent tussentijds
op als concept.

Met **Uitloggen** in de menubalk log je uit.

Menubalk
^^^^^^^^

Nadat je bent ingelogd, zie je bovenaan in de menubalk de volgende items:

* :ref:`handleiding_gebruikers_index_mijnpublicaties`
* :ref:`handleiding_gebruikers_index_publicatiesvancollegas`
* :ref:`Gebruikersgroepen <handleiding_beheerders_index>` (alleen zichtbaar voor beheerders)
* Uitloggen

.. _handleiding_gebruikers_index_mijnpublicaties:

Mijn publicaties
----------------

Onder het menu-item "Mijn publicaties" zie je een lijst met de publicaties waarvan jij, de
ingelogde gebruiker, de :ref:`eigenaar <handleiding_index_begrippen>` bent. Dat zijn de
publicaties die je zelf hebt aangemaakt of hebt geclaimd. Dit is ook het scherm dat je
direct na het inloggen ziet.

Bovenaan zie je een knop **Nieuwe publicatie**. Daarmee maak je een nieuwe publicatie aan,
zie :ref:`handleiding_gebruikers_index_nieuw`.

.. _handleiding_gebruikers_index_zoeken:

Zoeken, filteren en sorteren
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Boven de lijst staan velden om publicaties te zoeken en te filteren:

* ``Titel``: zoek op (een deel van) de titel. Klik daarna op **Zoek**.
* ``Datum van`` en ``Datum tot en met``: toon alleen publicaties met een registratiedatum
  in deze periode.
* ``Filter op informatiecategorie``, ``Filter op onderwerp`` en
  ``Filter op publicatiestatus``: toon alleen publicaties met de gekozen
  informatiecategorie, het gekozen onderwerp of de gekozen status. Je kunt hier kiezen uit
  de informatiecategorieën en onderwerpen van je eigen gebruikersgroepen.

Boven de resultaten kun je met ``Sorteer op`` de volgorde kiezen: op titel, verkorte
titel of registratiedatum, oplopend of aflopend. Standaard staan de nieuwste publicaties
bovenaan. Per pagina worden 10 publicaties getoond; met de knoppen naast de sortering blader
je naar de vorige of volgende pagina.

De lijst
^^^^^^^^

Elke publicatie in de lijst toont de titel, de status en de registratiedatum:

* Een publicatie in **concept** is gemarkeerd met "Waarschuwing: deze publicatie is nog in
  concept!". Vergeet niet deze later te publiceren.
* Bij een **ingetrokken** publicatie is de titel doorgestreept.
* Een **potloodje** geeft aan dat jij de eigenaar bent en de publicatie dus kunt wijzigen.

Klik op een publicatie om deze te openen, zie
:ref:`handleiding_gebruikers_index_wijzigen`.

.. _handleiding_gebruikers_index_nieuw:

Een nieuwe publicatie aanmaken
------------------------------

Klik in "Mijn publicaties" op de knop **Nieuwe publicatie**.

Profiel kiezen
^^^^^^^^^^^^^^

Als eerste kies je bij ``Profiel`` met welke gebruikersgroep je de publicatie aanmaakt.
Maak je bijvoorbeeld een publicatie aan als "Team Juridisch" of als "Team Vergunningen"? Ben
je lid van slechts één gebruikersgroep, dan wordt het profiel automatisch voor je gekozen.

Het profiel bepaalt uit welke organisaties, informatiecategorieën en onderwerpen je kunt
kiezen. Ook kunnen alle leden van die gebruikersgroep de publicatie zien onder
:ref:`handleiding_gebruikers_index_publicatiesvancollegas`.

.. tip:: Kan je gebruikersgroep maar voor één organisatie of één informatiecategorie
   publiceren, dan wordt die bij een nieuwe publicatie automatisch aangevinkt.

Gegevens van de publicatie
^^^^^^^^^^^^^^^^^^^^^^^^^^

Zodra je profiel is ingevuld, verschijnen links alle velden van de publicatie. Rechts zie je
een vak om documenten te uploaden, zie :ref:`handleiding_gebruikers_index_documenten`.

Verplichte velden zijn gemarkeerd met een sterretje (*). Achter sommige velden staat een
vraagteken; klik daarop voor een toelichting.

- ``Titel`` \*: Geef hier de officiële, formele titel van de publicatie.
- **Meer details** (klik om open te klappen):

  - ``Verkorte titel``: Geef hier eventueel een verkorte titel c.q. citeertitel aan de
    publicatie.
  - ``Omschrijving``: Geef hier een korte omschrijving of samenvatting van de publicatie.
    Publiceer je een publicatie zonder documenten, bijvoorbeeld om te melden dat een
    document niet openbaar gemaakt wordt, dan neem je die mededeling hier op.
  - ``Datum in werking``: Indien van toepassing, de datum waarop de rechten en plichten uit
    de documenten in werking treden. Bijvoorbeeld de datum waarop een besluit in werking
    treedt, een convenant ingaat of een beleidsplan start.
  - ``Datum buiten werking``: Indien van toepassing en bekend, de datum waarop die rechten
    en plichten vervallen. Bijvoorbeeld de vervaldatum van een vergunning of de einddatum
    van een convenant. Deze datum mag niet vóór de ``Datum in werking`` liggen.
  - ``Kenmerk toevoegen``: Geef hier een of meerdere kenmerken op waarmee de relevante zaak
    of het relevante dossier intern gevonden kan worden, bijv. het zaak- of dossiernummer.

- **Organisatie** \*: Kies namens welke organisatie je publiceert.
- **Informatiecategorieën** \*: Vink één of meer informatiecategorieën aan waar deze
  publicatie onder valt. Met "selecteer alles" vink je alle opties in één keer aan. Achter
  een informatiecategorie staat soms een vraagteken met een toelichting.
- **Onderwerpen**: Indien van toepassing, vink één of meer onderwerpen aan waar deze
  publicatie over gaat. Onderwerpen bundelen publicaties over een maatschappelijk relevant
  thema, bijvoorbeeld een groot project.
- **Inzage-procedure**: alleen zichtbaar als je gebruikersgroep daarvoor geautoriseerd is,
  zie :ref:`handleiding_gebruikers_index_inzageprocedure`.

.. note:: De informatiecategorieën bepalen ook hoe lang de publicatie bewaard wordt. Zie
   :ref:`handleiding_gebruikers_index_archivering`.

.. _handleiding_gebruikers_index_documenten:

Documenten toevoegen
^^^^^^^^^^^^^^^^^^^^

Rechts zie je een vak om documenten te uploaden. Klik hierop om bestanden te selecteren, of
sleep bestanden vanaf je computer of netwerkschijf naar dit vak (drag-and-drop). Je kunt
meerdere bestanden tegelijk toevoegen.

De volgende bestandsformaten worden geaccepteerd: PDF, Word (DOC, DOCX), Excel (XLS, XLSX),
PowerPoint (PPT, PPTX, PPS, PPSX), OpenDocument (ODT, ODS, ODP, ODF), RTF, TXT, CSV, HTML,
ZIP en 7z. Bij een ander formaat of een leeg bestand verschijnt de melding "Onbekend formaat
of leeg bestand." en wordt het bestand niet toegevoegd.

.. tip:: Bij het publiceren verwijdert de gekoppelde publicatiebank bij veel bestandstypen
   automatisch de verborgen metadata uit het bestand, zoals de naam van de auteur. Controleer
   desondanks altijd zelf of een document geen informatie bevat die niet openbaar mag worden,
   zoals persoonsgegevens in de tekst.

Na het toevoegen verschijnen de documenten onder het kopje "Nieuwe documenten". Per document
kun je de volgende velden invullen:

- ``Datum document`` \*: De datum van het document. Die staat vaak in het document zelf
  vermeld. Standaard wordt hier de datum van vandaag ingevuld. Een datum in de toekomst is
  niet toegestaan.
- ``Titel`` \*: De officiële, formele titel van het document. Deze wordt vooraf ingevuld met
  de bestandsnaam (zonder extensie), maar je kunt hier ook een andere titel invullen.
- ``Verkorte titel``: Eventueel een verkorte titel c.q. citeertitel van het document.
- ``Omschrijving``: Een korte omschrijving of samenvatting van het document.
- ``Datum ontvangst``: Indien van toepassing, wanneer het document is ontvangen.
- ``Datum ondertekening (intern)``: Indien van toepassing, wanneer het document intern is
  ondertekend.
- ``Kenmerk toevoegen``: Een of meerdere kenmerken waarmee het document intern gevonden kan
  worden, bijv. het interne of externe documentnummer.

Heb je per ongeluk een verkeerd bestand toegevoegd? Klik dan bij dat document op
**Verwijderen**. Zolang de publicatie nog niet is opgeslagen, wordt het document dan alleen
uit de lijst gehaald.

**Dubbele documenten**

Voeg je een document toe met dezelfde bestandsnaam of dezelfde titel als een ander document
in de publicatie, dan verschijnt de melding "Er zijn dubbele documenten gedetecteerd." en
staat bij het document "mogelijk dubbel". Controleer dan of het document niet al in de
publicatie zit. Je mag de publicatie wel opslaan; de melding is een waarschuwing.

Opslaan en publiceren
^^^^^^^^^^^^^^^^^^^^^

Onderaan het scherm staan de volgende knoppen:

- **Publiceren**: De publicatie en de documenten worden opgeslagen en direct online
  gepubliceerd. Alle verplichte velden moeten ingevuld zijn. Heeft de publicatie geen
  documenten, dan wordt eerst gevraagd of je zeker weet dat je wilt publiceren.
- **Opslaan als concept**: De publicatie wordt opgeslagen, maar nog niet online
  gepubliceerd. Je kunt de publicatie later aanvullen en alsnog publiceren. Voor een concept
  zijn alleen het profiel en de ``Titel`` verplicht.
- **Annuleren**: De publicatie wordt niet opgeslagen en je keert terug naar de lijst.

Na het opslaan verschijnt een bevestiging en keer je terug naar de lijst met publicaties.

**Uploadvoortgang**

Voeg je documenten toe, dan zie je na het opslaan hoe ver het uploaden is ("2 van 5
documenten geüpload"). Sluit het scherm niet en blijf op de pagina tot de upload is
afgerond, vooral bij grote bestanden.

Mislukt het uploaden van een of meer documenten, dan verschijnt de melding "Niet alle
documenten zijn opgeslagen. Controleer de publicatie en voeg ontbrekende documenten opnieuw
toe." De publicatie zelf is dan wel opgeslagen. Een document waarvan de upload mislukt is,
herken je aan een uitroepteken in een driehoek. Voeg zo'n document opnieuw toe en verwijder
of trek het mislukte document in.

.. _handleiding_gebruikers_index_inzageprocedure:

Inzageprocedure
---------------

Bij sommige publicaties, zoals een ontwerpbesluit, kunnen burgers gedurende een
inzagetermijn reageren met een zienswijze of een bezwaar. Dit leg je vast in een
inzageprocedure. Een publicatie heeft hooguit één inzageprocedure.

.. note:: Het onderdeel **Inzage-procedure** zie je alleen als je gebruikersgroep daarvoor
   door de beheerder is geautoriseerd, of als de publicatie al een inzageprocedure heeft.

Klap het onderdeel **Inzage-procedure** open en klik op **Inzage-procedure koppelen**. Vul
daarna de volgende velden in:

- ``Begindatum inzagetermijn`` \*: De eerste dag waarop burgers kunnen reageren. Standaard
  is dit vandaag. Deze datum mag niet vóór de publicatiedatum liggen.
- ``Einddatum inzagetermijn`` \*: De laatste dag waarop burgers kunnen reageren. Deze moet na
  de begindatum liggen. Valt de einddatum in een weekend of op een feestdag, dan schuift de
  publicatiebank deze bij het opslaan automatisch op naar de eerstvolgende werkdag.
- ``Beschikbaar rechtsmiddel`` \*: Kies of burgers een **Zienswijze** of een **Bezwaar** kunnen
  indienen.
- ``URL reactieformulier``: De link naar het formulier waarmee burgers kunnen reageren. Dit
  veld vul je niet zelf in: na het opslaan wordt het standaardformulier voor het gekozen
  rechtsmiddel ingevuld, zoals ingesteld in de publicatiebank.
- ``Toelichting`` \*: Een toelichting voor burgers, bijvoorbeeld wat er ter inzage ligt en
  hoe ze kunnen reageren.
- ``URL bekendmaking``: Indien van toepassing, de link naar de officiële bekendmaking,
  bijvoorbeeld op officielebekendmakingen.nl.
- ``Automatisch intrekken na afloop inzagetermijn``: Vink dit aan als de publicatie na
  afloop van de inzagetermijn offline gehaald moet worden.

.. warning:: Met **Automatisch intrekken na afloop inzagetermijn** wordt na afloop van de
   inzagetermijn de hele publicatie inclusief documenten ingetrokken. Dat kan niet ongedaan
   gemaakt worden!

Heb je een nieuwe inzageprocedure per ongeluk gekoppeld, dan haal je deze weg met
**Inzage-procedure verwijderen**. Bij een opgeslagen inzageprocedure vink je
**Inzage-procedure verwijderen** aan en sla je de publicatie op. Dit kan niet ongedaan
gemaakt worden.

De inzageprocedure wordt opgeslagen samen met de publicatie, dus met **Publiceren** of
**Opslaan als concept**.

.. _handleiding_gebruikers_index_wijzigen:

Een bestaande publicatie wijzigen of intrekken
----------------------------------------------

Klik in de lijst op een publicatie om deze te openen. Wat je kunt doen, hangt af van de
status van de publicatie en of je de eigenaar bent:

* Een publicatie in **concept** of met de status **gepubliceerd** kun je als eigenaar
  aanpassen.
* Een **ingetrokken** publicatie kun je alleen nog raadplegen.
* Ben je niet de eigenaar, dan kun je de publicatie alleen raadplegen of eerst
  :ref:`claimen <handleiding_gebruikers_index_claimen>`.

Is de publicatie gepubliceerd, dan staat rechtsboven een knop **Bekijk online**. Daarmee
open je de publicatie in het burgerportaal, zoals burgers hem ook zien.

Op een bestaande publicatie zie je dezelfde velden als bij het aanmaken. Daarnaast zie je:

- ``ID``: Een automatisch gegenereerd uniek identificatienummer.
- ``Publicatie-eigenaar``: De medewerker die nu de eigenaar is van de publicatie en hem kan
  bewerken.
- **Archivering**: zie :ref:`handleiding_gebruikers_index_archivering`.

Wissel je bij een bestaande publicatie van ``Profiel`` naar een gebruikersgroep die niet
dezelfde organisatie en informatiecategorieën mag gebruiken, dan worden de organisatie, de
informatiecategorieën en de onderwerpen leeggemaakt. Kies ze dan opnieuw.

Documenten van een bestaande publicatie
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Onder "Toegevoegde documenten" staan de documenten die al bij de publicatie horen. Klik op
een document om de gegevens open te klappen. Met het downloadicoon download je het bestand.
Je kunt ook nieuwe documenten toevoegen, zoals hierboven beschreven.

Per document staat er een optie om het document te verwijderen of in te trekken:

- **Document verwijderen** (bij een document in concept): het document wordt bij het opslaan
  volledig verwijderd.
- **Document intrekken** (bij een gepubliceerd document): alleen dat document wordt bij het
  opslaan offline gehaald. Zo hoef je niet de hele publicatie in te trekken. Een ingetrokken
  document blijft zichtbaar met een doorgestreepte titel.

Beide acties kunnen niet ongedaan gemaakt worden.

Knoppen
^^^^^^^

Onderaan staan de knoppen waarmee je kunt opslaan. Welke knoppen je ziet, hangt af van de
status van de publicatie:

- Als de publicatie nog **in concept** is:

  - **Publiceren**: De wijzigingen worden opgeslagen en de hele publicatie wordt direct
    online gepubliceerd, inclusief de documenten.
  - **Opslaan als concept**: De wijzigingen worden opgeslagen, maar de publicatie wordt nog
    niet online gepubliceerd.
  - **Publicatie verwijderen**: De hele publicatie wordt inclusief documenten permanent
    verwijderd.
  - **Annuleren**: De wijzigingen worden niet opgeslagen.

- Als de publicatie **gepubliceerd** is:

  - **Publiceren**: De wijzigingen worden opgeslagen en direct online gezet.
  - **Publicatie intrekken**: De hele publicatie wordt inclusief documenten offline gehaald.
  - **Annuleren**: De wijzigingen worden niet opgeslagen.

- Als de publicatie is **ingetrokken**, kun je alleen nog klikken op **Annuleren**.

Een gepubliceerde publicatie kan niet terug naar concept. Bij verwijderen en intrekken wordt
altijd eerst om een bevestiging gevraagd.

.. warning:: Het verwijderen van een concept-publicatie kan niet teruggedraaid worden! In de
   gekoppelde publicatiebank wordt dit wel gelogd.

.. warning:: Het intrekken van een document of publicatie kan niet teruggedraaid worden! Wel
   kan de publicatie opnieuw aangemaakt worden en/of de documenten opnieuw geüpload worden.

.. _handleiding_gebruikers_index_archivering:

Archivering
^^^^^^^^^^^

Bij een publicatie die niet meer in concept is, zie je het onderdeel **Archivering**. Deze
gegevens worden door de gekoppelde publicatiebank automatisch ingevuld op basis van de
informatiecategorieën, op het moment van publiceren. Je kunt ze in de GPP-App niet wijzigen.

- ``Bron bewaartermijn``: de selectielijst of het selectiebesluit waarop de bewaartermijn is
  gebaseerd.
- ``Categorie in bron``: de categorie binnen die selectielijst.
- ``Archief-actie``: of de publicatie na afloop van de bewaartermijn vernietigd of
  overgebracht wordt naar een archiefbewaarplaats.
- ``Archief-actie gepland op``: de datum waarop die actie moet plaatsvinden.
- ``Toelichting``: een eventuele toelichting op de bewaartermijn.

Heeft een publicatie meerdere informatiecategorieën, dan geldt de langste bewaartermijn.
Vragen over deze gegevens kun je stellen aan de informatiebeheerder van je organisatie.

.. _handleiding_gebruikers_index_publicatiesvancollegas:

Publicaties van collega's
-------------------------

Onder het menu-item "Publicaties van collega's" zie je de publicaties van een gebruikersgroep
waar je lid van bent, ook als een collega de eigenaar is.

Bovenaan kies je bij ``Gebruikersgroep`` van welke gebruikersgroep je de publicaties wilt
zien. Ben je lid van slechts één gebruikersgroep, dan wordt deze automatisch voor je gekozen.
Je kunt hier altijd wisselen naar een andere gebruikersgroep waar je lid van bent.

Daaronder kun je de lijst op dezelfde manier
:ref:`zoeken, filteren en sorteren <handleiding_gebruikers_index_zoeken>` als bij "Mijn
publicaties".

.. _handleiding_gebruikers_index_claimen:

Een publicatie van een collega claimen
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Klik op een publicatie om deze te openen, en kijk naar het veld ``Publicatie-eigenaar``:

- Staat daar je eigen naam? Dan kun je de publicatie direct gaan bewerken.
- Staat daar de naam van een collega? Dan kun je de publicatie alleen raadplegen. Wil je hem
  bewerken, dan moet je de publicatie eerst claimen.

Om een publicatie te claimen, klik je rechtsonder op de knop **Publicatie claimen** en
bevestig je dit. Jouw naam wordt dan ingevuld bij de ``Publicatie-eigenaar``. Je kunt de
publicatie vervolgens direct bewerken of intrekken. De publicatie staat vanaf dan onder jouw
"Mijn publicaties" en niet meer onder die van je collega.

Een ingetrokken publicatie kun je niet claimen.

.. warning:: Je collega krijgt géén signaal dat de publicatie door jou geclaimd is! Breng
   je collega daar dus zelf van op de hoogte als dat nodig is.

.. _handleiding_gebruikers_index_extern:

Publicaties uit andere systemen
-------------------------------

Publicaties kunnen ook door een ander systeem, zoals een zaaksysteem, in de publicatiebank
klaargezet worden. Is jouw inlognaam daarbij als eigenaar meegegeven, dan zie je zo'n
publicatie onder "Mijn publicaties", meestal als concept. Zo'n publicatie heeft nog geen
profiel. Open de publicatie, kies je ``Profiel``, vul de ontbrekende gegevens aan en
publiceer de publicatie.

.. _handleiding_gebruikers_index_meldingen:

Veelvoorkomende meldingen
-------------------------

"Er is iets misgegaan bij het ophalen van de gegevens. Neem contact op met de beheerder."
   Je bent (nog) geen lid van een gebruikersgroep, of de gegevens konden (tijdelijk) niet
   opgehaald worden. Neem contact op met de beheerder van de GPP-App.

"Onder dit profiel heb je niet (meer) de juiste rechten om deze publicatie aan te passen."
   Je gebruikersgroep is niet geautoriseerd voor minstens één organisatie en één
   informatiecategorie, of de publicatie gebruikt een organisatie, informatiecategorie of
   onderwerp waarvoor je gebruikersgroep (inmiddels) niet geautoriseerd is, of je bent geen
   lid meer van de gebruikersgroep van de publicatie. Je kunt de publicatie alleen
   raadplegen. Neem contact op met de beheerder.

"Er is iets misgegaan bij het ophalen van de publicatie of de publicatie is niet (meer) beschikbaar..."
   De publicatie is verwijderd, of de verbinding met de publicatiebank is (tijdelijk)
   verstoord. Probeer het later opnieuw; blijft de melding staan, neem dan contact op met de
   beheerder.

"Onbekend formaat of leeg bestand."
   Het bestand heeft een formaat dat niet wordt geaccepteerd, of is leeg. Zie
   :ref:`handleiding_gebruikers_index_documenten` voor de geaccepteerde formaten.

"De begindatum van de inzage-procedure ligt vóór de publicatiedatum."
   Pas de ``Begindatum inzagetermijn`` aan naar vandaag of later en sla de publicatie
   opnieuw op.

Je kunt een publicatie niet aanpassen en ziet geen knoppen om op te slaan
   Je bent niet de eigenaar van de publicatie (claim hem eerst), of de publicatie is
   ingetrokken.
