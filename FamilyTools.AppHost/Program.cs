using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Cible de publication : en mode publish (`aspire publish`), Aspire genere un docker-compose.yaml
// (+ .env) decrivant toute la topologie (SQL/Redis/RabbitMQ/Keycloak + APIs + web) pour un
// deploiement sur n'importe quel hote Docker. Inerte en run local (F5/dotnet run) : ne s'active
// que pendant la publication, n'impacte donc pas le dev quotidien.
if (builder.ExecutionContext.IsPublishMode)
{
    // Nom distinct des ressources : "FamilyTools" est deja pris par le serveur SQL
    // (AddSqlServer plus bas) et les noms sont insensibles a la casse -> collision sinon.
    builder.AddDockerComposeEnvironment("familytools-compose");
}

var cache = builder.AddRedis("cache");

// Fournisseur d'identite centralise (SSO transversal a toutes les apps FamilyTools).
// Port fige (8080) pour que les URLs de redirection OIDC soient stables cote SPA/navigateur.
// Volume nomme : le realm, les comptes et la config survivent a la recreation du conteneur.
// Le realm "familytools" (politique de mot de passe, roles, clients, admin de bootstrap)
// est importe depuis le dossier Realms/ au premier demarrage.
// Secret du client service-account (API Accounts -> API Admin Keycloak), source de verite UNIQUE.
// Parametre Aspire (secret) resolu depuis la config : `Parameters:account-svc-secret`.
//   - Dev : valeur dans les user-secrets de l'AppHost (hors source, comme les mdp SQL/Keycloak).
//   - Prod : surcharge via l'environnement (Parameters__account-svc-secret), fourni par la CI.
// Injecte a la fois dans l'API Accounts (Keycloak__ServiceClientSecret) et dans le conteneur
// Keycloak (FAMILYTOOLS_ACCOUNT_SVC_SECRET), ou il alimente la substitution ${...} du realm importe.
var accountSvcSecret = builder.AddParameter("account-svc-secret", secret: true);

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("familytools-keycloak-data")
    // Aspire 13.4 active par defaut la terminaison TLS (dev cert) sur les conteneurs Keycloak,
    // ce qui bascule l'endpoint primaire de http:8080 vers https:8443. On la desactive pour
    // garder Keycloak en HTTP simple sur http://localhost:8080 : c'est l'URL stable attendue
    // par le SPA (auth.config.ts), le proxy Angular et les redirectUris du realm.
    .WithoutHttpsCertificate()
    // Secret du client service-account, lu par la substitution ${FAMILYTOOLS_ACCOUNT_SVC_SECRET}
    // du realm importe (un seul secret partage avec l'API Accounts).
    .WithEnvironment("FAMILYTOOLS_ACCOUNT_SVC_SECRET", accountSvcSecret)
    // Theme de connexion aux couleurs FamilyTools (pages login/OTP/reset).
    .WithBindMount("./Themes/familytools", "/opt/keycloak/themes/familytools")
    .WithRealmImport("./Realms");

var messaging = builder.AddRabbitMQ("messaging")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

var sql = builder.AddSqlServer("FamilyTools", port: 14329)
    .WithEndpoint(name: "sqlEndpoint", targetPort: 14330)
    .WithLifetime(ContainerLifetime.Persistent)
    // Volume Docker nommé : les données survivent à la recreation du conteneur
    // (sinon elles vivent dans la couche du conteneur et sont perdues a chaque recreation).
    .WithDataVolume("familytools-sql-data")
    .AddDatabase("easycompta");

var migration = builder.AddProject<FamilyTools_MigrationService>("migrations")
    .WithReference(sql)
    .WaitFor(sql);

var easyComptaAPI = builder
    .AddProject<FamilyTools_EasyCompta>("EasyComptaAPI")
    .WithReference(sql)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(messaging)
    .WaitFor(messaging)
    .WithReference(migration)
    .WaitForCompletion(migration)
    // L'API valide les jetons JWT emis par Keycloak (resolution de l'autorite via service discovery).
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// API transversale "Mon compte" : self-service (profil, mot de passe, 2FA) adosse a Keycloak.
// Partagee par toutes les applications du portail, independante d'EasyCompta.
var accountsApi = builder
    .AddProject<FamilyTools_Accounts>("accounts")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__ServiceClientSecret", accountSvcSecret)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.AddProject<FamilyTools_CsvWorker>("csv-worker")
    .WithReference(sql)
    .WithReference(messaging)
    .WaitFor(messaging)
    .WaitForCompletion(migration);

builder.AddNpmApp("webfrontend", "../FamilyTools.Web")
    .WithReference(easyComptaAPI)
    .WaitFor(easyComptaAPI)
    .WithReference(accountsApi)
    .WaitFor(accountsApi)
    .WaitFor(keycloak)
    // Port fige (4200) : doit correspondre aux redirectUris du client Keycloak "familytools-web".
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.Build().Run();