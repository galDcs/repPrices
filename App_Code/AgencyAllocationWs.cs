using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

public class AgencyAllocationWs
{
    public bool takeAllocationsFromDateToDate(string iSupplierId, int iRoomsAmount, DateTime iFromDate, DateTime iToDate)
    {
        bool hasEnoughRooms;

        try
        {
            hasEnoughRooms = DAL_SQL_Helper.takeAllocationsFromDateToDate(iSupplierId, iRoomsAmount, iFromDate, iToDate);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to take allocations. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;
    }

    public bool reduceAllocationsFromDateToDate(string iSupplierId, int iRoomsAmount, DateTime iFromDate, DateTime iToDate)
    {
        bool hasEnoughRooms;

        try
        {
            hasEnoughRooms = DAL_SQL_Helper.reduceAllocationsFromDateToDate(iSupplierId, iRoomsAmount, iFromDate, iToDate);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to reduce allocations. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;        
    }

    public bool hasEnoughRooms(string iSupplierId, int iRoomsAmount, string iFromDate, string iToDate)
    {
        bool hasEnoughRooms;

        try
        {
            hasEnoughRooms = DAL_SQL_Helper.hasEnoughRooms(iSupplierId, iRoomsAmount, iFromDate, iToDate);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to check allocations. exception = " + ex.Message);
            hasEnoughRooms = false;
        }

        return hasEnoughRooms;
    }
}
