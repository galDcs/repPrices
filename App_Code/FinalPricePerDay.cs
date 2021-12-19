//using AgencyPricesWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for FinalPricePerDay
/// </summary>
public class FinalPricePerDay
{
    public DateTime mDate { get; private set; }
    public decimal mPrice { get; private set; }
	public decimal mPriceNetto { get; private set; }
    public int mRoomsLeft { get; private set; }
    public bool mStatus { get; private set; }
    public string mColor { get; private set; }

    public FinalPricePerDay(DateTime iDate, decimal iPrice, decimal iPriceNetto, int iRoomsLeft, bool iStatus, string iColor)
	{
        mDate = iDate;
        mPrice = iPrice;
		mPriceNetto = iPriceNetto;
        mRoomsLeft = iRoomsLeft;
        mStatus = iStatus;
        mColor = iColor;
	}

    public string toXmlString()
    {
        XmlDocument xml = new XmlDocument();
        XmlElement composition = (XmlElement)xml.AppendChild(xml.CreateElement("FinalPricePerDay"));

        composition.AppendChild(xml.CreateElement("Date")).InnerText = mDate.ToString("dd-MMM-yy");
        composition.AppendChild(xml.CreateElement("Price")).InnerText = mPrice.ToString();
		composition.AppendChild(xml.CreateElement("PriceNetto")).InnerText = mPriceNetto.ToString();
        composition.AppendChild(xml.CreateElement("RoomsLeft")).InnerText = mRoomsLeft.ToString();
        composition.AppendChild(xml.CreateElement("Status")).InnerText = mStatus.ToString();
        composition.AppendChild(xml.CreateElement("Color")).InnerText = mColor.ToString();

        return xml.OuterXml;
    }
}