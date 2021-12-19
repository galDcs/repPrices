using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MonthlyAllocation
/// </summary>
public class MonthlyAllocation
{
    public int mMonth { get; set; }
    public int mYear { get; set; }
    public int mMonthlyAllocation { get; set; }

    public MonthlyAllocation(int iMonth, int iYear, int iMonthlyAllocation)
	{
        mMonth = iMonth;
        mYear = iYear;
        mMonthlyAllocation = iMonthlyAllocation;
	}
}