using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;

public static async Task Run(TimerInfo myTimer, ILogger log)
{
    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    log.LogInformation(Environment.GetEnvironmentVariable("MSI_ENDPOINT"));
    log.LogInformation(Environment.GetEnvironmentVariable("MSI_SECRET").Substring(0,6) + "...");

    try
    {
        AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App;AppId=<<ClientID>>");
        var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        var secret = await keyVaultClient.GetSecretAsync("https://keyvaultname.vault.azure.net/secrets/secret")
            .ConfigureAwait(false);

        log.LogInformation($"{secret.Value}");
        
    }
    catch(Exception ex)
    {
        log.LogInformation($"Exception thrown : {ex}");
    }
}
