using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ANPRTechOps.RingGoIntegration.Web.Helpers;

namespace ANPRTechOps.RingGoIntegration.Web.Models
{
    public class SessionModel
    {
        [Required]
        [JsonPropertyName("ringgoRef")]
        public string? RingGoReference { get; set; }

        [Required]
        [JsonPropertyName("vehicle")]
        public VehicleDto? Vehicle { get; set; }

        [Required]
        [JsonPropertyName("parking")]
        public ParkingDto? Parking { get; set; }

        [JsonPropertyName("location")]
        public LocationDto? Location { get; set; }

        public class VehicleDto
        {
            [Required]
            [JsonPropertyName("vrm")]
            public string? Plate { get; set; }

            [JsonPropertyName("make")]
            public string? Make { get; set; }

            [JsonPropertyName("colour")]
            public string? Colour { get; set; }

            [JsonPropertyName("type")]
            public int? Type { get; set; }
        }

        public class ParkingDto
        {
            [Required]
            [JsonPropertyName("zone")]
            [JsonConverter(typeof(IntToStringConverter))]
            public int Zone { get; set; }

            [Required]
            [JsonPropertyName("startTime")]
            public DateTime StartTime { get; set; }

            [Required]
            [JsonPropertyName("endTime")]
            public DateTime EndTime { get; set; }

            [Required]
            [JsonPropertyName("paymentPence")]
            public int PaymentPence { get; set; }

            [JsonPropertyName("permitType")]
            public int? PermitType { get; set; }

            [JsonPropertyName("disabledBadge")]
            public bool DisabledBadge { get; set; }

            [JsonPropertyName("onWhiteList")]
            public bool OnWhiteList { get; set; }

            [JsonPropertyName("notes")]
            public string? Notes { get; set; }

            [JsonIgnore]
            [JsonPropertyName("bay")]
            public string? Bay { get; set; }
        }

        public class LocationDto
        {
            [JsonPropertyName("countryCode")]
            public string? CountryCode { get; set; }

            [JsonPropertyName("operatorId")]
            public int OperatorId { get; set; }
        }
    }
}