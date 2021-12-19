//using AgencyPricesWS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

/// <summary>
/// Summary description for PriceDetails
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class PriceDetailsWs : System.Web.Services.WebService {

    public PriceDetailsWs()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }
        
    [WebMethod]
    public HotelPrice getHotelPrice(string supplierId, int month, int year)
    {
        HotelPrice hotelPrice = null;
        string supplierPriceId = Utils.getHotelPriceId(supplierId);
        int day = 1;

        if (!string.IsNullOrEmpty(supplierPriceId))
        {
            hotelPrice = Utils.getHotelPriceDetails(supplierId);
            DateTime monthlyDate = new DateTime(year, month, day);
            
            PricePerDay pricePerDay = null;
            hotelPrice.mSupplierId = supplierId;

            while (monthlyDate.Month == month)
            {
                pricePerDay = Utils.getSpecificDatePriceDetails(supplierId, monthlyDate);
                hotelPrice.savePricePerDay(pricePerDay);
                monthlyDate = monthlyDate.AddDays(1);
            }
        }

        return hotelPrice;
    }

    /// Return all data about specific date
    /// basePrice, baseCombo, allocations...
    [WebMethod]
    public string getDatePriceDetails(string supplierId, string date)
    {
        DateTime specificDate = DateTime.Parse(date);
        bool isDeep = true;
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement rootElement = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));

        XmlDocument baseDetailXmlDoc = Utils.getDatePriceBaseDetails(supplierId, specificDate);
        XmlDocument dateSalesCyclesXmlDoc = Utils.getDatePriceSalesCycles(supplierId, specificDate);

        rootElement.AppendChild(xmlDoc.ImportNode(baseDetailXmlDoc.DocumentElement, isDeep));
        xmlDoc.SelectNodes("/Root/PricePerDayDetails")[0].AppendChild(xmlDoc.ImportNode(dateSalesCyclesXmlDoc.DocumentElement, isDeep));

        return xmlDoc.OuterXml;
    }

    public bool saveDateDetails(string iSupplierId, PricePerDay iDatePrice)
    {
        bool isSaveSucceded = true;
        string iHotelPriceId = Utils.getHotelPriceId(iSupplierId);
        string pricePerDay = Utils.getHotelToPricePerDayId(iSupplierId, iDatePrice.mDate);

        if (!string.IsNullOrEmpty(pricePerDay))
        {
            try
            {
                DAL_SQL_Helper.updatePricePerDay(iHotelPriceId, iDatePrice.mDate, iDatePrice.mStatus, iDatePrice.mBasePrice.mPrice, iDatePrice.mBasePrice.mPriceNetto, iDatePrice.mAllocation.mRoomsAmount, iDatePrice.mAllocation.mRoomsDisable, iDatePrice.mColor);
				DAL_SQL.RunSql("UPDATE P_HOTEL_TO_PRICE_PER_DAY SET close_date_reason = N'" + iDatePrice.CloseDateReason + "' WHERE id = " + pricePerDay);
            }
            catch (Exception exUpdate)
            {
                Logger.Log("Failed to update pricePerDay. date = " + iDatePrice.mDate.ToString("dd-MMM-yy"));
                isSaveSucceded = false;
            }
        }
        else
        {
            try
            {
                DAL_SQL_Helper.insertPricePerDay(iHotelPriceId, iDatePrice.mDate, iDatePrice.mStatus, iDatePrice.mBasePrice.mPrice, iDatePrice.mBasePrice.mPriceNetto, iDatePrice.mAllocation.mRoomsAmount, iDatePrice.mAllocation.mRoomsDisable, iDatePrice.mColor);
            }
            catch (Exception exInsert)
            {
                Logger.Log("Failed to insert pricePerDay. date = " + iDatePrice.mDate.ToString("dd-MMM-yy"));
                isSaveSucceded = false;
            }
        }

        //Save cycle days.
        string pricePerDayId = Utils.getHotelToPricePerDayId(iSupplierId,iDatePrice.mDate);

        foreach (int saleCycle in iDatePrice.mSalesCycles)
        {
            bool isCycleDayExists = DAL_SQL_Helper.IsCycleDayExist(pricePerDayId, saleCycle);
            
            if (!isCycleDayExists)
            {
                try
                {
                    DAL_SQL_Helper.insertDatePriceSaleCycle(pricePerDayId, saleCycle);
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to insert sale cycle. (pricePerDayId = " + pricePerDayId + ", saleCycle = " + saleCycle + "). Exception = " + ex.Message);
                    isSaveSucceded = false;
                }
            }
        }

        //Disable each cycle day that the client did not mention.
        XmlDocument xmlDoc = Utils.getDatePriceSalesCycles(iSupplierId, iDatePrice.mDate);
        List<int> saleCyclesFromDB = new List<int>();

        if (xmlDoc.SelectNodes("/SalesCycles/CycleDays") != null)
        {
            foreach (XmlNode node in xmlDoc.SelectNodes("/SalesCycles/CycleDays"))
            {
                saleCyclesFromDB.Add(int.Parse(node.InnerText));
            }

            //Set true to all existed days cycles in DB are false now.
            List<int> result = new List<int>(iDatePrice.mSalesCycles.Where(x => !saleCyclesFromDB.Any(y => y == x)));
            foreach (int saleCycle in result)
            {
                try
                {
                    DAL_SQL_Helper.updateDatePriceSaleCycle(pricePerDayId, saleCycle, true);
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to update sale cycle. (pricePerDayId = " + pricePerDayId + ", saleCycle = " + saleCycle + "). Exception = " + ex.Message);
                    isSaveSucceded = false;
                }
            }

            //Set false to all 'deleted' days cycles.
            result = new List<int>(saleCyclesFromDB.Where(x => !iDatePrice.mSalesCycles.Any(y => y == x)));
            foreach (int saleCycle in result)
            {
                try
                {
                    DAL_SQL_Helper.updateDatePriceSaleCycle(pricePerDayId, saleCycle, false);
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to update sale cycle. (pricePerDayId = " + pricePerDayId + ", saleCycle = " + saleCycle + "). Exception = " + ex.Message);
                    isSaveSucceded = false;
                }
            }
        }

        return isSaveSucceded;
    }

    public bool saveHotelPrice(HotelPrice iHotelPrice, string iClerkId)
    {
        bool isSaveSucceded = true;
        string hotelPriceId = Utils.getHotelPriceId(iHotelPrice.mSupplierId);
        string msg = string.Empty;

        //Update hotel price general details
        try
        {
            DAL_SQL_Helper.updateHotelPriceGeneralDetails(iHotelPrice.mSupplierId, iHotelPrice.mPriceType, iHotelPrice.mStatus, iClerkId, iHotelPrice.mPriceColor, iHotelPrice.mCurrency,
                                            iHotelPrice.mBaseComposition.getId(),iHotelPrice.mBaseBase.getId(), iHotelPrice.mBaseRoomType.getId(), iHotelPrice.mIsBaseAmount, 
                                            iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mCommission, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
        }
        catch (Exception ex)
        {
            isSaveSucceded = false;
            msg = "Failed to update general details";
            Logger.Log(msg);
            throw new Exception(msg);
        }

        //Update all combos
        if (isSaveSucceded)
        {
            //Update compositions
            foreach (Composition composition in iHotelPrice.mCompositions.Keys)
            {
                try
                {
                    DAL_SQL_Helper.updateHotelPriceComboByType(hotelPriceId, composition.getId(), iHotelPrice.mCompositions[composition], 0, composition.mPercent, composition.mPercentNetto, eComboType.Composition);
                }
                catch (Exception exUpdateCompositions)
                {
                    isSaveSucceded = false;
                    msg = "Failed to update composition. id = " + composition.getId() + ". Exception = " + exUpdateCompositions.Message;
                    Logger.Log(msg);
                    throw new Exception(msg);
                }
            }

            //Update bases
            foreach (Base baseItem in iHotelPrice.mBases.Keys)
            {
                try
                {
                    decimal amount;
                    float percent;

                    if (iHotelPrice.mIsBaseAmount)
                    {
                        amount = baseItem.mAmount;
                        percent = 0;
                    }
                    else
                    {
                        amount = 0;
                        percent = baseItem.mPercent;
                    }

                    DAL_SQL_Helper.updateHotelPriceComboByType(hotelPriceId, baseItem.getId(), iHotelPrice.mBases[baseItem], amount, percent , 0, eComboType.Base);
                }
                catch (Exception exUpdateBases)
                {
                    isSaveSucceded = false;
                    msg = "Failed to update base. id = " + baseItem.getId() + ". Exception = " + exUpdateBases.Message;
                    Logger.Log(msg);
                    throw new Exception(msg);
                }
            }

            //Update room types
            foreach (RoomType roomType in iHotelPrice.mRoomTypes.Keys)
            {
                try
                {
                    DAL_SQL_Helper.updateHotelPriceComboByType(hotelPriceId, roomType.getId(), iHotelPrice.mRoomTypes[roomType], roomType.mAmount,0, 0, eComboType.RoomType);
                }
                catch (Exception exUpdateRoomTypes)
                {
                    isSaveSucceded = false;
                    msg = "Failed to update room types. id = " + roomType.getId() + ". Exception = " + exUpdateRoomTypes.Message;
                    Logger.Log(msg);
                    throw new Exception(msg);
                }
            }
        }

        ////Update all month days.
        //if (isSaveSucceded)
        //{
        //    foreach (PricePerDay datePrice in iHotelPrice.getPricePerDayList())
        //    {
        //        isSaveSucceded = saveDateDetails(iHotelPrice.mSupplierId, datePrice);
        //        if (!isSaveSucceded)
        //        {
        //            break;
        //        }
        //    }
        //}

        return isSaveSucceded;
    }

    public void createNewEmptyHotelPrice(string iSupplierId, string iClerkId)
    {
        try
        {
            DAL_SQL_Helper.createNewEmptyHotelPrice(iSupplierId, iClerkId);
        }
        catch(Exception ex)
        {
            
        }
    }

    public void SetMonthlyPricePerDay(string iSupplierId, List<PricePerDay> iMonthlyPricesPerDay)
    {
        try
        {
            foreach (PricePerDay datePrice in iMonthlyPricesPerDay)
            {
                saveDateDetails(iSupplierId, datePrice);
            }
        }
        catch (Exception ex)
        {
			Logger.Log("Exception in SetMonthlyPricePerDay. Message = " + ex.Message + ", Trace = " + ex.StackTrace);
        }
    }

    public void setMonthlySalesCycles(string iSupplierId, List<PricePerDay> iMonthlyPricesPerDay)
    {
        try
        {
            foreach (PricePerDay datePrice in iMonthlyPricesPerDay)
            {
                saveDateDetails(iSupplierId, datePrice);
            }
        }
        catch (Exception ex)
        {

        }
    }

    public void SetMonthlyAllocations(string iSupplierId, List<PricePerDay> iMonthlyPricesPerDay)
    {
        try
        {
            foreach (PricePerDay datePrice in iMonthlyPricesPerDay)
            {
                saveDateDetails(iSupplierId, datePrice);
            }
        }
        catch (Exception ex)
        {

        }
    }
}
