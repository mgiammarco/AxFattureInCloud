using System;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic; 

public class DatiRisposta
{
    public string messaggio { get; set; }
    public string urlLink { get; set; }
    public string errore { get; set; }
    public DatiRisposta()
    {

    }
}

public class MyDbUtility
{
    public static string cString(string connectionString = "connectionString")
    {
        return ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
    }

    public static int nextID(string min, string campo, string tabella, string condizioni = "", string connectionString = "connectionString")
    {
        int valore = 0;
        string sql = "SELECT ISNULL(MAX(" + campo + ") + 1, " + min + ") AS [MAXID] FROM " + tabella + condizioni;
        scriviLog(sql);
        SqlConnection con = new SqlConnection(cString(connectionString));
        try
        {
            con.Open();
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        dr.Read();
                        valore = Convert.ToInt32(dr["MAXID"]);
                    }
                }
            }
        }
        finally
        {
            con.Close();
        }
        return valore;

    }

    private static Dictionary<string, string> UrlCache;
    private static void CaricaUrlCache() {
        string sql = "SELECT [url],[page] from vwMenu WHERE [LINGUA]='it-it'";
        UrlCache = new Dictionary<string, string>();
        SqlConnection con = new SqlConnection(cString("connectionString"));
        try
        {
            con.Open();
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            UrlCache.Add(dr["page"].ToString().ToLower(), dr["url"].ToString());
                        }
                    }
                }
            }
        }
        finally
        {
            con.Close();
        }
    }

    public static string LeggiUrl(string pagina)
    {
        if (UrlCache == null) CaricaUrlCache();

        string valore = "";

        if (UrlCache != null && (UrlCache.TryGetValue(pagina.ToLower(), out valore)))
        {
            return valore;
        }
        else
        {
            if (!pagina.StartsWith("/"))
            {
                pagina = "/" + pagina;
            }
            string sql = "SELECT [url] from vwMenu WHERE [LINGUA]='it-it' and page=@page";

            SqlConnection con = new SqlConnection(cString("connectionString"));
            try
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@page", pagina);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            dr.Read();
                            valore = dr["url"].ToString();
                        }
                    }
                }
            }
            finally
            {
                con.Close();
            }
            return valore;
        }
    }

    public static bool open(ref SqlConnection con, string connectionString = "connectionString")
    {
        bool riuscito = true;
        con = new SqlConnection(cString(connectionString));
        try
        {
            con.Open();
        }
        catch (Exception ex)
        {
            riuscito = false;
            //MyEmail es = new MyEmail();
            //es.toList.Add(ConfigurationManager.AppSettings["SUPPORTemail"]);
            //es.send(true, ConfigurationManager.AppSettings["INFOemail"], "", ConfigurationManager.AppSettings["titolo"],ex.StackTrace.ToString() +"<br/>"+ ex.Message);
            con.Close();
            // HttpContext.Current.Response.Write(ex.Message)
            // HttpContext.Current.Response.Redirect("/errore.aspx?errore=connessione")
        }
        return riuscito;
    }

    public static SqlDataReader read(SqlCommand cmd, string sql)
    {
        string esito = "OK | ";
        SqlDataReader dr;
        try
        {
            dr = cmd.ExecuteReader();
        }
        catch (Exception ex)
        {
            dr = null;
            esito = "KO | ";
            //MyEmail es = new MyEmail();
            //es.toList.Add(ConfigurationManager.AppSettings["SUPPORTemail"]);
            //es.send(true, ConfigurationManager.AppSettings["INFOemail"], "", ConfigurationManager.AppSettings["titolo"], ex.Message);
            // For Each p As SqlClient.SqlParameter In cmd.Parameters
            //     strSQL = Replace(strSQL, p.ParameterName, p.Value.ToString())
            // Next
            scriviLog((esito + (sql + ex.Message)));
        }
        return dr;
    }

    public static bool execute(SqlCommand cmd, string sql)
    {
        bool riuscito = true;
        string esito = "OK | ";
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            riuscito = false;
            esito = "KO | ";
            //MyEmail es = new MyEmail();
            //es.toList.Add("info@axterisco.it");// ConfigurationManager.AppSettings["SUPPORTemail"]);
            //es.send(true, ConfigurationManager.AppSettings["INFOemail"], "", ConfigurationManager.AppSettings["titolo"], ex.Message);
            sql += ex.Message;
        }
        finally
        {
            // For Each p As SqlClient.SqlParameter In cmd.Parameters
            // strSQL = Replace(strSQL, p.ParameterName, p.Value.ToString())
            // Next
            scriviLog((esito + sql));
        }
        return riuscito;
    }

    public static bool scriviLog(string sql)
    {
        try
        {
            string percorso = System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["PercorsoLogs"]);
            if (!Directory.Exists(percorso))
            {
                Directory.CreateDirectory(percorso);
            }
            string path = percorso + "/" + (DateTime.Now.ToString("yyyyMMdd") + ".txt");

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            using (TextWriter tw = new StreamWriter(path, true))
            {
                tw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + (" | " + sql));
                tw.Close();
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
        }
        return true;
    }

    
}