using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class CreateDocketResult
{
    public string mDocketId {get; set;}
    public string mVoucherId {get; set;}
    public bool mStatus {get; set;}
    public string Message;
    
    public string mMessage
    {
        get
        {
            return Message;
        }
        set
        {
            mStatus = true;
            Message = value;
        }
    }

	public CreateDocketResult()
	{
        mDocketId = string.Empty;
        mVoucherId = string.Empty;
        mStatus = false;
        mMessage = string.Empty;
	}


}