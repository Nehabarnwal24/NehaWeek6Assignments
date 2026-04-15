using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using System.Text;

namespace nehabatch2app
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // 🔐 Get credentials from environment variables
            string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // 🔹 Azure resources from env
            string vaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL");
            string keyName = Environment.GetEnvironmentVariable("AZURE_KEY_NAME");

            var keyClient = new KeyClient(new Uri(vaultUrl), credential);
            KeyVaultKey key = (await keyClient.GetKeyAsync(keyName)).Value;

            string originalText = "Sensitive order data for CloudXeus Technology Services";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(originalText);

            var cryptoClient = new CryptographyClient(key.Id, credential);

            EncryptResult encryptResult = await cryptoClient.EncryptAsync(
                EncryptionAlgorithm.RsaOaep,
                plaintextBytes);

            Console.WriteLine("Encrypted text (Base64):");
            Console.WriteLine(Convert.ToBase64String(encryptResult.Ciphertext));

            DecryptResult decryptResult = await cryptoClient.DecryptAsync(
                EncryptionAlgorithm.RsaOaep,
                encryptResult.Ciphertext);

            string decryptedText = Encoding.UTF8.GetString(decryptResult.Plaintext);

            Console.WriteLine("\nDecrypted text:");
            Console.WriteLine(decryptedText);

            Console.ReadLine();
        }
    }
}