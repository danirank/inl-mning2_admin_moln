# Rapport uppgift 2 

## Kod  

- Metoder i Blazor projekt. --> CRUD för Uppdrag & Konsulter 

## Deployment 

- CI/CD med github actions
- 


## Nätverkssäkerhet i Azure

Skurk AB-portalen är deployad till Azure App Service. För att skapa en tydlig och säker nätverksstruktur skapades även ett Virtual Network (VNet) i Azure.

### Skapa VNet och subnets

I Azure Portal skapades först ett nytt Virtual Network. I detta nätverk skapades två separata subnets:

app-subnet – för applikationsrelaterad trafik.
data-subnet – för intern kommunikation mot exempelvis databas eller lagring.

Genom att dela upp resurser i olika subnets går det att separera applikationsdelen från datalagret och styra trafiken mellan dem.

### Skapa och koppla NSG

Därefter skapades en Network Security Group (NSG). NSG kopplades till app-subnet, vilket innebär att dess regler används för den nätverkstrafik som passerar subnetet.

Följande inbound-regler skapades:

- Port 443 – Allow från Internet
- Tillåter HTTPS-trafik. HTTPS använder TLS-kryptering och skyddar information mellan användaren och applikationen.
- Port 80 – Deny från Internet
- Blockerar vanlig HTTP eftersom trafiken annars kan skickas okrypterad.
- All annan inkommande trafik – Deny
- Azure har standardregeln DenyAllInBound, vilket gör att trafik som inte uttryckligen tillåts blockeras.
- Intern VNet-trafik – Allow
- Standardregeln AllowVnetInBound tillåter kommunikation mellan resurser och subnets inom samma VNet.
- Koppling till App Service

App Service kan kopplas till app-subnet med VNet Integration. Detta gör att applikationen kan kommunicera med resurser i det virtuella nätverket, exempelvis resurser i data-subnet.

Det är viktigt att känna till att VNet Integration främst styr utgående trafik från App Service. App Service ligger inte direkt i subnetet på samma sätt som en virtuell maskin. Därför aktiveras även HTTPS Only på App Service för att säkerställa att portalen endast används över HTTPS.

Lösningen följer principen deny by default: endast nödvändig trafik tillåts, vilket minskar attackytan och skyddar både applikationen och interna resurser.


![Alt-text](/rapport/skurkab.png)


## Rollmotivering  


## 

