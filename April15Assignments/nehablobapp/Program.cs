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
            string tenantId = "";
            string clientId = "";
            string clientSecret = "";

            // 🔹 Azure Resources (YOURS)
            string vaultUrl = "";
            string keyName = "";

            string storageUrl = "";
            string containerName = "data";

            // 🔹 File paths (FIX THIS PATH)
            string inputImagePath = @"";
            string outputImagePath = @"";

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