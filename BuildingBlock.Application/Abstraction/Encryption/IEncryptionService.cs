namespace BuildingBlock.Application.Abstraction.Encryption
{
    public interface IEncryptionService
    {
        /// <summary>تشفير نص خام وإرجاع تمثيل آمن للنقل/التخزين (Base64).</summary>
        string Encrypt(string plaintext);

        /// <summary>فك تشفير النص المشفَّر وإرجاع النص الخام.</summary>
        string Decrypt(string ciphertextBase64);
    }
}