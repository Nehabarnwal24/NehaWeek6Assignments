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
            string tenantId = "189fb143-6ba1-4c35-bc5c-029e2e66f97e";
            string clientId = "1ee9d01f-f9f9-4c55-b2c7-de026f75cdec";
            string clientSecret = "5bN8Q~N8bGKxe1~RT4GSWeecDTuO2PHG8uV6Vc~e";


            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            string vaultUrl = "https://nehakeyvault1.vault.azure.net/";
            string keyName = "keyvaultnb";


            var keyClient = new KeyClient(new Uri(vaultUrl), credential);
            KeyVaultKey key;

            key = await keyClient.GetKeyAsync(keyName);

            string originalText = "Sensitive order data for CloudXeus Technology Services";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(originalText);

            var cryptoClient = new CryptographyClient(key.Id, credential);

            EncryptResult encryptResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep,
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
