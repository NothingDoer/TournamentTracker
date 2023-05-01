﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents what the prize is for the given place.
    /// </summary>
    public class PrizeModel
    {
        /// <summary>
        /// The unique identifier for the prize.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The numeric identifier for the place.
        /// </summary>
        public int PlaceNumber { get; set; }

        /// <summary>
        /// The friendly name for the place.
        /// </summary>
        public string PlaceName { get; set; }

        /// <summary>
        /// The fixed amount this place earns or zero if it is not used.
        /// </summary>
        public decimal PrizeAmount { get; set; }

        /// <summary>
        /// The number that represents the percentage of the overall take or
        /// zero if it is not used. The procentage is a fraction of 1
        /// (so 0.5 fo 50%).
        /// </summary>
        public double PrizeProcentage { get; set; }

        public PrizeModel()
        {

        }

        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            int placeNumberValue = 0;
            decimal prizeAmountValue = 0;
            double prizeProcentageValue = 0;

            PlaceName = placeName;

            int.TryParse(placeNumber, out placeNumberValue);
            PlaceNumber = placeNumberValue;

            decimal.TryParse(prizeAmount, out prizeAmountValue);
            PrizeAmount = prizeAmountValue;

            double.TryParse(prizePercentage, out prizeProcentageValue);
            PrizeProcentage = prizeProcentageValue;
        }
    }
}
