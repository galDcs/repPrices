using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

//namespace AgencyPricesWS
//{
    public class Composition : ComboDetails
    {
        public int mAdultsAmount { get; set; }
        public int mChildsAmount { get; set; }
        public int mInfantsAmount { get; set; }
        public float mPercent { get; set; }
        public float mPercentNetto { get; set; }
        
        public Composition() : base()
        {

        }

        public Composition(string iId, float iPercent, float iPrecent_Netto)
        {
            string name = string.Empty;

            try
            {
                DataSet ds = DAL_SQL.RunSqlDataSet("SELECT * FROM Agency_Admin.dbo.HOTEL_ROOM_TYPE WHERE  id = " + iId);
                DataRow row;

                if (Utils.isDataSetRowsNotEmpty(ds))
                {
                    row = ds.Tables[0].Rows[0];
                    name = row["name"].ToString();
                    mAdultsAmount = int.Parse(row["adt_amount"].ToString());
                    mChildsAmount = int.Parse(row["chd_amount"].ToString());
                    mInfantsAmount = int.Parse(row["babies"].ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to get RoomType. id = " + iId + ", Exception = " + ex.Message);
            }

            this.setId(iId);
            this.setName(name);
            mPercent = iPercent;
            mPercentNetto = iPrecent_Netto;
        }

        public Composition(string iId, string iName, float iPercent, float iPrecentNetto)
        {
            this.setId(iId);
            this.setName(iName);
            mPercent = iPercent;
            mPercentNetto = iPrecentNetto;
        }

        public string toXmlString()
        {
            XmlDocument xml = new XmlDocument();
            XmlElement composition = (XmlElement)xml.AppendChild(xml.CreateElement("Composition"));

            composition.AppendChild(xml.CreateElement("Id")).InnerText = mId;
            composition.AppendChild(xml.CreateElement("Name")).InnerText = mName;
            composition.AppendChild(xml.CreateElement("AdultsAmount")).InnerText = mAdultsAmount.ToString();
            composition.AppendChild(xml.CreateElement("ChildsAmount")).InnerText = mChildsAmount.ToString();
            composition.AppendChild(xml.CreateElement("InfantsAmount")).InnerText = mInfantsAmount.ToString();
            composition.AppendChild(xml.CreateElement("Percent")).InnerText = mPercent.ToString();
            composition.AppendChild(xml.CreateElement("NettoPercent")).InnerText = mPercentNetto.ToString();

            return xml.OuterXml;
        }
    }
//}
