using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//namespace AgencyPricesWS
//{
    public class PricePerDay
    {
        public PricePerDay()
        {
            mBasePrice = new BasePrice();
            mAllocation = new Allocation();
            mSalesCycles = new List<int>();
        }

        public DateTime mDate { get; set; }
        public BasePrice mBasePrice { get; set; }
        public bool mStatus { get; set; }
        public Allocation mAllocation { get; set; }
        public List<int> mSalesCycles { get; set; }
        public string mColor { get; set; }
        public string CloseDateReason { get; set; }

        public List<Combo> AvailableCombos { get; set; }

        public decimal getFinalPriceForCombo(string iSupplierId, Composition iComposition, Base iBase, RoomType iRoomType, bool iIsAmount, float iOfficeDiscount, float iSiteDiscount, float iGeneralAdditive, float iBabiesAdditive)
        {
            decimal price = 0;
            decimal priceForComposition = 0;

            if ((iComposition != null && iBase != null && iRoomType != null) && (AvailableCombos.Find(x => x.mBase.mId == iBase.mId && x.mComposition.mId == iComposition.mId && x.mRoomType.mId == iRoomType.mId) != null))
            {
                //All additive are added to the price according to the composition.
                priceForComposition = mBasePrice.mPrice * (decimal)iComposition.mPercent / 100;

                if (iIsAmount)
                {
                    price = priceForComposition * (decimal)((100 - (iOfficeDiscount + iSiteDiscount)) / 100) + iBase.mAmount + iRoomType.mAmount + (decimal)iGeneralAdditive + (decimal)iBabiesAdditive;
                }
                else
                {
                    price = priceForComposition * (decimal)((100 - (iOfficeDiscount + iSiteDiscount)) / 100 * iBase.mPercent / 100) + iRoomType.mAmount + (decimal)iGeneralAdditive + (decimal)iBabiesAdditive;
                }
            }
            else
            {
            //TODO: GAL!!!!! detect why no result
                throw new Exception("Combo Cannot be null");
            }

            return Math.Round(price, 2);
        }
		
		public decimal getFinalPriceNettoForCombo(string iSupplierId, Composition iComposition, Base iBase, RoomType iRoomType, bool iIsAmount, float iOfficeDiscount, float iSiteDiscount, float iGeneralAdditive, float iBabiesAdditive)
        {
            decimal priceNetto = 0;
            decimal priceForComposition = 0;

            if ((iComposition != null && iBase != null && iRoomType != null) && (AvailableCombos.Find(x => x.mBase.mId == iBase.mId && x.mComposition.mId == iComposition.mId && x.mRoomType.mId == iRoomType.mId) != null))
            {
            //All additive are added to the price according to the composition.
                decimal tempPrecent = (float)iComposition.mPercentNetto == -1 ? (decimal)iComposition.mPercent : (decimal)iComposition.mPercentNetto;
                priceForComposition = (decimal) (mBasePrice.mPriceNetto * tempPrecent / 100);

                if (iIsAmount)
                {
                    priceNetto = priceForComposition * (decimal)((100 - (iOfficeDiscount + iSiteDiscount)) / 100) + iBase.mAmount + iRoomType.mAmount + (decimal)iGeneralAdditive + (decimal)iBabiesAdditive;
                }
                else
                {
                    priceNetto = priceForComposition * (decimal)((100 - (iOfficeDiscount + iSiteDiscount)) / 100 * iBase.mPercent / 100) + iRoomType.mAmount + (decimal)iGeneralAdditive + (decimal)iBabiesAdditive;
                }
            }
            else
            {
                throw new Exception("Combo Cannot be null");
            }

            return Math.Round(priceNetto, 2);
        }
    }
//}