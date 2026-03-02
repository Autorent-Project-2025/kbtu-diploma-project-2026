using System.Security.Cryptography;

namespace IdentityService.Infrastructure.Security;

public static class RsaKeyMaterial
{
    public static string NormalizePem(string pem)
    {
        if (string.IsNullOrWhiteSpace(pem))
        {
            throw new InvalidOperationException("RSA PEM value cannot be empty.");
        }

        return pem.Replace("\\n", "\n").Trim();
    }

    public static string DerivePublicKeyPem(string privateKeyPem)
    {
        var normalizedPrivateKey = NormalizePem(privateKeyPem);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(normalizedPrivateKey);
        return rsa.ExportSubjectPublicKeyInfoPem();
    }

    public static RSAParameters ReadPublicKeyParameters(string publicKeyPem)
    {
        var normalizedPublicKey = NormalizePem(publicKeyPem);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(normalizedPublicKey);
        return rsa.ExportParameters(false);
    }
}
