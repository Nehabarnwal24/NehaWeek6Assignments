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
            // 🔐 Azure Credentials (YOURS)
            string tenantId = "189fb143-6ba1-4c35-bc5c-029e2e66f97e";
            string clientId = "88270609-7695-4c50-baa2-060686e2642c";
            string clientSecret = "4OJ8Q~yAxtT5AewE7yZf-CuIUYkblBaiBJzmOc.p";

            // 🔹 Azure Resources (YOURS)
            string vaultUrl = "https://nehakeyvault1.vault.azure.net/";
            string keyName = "keyvaultnb";

            string storageUrl = "https://nehacgstorage.blob.core.windows.net/";
            string containerName = "data";

            // 🔹 File paths (FIX THIS PATH)
            string inputImagePath = @"C:\Users\91979\OneDrive\Pictures\cog.png";
            string outputImagePath = @"C:\Users\91979\OneDrive\Pictures\output.jpg";

            // 🔹 Blob names
            string encryptedBlobName = "image.enc";
            string encryptedKeyBlobName = "key.enc";
            string ivBlobName = "iv.bin";

            // 🔐 Authentication
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // =========================
            // 🔹 KEY VAULT SETUP
            // =========================
            var keyClient = new KeyClient(new Uri(vaultUrl), credential);
            KeyVaultKey key = (await keyClient.GetKeyAsync(keyName)).Value;

            var cryptoClient = new CryptographyClient(key.Id, credential);

            // =========================
            // 🔹 READ IMAGE
            // =========================
            byte[] imageBytes = File.ReadAllBytes(inputImagePath);

            // =========================
            // 🔹 AES ENCRYPTION
            // =========================
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

            // =========================
            // 🔹 ENCRYPT AES KEY (Key Vault)
            // =========================
            EncryptResult encryptedKey = await cryptoClient.EncryptAsync(
                EncryptionAlgorithm.RsaOaep,
                aes.Key);

            // =========================
            // 🔹 BLOB STORAGE SETUP
            // =========================
            var blobServiceClient = new BlobServiceClient(new Uri(storageUrl), credential);
            var container = blobServiceClient.GetBlobContainerClient(containerName);

            // =========================
            // 🔹 UPLOAD ALL FILES
            // =========================
            await container.GetBlobClient(encryptedBlobName)
                .UploadAsync(new MemoryStream(encryptedImage), overwrite: true);

            await container.GetBlobClient(encryptedKeyBlobName)
                .UploadAsync(new MemoryStream(encryptedKey.Ciphertext), overwrite: true);

            await container.GetBlobClient(ivBlobName)
                .UploadAsync(new MemoryStream(aes.IV), overwrite: true);

            Console.WriteLine("✅ Encrypted and uploaded.");

            // =========================
            // 🔹 DOWNLOAD FILES
            // =========================
            byte[] downloadedImage = (await container.GetBlobClient(encryptedBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            byte[] downloadedKey = (await container.GetBlobClient(encryptedKeyBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            byte[] downloadedIV = (await container.GetBlobClient(ivBlobName)
                .DownloadContentAsync()).Value.Content.ToArray();

            // =========================
            // 🔹 DECRYPT AES KEY
            // =========================
            DecryptResult decryptedKey = await cryptoClient.DecryptAsync(
                EncryptionAlgorithm.RsaOaep,
                downloadedKey);

            // =========================
            // 🔹 AES DECRYPTION
            // =========================
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