using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Objects.Undocumented
{
    [ExcludeFromCodeCoverage]
    internal class SpecialPropertySettings
    {
        public string WindowsPassword { get; set; }

        public string LinuxPassword { get; set; }

        public string LinuxPrivateKey { get; set; }

        public string VMwarePassword { get; set; }

        public string SSHElevationPassword { get; set; }

        public string SNMPv3Password { get; set; }

        public string SNMPv3EncryptionKey { get; set; }

        public string DBPassword { get; set; }

        public string AmazonSecretKey { get; set; }

        public bool? InheritTriggers { get; set; }
    }
}
