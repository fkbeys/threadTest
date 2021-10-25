using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace threadTest
{
    public class DirekbaglanHedef
    {
        public   string baglantiCumlesi = "";
        public   DataTable dt;
        public   SqlConnection baglantiKur;
        public   SqlDataAdapter veriAl;
        public   SqlCommand komutGonder;


        public   string serverim = "10.10.10.59";
        public   string databaseim = "MikroDB_V16_KontaktHome2021";
        public   string user_im = "kaya";
        public   string password_im = "18821882";

        //public   string serverim = "10.10.10.144"; 
        //public   string databaseim = "MikroDB_V16_KontaktHome2021-Ali";
        //public   string user_im = "kaya";
        //public   string password_im = "18821882";

        public   bool Baglan()
        {
            try
            {
                baglantiCumlesi = "Data Source=" + serverim + ";Initial Catalog=" + databaseim + "; Persist Security Info=True; User ID=" + user_im + "; Password=" + password_im + "";

                // baglantiCumlesi = "Data Source=.;Initial Catalog=MikroDB_V16_KontaktHome2021;Trusted_Connection=True;";

                baglantiKur = new SqlConnection(baglantiCumlesi);

                return true;
            }
            catch (Exception ex)
            {
                baglantiCumlesi = "";
                //1  MessageBox.Show(" Baglantı ayarları geçerli değil" + ex.Message + "_" + baglantiCumlesi);
                return false;
            }
        }

        public   void DBislemBaslat()
        {
            baglantiKur.Open();
        }

        public   void BaglantiKopar(int i)
        {
            if (i == 0)
            {
                baglantiKur.Close();
                komutGonder.Dispose();
            }
            else if (i == 1)
            {
                baglantiKur.Close();
                veriAl.Dispose();
                dt.Dispose();
            }
        }

        public   int DBIslem(string sorgu)
        {
            try
            {
                Baglan();
                DBislemBaslat();
                komutGonder = new SqlCommand();
                komutGonder.Connection = baglantiKur;
                komutGonder.CommandText = sorgu;

                return komutGonder.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Log.Error(sorgu + "/" + ex.Message);
                return -1;
            }
            finally
            {
                BaglantiKopar(0);
            }
        }

        public int cmd(string sqlcumle)
        {
            Baglan();
            DBislemBaslat();

            SqlCommand sorgu = new SqlCommand(sqlcumle, baglantiKur);
            int sonuc = 0;

            try
            {
                sonuc = sorgu.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                //1 MessageBox.Show("işlemde hata oluştu\n" + ex.Message);
                // ProgParam.dosyayaz("Veri tabanı işleminde\n" + "sorgu= " + sorgu + "\n hata mesajı= " + ex.Message, "");
                return -1;
            }
            finally
            {

                BaglantiKopar(0);
            }
            return (sonuc);
        }



        public   DataTable DBListele(string sorgu)
        {
            try
            {
                Baglan();
                DBislemBaslat();
                dt = new DataTable();
                veriAl = new SqlDataAdapter(sorgu, baglantiKur);
                veriAl.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                //1 MessageBox.Show("Listeleme işleminde hata oluştu\n" + "sorgu= " + sorgu + "\n hata mesajı= " + ex.Message);
                //  ProgParam.dosyayaz(ex.Message + "_" + baglantiCumlesi, "");
                return null;
            }
            finally
            {
                BaglantiKopar(1);
            }
        }




        public   int DBVeriAlX(DataTable dtremote, string sorgu)
        {
            dt = new DataTable();
            try
            {
                Baglan();
                DBislemBaslat();
                veriAl = new SqlDataAdapter(sorgu, baglantiKur);
                veriAl.Fill(dt);
                SqlCommandBuilder cbCE = new SqlCommandBuilder(veriAl);
                for (int i = 0; i < dtremote.Rows.Count; i++)
                {
                    dt.ImportRow(dtremote.Rows[i]);
                    dt.Rows[dt.Rows.Count - 1].SetAdded();
                    veriAl.Update(dt);
                    dt.Clear();
                }
                return 0;
            }
            catch (Exception ex)
            {
                //1  MessageBox.Show(ex.Message);
                return -1;
            }
            finally
            {
                BaglantiKopar(1);
            }
        }

        public   int DBinsertrow(DataTable dtremote, string sorgu, string evrakSeriSira)
        {
            dt = new DataTable();
            try
            {
                Baglan();
                DBislemBaslat();
                veriAl = new SqlDataAdapter(sorgu, baglantiKur);
                veriAl.SelectCommand.CommandTimeout = 10000;
                veriAl.Fill(dt);
                SqlCommandBuilder cbCE = new SqlCommandBuilder(veriAl);

                for (int i = 0; i < dtremote.Rows.Count; i++)
                {
                    dt.ImportRow(dtremote.Rows[i]);
                    // dt.Rows[dt.Rows.Count - 1].SetAdded();
                    veriAl.Update(dt);
                    dt.Clear();
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.Message);
                Log.Error($"DBinsertrow Evrak:{evrakSeriSira} - {sorgu} - {ex.Message} ");
                return -1;
            }
            finally
            {
                BaglantiKopar(1);
            }
        }
        public   int DBVeriUPDATE(DataTable dtremote, string sorgu)
        {
            dt = new DataTable();
            try
            {
                Baglan();
                DBislemBaslat();
                SqlDataAdapter veriAl = new SqlDataAdapter(sorgu, baglantiKur);
                veriAl.Fill(dt);
                SqlCommandBuilder cbCE = new SqlCommandBuilder(veriAl);
                for (int i = 0; i < dtremote.Columns.Count; i++)
                    dt.Rows[0][dtremote.Columns[i].ColumnName] = dtremote.Rows[0][i];
                veriAl.Update(dt);
                return dtremote.Rows.Count;
            }
            catch (Exception ex)
            {
                //1   MessageBox.Show(ex.Message);
                return -1;
            }
            finally
            {
                BaglantiKopar(1);
            }
        }
    }
}
