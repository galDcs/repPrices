using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

//namespace AgencyPricesWS
//{
    public class RoomType : ComboDetails
    {
        public int mRoomCapacity { get; set; }
        public decimal mAmount { get; set; }

        public RoomType()
        {


        }
        public RoomType(string iId, decimal iAmount)
        {
            string name = string.Empty;
            
            try
            {
                name = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS_HOTEL_ADDS", "name", "id", iId);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to get RoomType. id = " + iId + ", Exception = " + ex.Message);
            }

            
            this.setId(iId);
            this.setName(name);
            mAmount = iAmount;
        }

        public RoomType(string iId, string iName, decimal iAmount)
        {
            this.setId(iId);
            this.setName(iName);
            mAmount = iAmount;
        }

        public string toXmlString()
        {
            XmlDocument xml = new XmlDocument();
            XmlElement composition = (XmlElement)xml.AppendChild(xml.CreateElement("RoomType"));

            composition.AppendChild(xml.CreateElement("Id")).InnerText = mId;
            composition.AppendChild(xml.CreateElement("Name")).InnerText = mName;
            composition.AppendChild(xml.CreateElement("RoomCapacity")).InnerText = mRoomCapacity.ToString();
            composition.AppendChild(xml.CreateElement("Amount")).InnerText = mAmount.ToString();

            return xml.OuterXml;
        }
    }
//}
