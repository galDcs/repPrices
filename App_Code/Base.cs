using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

//namespace AgencyPricesWS
//{
    public class Base : ComboDetails
    {
        public int mMealsAmount { get; set; }
        public decimal mAmount { get; set; }
        public float mPercent { get; set; }

        public Base()
        {

        }

        public Base(string iId, decimal iAmount)
        {
            string name = string.Empty;

            try
            {
                name = DAL_SQL.GetRecord("Agency_Admin.dbo.HOTEL_ON_BASE", "name", "id", iId);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to get RoomType. id = " + iId + ", Exception = " + ex.Message);
            }

            this.setId(iId);
            this.setName(name);
            mAmount = iAmount;
            mPercent = 0;
        }

        public Base(string iId, float iPercent)
        {
            string name = string.Empty;

            try
            {
                name = DAL_SQL.GetRecord("Agency_Admin.dbo.HOTEL_ON_BASE", "name", "id", iId);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to get RoomType. id = " + iId + ", Exception = " + ex.Message);
            }

            this.setId(iId);
            this.setName(name);
            mPercent = iPercent;
            mAmount = 0;
        }

        public Base(string iId, string iName, decimal iAmount)
        {
            this.setId(iId);
            this.setName(iName);
            mAmount = iAmount;
            mPercent = 0;
        }

        public Base(string iId, string iName, float iPercent)
        {
            this.setId(iId);
            this.setName(iName);
            mPercent = iPercent;
            mAmount = 0;
        }

        public string toXmlString()
        {
            XmlDocument xml = new XmlDocument();
            XmlElement composition = (XmlElement)xml.AppendChild(xml.CreateElement("Base"));

            composition.AppendChild(xml.CreateElement("Id")).InnerText = mId;
            composition.AppendChild(xml.CreateElement("Name")).InnerText = mName;
            composition.AppendChild(xml.CreateElement("MealsAmount")).InnerText = mMealsAmount.ToString();
            composition.AppendChild(xml.CreateElement("Amount")).InnerText = mAmount.ToString();
            composition.AppendChild(xml.CreateElement("Percent")).InnerText = mPercent.ToString();

            return xml.OuterXml;
        }
    }
//}
