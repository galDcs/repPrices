using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

//namespace AgencyPricesWS
//{
    public class FinalPrice
    {
        public string mSupplierId { get; set; }        
        public Composition mComposition { get; set; }
        public Base mBase { get; set; }
        public RoomType mRoomType { get; set; }
        public decimal mFinalPrice { get; set; }
        public decimal mFinalPriceNetto { get; set; }
        public List<FinalPricePerDay> mFinalPricePerDay{ get; set; }
        public bool mHasError { get; set; }
        private string mErrorDescription;
		
		//new 
		public string mCurrency { get; set; }
		public float mCommission { get; set; }
		public float mOfficeDiscount { get; set; }
		public float mSiteDiscount { get; set; }

        public string ErrorDescription
        {
            get
            {
                return mErrorDescription;
            }
            set
            {
                mHasError = true;
                mErrorDescription = value;
            }
        }
        
        public FinalPrice()
        {
            mFinalPrice = 0;
            mFinalPriceNetto = 0;
            mErrorDescription = string.Empty;
            mHasError = false;
            mFinalPricePerDay = new List<FinalPricePerDay>();
			this.mComposition = new Composition();
            this.mBase = new Base();
            this.mRoomType = new RoomType();
			
			//new
			mCurrency = string.Empty;
			mCommission = 0;
			mOfficeDiscount = 0;
			mSiteDiscount = 0;
        }

        public FinalPrice(Composition iComposition, Base iBase, RoomType iRoomType, float iCommision, float iOfficeDiscount, float iSiteDiscount, string iCurrency)
        {
            mFinalPrice = 0;
            mFinalPriceNetto = 0;
            mErrorDescription = string.Empty;
            mHasError = false;
            this.mComposition = iComposition;
            this.mBase = iBase;
            this.mRoomType = iRoomType;
            mFinalPricePerDay = new List<FinalPricePerDay>();
			
			//new
			mCurrency = iCurrency;
			mCommission = iCommision;
			mOfficeDiscount = iOfficeDiscount;
			mSiteDiscount = iSiteDiscount;
        }

        public string toXmlString()
        {
            XmlDocument xml = new XmlDocument(), compositionXml = new XmlDocument(), BaseXml = new XmlDocument(), roomTypeXml = new XmlDocument(), finalPricePerDayXml = new XmlDocument();
            XmlElement finalPrices = (XmlElement)xml.AppendChild(xml.CreateElement("FinalPrices"));
            XmlElement finalPrice = (XmlElement)finalPrices.AppendChild(xml.CreateElement("FinalPrice"));
            XmlElement finalPricesPerDays = (XmlElement)finalPrice.AppendChild(xml.CreateElement("FinalPricesPerDays"));
            //FinalPrice
            finalPrice.AppendChild(xml.CreateElement("SupplierId")).InnerText = mSupplierId;
			
			// new rows
			finalPrice.AppendChild(xml.CreateElement("Commission")).InnerText = mCommission.ToString();
			finalPrice.AppendChild(xml.CreateElement("OfficeDiscount")).InnerText = mOfficeDiscount.ToString();
			finalPrice.AppendChild(xml.CreateElement("SiteDiscount")).InnerText = mSiteDiscount.ToString();
			finalPrice.AppendChild(xml.CreateElement("Currency")).InnerText = mCurrency;
			// - till here new
			
            finalPrice.AppendChild(xml.CreateElement("FinalPriceBrutto")).InnerText = mFinalPrice.ToString();
            finalPrice.AppendChild(xml.CreateElement("FinalPriceNetto")).InnerText = mFinalPriceNetto.ToString();
            finalPrice.AppendChild(xml.CreateElement("HasError")).InnerText = mHasError.ToString();
            finalPrice.AppendChild(xml.CreateElement("ErrorDescription")).InnerText = mErrorDescription;
            //Composition, Base, RoomType
            compositionXml.LoadXml(mComposition.toXmlString());
            BaseXml.LoadXml(mBase.toXmlString());
            roomTypeXml.LoadXml(mRoomType.toXmlString());
            finalPrice.AppendChild(xml.ImportNode(compositionXml.DocumentElement, true));
            finalPrice.AppendChild(xml.ImportNode(BaseXml.DocumentElement, true));
            finalPrice.AppendChild(xml.ImportNode(roomTypeXml.DocumentElement, true));

            foreach(FinalPricePerDay finalPricePerDay in mFinalPricePerDay)
            {
                finalPricePerDayXml.LoadXml(finalPricePerDay.toXmlString());
                finalPricesPerDays.AppendChild(xml.ImportNode(finalPricePerDayXml.DocumentElement, true));
            }

            finalPrice.AppendChild(finalPricesPerDays);

            return xml.OuterXml;
        }
    }
//}
