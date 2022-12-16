using System;

namespace ANPRTechOps.RingGoIntegration.Web.Models
{
    public class SessionDto
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }
        
        public string Plate { get; set; }

        public string Make { get; set; }

        public string Colour { get; set; }

        public int? Type { get; set; }

        public string Reference { get; set; }

        public int Zone { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int PaymentPence { get; set; }

        public int? PermitType { get; set; }

        public bool DisabledBadge { get; set; }

        public bool OnWhiteList { get; set; }

        public string Notes { get; set; }

        public string Bay { get; set; }

        public string CountryCode { get; set; }

        public int OperatorId { get; set; }

        public bool IsProcessed { get; set; }

        public bool IsDeleted { get; set; }

        public SessionModel AsSessionModel()
        {
            return new SessionModel()
            {
                RingGoReference = Reference,
                Vehicle = new SessionModel.VehicleDto()
                {
                    Plate = Plate,
                    Make = Make,
                    Colour = Colour,
                    Type = Type
                },
                Parking = new SessionModel.ParkingDto()
                {
                    Zone = Zone,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    PaymentPence = PaymentPence,
                    PermitType = PermitType,
                    Bay = Bay,
                    DisabledBadge = DisabledBadge,
                    OnWhiteList = OnWhiteList,
                    Notes = Notes
                },
                Location = new SessionModel.LocationDto()
                {
                    CountryCode = CountryCode,
                    OperatorId = OperatorId
                }
            };
        }
    }
}