using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Infrastracture.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace BuildingBlock.Infrastracture.Service
{
    public sealed class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key; // 256-bit

        public EncryptionService(IOptions<EncryptionOptions> options)
        {
            if (options?.Value?.KeyBase64 is null)
                throw new ArgumentNullException(nameof(options));

            _key = Convert.FromBase64String(options.Value.KeyBase64);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption key must be 256-bit (32 bytes).");
        }

        public string Encrypt(string plaintext)
        {
            if (plaintext is null) throw new ArgumentNullException(nameof(plaintext));

            // AES-GCM يتطلب nonce (12 bytes) + tag (16 bytes)
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using var aes = new AesGcm(_key);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // نجمع: [nonce | tag | ciphertext] ثم Base64 للسهولة
            byte[] payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, payload, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(payload);
        }

        public string Decrypt(string ciphertextBase64)
        {
            if (string.IsNullOrWhiteSpace(ciphertextBase64))
                throw new ArgumentException("Ciphertext cannot be empty.", nameof(ciphertextBase64));

            byte[] payload = Convert.FromBase64String(ciphertextBase64);

            // نستخرج [nonce | tag | ciphertext]
            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] ciphertext = new byte[payload.Length - nonce.Length - tag.Length];

            Buffer.BlockCopy(payload, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(payload, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(payload, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

            byte[] plaintextBytes = new byte[ciphertext.Length];
            using var aes = new AesGcm(_key);
            aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}