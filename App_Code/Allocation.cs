using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//namespace AgencyPricesWS
//{
    public class Allocation
    {
        public int mRoomsAmount { get; set; }
        public int mRoomsInUse { get; set; }
        public int mRoomsDisable { get; set; }

        public Allocation()
        {
            mRoomsAmount = 0; 
            mRoomsInUse = 0;
            mRoomsDisable = 0;
        }

        public Allocation(Allocation iOtherAllocation)
        {
            mRoomsAmount = iOtherAllocation.mRoomsAmount;
            mRoomsInUse = iOtherAllocation.mRoomsInUse;
            mRoomsDisable = iOtherAllocation.mRoomsDisable;
        }
    }
//}