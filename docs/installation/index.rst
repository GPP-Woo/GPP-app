.. _installation_index:

Installatie
============

De GPP-App kan in een Kubernetes-cluster geïnstalleerd worden m.b.v. `Helm <https://helm.sh/>`_. In een aparte repostitory houden de ontwikkelaars `de helm charts bij <https://github.com/GPP-Woo/charts>`_. Daar zijn ook verdere installatie-instructies te vinden.

Omgevingsvariabelen
--------------------

Bij de installatie heeft de GPP-App verschillende omgevingsvariabelen (“environment variables”) nodig, om goed te functioneren. Deze staan opgesomd en uitgelegd bij de `Github-repository van de GPP-App <https://github.com/GPP-Woo/GPP-APP?tab=readme-ov-file#omgevingsvariabelen>`_. Het gaat hier om variabelen t.b.v.: 
- de koppeling met een  OpenID Connect Identity Provider
- de koppeling met de GPP-Publicatiebank

Configuratie voor de OpenID Connect Identity Provider
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
GPP-app ondersteunt Single Sign On (SSO) met behulp van OpenID Connect identiteitsproviders. Dit betekent dat je je bestaande organisatieaccount kunt gebruiken als je gecentraliseerd accountbeheer hebt, zoals bijvoorbeeld Azure AD. Het gebruik van OIDC stelt je meestal ook in staat om gebruikers automatisch de juiste rollen mee te geven op basis van hun rollen die beheerd worden in je Identiteits- en Toegangsbeheer (IAM) oplossing. Hiervoor moeten er specifieke zaken ingeregeld worden in de eigen Identity Provider van de gemeente. Daarnaast moeten er speciefieke Omgevingsvariabelen worden aangeleverd voor de installatie van de GPP-app. In de genoemde `lijst met omgevingsvariabelen <https://github.com/GPP-Woo/GPP-APP?tab=readme-ov-file#omgevingsvariabelen>`_ gaan alle variabelen die beginnen met ``OIDC_`` over de inrichting van SSO.

Allereerst moeten de IAM-beheerders in uw organisatie een set referenties in uw omgeving creëren voor GPP-App. Gewoonlijk vereist dit dat ze een Cliënt of een App in een of ander beheersportaal creëren. Deze app moet de callback-eindpunt op een toestemmingslijst plaatsen. Dit wordt meestal de Redirect URI genoemd. Dan moeten de beheerders een aantal gegevens inrichten waarmee GPP-app verbinding kan leggen met uw OpenID Connect identiteitsprovider. Die gegevens moeten worden overgenomen in de Omgevingsvariabelen. 

Alle gebruikers die gekoppeld zijn aan de hierboven genoemde Client of App, hebben toegang tot GPP-App. Voor gebruikers die ook beheeractiviteiten moeten uitvoeren, is het van belang dat er een specifieke rol meekomt uit de Identity Provider. De naam van deze rol, bijvoorbeeld ``app-admin``, moet ook opgenomen worden in de Omgevingsvariabele ``OIDC_ADMIN_ROLE``. Daarnaast moet worden vastgelegd wat de naam is van de claim waarin deze rol wordt meegegegven.