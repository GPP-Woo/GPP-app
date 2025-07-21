.. _handleiding_beheerders_index:

Handleiding voor beheerders
===========================

Beheerders kunnen de GPP-App gebruikersgroepen aanmaken, en daar gebruikers aan toevogen. 
Een gebruikersgroep is gekoppeld aan specfieke organisaties en informatiecategorieën. Gebruikers binnen de gebruikersgroep kunnen alleen publicaties aanmaken voor die organisatie, en binnen die informatiecategorieën. 

Hoe krijgt een gebruiker beheerders-rechten
--------------------------------------------
Om een gebruiker beheerders-rechten te geven, moet deze een specifieke rol krijgen in de OpenID Connect Identity Provider (bijv. Azure AD). De naam van deze rol moet zijn afgestemd met de beheerders van de Identity Provider, en bij installatie van de App zijn inrgeregeld. Neem hiervoor contact op met de beheerders van de Identity Provider.


Gebruikersgroepen
-------------------------
Als je bent ingelogd op de GPP-App en je hebt beheerders-rechten, dan zie je in de menubalk óók een knop 'Gebruikersgroepen'. Achter deze knop vind je de mogelijkheid om gebruikersgroepen aan te maken, en de mogelijkheid om bestaande gebruikersgroepen te beheren. 

De aanwezige gebruikersgroepen staan hier op alfabetische volgorde. Let op: de naam van een gebruikersgroep kan best lang zijn. Houd er rekening mee dat één hele lange naam de hoogte van de rij beïnvloedt. 

Met de knop Nieuwe gebruikersgroep open je het scherm om een gebruikersgroep aan te maken of te beheren. Dit scherm bestaat uit twee delen.

Gebruikersgroep gegevens
^^^^^^^^^^^^^^^^^^^^^^^^^^

* **Naam**: De naam van een gebruikersgroep is verplicht. 
* **Omschrijving**: Hier kun je een omschrijving van de gebruikersgroep meegeven. 
* **Gebruiker toevoegen**: in dit veld vul je de identificatie van de nieuwe gebruiker in. 
* **Toegevoegde gebruikers**: hier zie je de identificaties van de gebruikers die aan deze groep zijn toegevoegd. 

Gebruikers worden toegeveogd aan een gebruikersgroep op basis van hun identificatie in de Identity Provider. Vaak is dit het e-mailadres. Stem dit af met de beheerders van de Identity Provider




