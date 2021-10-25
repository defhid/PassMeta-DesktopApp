namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface ICryptoService
    {
        string Encrypt(string data, string keyPhrase);
        
        string Decrypt(string data, string keyPhrase);

        string MakeCheckKey(string keyPhrase);

        string GeneratePassword(int length, bool includeDigits, bool includeSpecial);
    }
}