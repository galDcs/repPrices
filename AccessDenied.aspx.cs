using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

public partial class AccessDenied : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        Message.Text = Utils.getTextFromFile("NoPermission", (eLanguage)1255) +  " - " + Request.QueryString["resourceId"];
        messageContainer.Visible = true;
        messageContainer.Style.Add("color", "red");
    }
}
