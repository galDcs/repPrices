using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Data.SqlClient;
using System.Collections.Generic;

public class DAL_SQL_Helper
{
    public static DataSet GetAllHotels(string lang)
    {
        DataSet ds = new DataSet();
        string serviceType = "2";//Hotels

        try
        {
            ds = DAL_SQL.RunSqlDataSet(string.Format(@" SELECT	S.id, S.name_1255, S.name_1252, S.name_1251
														FROM      dbo.SUPPLIER_DETAILS SD INNER JOIN
																  dbo.SUPPLIERS_to_PRODUCT_TYPES S_T_P ON SD.supplier_id = S_T_P.supplier_id INNER JOIN
																  AGENCY_ADMIN.dbo.SUPPLIERS S INNER JOIN
																  AGENCY_ADMIN.dbo.AREAS A ON S.area_id = A.id ON SD.supplier_id = S.id
																  left outer join Agency_Admin.dbo.HOTELS_NETWORKS HN on HN.id=SD.network_id
														WHERE
														(S_T_P.product_type = {0}) AND (SD.isActive = 1) 
														order by S.name_{1}", serviceType, lang));
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
    }
	
	public static DataSet GetAllHotelsByArea(string iGeneralAreaId, string lang)
    {
        DataSet ds = new DataSet();
        string serviceType = "2";//Hotels

        try
        {
            ds = DAL_SQL.RunSqlDataSet(string.Format(@" SELECT	S.id, S.name_1255, S.name_1252, S.name_1251
														FROM      dbo.SUPPLIER_DETAILS SD INNER JOIN
																  dbo.SUPPLIERS_to_PRODUCT_TYPES S_T_P ON SD.supplier_id = S_T_P.supplier_id INNER JOIN
																  AGENCY_ADMIN.dbo.SUPPLIERS S INNER JOIN
																  AGENCY_ADMIN.dbo.AREAS A ON S.area_id = A.id ON SD.supplier_id = S.id
																  left outer join Agency_Admin.dbo.HOTELS_NETWORKS HN on HN.id=SD.network_id
														WHERE
														(S_T_P.product_type = {0}) AND (SD.isActive = 1) AND A.general_area_id = {2}
														order by S.name_{1} ", serviceType, lang, iGeneralAreaId));
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
    }

    public static DataSet searchHotels(string stringToSearch, string lang)
    {
        string serviceType = "2";//Hotels
        string query = string.Format(@" SELECT	S.id as hotel_code, S.name_{0} as name, S.carrier, S.description, 
				                        A.description AS area_description,
                                        A.id as areaId, 
				                        ISNULL(SD.account_code, - 1) AS account_code, 
				                        ISNULL(S.email, 'No mail') AS email, 
				                        ISNULL(SD.commission_percentage, 0) AS commission_percentage, 
				                        ISNULL(SD.tax, 0) AS tax, ISNULL(SD.isActive, - 1) AS isActive, 
                                        ISNULL(ICT.name, 'No name') AS income_type_name 

				                        FROM		dbo.INCOME_TYPES ICT INNER JOIN 
                                        			dbo.SUPPLIER_DETAILS SD ON ICT.id = SD.income_type_id and (SD.isActive = 1) INNER JOIN 
                                                    dbo.SUPPLIERS_to_PRODUCT_TYPES S_T_P ON SD.supplier_id = S_T_P.supplier_id RIGHT OUTER JOIN 
                                        			AGENCY_ADMIN.dbo.SUPPLIERS S INNER JOIN 
                                        			AGENCY_ADMIN.dbo.AREAS A ON S.area_id = A.id ON SD.supplier_id = S.id 

 				                        WHERE     (1 = 1)  AND S.name_{0} LIKE N'%{1}%' and S_T_P.product_type = {2} ", lang, stringToSearch, serviceType);

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

        return ds;
    }

    //Hotel Prices
    public static void updatePricePerDay(string iHotelPriceId, DateTime iDate, bool iStatus, decimal iBasePrice, decimal iBasePriceNetto, int iDaysAllocated, int iDaysDisable, string iColor)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "UpdatePricePerDay",
                SqlDalParam.formatParam("@HotelPriceId", SqlDbType.Int, iHotelPriceId),
                SqlDalParam.formatParam("@Date", SqlDbType.Date, iDate),
                SqlDalParam.formatParam("@Status", SqlDbType.Bit, iStatus),
                SqlDalParam.formatParam("@BasePrice", SqlDbType.Decimal, iBasePrice),
                SqlDalParam.formatParam("@BasePriceNetto", SqlDbType.Decimal, iBasePriceNetto),
                SqlDalParam.formatParam("@DaysAllocated", SqlDbType.Int, iDaysAllocated),
                SqlDalParam.formatParam("@DaysDisable", SqlDbType.Int, iDaysDisable),
                SqlDalParam.formatParam("@Color", SqlDbType.NVarChar, iColor)
                );
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to update date prices. exception = " + ex.Message);
        }
    }

    public static void insertPricePerDay(string iHotelPriceId, DateTime iDate, bool iStatus, decimal iBasePrice, decimal iBasePriceNetto, int iDaysAllocated, int iDaysDisable, string iColor)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "InsertPricePerDay",
                SqlDalParam.formatParam("@HotelPriceId", SqlDbType.Int, iHotelPriceId),
                SqlDalParam.formatParam("@Date", SqlDbType.Date, iDate),
                SqlDalParam.formatParam("@Status", SqlDbType.Bit, iStatus),
                SqlDalParam.formatParam("@BasePrice", SqlDbType.Decimal, iBasePrice),
                SqlDalParam.formatParam("@BasePriceNetto", SqlDbType.Decimal, iBasePriceNetto),
                SqlDalParam.formatParam("@DaysAllocated", SqlDbType.Int, iDaysAllocated),
                SqlDalParam.formatParam("@DaysDisable", SqlDbType.Int, iDaysDisable),
                SqlDalParam.formatParam("@Color", SqlDbType.NVarChar, iColor)
                );
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to insert date prices. exception = " + ex.Message);
        }
    }

    public static void updateHotelPriceGeneralDetails(string iSupplierId, string iPriceType, bool iStatus, string iClerkId, string iPriceColor, string iCurrency,
                                                      string iBaseCompositionId, string iBaseBaseId, string iBaseRoomTypeId, bool iIsBaseAmount,
                                                      float iOfficeDiscount, float iSiteDiscount, float iCommission, float iGeneralAdditive, float iBabiesAdditive)
    {
        DataSet ds = new DataSet();
Logger.Log("dal_helper iSupplierId - " + iSupplierId);		
Logger.Log("dal_helper iBaseRoomTypeId - " + iBaseRoomTypeId);
Logger.Log("dal_helper DAL_SQL.ConnStr - " + DAL_SQL.ConnStr);
        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "UpdateHotelPriceGeneralDetails",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@PriceType", SqlDbType.Int, iPriceType),
                SqlDalParam.formatParam("@Status", SqlDbType.Bit, iStatus),
                SqlDalParam.formatParam("@Date", SqlDbType.NVarChar, DateTime.Now.Date.ToString("dd-MMM-yy")),
                SqlDalParam.formatParam("@ClerkId", SqlDbType.Int, iClerkId),
                SqlDalParam.formatParam("@PriceColor", SqlDbType.Int, iPriceColor),
                SqlDalParam.formatParam("@Currency", SqlDbType.Int, iCurrency),
                SqlDalParam.formatParam("@BaseCompositionId", SqlDbType.Int, iBaseCompositionId),
                SqlDalParam.formatParam("@BaseBaseId", SqlDbType.Int, iBaseBaseId),
                SqlDalParam.formatParam("@BaseRoomTypeId", SqlDbType.Int, iBaseRoomTypeId),
                SqlDalParam.formatParam("@IsBaseAmount", SqlDbType.Bit, iIsBaseAmount),
                SqlDalParam.formatParam("@OfficeDiscount", SqlDbType.Decimal, iOfficeDiscount),
                SqlDalParam.formatParam("@SiteDiscount", SqlDbType.Decimal, iSiteDiscount),
                SqlDalParam.formatParam("@Commission", SqlDbType.Decimal, iCommission),
                SqlDalParam.formatParam("@GeneralAdditive", SqlDbType.Decimal, iGeneralAdditive),
                SqlDalParam.formatParam("@BabiesAdditive", SqlDbType.Decimal, iBabiesAdditive)
                );
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to update date prices. exception = " + ex.Message);
            throw new Exception("Failed to update date prices. exception = " + ex.Message);
        }
    }
    
    public static void updateHotelPriceComboByType(string iHotelPriceId, string iComboTypeId, bool iStatus, decimal iAmount, float iPercent, float iNettoPercent, eComboType iCombpType)
    {
        string selectQuery = string.Empty;
        string updateQuery = string.Empty;
        string insertQuery = string.Empty;

        switch (iCombpType)
        {
            case eComboType.Composition:
                selectQuery = string.Format("SELECT * FROM P_HOTEL_PRICE_TO_COMPOSITIONS WHERE hotel_price_id = {0} and composition_id = {1}", iHotelPriceId, iComboTypeId);
                updateQuery = string.Format("UPDATE P_HOTEL_PRICE_TO_COMPOSITIONS SET status = '{0}', [percent] = {1}, [percent_netto] = {4} WHERE hotel_price_id = {2} and composition_id = {3}", iStatus, iPercent, iHotelPriceId, iComboTypeId, iNettoPercent);
                insertQuery = string.Format("INSERT INTO P_HOTEL_PRICE_TO_COMPOSITIONS (hotel_price_id, status, composition_id, [percent],  [percent_netto]) VALUES({0}, '{1}', {2}, {3}, {4})", iHotelPriceId, iStatus, iComboTypeId, iPercent, iNettoPercent);
                break;
            case eComboType.Base:
                selectQuery = string.Format("SELECT * FROM P_HOTEL_PRICE_TO_BASES WHERE hotel_price_id = {0} and base_id = {1}", iHotelPriceId, iComboTypeId);
                updateQuery = string.Format("UPDATE P_HOTEL_PRICE_TO_BASES SET status = '{0}', amount = {1}, [percent] = {4} WHERE hotel_price_id = {2} and base_id = {3}", iStatus, iAmount, iHotelPriceId, iComboTypeId, iPercent);
                insertQuery = string.Format("INSERT INTO P_HOTEL_PRICE_TO_BASES (hotel_price_id, status, base_id, [amount], [percent]) VALUES({0}, '{1}', {2}, {3}, {4})", iHotelPriceId, iStatus, iComboTypeId, iAmount, iPercent);
                break;
            case eComboType.RoomType:
                selectQuery = string.Format("SELECT * FROM P_HOTEL_PRICE_TO_ROOM_TYPES WHERE hotel_price_id = {0} and room_type_id = {1}", iHotelPriceId, iComboTypeId);
                updateQuery = string.Format("UPDATE P_HOTEL_PRICE_TO_ROOM_TYPES SET status = '{0}', amount = {1} WHERE hotel_price_id = {2} and room_type_id = {3}", iStatus, iAmount, iHotelPriceId, iComboTypeId);
                insertQuery = string.Format("INSERT INTO P_HOTEL_PRICE_TO_ROOM_TYPES (hotel_price_id, status, room_type_id, [amount]) VALUES({0}, '{1}', {2}, {3})", iHotelPriceId, iStatus, iComboTypeId, iAmount);
                break;
        }

        DataSet ds = DAL_SQL.RunSqlDataSet(selectQuery);
        bool isUpdate = Utils.isDataSetRowsNotEmpty(ds);
        string query = string.Empty;

        if (Utils.isDataSetRowsNotEmpty(ds))
        {
            query = updateQuery;
        }
        else
        {
            query = insertQuery;
        }

        try
        {
            DAL_SQL.RunSql(query);
        }
        catch (Exception ex)
        {
            string msg = "Failed to " + query + ". Exception = " + ex.Message;
            Logger.Log(msg);
            throw new Exception(msg);
        }
    }

    //If there a cycle day in the DB that is no longer relevant will disable it.
    public static void updateDatePriceSaleCycle(string iPricePerDayId, int iSaleCycle, bool iStatus)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "UpdatePricePerDaySaleCycle",
                SqlDalParam.formatParam("@PricePerDayId", SqlDbType.Int, iPricePerDayId),
                SqlDalParam.formatParam("@CycleDay", SqlDbType.Int, iSaleCycle),
                SqlDalParam.formatParam("@Status", SqlDbType.Bit, iStatus)
                );
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to insert date prices. exception = " + ex.Message);
        }
    }

    public static void insertDatePriceSaleCycle(string iPricePerDayId, int iSaleCycle)
    {
        DataSet ds = new DataSet();
        bool status = true;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "InsertPricePerDaySaleCycle",
                SqlDalParam.formatParam("@PricePerDayId", SqlDbType.Int, iPricePerDayId),
                SqlDalParam.formatParam("@CycleDay", SqlDbType.Int, iSaleCycle),
                SqlDalParam.formatParam("@Status", SqlDbType.Bit, status)
                );
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to insert date prices. exception = " + ex.Message);
        }
    }

    public static bool IsCycleDayExist(string iPricePerDayId, int iSaleCycle)
    {
        bool IsCycleDayExist = false;
        DataSet ds = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_SALES_CYCLES WHERE price_per_day_id = " + iPricePerDayId + " and cycle_days = " + iSaleCycle);

        if (Utils.isDataSetRowsNotEmpty(ds))
        {
            IsCycleDayExist = true;
        }

        return IsCycleDayExist;
    }

    public static bool createNewEmptyHotelPrice(string iSupplierId, string iClerkId)
    {
        bool isSucceeded = true;

        try
        {
            DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "CreateNewEmptyHotelPrice",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@ClerkId", SqlDbType.Int, iClerkId)
                );
        }
        catch (Exception ex)
        {
            isSucceeded = false;
            Logger.Log(ex.Message);
            throw new Exception("Failed to insert date prices. exception = " + ex.Message);
        }

        return isSucceeded;
    }

    //Search hotel code
    public static DataSet GetGeneralAreas(string makat1, string makat2)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetGeneralAreas",
                SqlDalParam.formatParam("@Makat1", SqlDbType.NVarChar, 12, makat1),
                SqlDalParam.formatParam("@Makat2", SqlDbType.NVarChar, 12, makat2)
            );
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to get general areas"  + ex.Message);
        }

        return ds;
    }

    public static DataSet GetHotelsByArea(string generalAreaId, string lang)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.RunSqlDataSet(string.Format(@" DECLARE @GeneralAreaId int = {0}

                                                        select	A.id area_id,   
		                                                        A.description area_name,     
		                                                        S.id,   
		                                                        S.name_{1}
		
                                                        FROM    Agency_Admin.dbo.AREAS A INNER JOIN  
		                                                        Agency_Admin.dbo.SUPPLIERS S on A.id = S.area_id 
		
                                                        where	S.system_type = 3
		                                                        and A.id in (select GATA.area_id   
					                                                        from	Agency_Admin.dbo.GENARAL_AREAS GA inner join  
							                                                        Agency_Admin.dbo.GENERAL_AREAS_TO_AREAS GATA ON GATA.general_area_id = GA.id  
					                                                        where general_area_id=@GeneralAreaId)
                                                                and S.id in (SELECT supplier_id FROM P_HOTEL_PRICES WHERE status = 'True');", generalAreaId, lang));
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
    }

    public static DataSet getUserNameAndPassByClerkId(string iClerkId)
    {
        DataSet ds = null;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GetUserNameAndPasswordByClerkId",
                SqlDalParam.formatParam("@ClerkId", SqlDbType.Int, iClerkId)
                );
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to get usernameand pass. exception = " + ex.Message);
        }

        return ds;
    }

    internal static bool hasEnoughRooms(string iSupplierId, int iRoomsAmount, string iFromDate, string iToDate)
    {
        DataSet ds = null;
        bool hasEnoughRooms = false;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@RoomsAmount", SqlDbType.Int, iRoomsAmount),
                SqlDalParam.formatParam("@FromDate", SqlDbType.Date, iFromDate),
                SqlDalParam.formatParam("@ToDate", SqlDbType.Date, iToDate)
                );

            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                hasEnoughRooms = bool.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to get usernameand pass. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;
    }

    public static bool takeAllocationsFromDateToDate(string iSupplierId, int iRoomsAmount, DateTime iFromDate, DateTime iToDate)
    {
        DataSet ds = null;
        bool hasEnoughRooms = false;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "takeAllocationsFromDateToDate",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@RoomsAmount", SqlDbType.Int, iRoomsAmount),
                SqlDalParam.formatParam("@FromDate", SqlDbType.Date, iFromDate),
                SqlDalParam.formatParam("@ToDate", SqlDbType.Date, iToDate)
                );

            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                hasEnoughRooms = bool.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to take allocations. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;
    }

    public static bool reduceAllocationsFromDateToDate(string iSupplierId, int iRoomsAmount, DateTime iFromDate, DateTime iToDate)
    {
        DataSet ds = null;
        bool hasEnoughRooms = false;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "reduceAllocationsFromDateToDate",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@RoomsAmount", SqlDbType.Int, iRoomsAmount),
                SqlDalParam.formatParam("@FromDate", SqlDbType.Date, iFromDate),
                SqlDalParam.formatParam("@ToDate", SqlDbType.Date, iToDate)
                );

            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                hasEnoughRooms = bool.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to reduce allocations. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;
    }

    public static bool takeAllocations(string iSupplierId, int iRoomsAmount, DateTime iDate)
    {
        bool isSuccedded = true;
        DataSet ds;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "addAllocations",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@RoomsAmount", SqlDbType.Int, iRoomsAmount),
                SqlDalParam.formatParam("@Date", SqlDbType.NVarChar, iDate)
                );
            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                isSuccedded = bool.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
        }
        catch (Exception ex)
        {
            isSuccedded = false;
            throw new Exception("Failed update allocations. Ex = " + ex.Message);
        }

        if (!isSuccedded)
        {
            throw new Exception("Not enought rooms");
        }

        return isSuccedded;
    }

    public static bool reduceAllocations(string iSupplierId, int iRoomsAmount, DateTime iDate)
    {
        bool isSuccedded = true;
        DataSet ds;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "reduceAllocations",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@RoomsAmount", SqlDbType.Int, iRoomsAmount),
                SqlDalParam.formatParam("@Date", SqlDbType.NVarChar, iDate)
                );
            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                isSuccedded = bool.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
        }
        catch (Exception ex)
        {
            isSuccedded = false;
            throw new Exception("Failed update allocations. Ex = " + ex.Message);
        }

        if (!isSuccedded)
        {
            throw new Exception("Cannot reduce rooms. under 0.");
        }

        return isSuccedded;
    }


    internal static void SaveMonthlyAllocation(string iSupplierId, int iMonth, int iYear, string iMonthlyAllocation)
    {
        try
        {
            DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "saveMonthlyAllocation",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@Year", SqlDbType.Int, iYear),
                SqlDalParam.formatParam("@Month", SqlDbType.Int, iMonth),
                SqlDalParam.formatParam("@MonthlyAllocation", SqlDbType.Int, iMonthlyAllocation)
                );
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }
    }

    public static DataSet getMonthlyUsedAllocation(string iSupplierId, int iMonth, int iYear)
    {
        DataSet ds = null;

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "getMonthlyUsedAllocation",
                SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@Month", SqlDbType.Int, iMonth),
                SqlDalParam.formatParam("@Year", SqlDbType.Int, iYear)
                );
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }

    public static string getCompositionId(string iAdultsAmount, string iKidsAmount, string iInfantsAmount)
    {
        DataSet ds = null;
        string compositionId = "0";

        try
        {

            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "Agency_Admin.dbo.GetCompositionId",
                SqlDalParam.formatParam("@Adults", SqlDbType.Int, iAdultsAmount),
                SqlDalParam.formatParam("@Kids", SqlDbType.Int, iKidsAmount),
                SqlDalParam.formatParam("@Infants", SqlDbType.Int, iInfantsAmount)
                );

            if (Utils.isDataSetRowsNotEmpty(ds))
            {
                compositionId = ds.Tables[0].Rows[0]["id"].ToString();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get compositionId. Ex = " + ex.Message);
        }

        return compositionId;
    }

    public static Dictionary<string,string> getBasesNames(string iSupplierId)
    {
        DataSet ds = new DataSet();
        Dictionary<string, string> retVal = new Dictionary<string, string>();
        string hotelPriceID = Utils.getHotelPriceId(iSupplierId);
        string query = string.Empty;

        try
        {
            query = @"
                    SELECT HOB.id, HOB.name
                    FROM Agency_Admin.dbo.HOTEL_ON_BASE HOB
                    JOIN P_HOTEL_PRICE_TO_BASES PHPTB ON PHPTB.base_id = HOB.id
                    WHERE PHPTB.hotel_price_id = @hotel_price_id
                    ";
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
            SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, int.Parse(hotelPriceID))

            );

            if(Utils.isDataSetRowsNotEmpty(ds))
            {
                foreach(DataRow row in ds.Tables[0].Rows)
                {
                    string currName = row["name"].ToString();
                    string currid = row["id"].ToString();
                    retVal[currid] = currName;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to update date prices. exception = " + ex.Message);
        }
        return retVal;
    }
    public static string getBasesName(string iBasesId)
    {
        DataSet ds = new DataSet();
        string retVal = string.Empty;
        string query = string.Empty;

        try
        {

                query = @"
                    SELECT id,name
                    FROM Agency_Admin.dbo.HOTEL_ON_BASE
                    WHERE id = @id
                    ";
                ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                SqlDalParam.formatParam("@id", SqlDbType.Int, int.Parse(iBasesId))

                );

                string currName = ds.Tables[0].Rows[0]["name"].ToString();
                retVal = currName;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            throw new Exception("Failed to update date prices. exception = " + ex.Message);
        }
        return retVal;
    }

    public static DataSet getYearBaseByHotelId(int iHotelPriceId, DateTime iCurrDateTime, DateTime iNextDateTime)
    {
        DataSet ds = null;
        string query = string.Empty;

        try
        {
            query = @"
                    SELECT *
                    FROM P_HOTEL_BASES_TO_DATES
                    WHERE hotel_price_id = @hotel_price_id AND (date BETWEEN @curr_year AND @next_year) AND status = @status
                    ";
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                SqlDalParam.formatParam("@curr_year", SqlDbType.DateTime, iCurrDateTime),
                SqlDalParam.formatParam("@next_year", SqlDbType.DateTime, iNextDateTime),
                SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                );
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }
    public static DataSet getBaseByHotelIdAndDate(string iSupplierID, DateTime iCurrDateTime)
    {
        DataSet ds = null;
        string query = string.Empty;
        int hotelPriceId = int.Parse(Utils.getHotelPriceId(iSupplierID));
        try
        {
            query = @"
                    SELECT *
                    FROM P_HOTEL_BASES_TO_DATES
                    WHERE hotel_price_id = @hotel_price_id AND date = @curr_year AND status = @status
                    ";
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, hotelPriceId),
                SqlDalParam.formatParam("@curr_year", SqlDbType.DateTime, iCurrDateTime),
                SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                );
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }

    public static DataSet SaveAndUpdateDetails(int iHotelPriceId, int iBaseId, DateTime iFromDateTime, DateTime iToDateTime)
    {
        DataSet ds = null;
        string query = string.Empty;

        try
        {
            DateTime tempDateTime = iFromDateTime;
            while(tempDateTime <= iToDateTime)
            {
                query = @"
                        SELECT *
                        FROM P_HOTEL_BASES_TO_DATES
                        WHERE hotel_price_id = @hotel_price_id AND base_id = @base_id AND date = @curr_date AND status = @status
                        ";
                ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                    SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                    SqlDalParam.formatParam("@curr_date", SqlDbType.DateTime, tempDateTime),
                    SqlDalParam.formatParam("@base_id", SqlDbType.Int, iBaseId),
                    SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                    );

                if (Utils.isDataSetRowsNotEmpty(ds))
                {
                    query = @"
                            UPDATE P_HOTEL_BASES_TO_DATES
                            SET base_id = @base_id 
                            WHERE hotel_price_id = @hotel_price_id AND date = @curr_date AND status = @status;
                        ";
                    ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                        SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                        SqlDalParam.formatParam("@base_id", SqlDbType.Int, iBaseId),
                        SqlDalParam.formatParam("@curr_date", SqlDbType.DateTime, tempDateTime),
                        SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                        );
                }
                else
                {
                    query = @"
                            INSERT INTO P_HOTEL_BASES_TO_DATES(hotel_price_id, base_id, date, status)
                            VALUES (@hotel_price_id, @base, @curr_date, @status);
                        ";
                    ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                        SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                        SqlDalParam.formatParam("@base", SqlDbType.Int, iBaseId),
                        SqlDalParam.formatParam("@curr_date", SqlDbType.DateTime, tempDateTime),
                        SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                        );
                }

                tempDateTime = tempDateTime.AddDays(1);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }
    public static DataSet DelteDetails(int iHotelPriceId, int iBaseId, DateTime iFromDateTime, DateTime iToDateTime)
    {
        DataSet ds = null;
        string query = string.Empty;

        try
        {
            DateTime tempDateTime = iFromDateTime;
            while (tempDateTime <= iToDateTime)
            {
                query = @"
                        SELECT *
                        FROM P_HOTEL_BASES_TO_DATES
                        WHERE hotel_price_id = @hotel_price_id AND base_id = @base_id AND date = @curr_date AND status = @status
                        ";
                ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                    SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                    SqlDalParam.formatParam("@curr_date", SqlDbType.DateTime, tempDateTime),
                    SqlDalParam.formatParam("@base_id", SqlDbType.Int, iBaseId),
                    SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                    );

                if (Utils.isDataSetRowsNotEmpty(ds))
                {
                    query = @"
                            UPDATE P_HOTEL_BASES_TO_DATES
                            SET status = @status
                            WHERE hotel_price_id = @hotel_price_id AND date = @curr_date AND base_id = @base_id ;
                        ";
                    ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                        SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, iHotelPriceId),
                        SqlDalParam.formatParam("@base_id", SqlDbType.Int, iBaseId),
                        SqlDalParam.formatParam("@curr_date", SqlDbType.DateTime, tempDateTime),
                        SqlDalParam.formatParam("@status", SqlDbType.Bit, false)
                        );
                }

                tempDateTime = tempDateTime.AddDays(1);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }


}
