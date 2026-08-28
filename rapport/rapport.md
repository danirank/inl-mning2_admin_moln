# Rapport uppgift 2 

## Kod

- Blazor-projektet innehåller metoder för **CRUD-operationer** för uppdrag och konsulter.
- Frontend kommunicerar med backend-API:t via `HttpClient`.
- API:t använder en **InMemory-databas**, vilket räcker för projektets syfte utan en separat databasresurs.
- Roller och behörigheter används för att styra vilka användare som får utföra olika operationer.

## Deployment

- Frontend och API är deployade som **Azure App Services**.
- Deployment sker automatiskt med **CI/CD via GitHub Actions**.
- Vid push till `main` byggs och publiceras projekten till Azure.
- GitHub Actions gör deploymenten repeterbar och minskar behovet av manuella publiceringar.
- Azure-resurserna är kopplade till projektets nätverks- och säkerhetslösning med VNet, subnets, NSG och Entra ID.


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

Lösningen följer principen deny by default: endast nödvändig trafik tillåts, vilket minskar risker för attacker och skyddar både applikationen och interna resurser.


![Alt-text](/rapport/skurkab.png)


## Entra  


# Varför Microsoft Entra och Easy auth?

Genom Entra Id och Easy Auth säkerställer vi att all som kommer in får vara där och alla som är där kan bara göra saker som vi tillåter dom att göra. Väldigt skyddart och säkert

## Skapa de fyra Entra Id användare

För att skapa upp användarna går vi in i terminalen och

```powershell
az ad user create `
  --display-name "Praktikant" `
  --user-principal-name "praktikant@IThogskolan.onmicrosoft.com" `
  --password "Hitta-På-Ett-temporärt-Lösenord"

az ad user create `
  --display-name "Mellanchef" `
  --user-principal-name "mellanchef@IThogskolan.onmicrosoft.com" `
  --password "Hitta-På-Ett-temporärt-Lösenord"

az ad user create `
  --display-name "Konsultchef" `
  --user-principal-name "konsultchef@IThogskolan.onmicrosoft.com" `
  --password "Hitta-På-Ett-temporärt-Lösenord"

az ad user create `
  --display-name "Admin" `
  --user-principal-name "admin@IThogskolan.onmicrosoft.com" `
  --password "Hitta-På-Ett-temporärt-Lösenord"
```

# RBAC

När alla användare är skapade behöver vi ge dom RBAC så att dom får tillgång till vår app. Vi ger dom rollen ”Reader” för att dom inte ska kunna ändra/skapa/ta bort något, bara läsa.

Först hämtar vi vårat subscribtion id för att sätta rätt scope sen.

```powershell
az group show `
  --name resource-group-here `
  --query id `
  -o tsv
```

Sedan hämtar du och ger varje användares id

```powershell
az ad user show `
  --id "praktikant@IThogskolan.onmicrosoft.com" `
  --query id `
  -o tsv
```

Och ger dom ”Reader” rollen en i taget

```powershell
az role assignment create `
  --assignee "a1b2c3d4-...." `
  --role "Reader" `
  --scope "/subscriptions/xxxx-xxxx/resourceGroups/MyResourceGroup"
```


App registrering & app roller
Nu skapar vi en application som representerar vårt api och sedan skapar upp roller specefika för applicationen. Den här delen är faktiskt enklare att göra i portalen istället för terminalen.
Gå in i Microsoft Entra Id, sedan App registrations, sedan New registration. Här skapar du upp en app registration som då representerar api:t som en app i Entra Id. När den är skapad så går du tillbaka till app registrations och hittar din app, går in i den, går till App roles. Nu skapar du de fyra olika rollerna och ger de tillgång till de enda sakerna som behövs.  Här är ett exempel:
Praktikant:	Name + phone	Read
Mellanchef:	Name + address	Read + update
Konsultchef:	Everything	Read + create + update + assign
Admin:	Everything	Everything, including delete
När alla roller är skapade går man till Enterprise applications, sedan appen, sedan users and groups. Nu ger vi de olika användarna deras roll.
När man nu loggar in via Entra Id så får användaren en token som bland annat innehåller vilken roll användaren har och därifrån vet applikationen vad den ha access till.

Easy Auth
Här näst går vi in i App servicen, sedan Authentication, sedan Add Identity Provider, sedan Microsoft och konfigurera för Microsoft Entra Id. Vi sätter också Unauthenticated requests till 401.
Nu är vi färdiga. Alla behöver gå genom entra för att få tillgång till api och genom entra får vi rollerna som dikterar vem som får göra vad.

## Reflektion

I vårt projekt så har vi somsagt data-subnet, den finns och kan ta emot intern trafik men i dagsläget innehåller ingen resurs. Våran databas är InMemory och körs i samma process som API:t, snarare än som en egen Azure resurs. I en riktig miljö hade det istället varit en riktig databas, som tex Azure SQL Database eller CosmosDB som skulle ligga inuti data-subnet och nådd via en Private Endpoint. Då hade databasen fått en privat IP i VNet:et och aldrig exponerats med en publik anslutnings string, oavsett vad NSG reglerna tillåter på papper.

Ett andra lager rör hemligheter. Med en InMemory databas finns inga lösenord eller anslutnings string att skydda, men i det scenariot ovan, om det nu vore en riktig miljö med en riktig databas, så hade databasens anslutnings string annars troligen legat i klartext i App Service konfiguration. Men genom att aktivera Managed Identity på API:ts App Service och ge den en begränsad roll (text Key Vault Secret User) på en Key Vault, så hade appen kunnat hämta hemligheten utan att den någonsin lagras i kod eller konfigurationen.

Utöver det så saknas det i dagsläget även en brandvägg på applikationsnivå (text Web Application Firewall via Application Gateway) som filtrerar skadliga requests innan de når App Service, samt central loggning och avvikelserdetektering (Azure Monitor / Microsoft Defender för Cloud) som hade kunnat fånga in onormala anropsmönster, något som NSG och RBAC i sig inte larmar om. RBAC rollerna som vi satte upp är dessutom statiska. I en riktig organisation hade de behövt granskas löpande i takt med att personer byter roller eller slutar, så att de inte blir för generösa över tid.



### Praktikant får 403 vid delete 
![Alt-text](/rapport/praktikant_403.png)



### Praktikant kan inte se adress
![Alt-text](/rapport/praktikant_adress.png)


### Mellanchef kan inte se telefon
![Alt-text](/rapport/mellanchef.png)