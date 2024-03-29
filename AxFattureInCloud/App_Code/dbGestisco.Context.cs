﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Codice generato da un modello.
//
//     Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//     Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Linq;

public partial class EntitiesGestisco : DbContext
{
    public EntitiesGestisco()
        : base("name=EntitiesGestisco")
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }

    public virtual DbSet<TACTIVITY_TYPE> TACTIVITY_TYPE { get; set; }
    public virtual DbSet<TACTIVITY_TYPE_CATEGORY> TACTIVITY_TYPE_CATEGORY { get; set; }
    public virtual DbSet<TCLIENT> TCLIENT { get; set; }
    public virtual DbSet<TREGISTRY_PROVINCE> TREGISTRY_PROVINCE { get; set; }
    public virtual DbSet<TREGISTRY_STATE> TREGISTRY_STATE { get; set; }
    public virtual DbSet<TCLIENT_BILL> TCLIENT_BILL { get; set; }
    public virtual DbSet<TCLIENT_BILL_DETAIL> TCLIENT_BILL_DETAIL { get; set; }
    public virtual DbSet<TORDER_ACTIVITY> TORDER_ACTIVITY { get; set; }
    public virtual DbSet<TORDER_ACTIVITY_BILLING> TORDER_ACTIVITY_BILLING { get; set; }
    public virtual DbSet<TORDER_ACTIVITY_BILLING_STEP> TORDER_ACTIVITY_BILLING_STEP { get; set; }
    public virtual DbSet<TCLIENT_DOMAIN_SERVICE> TCLIENT_DOMAIN_SERVICE { get; set; }
    public virtual DbSet<TCLIENT_DOMAIN_SERVICE_PLANNING> TCLIENT_DOMAIN_SERVICE_PLANNING { get; set; }
    public virtual DbSet<TCLIENT_DOMAIN_SERVICE_TYPE> TCLIENT_DOMAIN_SERVICE_TYPE { get; set; }
    public virtual DbSet<TCLIENT_ACCOUNTING> TCLIENT_ACCOUNTING { get; set; }
    public virtual DbSet<TCOURSE_EDITION_MEMBER> TCOURSE_EDITION_MEMBER { get; set; }
    public virtual DbSet<TCOURSE_MEMBER> TCOURSE_MEMBER { get; set; }
    public virtual DbSet<TCLIENT_DATI_FATTURAZIONE> TCLIENT_DATI_FATTURAZIONE { get; set; }

    [DbFunction("EntitiesGestisco", "udfTCLIENT_BILL_YM")]
    public virtual IQueryable<udfTCLIENT_BILL_YM_Result> udfTCLIENT_BILL_YM(Nullable<int> iCompany, Nullable<int> iClient, Nullable<int> iDBILL, Nullable<int> iMonth, Nullable<int> iYear, Nullable<int> iIva)
    {
        var iCompanyParameter = iCompany.HasValue ?
            new ObjectParameter("iCompany", iCompany) :
            new ObjectParameter("iCompany", typeof(int));

        var iClientParameter = iClient.HasValue ?
            new ObjectParameter("iClient", iClient) :
            new ObjectParameter("iClient", typeof(int));

        var iDBILLParameter = iDBILL.HasValue ?
            new ObjectParameter("IDBILL", iDBILL) :
            new ObjectParameter("IDBILL", typeof(int));

        var iMonthParameter = iMonth.HasValue ?
            new ObjectParameter("iMonth", iMonth) :
            new ObjectParameter("iMonth", typeof(int));

        var iYearParameter = iYear.HasValue ?
            new ObjectParameter("iYear", iYear) :
            new ObjectParameter("iYear", typeof(int));

        var iIvaParameter = iIva.HasValue ?
            new ObjectParameter("iIva", iIva) :
            new ObjectParameter("iIva", typeof(int));

        return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<udfTCLIENT_BILL_YM_Result>("[EntitiesGestisco].[udfTCLIENT_BILL_YM](@iCompany, @iClient, @IDBILL, @iMonth, @iYear, @iIva)", iCompanyParameter, iClientParameter, iDBILLParameter, iMonthParameter, iYearParameter, iIvaParameter);
    }

    [DbFunction("EntitiesGestisco", "udfTcourseEditionBilling")]
    public virtual IQueryable<udfTcourseEditionBilling_Result> udfTcourseEditionBilling(string billingList)
    {
        var billingListParameter = billingList != null ?
            new ObjectParameter("BillingList", billingList) :
            new ObjectParameter("BillingList", typeof(string));

        return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<udfTcourseEditionBilling_Result>("[EntitiesGestisco].[udfTcourseEditionBilling](@BillingList)", billingListParameter);
    }
}
