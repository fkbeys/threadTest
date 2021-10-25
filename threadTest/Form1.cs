using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace threadTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int BorcAlacakSil(string carikod)
        {
            try
            {
                var direkbaglan = new DirekbaglanHedef();
                string query = $"DELETE FROM CARI_HAREKET_BORC_ALACAK_ESLEME WHERE chk_ChKodu='{carikod}'";
                return direkbaglan.DBIslem(query);

            }
            catch (Exception ex)
            {
                string innerexmessage = "";
                if (ex.InnerException != null)
                {
                    innerexmessage = ex.Message;
                }
                Log.Error("Class:BorcAlacakSil-CariKod:" + carikod + "-Message:" + ex.Message + "-Innerex.:" + innerexmessage);
                return -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dursunmu = false;
            metroProgressSpinner1.Visible = true;
            metroProgressSpinner1.Spinning = true;

            panel1.Enabled = false;

            var query = "select  sira=1,cha_kod=odenisCariKod  from AAAA_ODENIS_HAVUZU  WHERE ondenisAktarildi=1  order by odenisCariKod ";
            borcAlacakEslesmeBaslat(query, "Sira-1"); 
        }

        public static int toplamSayi = 0;
        public static int yapilanSayi = 0;
        private void borcAlacakEslesmeBaslat(string cariListQuery, string sira)
        {
            DirekbaglanHedef Direk = new DirekbaglanHedef();

            var dt = Direk.DBListele(cariListQuery);
            List<Task> TaskList = new List<Task>();

            toplamSayi = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var cha_kod = dt.Rows[i]["cha_kod"].ToString();
                var siraxxx = dt.Rows[i]["sira"].ToString();

                var LastTask = Task.Factory.StartNew(() => CariBorcAlacaqInsert(cha_kod)); 
                TaskList.Add(LastTask);
            }

            Task.WhenAll(TaskList.ToArray()).ContinueWith(x =>

            DurduUI()

            );

        }

        public void DurduUI()
        {
            Invoke(new Action(() =>
            {
                metroProgressSpinner1.Visible = false;
                metroProgressSpinner1.Spinning = false;
                panel1.Enabled = true;
            }));
           

        } 
        public void bilgiver(string mesaj)
        {
            Invoke(new Action(() =>
            {
                label1.Text = mesaj;
            }));
        }

        public void CariBorcAlacaqInsert(string carikod)
        {
            try
            {
                if (dursunmu)
                {
                    return;
                }
                 
                var direkbaglanBA = new DirekbaglanHedef();
                var cha_kod = carikod;
                BorcAlacakSil(carikod);

                var fakturayiBulQuery = "select cha_evrak_tip,cha_normal_Iade, cha_tarihi,(select cha_Guid from CARI_HESAP_HAREKETLERI WITH(NOLOCK) where" +
                    " cha_evrakno_seri=c1.cha_evrakno_seri and cha_evrakno_sira=c1.cha_evrakno_sira and cha_kod='" + cha_kod + "'" +
                    " and cha_evrak_tip=c1.cha_evrak_tip and cha_satir_no=0) as fakturaguid,cha_kod,cha_belge_no,Sum(cha_meblag) as" +
                    " cha_meblag,cha_evrakno_seri,cha_evrakno_sira from CARI_HESAP_HAREKETLERI c1 with(nolock)  where  cha_evrak_tip=63 and" +
                    " cha_kod='" + cha_kod + "' and cha_meblag>0 group by cha_evrak_tip,cha_normal_Iade, cha_evrakno_seri,cha_evrakno_sira," +
                    " cha_tarihi,cha_kod,cha_belge_no order by cha_tarihi";

                var FakturlarDT = direkbaglanBA.DBListele(fakturayiBulQuery);

                var medaxillerQuery = $" select  odeme_tarihi=cha_tarihi,odeme_guid=cha_Guid ,cha_belge_no ,  cha_kod,cha_evrakno_seri,cha_evrakno_sira,odenis_meblegi=cha_meblag,odenis_meblegi2=cha_meblag   from CARI_HESAP_HAREKETLERI   WITH(NOLOCK) where cha_kod='{cha_kod}'  and cha_tip =1 order by cha_tarihi ";
                var medaxillerDT = direkbaglanBA.DBListele(medaxillerQuery);

                var mexaricQuery = $" select  odeme_tarihi=cha_tarihi,odeme_guid=cha_Guid ,cha_belge_no ,  cha_kod,cha_evrakno_seri,cha_evrakno_sira,odenis_meblegi=cha_meblag,odenis_meblegi2=cha_meblag   from CARI_HESAP_HAREKETLERI   WITH(NOLOCK) where cha_kod='{cha_kod}'  and cha_cinsi in (22,0) and cha_tip=0 order by cha_tarihi ";
                var mexariclerDt = direkbaglanBA.DBListele(mexaricQuery);

                for (int t = 0; t < FakturlarDT.Rows.Count; t++)
                {
                    var cha_evrakno_seri = FakturlarDT.Rows[t]["cha_evrakno_seri"];
                    var cha_evrakno_sira = FakturlarDT.Rows[t]["cha_evrakno_sira"];
                    var cha_evrak_tip = FakturlarDT.Rows[t]["cha_evrak_tip"];
                    var cha_belge_no = FakturlarDT.Rows[t]["cha_belge_no"].ToString();

                    var vadeBulQuery = $"select cop_vade,cop_tutar,cop_tutar as cop_tutar2 from CARI_HAREKET_ODEME_VADELERI  WITH(NOLOCK) where cop_evrak_tip='{cha_evrak_tip}' AND cop_evrakno_seri='{cha_evrakno_seri}' and cop_evrakno_sira='{cha_evrakno_sira}' order by cop_vade ";
                    var vadelerDT = direkbaglanBA.DBListele(vadeBulQuery);

                    var medaxillerDT2 = (from p in medaxillerDT.AsEnumerable()
                                         where p.Field<string>("cha_belge_no") == cha_belge_no
                                         select p).ToList();
                    var medaxillerDT2Add = (from p in medaxillerDT.AsEnumerable()
                                            where p.Field<string>("cha_belge_no") == cha_evrakno_seri.ToString() + cha_evrakno_sira.ToString()
                                            select p).ToList();
                    if (medaxillerDT2Add.Count > 0)
                    {
                        medaxillerDT2.AddRange(medaxillerDT2Add);
                    }

                    var mexariclerDT2 = (from p in mexariclerDt.AsEnumerable()
                                         where p.Field<string>("cha_belge_no") == cha_belge_no
                                         select p).ToList();
                    var mexariclerDT2Add = (from p in mexariclerDt.AsEnumerable()
                                            where p.Field<string>("cha_belge_no") == cha_evrakno_seri.ToString() + cha_evrakno_sira.ToString()
                                            select p).ToList();
                    if (mexariclerDT2Add.Count > 0)
                    {
                        mexariclerDT2.AddRange(mexariclerDT2Add);
                    }



                    if (mexariclerDT2.Count > 0)
                    {
                        for (int i = 0; i < medaxillerDT2.Count; i++)
                        {
                            double.TryParse(medaxillerDT2[i]["odenis_meblegi"].ToString(), out double medaxil_meblegi);
                            if (medaxil_meblegi > 0)
                            {
                                for (int j = 0; j < mexariclerDT2.Count; j++)
                                {
                                    double.TryParse(medaxillerDT2[i]["odenis_meblegi"].ToString(), out double medaxil_meblegi2);
                                    medaxil_meblegi = medaxil_meblegi2;
                                    double.TryParse(mexariclerDT2[j]["odenis_meblegi"].ToString(), out double mexaric_meblegi);
                                    if (mexaric_meblegi > 0 && medaxil_meblegi > 0)
                                    {
                                        if (medaxil_meblegi > mexaric_meblegi)
                                        {
                                            medaxil_meblegi = medaxil_meblegi - mexaric_meblegi;
                                            medaxillerDT2[i]["odenis_meblegi"] = medaxil_meblegi.ToString();
                                            mexariclerDT2[j]["odenis_meblegi"] = "0";
                                        }
                                        else
                                        {
                                            mexaric_meblegi = mexaric_meblegi - medaxil_meblegi;
                                            medaxillerDT2[i]["odenis_meblegi"] = "0";
                                            mexariclerDT2[j]["odenis_meblegi"] = mexaric_meblegi.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }


                    double.TryParse(FakturlarDT.Rows[t]["cha_meblag"].ToString(), out double chameblag);
                    double.TryParse(FakturlarDT.Rows[t]["cha_meblag"].ToString(), out double chameblag2);
                    for (int i = 0; i < medaxillerDT2.Count; i++)
                    {
                        var odeme_guid = medaxillerDT2[i]["odeme_guid"].ToString();
                        double.TryParse(medaxillerDT2[i]["odenis_meblegi"].ToString(), out double odenis_meblegi);
                        double.TryParse(medaxillerDT2[i]["odenis_meblegi2"].ToString(), out double sabitOdenis);
                        DateTime.TryParse(medaxillerDT2[i]["odeme_tarihi"].ToString(), out DateTime odeme_tarihi);

                        if (odenis_meblegi > 0)
                        {
                            var cha_tarihi = FakturlarDT.Rows[t]["cha_tarihi"].ToString();
                            var fakturaguid = FakturlarDT.Rows[t]["fakturaguid"].ToString();

                            if (vadelerDT.Rows.Count == 0)
                            {
                                if (chameblag > odenis_meblegi)
                                { 
                                    PARAMETRELI_BORC_ALACAK_ESLESTIR_INSERT(cha_kod, fakturaguid, cha_tarihi, odenis_meblegi, odeme_guid, odeme_tarihi, chameblag2, sabitOdenis);
                                    chameblag -= odenis_meblegi;
                                    FakturlarDT.Rows[t]["cha_meblag"] = chameblag.ToString();
                                    odenis_meblegi = 0;
                                    medaxillerDT2[i]["odenis_meblegi"] = odenis_meblegi.ToString();
                                }
                                else
                                {
                                    PARAMETRELI_BORC_ALACAK_ESLESTIR_INSERT(cha_kod, fakturaguid, cha_tarihi, chameblag, odeme_guid, odeme_tarihi, chameblag2, sabitOdenis);
                                    odenis_meblegi = odenis_meblegi - chameblag;
                                    medaxillerDT2[i]["odenis_meblegi"] = odenis_meblegi.ToString();
                                    chameblag = 0;
                                    FakturlarDT.Rows[t]["cha_meblag"] = chameblag.ToString();
                                    break;
                                }
                            }
                            else
                            {
                                for (int k = 0; k < vadelerDT.Rows.Count; k++)
                                {
                                    double.TryParse(vadelerDT.Rows[k]["cop_tutar"].ToString(), out double cop_tutar);
                                    double.TryParse(vadelerDT.Rows[k]["cop_tutar2"].ToString(), out double cop_tutar2);

                                    var cop_vade = vadelerDT.Rows[k]["cop_vade"].ToString();

                                    if (odenis_meblegi > 0 && cop_tutar2 > 0)
                                    {
                                        if (odenis_meblegi - cop_tutar2 < 0)
                                        {
                                            PARAMETRELI_BORC_ALACAK_ESLESTIR_INSERT(cha_kod, fakturaguid, cop_vade, odenis_meblegi, odeme_guid, odeme_tarihi, cop_tutar, sabitOdenis);

                                            cop_tutar2 -= odenis_meblegi;
                                            vadelerDT.Rows[k]["cop_tutar2"] = cop_tutar2.ToString();
                                            odenis_meblegi = 0;
                                            medaxillerDT2[i]["odenis_meblegi"] = odenis_meblegi.ToString();
                                            break;
                                        }
                                        else
                                        {
                                            PARAMETRELI_BORC_ALACAK_ESLESTIR_INSERT(cha_kod, fakturaguid, cop_vade, cop_tutar2, odeme_guid, odeme_tarihi, cop_tutar, sabitOdenis);
                                            odenis_meblegi = odenis_meblegi - cop_tutar2;
                                            medaxillerDT2[i]["odenis_meblegi"] = odenis_meblegi.ToString();
                                            vadelerDT.Rows[k]["cop_tutar2"] = 0;
                                        }

                                    }
                                }
                            }
                        }
                    }
                    medaxillerDT.AcceptChanges();
                }


                yapilanSayi++;
                bilgiver($"Yapilan:{yapilanSayi}/Topalam:{toplamSayi}");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static int PARAMETRELI_BORC_ALACAK_ESLESTIR_INSERT(string chk_ChKodu, string chk_Borc_uid, string chk_BorcVade,
    double chk_Tutar, string chk_Alc_uid, DateTime chk_Alacakvade, double chk_OrjBorcTutar, double chk_OrjAlacakTutar)
        {
            try
            {
                var Direkbaglan = new DirekbaglanHedef();
                var dtislem = Direkbaglan.DBListele("SELECT * FROM  CARI_HAREKET_BORC_ALACAK_ESLEME where 1=2");
                DataRow dr = dtislem.NewRow();

                dr["chk_Guid"] = Guid.NewGuid();
                dr["chk_DBCno"] = 0;
                dr["chk_SpecRECno"] = 0;
                dr["chk_iptal"] = false;
                dr["chk_fileid"] = 74;
                dr["chk_hidden"] = false;
                dr["chk_kilitli"] = false;
                dr["chk_degisti"] = false;
                dr["chk_checksum"] = 0;
                dr["chk_create_user"] = 999;
                dr["chk_create_date"] = DateTime.Today;
                dr["chk_lastup_user"] = 999;
                dr["chk_lastup_date"] = DateTime.Today;
                dr["chk_special1"] = "HE";
                dr["chk_special2"] = "SE";
                dr["chk_special3"] = "N";
                dr["chk_ChCinsi"] = 0;
                dr["chk_ChKodu"] = chk_ChKodu;
                dr["chk_Borc_uid"] = chk_Borc_uid;
                dr["chk_BorcVade"] = chk_BorcVade;
                dr["chk_Tutar"] = chk_Tutar;
                dr["chk_Alc_uid"] = chk_Alc_uid;
                dr["chk_Alacakvade"] = chk_Alacakvade;
                dr["chk_Aciklama1"] = "";
                dr["chk_Aciklama2"] = "";
                dr["chk_OrjBorcTutar"] = chk_OrjBorcTutar;
                dr["chk_OrjAlacakTutar"] = chk_OrjAlacakTutar;

                dtislem.Rows.Add(dr);
                return Direkbaglan.DBinsertrow(dtislem, "SELECT * FROM  CARI_HAREKET_BORC_ALACAK_ESLEME where 1=2", $"CariKod={chk_ChKodu}");

            }
            catch (Exception ex)
            {
                Log.Error($"Xeta Borc Alacak Eslesme : cariKod={chk_ChKodu}-odemeGuid={chk_Alc_uid} Xeta:{ex.Message} inner:{ex.InnerException}");
                return -1;
            }
        }

        public static bool dursunmu = false;
        private void button2_Click(object sender, EventArgs e)
        {
            dursunmu = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
    }

}
