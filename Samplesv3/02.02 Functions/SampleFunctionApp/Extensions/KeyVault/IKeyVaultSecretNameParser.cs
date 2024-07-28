using Azure.Security.KeyVault.Secrets;

namespace SampleFunctionApp;

public interface IKeyVaultSecretNameParser
{
    string Parse(KeyVaultSecret secret);
}
