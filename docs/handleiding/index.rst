.. _handleiding_index:

Handleiding
============

De Wet open overheid (Woo) verplicht overheden om documenten uit 17 informatiecategorieën actief openbaar te maken. Ook buiten deze 17 informatiecategorieën mogen documenten actief openbaar gemaakt worden (inspanningsverplichting). Voor een correct werkende aansluiting op de landelijke Generieke Woo-Voorziening (GWV, `open.overheid.nl <https://open.overheid.nl/>`_) moet een eindgebruiker bij iedere publicatie aangeven binnen welke informatiecategorie deze valt.

In de GPP-App kunnen gebruikers documenten openbaar maken. Dit doen zij door publicaties aan te maken, en deze koppelen aan de juiste organisaties en informatiecategorieën. Hiermee worden ze vindbaar op `open.overheid.nl <https://open.overheid.nl/>`_, en in een eigen website die deze publicaties ontsluit, zoals het GPP-Burgerportaal.

.. note:: De xml-sitemap waarmee de publicaties uit GPP-Woo worden toegevoegd aan de
   Generieke Woo-Voorziening, is onderdeel van een ander GPP-Woo-component, het
   `GPP-Burgerportaal <https://github.com/GPP-woo/GPP-burgerportaal>`_. De GPP-App zelf
   publiceert dus niet rechtstreeks op open.overheid.nl.

In de GPP-App gaan twee groepen gebruikers aan de slag:

1. **Beheerders** geven gebruikers via gebruikersgroepen toegang tot de juiste organisaties,
   informatiecategorieën en onderwerpen. Zie :ref:`handleiding_beheerders_index`.
2. **Gebruikers** voeren publicaties in en voegen daar één of meer documenten aan toe. Zie
   :ref:`handleiding_gebruikers_index`.

Een beheerder is meestal ook gebruiker: om zelf publicaties aan te maken, moet ook een
beheerder lid zijn van minstens één gebruikersgroep.

.. toctree::
   :maxdepth: 1

   beheerders/index
   gebruikers/index

.. _handleiding_index_begrippen:

Begrippen
---------

Publicatie
   Een verzameling gegevens (metadata) over iets dat openbaar gemaakt wordt, bijvoorbeeld een
   besluit of een convenant. Een publicatie heeft doorgaans één of meer documenten.

Document
   Een bestand (bijvoorbeeld een PDF) met eigen gegevens, dat altijd bij één publicatie
   hoort.

Gebruikersgroep en profiel
   Een gebruikersgroep bepaalt namens welke organisaties en binnen welke
   informatiecategorieën en onderwerpen de leden publicaties mogen maken. In de
   publicatieschermen heet de gebruikersgroep waarmee je werkt je **profiel**. Een
   publicatie hoort altijd bij één gebruikersgroep.

Waardelijsten
   De lijsten met organisaties, informatiecategorieën en onderwerpen. Deze worden beheerd in
   de gekoppelde `GPP-Publicatiebank <https://gpp-publicatiebank.readthedocs.io/>`_, niet in
   de GPP-App.

Publicatie-eigenaar
   De medewerker die een publicatie mag wijzigen. Dat is standaard degene die de publicatie
   heeft aangemaakt. Een collega uit dezelfde gebruikersgroep kan de publicatie
   :ref:`claimen <handleiding_gebruikers_index_claimen>` en wordt dan de nieuwe eigenaar.

Inzageprocedure
   Bij sommige publicaties, zoals een ontwerpbesluit, mogen burgers gedurende een
   inzagetermijn reageren met een zienswijze of bezwaar. De inzageprocedure legt vast hoe en
   tot wanneer dat kan. Zie :ref:`handleiding_gebruikers_index_inzageprocedure`.

.. _handleiding_index_statussen:

Statussen van een publicatie
----------------------------

Een publicatie heeft altijd één van de volgende statussen:

``concept``
   De publicatie is opgeslagen, maar staat niet online. Alleen de titel is verplicht; de
   overige velden kun je later invullen. Een concept kan nog volledig verwijderd worden.

``gepubliceerd``
   De publicatie en de bijbehorende documenten staan online en zijn vindbaar voor burgers.
   Je kunt de publicatie nog aanpassen of intrekken.

``ingetrokken``
   De publicatie is offline gehaald, samen met al haar documenten. Een ingetrokken
   publicatie kun je alleen nog raadplegen; intrekken kan niet ongedaan gemaakt worden.

.. code-block:: none

   concept ──(Publiceren)──> gepubliceerd ──(Publicatie intrekken)──> ingetrokken
      │
      └──(Publicatie verwijderen)──> verwijderd
