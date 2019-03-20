# Azure-FunctionApp-MSI-UserAssignedMSI-Keyvault-Dotnet
Sample that shows how to fetch a secret from Azure Key Vault at run-time from an Function App with a User Assigned Managed Service Identity (MSI).

# Managed identities for Azure resources
Managed identities for Azure resources provide Azure services with an automatically managed identity in Azure Active Directory. Using a managed identity, you can authenticate to any service that supports Azure AD authentication without having credentials in your code. We are in the process of integrating managed identities for Azure resources and Azure AD authentication across Azure. 

#### How does the managed identities for Azure resources work?
##### There are two types of managed identities:
•	A **system-assigned managed identity** is enabled directly on an Azure service instance. When the identity is enabled, Azure creates an identity for the instance in the Azure AD tenant that's trusted by the subscription of the instance. After the identity is created, the credentials are provisioned onto the instance. The lifecycle of a system-assigned identity is directly tied to the Azure service instance that it's enabled on. If the instance is deleted, Azure automatically cleans up the credentials and the identity in Azure AD.

•	A **user-assigned managed identity** is created as a standalone Azure resource. Through a create process, Azure creates an identity in the Azure AD tenant that's trusted by the subscription in use. After the identity is created, the identity can be assigned to one or more Azure service instances. The lifecycle of a user-assigned identity is managed separately from the lifecycle of the Azure service instances to which it's assigned.
Your code can use a managed identity to request access tokens for services that support Azure AD authentication. Azure takes care of rolling the credentials that are used by the service instance.

##### Prerequisites
To run and deploy this sample, you need the following:
1.	App Service with MSI.
2.	Key Vault with a secret, and an access policy that grants the App Service access to Get Secrets.

##### Step 1: Create a user-assigned managed identity
To create a user-assigned managed identity, your account needs the [Managed Identity Contributor](https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#managed-identity-contributor) role assignment.
1.	Sign in to the [Azure portal](https://portal.azure.com) using an account associated with the Azure subscription to create the user-assigned managed identity.
2.	In the search box, type Managed Identities, and under Services, click Managed Identities.
3.	Click Add and enter values in the following fields under Create user assigned managed identity pane:
•	Resource Name: This is the name for your user-assigned managed identity, for example UAI1.
•	Subscription: Choose the subscription to create the user-assigned managed identity under
•	Resource Group: Create a new resource group to contain your user-assigned managed identity or choose Use existing to create the user-assigned managed identity in an existing resource group.
•	Location: Choose a location to deploy the user-assigned managed identity,for example West US.
4.	Click Create.

##### Step 2: Create an Function App with a User-Assigned Managed Service Identity (MSI) enabled
1.	Create an Function app in the portal as you normally would. Navigate to it in the portal.
2.	If using a function app, navigate to Platform features. 
3.	Select Managed identity.
4.	Within the User assigned (preview) tab, click Add.
5.	Search for the identity you created earlier and select it. Click Add.
6.  Select the user-assigned managed identity which we created on the step 1.

##### Step 3: Create & Grant User-Assigned MSI access to the Key Vault
1.  Create an Key Vault with a secret.
2.  Select "Overview", and click on Access policies.
3.	Click on "Add New", select "Secret Management" from the dropdown for "Configure from template"
4.	Click on "Select Principal", add the User-Assigned MSI which we created on step 1.
5.	Click on "OK" to add the new Access Policy, then click "Save" to save the Access Policies.

##### Step 4: Clone the repo
Clone the repo to your function app.

The project has two relevant Nuget packages:
1.  Microsoft.Azure.Services.AppAuthentication - makes it easy to fetch access tokens for Service-to-Azure-Service authentication scenarios.
2.  Microsoft.Azure.KeyVault - contains methods for interacting with Key Vault.

function.proj:
```json
<Project Sdk="Microsoft.NET.Sdk">  
    <PropertyGroup>  
        <TargetFramework>netstandard2.0</TargetFramework>  
    </PropertyGroup>  
    <ItemGroup>  
        <PackageReference Include="Microsoft.Azure.Services.AppAuthentication" Version="1.2.0-preview2" />  
		<PackageReference Include="Microsoft.Azure.KeyVault" Version="3.0.3" />
    </ItemGroup>  
</Project>  
```
The relevant code is in run.csx file. The AzureServiceTokenProvider class (which is part of Microsoft.Azure.Services.AppAuthentication) tries the following methods to get an access token:-
```dotnet
    try
    {
        AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App;AppId=<<ClientID>>");
        var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        var secret = await keyVaultClient.GetSecretAsync("https://keyvaultname.vault.azure.net/secrets/secret")
            .ConfigureAwait(false);

        log.LogInformation($"{secret.Value}");

    }
    catch (Exception ex)
    {
        log.LogInformation($"Exception thrown : {ex}");
    }
```
Note: You need to replace AppId with the value of the Client ID/Application ID you create in step #1.

##### Step 5: Run the Function

## Summary
The Function app was successfully able to get a secret at runtime from Azure Key Vault using your developer account during development, and using MSI when deployed to Azure. As a result, you did not have to explicitly handle a service principal credential to authenticate to Azure AD to get a token to call Key Vault.
