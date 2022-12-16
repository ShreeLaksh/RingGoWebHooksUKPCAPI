namespace ANPRTechOps.RingGoIntegration.Web.Models
{
    public class RingGoSettings
    {
        public string? PublicKey { get; set; }

        public string? PrivateKey { get; set; }

        public bool ValidateBearerHmac { get; set; }

        public int VendorId { get; set; }
    }
}