using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//namespace AgencyPricesWS
//{
    public class HotelPrice
    {
        //Supplier details
        public string mSupplierId { get; set; }
        public string mArea { get; set; }

        //Hotel Price details
        private List<PricePerDay> mMonthlyPricesPerDay;
        public List<MonthlyAllocation> mMonthlyAllocations { get; set; }
        public float mCommission { get; set; }
        public float mSiteDiscount { get; set; }
        public float mOfficeDiscount { get; set; }
        public float mGeneralAdditive { get; set; }
        public float mBabiesAdditive { get; set; }
        public string mCurrency { get; set; }
        public string mPriceType { get; set; }
        public string mPriceColor { get; set; }
        public bool mStatus { get; set; }
        public bool mIsBaseAmount { get; set; }

        public Dictionary<Composition, bool> mCompositions { get; set; }
        public Dictionary<Base, bool> mBases { get; set; }
        public Dictionary<RoomType, bool> mRoomTypes { get; set; }
        public Composition mBaseComposition { get; set; }
        public Base mBaseBase { get; set; }
        public RoomType mBaseRoomType { get; set; }

        public HotelPrice()
        {
            mMonthlyPricesPerDay = new List<PricePerDay>();
            mMonthlyAllocations = new List<MonthlyAllocation>();
            mCompositions = new Dictionary<Composition, bool>();
            mBases = new Dictionary<Base, bool>();
            mRoomTypes = new Dictionary<RoomType, bool>();
        }

        public List<PricePerDay> getPricePerDayList()
        {
            return mMonthlyPricesPerDay;
        }

        public void savePricePerDay(PricePerDay iDatePriceDetails)
        {
            if (iDatePriceDetails != null)
            {
                foreach (PricePerDay dateDetails in mMonthlyPricesPerDay)
                {
                    if (dateDetails.mDate.Date == iDatePriceDetails.mDate.Date)
                    {
                        removePricePerDay(dateDetails);
                        break;
                    }
                }

                mMonthlyPricesPerDay.Add(iDatePriceDetails);
            }
        }

        private void removePricePerDay(PricePerDay iDatePriceDetails)
        {
            mMonthlyPricesPerDay.Remove(iDatePriceDetails);
        }

        public void setAllComboListsFalseStatus()
        {
            foreach (Composition composition in mCompositions.Keys.ToList())
            {
                mCompositions[composition] = false;
            }

            foreach (Base baseItem in mBases.Keys.ToList())
            {
                mBases[baseItem] = false;
            }

            foreach (RoomType roomType in mRoomTypes.Keys.ToList())
            {
                mRoomTypes[roomType] = false;
            }
        }

        public Composition getCompositionById(string iId)
        {
            Composition composition = null;

            composition = mCompositions.Where(x => x.Key.getId() == iId).FirstOrDefault().Key;

            return composition;
        }

        public Base getBaseById(string iId)
        {
            Base baseItem = null;

            baseItem = mBases.Where(x => x.Key.getId() == iId).FirstOrDefault().Key;

            return baseItem;
        }

        public RoomType getRoomTypeById(string iId)
        {
            RoomType roomType = null;

            roomType = mRoomTypes.Where(x => x.Key.getId() == iId).FirstOrDefault().Key;

            return roomType;
        }
    }
//}