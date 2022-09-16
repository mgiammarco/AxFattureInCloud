using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public partial class TCLIENT {
    public TCLIENT_ACCOUNTING CLIENT_ACCOUNTING(int idCorporate) {
        return this.TCLIENT_ACCOUNTING.Where(w => w.IDACCOUNTING_COMPANY == idCorporate).FirstOrDefault();
    }
}

public partial class udfTcourseEditionBilling_Result {
    public int BILL_DATE_YEAR { get {
            int year = 0;
            if (int.TryParse(this.BILL_DATE.Substring(6), out year)) { return year; }
            else { return 0; }

        } }
}