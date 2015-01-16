namespace Dot42.CryptoUI
{
    /// <summary>
    /// Settings needed by the <see cref="CertificateBuilder"/>.
    /// </summary>
    public interface ICertificateSettings
    {
        string OrgName { get; }
        string OrgUnit { get; }
        string City { get; }
        string CountryCode { get; }
        string State { get; }
        string Email { get; }
        string UserName { get; }
        string Password { get; }
        string Path { get; }
        bool ImportInCertificateStore { get; }
        int MaxYears { get; }
    }
}
