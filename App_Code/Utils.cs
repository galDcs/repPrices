using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Xml;
//using AgencyPricesWS;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Reflection;

public enum eLanguage
{
    Hebrew = 1255,
    English = 1252,
    Russian = 1251
}

public enum eComboType
{
    Composition = 1,
    Base,
    RoomType
}

public static class Utils
{
    public static void setWeekendAndBussinessDays(DateTime iFromDate, DateTime iToDate, out int iBussinessDays, out int iWeekendDays)
    {
        iBussinessDays = getBussinessDays(iFromDate, iToDate);
        iWeekendDays = getWeekendDays(iFromDate, iToDate);
    }

    public static int getBussinessDays(DateTime iFromDate, DateTime iToDate)
    {
        int bussinessDays = 0;
        int i = 0;
        DateTime currDay = iFromDate;

        while (currDay != iToDate)
        {
            switch (currDay.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                    bussinessDays++;
                    break;
            }

            i++;
            currDay = iFromDate.AddDays(i);
        }

        return bussinessDays;
    }

    public static int getWeekendDays(DateTime iFromDate, DateTime iToDate)
    {
        int weekendDays = 0;
        int i = 0;
        DateTime currDay = iFromDate;

        while (currDay != iToDate)
        {
            switch (currDay.DayOfWeek)
            {
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                    weekendDays++;
                    break;
            }

            i++;
            currDay = iFromDate.AddDays(i);
        }

        return weekendDays;
    }

    public static string ConvertStringToAgencyUtf(string str)
    {
        string tmp = "";
        if (str != null && str != string.Empty)
        {
            foreach (char c in str)
            {
                tmp = tmp + "&#" + Convert.ToString((int)(c)) + ";";
            }
            return HttpUtility.UrlEncode(tmp);
        }
        return string.Empty;
    }

    public static bool CheckSecurity(int resourceID, int clerkId)
    {
        bool result = false;
        string userName, userPassword;
        int agencyId, systemType;

        try
        {

           // Agency2000WS.Agency2000WS ss = new Agency2000WS.Agency2000WS();
           //
           // result = ss.CheckSecurity(agencyId, systemType, userName, userPassword, resourceID);
        }
        catch (Exception ex)
        {
            result = false;
            Logger.Log("Exception. result " + ex.Message);
        }

        return result;
    }

    public static string ConvertMonthToHebName(int month)
    {
        switch (month)
        {
            case 1:
                return "ינואר";//"ינואר","פברואר","מרץ","אפריל","מאי","יוני","יולי","אוגוסט","ספטמבר","אוקטובר","נובמבר","דצמבר"
            case 2:
                return "פברואר";
            case 3:
                return "מרץ";
            case 4:
                return "אפריל";
            case 5:
                return "מאי";
            case 6:
                return "יוני";
            case 7:
                return "יולי";
            case 8:
                return "אוגוסט";
            case 9:
                return "ספטמבר";
            case 10:
                return "אוקטובר";
            case 11:
                return "נובמבר";
            case 12:
                return "דצמבר";

        }
        return " ";
    }

    public static string ConvertMonthToEngName(int month)
    {
        switch (month)
        {
            case 1:
                return "Jan";//"ינואר","פברואר","מרץ","אפריל","מאי","יוני","יולי","אוגוסט","ספטמבר","אוקטובר","נובמבר","דצמבר"
            case 2:
                return "Feb";
            case 3:
                return "Mar";
            case 4:
                return "Apr";
            case 5:
                return "May";
            case 6:
                return "Jun";
            case 7:
                return "Jul";
            case 8:
                return "Aug";
            case 9:
                return "Sep";
            case 10:
                return "Oct";
            case 11:
                return "Nov";
            case 12:
                return "Dec";

        }
        return " ";
    }

    public static string ConvertMonthWithSlashToAgencyFormat(string date)
    {
        string result;
        string[] splittedStr = date.Split('/');

        if (splittedStr.Length > 0)
        {
            string month = ConvertMonthToEngName(int.Parse(splittedStr[1]));
            result = splittedStr[0] + "-" + month + "-" + splittedStr[2];
        }
        else
        {
            result = date;
        }

        return result;
    }

	public static string getColumnValueByName(DataRow iRow, string iColumnName)
	{
		string value = string.Empty;
		
		if (iRow.Table.Columns.Contains(iColumnName))
		{
			value = iRow[iColumnName].ToString();
		}
		
		return value;
	}	
   
    public static bool isDataSetRowsNotEmpty(DataSet iDataSet)
    {//Check if the DataSet has rows in first table.
        bool isNotEmpty = false;

        if (iDataSet != null && iDataSet.Tables != null && iDataSet.Tables.Count > 0
                && iDataSet.Tables[0].Rows != null && iDataSet.Tables[0].Rows.Count > 0)
        {
            isNotEmpty = true;
        }

        return isNotEmpty;
    }

    public static int getDayFromDate(DateTime iDate)
    {

        //Enum of days in week (C# code)
        DayOfWeek dayInWeek = iDate.DayOfWeek;
        int day = 0;

        switch (dayInWeek)
        {
            case DayOfWeek.Sunday:
                day = 1;
                break;
            case DayOfWeek.Monday:
                day = 2;
                break;
            case DayOfWeek.Tuesday:
                day = 3;
                break;
            case DayOfWeek.Wednesday:
                day = 4;
                break;
            case DayOfWeek.Thursday:
                day = 5;
                break;
            case DayOfWeek.Friday:
                day = 6;
                break;
            case DayOfWeek.Saturday:
                day = 7;
                break;

            default:
                day = -1;
                break;
        }

        return day;
    }

    public static string getTextFromFile(string nameInFile, eLanguage lang)
    {
        string textValue = string.Empty;
        string fileName = HttpContext.Current.Request.MapPath("txt1.txt");//"D:\\projects\\txt1.txt";// ConfigurationManager.AppSettings["headersFile" + lang];
        string line = string.Empty;
        string txtFromLine = string.Empty;

        using (StreamReader reader = new StreamReader(fileName))
        {
            line = reader.ReadLine();

            while (line != null)
            {
                if (nameInFile.Length < line.Length)
                {
                    txtFromLine = line.Substring(0, nameInFile.Length + 1);
                    if (txtFromLine == nameInFile + "=")
                    {
                        textValue = line.Remove(0, nameInFile.Length + 1);
                    }
                }

                line = reader.ReadLine();
            }
        }

        return textValue;
    }

    public static decimal getForammatedNumber(decimal iNum)
    {
        int digitsAfterPoint = 2;

        return Math.Round(iNum, digitsAfterPoint);
    }

//Hotel prices code.

    public static HotelPrice getHotelPriceDetails(string iSupplierId)
    {
        HotelPrice hotelPrice = null;
        string hotelPriceId = getHotelPriceId(iSupplierId);

        if (!string.IsNullOrEmpty(hotelPriceId))
        {
            DataSet ds = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICES WHERE id = " + hotelPriceId);
            DataSet compositionPercents = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_COMPOSITIONS WHERE hotel_price_id = " + hotelPriceId);
            DataSet baseAmounts = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_BASES WHERE hotel_price_id = " + hotelPriceId);
            DataSet roomTypeAmounts = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_ROOM_TYPES WHERE hotel_price_id = " + hotelPriceId);
            DataSet monthlyAllocations = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_MONTHLY_ALLOCATION WHERE hotel_price_id = " + hotelPriceId);
            DataRow row = ds.Tables[0].Rows[0];
            
            hotelPrice = new HotelPrice();
            hotelPrice.mIsBaseAmount = bool.Parse(row["is_base_amount"].ToString());

            addComboToList(eComboType.Composition, compositionPercents, hotelPrice, false);
            addComboToList(eComboType.Base, baseAmounts, hotelPrice, hotelPrice.mIsBaseAmount);
            addComboToList(eComboType.RoomType, roomTypeAmounts, hotelPrice, false);

            hotelPrice.mBaseComposition = new Composition(row["base_price_composition_id"].ToString(), 0,0);

            if (hotelPrice.mIsBaseAmount)
            {
                hotelPrice.mBaseBase = new Base(row["base_price_base_id"].ToString(), (decimal)0);
            }
            else
            {
                hotelPrice.mBaseBase = new Base(row["base_price_base_id"].ToString(), (float)0);
            }

            hotelPrice.mBaseRoomType = new RoomType(row["base_price_room_type_id"].ToString(), 0);
            hotelPrice.mOfficeDiscount = float.Parse(row["office_discount"].ToString());
            hotelPrice.mSiteDiscount = float.Parse(row["site_discount"].ToString());
            hotelPrice.mCommission = float.Parse(row["commission"].ToString());
            hotelPrice.mGeneralAdditive = float.Parse(row["general_additive"].ToString());
            hotelPrice.mBabiesAdditive = float.Parse(row["babies_additive"].ToString());
            hotelPrice.mStatus = bool.Parse(row["status"].ToString());
            hotelPrice.mPriceType = row["price_type"].ToString();
            hotelPrice.mPriceColor = row["price_color"].ToString();
            hotelPrice.mCurrency = row["currency_id"].ToString();
            hotelPrice.mSupplierId = iSupplierId;
            
            if (Utils.isDataSetRowsNotEmpty(monthlyAllocations))
            {
                MonthlyAllocation monthlyAllocationTemp;
                int month, year, monthlyAllocation;

                foreach (DataRow monthlyAllocationRow in monthlyAllocations.Tables[0].Rows)
                {
                    month = int.Parse(monthlyAllocationRow["month"].ToString());
                    year = int.Parse(monthlyAllocationRow["year"].ToString());
                    monthlyAllocation = int.Parse(monthlyAllocationRow["monthly_allocation"].ToString());
                    monthlyAllocationTemp = new MonthlyAllocation(month, year, monthlyAllocation);

                    hotelPrice.mMonthlyAllocations.Add(monthlyAllocationTemp);
                }
            }
        }

        return hotelPrice;
    }

    private static void addComboToList(eComboType iComboType, DataSet comboDataSet, HotelPrice iHotelPrice, bool isBaseAmount)
    {
        if (isDataSetRowsNotEmpty(comboDataSet))
        {
            foreach (DataRow row in comboDataSet.Tables[0].Rows)
            {
                switch (iComboType)
                {
                    case eComboType.Composition:
                        //iHotelPrice.mCompositions.Add(new Composition(row["composition_id"].ToString(), float.Parse(row["percent"].ToString()), float.Parse(row["percent_netto"].ToString())), bool.Parse(row["status"].ToString()));
                        iHotelPrice.mCompositions.Add(new Composition(row["composition_id"].ToString(), float.Parse(row["percent"].ToString()), float.Parse(row["percent_netto"].ToString())), bool.Parse(row["status"].ToString()));
                        break;
                    case eComboType.Base:
						if (row["status"].ToString() == bool.TrueString)
						{
							if (isBaseAmount)
							{
								iHotelPrice.mBases.Add(new Base(row["base_id"].ToString(), decimal.Parse(row["amount"].ToString())), bool.Parse(row["status"].ToString()));
							}
							else
							{
								iHotelPrice.mBases.Add(new Base(row["base_id"].ToString(), float.Parse(row["percent"].ToString())), bool.Parse(row["status"].ToString()));
							}
						}
                        break;
                    case eComboType.RoomType:
                        iHotelPrice.mRoomTypes.Add(new RoomType(row["room_type_id"].ToString(), decimal.Parse(row["amount"].ToString())), bool.Parse(row["status"].ToString()));
                        break;
                }
            }
        }
    }

    public static XmlDocument getHotelPriceDetailsXml(string iSupplierId)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlElement = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("PricePerDayDetails"));
        HotelPrice hotelPrice = null;
        string hotelPriceId = getHotelPriceId(iSupplierId);

        if (string.IsNullOrEmpty(hotelPriceId))
        {
            hotelPrice = getHotelPriceDetails(iSupplierId);
            xmlElement.AppendChild(xmlDoc.CreateElement("BaseCompositionId")).InnerText = hotelPrice.mBaseComposition.getId();
            xmlElement.AppendChild(xmlDoc.CreateElement("BaseBaseId")).InnerText = hotelPrice.mBaseBase.getId();
            xmlElement.AppendChild(xmlDoc.CreateElement("BaseRoomTypeId")).InnerText = hotelPrice.mBaseRoomType.getId();
            xmlElement.AppendChild(xmlDoc.CreateElement("OfficeDiscount")).InnerText = hotelPrice.mOfficeDiscount.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("SiteDiscount")).InnerText = hotelPrice.mSiteDiscount.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("GeneralAdditive")).InnerText = hotelPrice.mGeneralAdditive.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("BabiesAdditive")).InnerText = hotelPrice.mBabiesAdditive.ToString();
        }

        return xmlDoc;
    }

    public static XmlDocument getDatePriceBaseDetails(string iSupplierId, DateTime iDate)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlElement = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("PricePerDayDetails"));
        PricePerDay pricePerDay = getSpecificDatePriceDetails(iSupplierId, iDate);

        if (pricePerDay != null)
        {
            xmlElement.AppendChild(xmlDoc.CreateElement("BasePrice")).InnerText = pricePerDay.mBasePrice.mPrice.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("BasePriceNetto")).InnerText = pricePerDay.mBasePrice.mPriceNetto.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("Status")).InnerText = pricePerDay.mStatus.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("RoomsAmount")).InnerText = pricePerDay.mAllocation.mRoomsAmount.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("RoomsInUse")).InnerText = pricePerDay.mAllocation.mRoomsInUse.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("RoomsDisable")).InnerText = pricePerDay.mAllocation.mRoomsDisable.ToString();
            xmlElement.AppendChild(xmlDoc.CreateElement("Color")).InnerText = pricePerDay.mColor.ToString();
			xmlElement.AppendChild(xmlDoc.CreateElement("CloseDateReason")).InnerText = pricePerDay.CloseDateReason.ToString();
        }

        return xmlDoc;
    }

    public static XmlDocument getDatePriceSalesCycles(string iSupplierId, DateTime iDate)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlElement = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("SalesCycles"));
        string pricePerDayId = getHotelToPricePerDayId(iSupplierId, iDate);
       
        if (!string.IsNullOrEmpty(pricePerDayId))
        {
            DataSet ds = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_SALES_CYCLES WHERE price_per_day_id = " + pricePerDayId + " and status = 1");

            if (isDataSetRowsNotEmpty(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    xmlElement.AppendChild(xmlDoc.CreateElement("CycleDays")).InnerText = row["cycle_days"].ToString();
                }
            }
        }

        return xmlDoc;
    }

    public static PricePerDay getSpecificDatePriceDetails(string iSupplierId, DateTime iDate)
    {
        string pricePerDayId = getHotelToPricePerDayId(iSupplierId, iDate);
        PricePerDay pricePerDay = null;
        
        if (!string.IsNullOrEmpty(pricePerDayId))
        {
            pricePerDay = new PricePerDay();
            pricePerDay.mDate = iDate;

            XmlDocument xmlDoc = getDatePriceSalesCycles(iSupplierId, iDate);
            DataSet dsHotelPrice;
            DataSet dsSpecificDate = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_TO_PRICE_PER_DAY WHERE id = " + pricePerDayId);
            string hotelPriceId = Utils.getHotelPriceId(iSupplierId);
            dsHotelPrice = DAL_SQL.RunSqlDataSet(string.Format("SELECT * FROM P_HOTEL_PRICES WHERE supplier_id = {0}", iSupplierId));
            DataSet basesInDate = DAL_SQL_Helper.getBaseByHotelIdAndDate(iSupplierId, iDate);

            /*
             * enter the combo for each day
             */

            List<Combo> comboList = new List<Combo>();
            DataSet compositionPercents = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_COMPOSITIONS WHERE hotel_price_id = " + hotelPriceId + " AND status = 1");
            DataSet roomTypeAmounts = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_ROOM_TYPES WHERE hotel_price_id = " + hotelPriceId + " AND status = 1");

            foreach (DataRow baseRow in basesInDate.Tables[0].Rows)
            {
                foreach(DataRow comopsitionRow in compositionPercents.Tables[0].Rows)
                {
                    foreach (DataRow roomTypRow in roomTypeAmounts.Tables[0].Rows)
                    {
                        Combo currCombo = new Combo();
                        currCombo.mBase = new Base(baseRow["base_id"].ToString(), (decimal)0);
                        currCombo.mComposition = new Composition(comopsitionRow["composition_id"].ToString(), 0, 0);
                        currCombo.mRoomType = new RoomType(roomTypRow["room_type_id"].ToString(), 0);
                        comboList.Add(currCombo);
                    }
                }
            }

            pricePerDay.AvailableCombos = comboList;
            pricePerDay.mBasePrice.mComposition = new Composition(dsHotelPrice.Tables[0].Rows[0]["base_price_composition_id"].ToString(), 0, 0);

            if (bool.Parse(dsHotelPrice.Tables[0].Rows[0]["is_base_amount"].ToString()))
            {
                pricePerDay.mBasePrice.mBase = new Base(dsHotelPrice.Tables[0].Rows[0]["base_price_base_id"].ToString(), (decimal)0);
            }
            else
            {
                pricePerDay.mBasePrice.mBase = new Base(dsHotelPrice.Tables[0].Rows[0]["base_price_base_id"].ToString(), (float)0);
            }
            
            pricePerDay.mBasePrice.mRoomType = new RoomType(dsHotelPrice.Tables[0].Rows[0]["base_price_room_type_id"].ToString(), 0);
            pricePerDay.mBasePrice.mPrice = decimal.Parse(dsSpecificDate.Tables[0].Rows[0]["base_price"].ToString());
            pricePerDay.mBasePrice.mPriceNetto = decimal.Parse(dsSpecificDate.Tables[0].Rows[0]["base_price_netto"].ToString());

            pricePerDay.mStatus = dsSpecificDate.Tables[0].Rows[0]["status"].ToString().ToLower() == "true";

            pricePerDay.mAllocation.mRoomsAmount = int.Parse(dsSpecificDate.Tables[0].Rows[0]["days_allocated"].ToString());
            pricePerDay.mAllocation.mRoomsInUse = int.Parse(dsSpecificDate.Tables[0].Rows[0]["days_used"].ToString());
            pricePerDay.mAllocation.mRoomsDisable = int.Parse(dsSpecificDate.Tables[0].Rows[0]["days_disable"].ToString());
            
            pricePerDay.mColor = dsSpecificDate.Tables[0].Rows[0]["color"].ToString();
            pricePerDay.CloseDateReason = dsSpecificDate.Tables[0].Rows[0]["close_date_reason"].ToString();

            if (xmlDoc.SelectNodes("/SalesCycles/CycleDays") != null)
            {
                foreach (XmlNode node in xmlDoc.SelectNodes("/SalesCycles/CycleDays"))
                {
                    pricePerDay.mSalesCycles.Add(int.Parse(node.InnerText));
                }
            }
        }

        return pricePerDay;
    }

    //Utils
    public static string getHotelPriceId(string iSupplierId)
    {
        DataSet ds = DAL_SQL.RunSqlDataSet("SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = " + iSupplierId);
        string supplierPriceId = string.Empty;

        if (isDataSetRowsNotEmpty(ds))
        {
            supplierPriceId = ds.Tables[0].Rows[0]["id"].ToString();
        }

        return supplierPriceId;
    }

    public static string getHotelToPricePerDayId(string iSupplierId, DateTime iDate)
    {
        string pricePerDayId = string.Empty;
        DataSet dsHotelPrice, dsSpecificDate;
        int hotelPriceId;

        dsHotelPrice = DAL_SQL.RunSqlDataSet(string.Format("SELECT * FROM P_HOTEL_PRICES WHERE supplier_id = {0}", iSupplierId));
        if (Utils.isDataSetRowsNotEmpty(dsHotelPrice))
        {
            hotelPriceId = int.Parse(dsHotelPrice.Tables[0].Rows[0]["id"].ToString());
            dsSpecificDate = DAL_SQL.RunSqlDataSet(string.Format(@"
SELECT id FROM P_HOTEL_TO_PRICE_PER_DAY 
WHERE hotel_price_id = {0} and 
cast(date as smalldatetime) = cast('{1}' as smalldatetime)", hotelPriceId, iDate.ToString("dd-MMM-yy")));
            if (Utils.isDataSetRowsNotEmpty(dsSpecificDate))
            {
                pricePerDayId = dsSpecificDate.Tables[0].Rows[0]["id"].ToString();
            }
        }
		
        return pricePerDayId;
    }

    public static XmlDocument getDateCombosPrices(HotelPrice iHotelPrice, DateTime iDate)
    {
        decimal pricePerCombo = 0;
        decimal nettoPricePerCombo = 0;
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlElement = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));
        XmlElement xmlElementDateCombosPrices = null;
        foreach (Composition hotelComposition in iHotelPrice.mCompositions.Keys)
        {
            foreach (Base hotelBase in iHotelPrice.mBases.Keys)
            {
                foreach (RoomType hotelRoomType in iHotelPrice.mRoomTypes.Keys)
                {
                    if (iHotelPrice.mCompositions[hotelComposition] == true && iHotelPrice.mBases[hotelBase] && iHotelPrice.mRoomTypes[hotelRoomType])
                    {
                        try
                        {
                            pricePerCombo = iHotelPrice.getPricePerDayList().Find(x => x.mDate.Date == iDate.Date).getFinalPriceForCombo(iHotelPrice.mSupplierId, hotelComposition, hotelBase, hotelRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
                            xmlElementDateCombosPrices = (XmlElement)xmlElement.AppendChild(xmlDoc.CreateElement("DateCombosPrices"));
                            nettoPricePerCombo = iHotelPrice.getPricePerDayList().Find(x => x.mDate.Date == iDate.Date).getFinalPriceNettoForCombo(iHotelPrice.mSupplierId, hotelComposition, hotelBase, hotelRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
                            xmlElementDateCombosPrices.AppendChild(xmlDoc.CreateElement("ComboPrice")).InnerText = pricePerCombo.ToString();
                            xmlElementDateCombosPrices.AppendChild(xmlDoc.CreateElement("ComboNettoPrice")).InnerText = nettoPricePerCombo.ToString();
                            xmlElementDateCombosPrices.AppendChild(xmlDoc.CreateElement("CompositionId")).InnerText = hotelComposition.getName();
                            xmlElementDateCombosPrices.AppendChild(xmlDoc.CreateElement("BaseId")).InnerText = hotelBase.getName();
                            xmlElementDateCombosPrices.AppendChild(xmlDoc.CreateElement("RoomTypeId")).InnerText = hotelRoomType.getName();
                            xmlElement.AppendChild(xmlElementDateCombosPrices);
                        }
                        catch(Exception e)
                        {

                        }
                    }
                }
            }
        }

        return xmlDoc;
    }

    internal static List<string> getSuppliersIdsListByGovArea(string iAreaId, string iLang)
    {
        List<string> suppliersIdsList = null;
        
        try
        {
            DataSet ds = DAL_SQL_Helper.GetHotelsByArea(iAreaId, iLang);
            if (isDataSetRowsNotEmpty(ds))
            {
                suppliersIdsList = new List<string>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    suppliersIdsList.Add(row["id"].ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to get suppliers. " + ex.Message);
            throw new Exception("Failed to get suppliers. " + ex.Message);
        }

        return suppliersIdsList;
    }
	
	internal static List<string> getSuppliersIdsListByArea(string iAreaId, string iLang)
    {
        List<string> suppliersIdsList = null;
        
        try
        {
            DataSet ds = DAL_SQL_Helper.GetAllHotelsByArea(iAreaId, iLang);
			//DataSet ds = DAL_SQL_Helper.GetHotelsByArea(iAreaId, iLang);
			
            if (isDataSetRowsNotEmpty(ds))
            {
                suppliersIdsList = new List<string>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    suppliersIdsList.Add(row["id"].ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to get suppliers. " + ex.Message);
            throw new Exception("Failed to get suppliers. " + ex.Message);
        }

        return suppliersIdsList;
    }

    public static string GetAreaNameById(string mAreaId, string iLang)
    {
        string areaName = string.Empty;
        
        try   
        {
            areaName = DAL_SQL.GetRecord("Agency_Admin.dbo.AREAS", "name_" + iLang, "id", mAreaId);
        }
        catch(Exception ex)
        {
            Logger.Log("Failed to get area name. exception = " + ex.Message);
        }

        return areaName;
    }

    public static void SaveMonthlyAllocation(string iSupplierId, int iMonth, int iYear, string iMonthlyAllocation)
    {
        try
        {
            DAL_SQL_Helper.SaveMonthlyAllocation(iSupplierId, iMonth, iYear, iMonthlyAllocation);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static bool isCrossMonth(DateTime iFromDate, DateTime iToDate)
    {
        bool isCrossMonth = false;

        if (iFromDate.Month != iToDate.Month)
        {
            isCrossMonth = true;
        }

        return isCrossMonth;
    }

    public static int getMonthlyUsedAllocations(string iSupplierId, int iMonth, int iYear)
    {
        int monthlyUsedAllocation = -1;
        DataSet ds;

        try
        {
            ds = DAL_SQL_Helper.getMonthlyUsedAllocation(iSupplierId, iMonth, iYear);
            if (isDataSetRowsNotEmpty(ds))
            {
                monthlyUsedAllocation = int.Parse(ds.Tables[0].Rows[0]["monthly_used"].ToString());
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return monthlyUsedAllocation;
    }


    public static void AddControlsToTable(List<TableCell> iCellsToAdd, ref Table iTableToUpdate)
    {
        TableRow row = new TableRow();
        foreach (TableCell cell in iCellsToAdd)
        {
            row.Controls.Add(cell);
        }
        iTableToUpdate.Controls.Add(row);
    }
    public static TableCell createLabelInCell(string iHeaderName)
    {
        TableCell retVal = new TableCell();
        Label retValLabel = new Label();
        retValLabel.Text = iHeaderName;
        retVal.CssClass = "gal";
        retVal.Controls.Add(retValLabel);
        return retVal;
    }

    public static string ConvertMonthToName(int month)
    {
        switch (month)
        {
            case 1:
                return "ינואר";//"ינואר","פברואר","מרץ","אפריל","מאי","יוני","יולי","אוגוסט","ספטמבר","אוקטובר","נובמבר","דצמבר"

            case 2:
                return "פברואר";
            case 3:
                return "מרץ";
            case 4:
                return "אפריל";
            case 5:
                return "מאי";
            case 6:
                return "יוני";
            case 7:
                return "יולי";
            case 8:
                return "אוגוסט";
            case 9:
                return "ספטמבר";
            case 10:
                return "אוקטובר";
            case 11:
                return "נובמבר";
            case 12:
                return "דצמבר";

        }
        return " ";
    }

    public static string GetDayHebrewLatter(DayOfWeek dayOfWeek)
    {
        string day = string.Empty;

        switch (dayOfWeek)
        {
            case DayOfWeek.Friday:
                day = "ו";
                break;
            case DayOfWeek.Monday:
                day = "ב";
                break;
            case DayOfWeek.Saturday:
                day = "ש";
                break;
            case DayOfWeek.Sunday:
                day = "א";
                break;
            case DayOfWeek.Thursday:
                day = "ה";
                break;
            case DayOfWeek.Tuesday:
                day = "ג";
                break;
            case DayOfWeek.Wednesday:
                day = "ד";
                break;
        }

        return day;
    }

    public static void FillDdl(DropDownList iDdlRef, object iData, string iText,
                     string iValue, bool iIsAddSelectAllOption, bool isAddChooseOption) //ddl, dataset, Text, value, iIsAddSelectAllOption
    {

        if (!iText.Equals(string.Empty) && !iValue.Equals(string.Empty))
        {
            iDdlRef.DataTextField = iText;
            iDdlRef.DataValueField = iValue;
        }
        iDdlRef.DataSource = iData;
        iDdlRef.DataBind();

        if (iIsAddSelectAllOption)
        {
            iDdlRef.Items.Insert(0, new ListItem("All", "1"));

        }

        if (isAddChooseOption)
        {
            iDdlRef.Items.Insert(0, new ListItem("בחירה", "0"));
        }
    }

}