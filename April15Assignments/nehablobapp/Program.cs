using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Storage.Blobs;

namespace ImageEncryptDecrypt
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 🔐 Get credentials from environment variables
            string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // 🔹 Azure resources
            string vaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL");
            string keyName = Environment.GetEnvironmentVariable("AZURE_KEY_NAME");
            string storageUrl = Environment.GetEnvironmentVariable("AZURE_STORAGE_URL");

            string containerName = "data";

            string inputImagePath = @"C:\Users\YOUR_PATH\input.png";
            string outputImagePath = @"C:\Users\YOUR_PATH\output.jpg";

            string encryptedBlobName = "image.enc";
            string encryptedKeyBlobName = "key.enc";
            string ivBlobName = "iv.bin";

            var keyClient = new KeyClient(new Uri(vaultUrl), credential);
            KeyVaultKey key = (await keyClient.GetKeyAsync(keyName)).Value;

            var cryptoClient = new CryptographyClient(key.Id, credential);

            byte[] imageBytes = File.ReadAllBytes(inputImagePath);

            using Aes aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            byte[] encryptedImage;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(imageBytes, 0, imageBytes.Length);
                cs.Close();
                encryptedImage = ms.ToArray();
            }

            EncryptResult encryptedKey = await cryptoClient.EncryptAsync(
                EncryptionAlgorithm.RsaOaep,
                aes.Key);

            var blobServiceClient = new BlobServiceClient(new Uri(storageUrl), credential);
            var container = blobServiceClient.GetBlobContainerClient(containerName);

            await container.GetBlobClient(encryptedBlobName)
                .UploadAsync(new MemoryStream(encryptedImage), overwrite: true);

            await container.GetBlobClient(encryptedKeyBlobName)
                .UploadAsync(new MemoryStream(encryptedKey.Ciphertext), overwrite: true);

            await container.GetBlobClient(ivBlobName)
                .UploadAsync(new MemoryStream(aes.IV), overwrite: true);

            Console.WriteLine("✅ Encrypted and uploaded.");

            byte[] downloadedImage = (await container.GetBlobClient(encryptedBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            byte[] downloadedKey = (await container.GetBlobClient(encryptedKeyBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            byte[] downloadedIV = (await container.GetBlobClient(ivBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            DecryptResult decryptedKey = await cryptoClient.DecryptAsync(
                EncryptionAlgorithm.RsaOaep,
                downloadedKey);

            using Aes aesDecrypt = Aes.Create();
            aesDecrypt.Key = decryptedKey.Plaintext;
            aesDecrypt.IV = downloadedIV;

            byte[] decryptedImage;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aesDecrypt.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(downloadedImage, 0, downloadedImage.Length);
                cs.Close();
                decryptedImage = ms.ToArray();
            }

            File.WriteAllBytes(outputImagePath, decryptedImage);

            Console.WriteLine("🔓 Decrypted image saved.");
        }
    }
}