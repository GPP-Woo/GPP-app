.. _handleiding_beheerders_index:

Handleiding voor beheerders
===========================

Mogelijk mag niet iedere gebruiker op álle informatiecategorieën en onderwerpen en namens álle (geactiveerde) organisaties publicaties aanmaken. In de GPP-app kunnen dan ook gebruikersgroepen aangemaakt worden en kunnen hier autorisaties aan toegekend worden. Een gebruikersgroep is gekoppeld aan specifieke organisaties, informatiecategorieën en onderwerpen. Gebruikers binnen de gebruikersgroep kunnen alleen publicaties aanmaken voor die organisatie, en binnen die informatiecategorieën. 

Alleen gebruikers met beheerders-rechten kunnen in de GPP-App gebruikersgroepen aanmaken, en daar gebruikers aan toevoegen. 


Hoe krijgt een gebruiker beheerders-rechten
--------------------------------------------
Om een gebruiker beheerders-rechten te geven, moet deze een specifieke rol krijgen in de OpenID Connect Identity Provider (bijv. Azure AD). De naam van deze rol moet zijn afgestemd met de beheerders van de Identity Provider, en bij installatie van de App zijn ingeregeld. Neem hiervoor contact op met de beheerders van de Identity Provider.


Gebruikersgroepen
-------------------------
Als je bent ingelogd op de GPP-App en je hebt beheerders-rechten, dan zie je in de menubalk óók een knop 'Gebruikersgroepen'. Achter deze knop vind je de mogelijkheid om gebruikersgroepen aan te maken, en de mogelijkheid om bestaande gebruikersgroepen te beheren. 

De aanwezige gebruikersgroepen staan hier op alfabetische volgorde. Let op: de naam van een gebruikersgroep kan best lang zijn. Houd er rekening mee dat één hele lange naam de hoogte van de rij beïnvloedt. 

Met de knop 'Nieuwe gebruikersgroep' open je het scherm om een gebruikersgroep aan te maken of te beheren. Dit scherm bestaat uit twee delen.

Gebruikersgroep gegevens
^^^^^^^^^^^^^^^^^^^^^^^^^^

* **Naam**: De naam van een gebruikersgroep is verplicht. 
* **Omschrijving**: Hier kun je een omschrijving van de gebruikersgroep meegeven. 
* **Gebruiker toevoegen**: Voeg een gebruiker toe aan deze gebruikersgroep, door in dit veld de identificatie van de nieuwe gebruiker in te vullen en op 'Toevoegen' te klikken. 
* **Toegevoegde gebruikers**: Hier zie je de identificaties van de gebruikers die aan deze groep zijn toegevoegd. Door op het kruisje achter de identificatie te klikken, verwijder je de gebruiker uit de gebruikersgroep.

Gebruikers worden toegevoegd aan een gebruikersgroep op basis van hun identificatie in de Identity Provider. Vaak is dit het e-mailadres. Stem dit af met de beheerders van de Identity Provider.

Waardelijsten
^^^^^^^^^^^^^^^

Bij Waardelijsten zie je voor welke Organisaties, Informatiecategorieën en Onderwerpen deze gebruikersgroep geautoriseerd is. Dit betekent dat de gebruikers uit deze gebruikersgroep Publicaties kunnen aanmaken voor die Organisaties, Informatiecategorieën en Onderwerpen. Door één van deze onderdelen open te vouwen, kun je zien welke items er binnen dat onderdeel zijn.

* Door een item aan te vinken, wordt de gebruikersgroep geautoriseerd voor dat item
* Door een item uit te vinken, wordt de autorisatie voor dat item voor de gebruikersgroep ingetrokken. 

  * Let op: houd er bij het intrekken van autorisaties rekening mee, dat gebruikers binnen deze gebruikersgroep mogelijk al wel publicaties hebben aangemaakt binnen die autorisatie. Na het intrekken van die autorisatie, krijgen deze gebruikers een foutmelding bij die publicaties. Hierin staat de instructie om contact op te nemen met de beheerder. 

Let op: het beheren van de waardes in de waardlijsten gebeurt in het registratiecomponent waaraan de GPP-App is gekoppeld. Bijvoorbeeld de `GPP-Publicatiebank <https://gpp-publicatiebank.readthedocs.io/en/latest/admin/index.html>`_. 

