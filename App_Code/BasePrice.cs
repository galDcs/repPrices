using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//namespace AgencyPricesWS
//{
    public class BasePrice
    {
        public BasePrice()
        {
            mComposition = new Composition();
            mBase = new Base();
            mRoomType = new RoomType();
        }

        public Composition mComposition { get; set; }
        public Base mBase { get; set; }
        public RoomType mRoomType { get; set; }
        public decimal mPrice { get; set; }
        public decimal mPriceNetto { get; set; }
    }
//}