﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlexCel.Core;
using FlexCel.Report;
using FlexCel.Render;
using FlexCel.XlsAdapter;
using System.Data;
using System.Data.SqlClient;
using DomainModel;
using VIETTEL.Models;
using VIETTEL.Controllers;
using System.IO;

namespace VIETTEL.Report_Controllers.DuToan
{
    public class rptDuToan_4_TungDonViController : Controller
    {

        public string sViewPath = "~/Report_Views/DuToan/";
        private const String sFilePath = "/Report_ExcelFrom/DuToan/rptDuToan_4_TungDonVi.xls";
       

        public ActionResult Index()
        {
            if(HamChung.CoQuyenXemTheoMenu(Request.Url.AbsolutePath,User.Identity.Name))
            {
            ViewData["path"] = "~/Report_Views/DuToan/rptDuToan_4_TungDonVi.aspx";
            ViewData["PageLoad"] = "0";
            return View(sViewPath + "ReportView.aspx");
            }
            else
            {
                return RedirectToAction("Index", "PermitionMessage");
            }
        }
        /// <summary>
        /// hàm lấy các giá trên form
        /// </summary>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public ActionResult EditSubmit(String ParentID)
        {
            String iID_MaDonVi = Request.Form["iID_MaDonVi"];
            ViewData["PageLoad"] = "1";
            ViewData["iID_MaDonVi"] = iID_MaDonVi;
            ViewData["path"] = "~/Report_Views/DuToan/rptDuToan_4_TungDonVi.aspx";
            return View(sViewPath + "ReportView.aspx");
        }
        /// <summary>
        /// hàm khởi tạo báo cáo
        /// </summary>
        /// <param name="path"></param>
        /// <param name="NamLamViec"></param>
        /// <returns></returns>

        public ExcelFile CreateReport(String path, String MaND, String iID_MaDonVi)
        {
            DataTable dtCauHinh = NguoiDungCauHinhModels.LayCauHinh(MaND);
            String iNamLamViec = Convert.ToString(dtCauHinh.Rows[0]["iNamLamViec"]);


            String sTenDonVi = DonViModels.Get_TenDonVi(iID_MaDonVi);
            XlsFile Result = new XlsFile(true);
            Result.Open(path);
            FlexCelReport fr = new FlexCelReport();
            LoadData(fr, MaND, iID_MaDonVi);
            fr = ReportModels.LayThongTinChuKy(fr, "rptDuToan_4_TungDonVi");
            fr.SetValue("Nam", iNamLamViec);
            fr.SetValue("sTenDonVi", sTenDonVi);
            fr.SetValue("Cap1", ReportModels.CauHinhTenDonViSuDung(1,User.Identity.Name));
            fr.SetValue("Ngay", ReportModels.Ngay_Thang_Nam_HienTai());
            fr.Run(Result);
            return Result;

        }

        //1020000
        /// <summary>
        /// Phụ lục 2c-c
        /// </summary>
        /// <param name="NamLamViec"></param>
        /// <returns></returns>
        public static DataTable DT_rptDuToan_4_TungDonVi(String MaND,String iID_MaDonVi)
        {
            String DKDonVi = "", DKPhongBan = "", DK = "";
            SqlCommand cmd = new SqlCommand();
            DKDonVi = ThuNopModels.DKDonVi(MaND, cmd);
            DKPhongBan = ThuNopModels.DKPhongBan_QuyetToan(MaND, cmd);
            int DVT = 1000;
            String SQL = String.Format(@"SELECT SUBSTRING(sLNS,1,3) as sLNS3,SUBSTRING(sLNS,1,5) as sLNS5, sLNS,sL,sK,sM,sTM,sTTM,sNG,sMoTa,iID_MaDonVi,sTenDonVi,iID_MaPhongBan
,rTuChi=SUM(rTuChi/{3})
,rPhanCap=SUM(rPhanCap/{3})
,rDuPhong=SUM(rDuPhong/{3})
 FROM DT_ChungTuChiTiet
 WHERE iTrangThai=1  AND SUBSTRING(sLNS,1,1) IN (4) AND iID_MaDonVi =@iID_MaDonVi AND iNamLamViec=@iNamLamViec {1} {2}
 GROUP BY sLNS,sL,sK,sM,sTM,sTTM,sNG,sMoTa,iID_MaDonVi,sTenDonVi,iID_MaPhongBan
HAVING SUM(rTuChi)<>0 OR SUM(rPhanCap/1000)<>0 OR SUM(rDuPhong/1000)<>0", iID_MaDonVi, DKPhongBan, DKDonVi, DVT);
            cmd.CommandText = SQL;
            cmd.Parameters.AddWithValue("@iNamLamViec", ReportModels.LayNamLamViec(MaND));
            cmd.Parameters.AddWithValue("@iID_MaDonVi", iID_MaDonVi);
            DataTable dt = Connection.GetDataTable(cmd);
            cmd.Dispose();
            return dt;
        }
        public static String LayMoTa(String sLNS)
        {
            String sMoTa = "";

            String SQL = String.Format(@"SELECT sMoTa FROM NS_MucLucNganSach WHERE iTrangThai=1 AND sLNS={0}", sLNS);
            sMoTa = Connection.GetValueString(SQL, "");
            return sMoTa;
        }
        /// <summary>
        /// hàm hiển thị dữ liệu ra báo cáo
        /// </summary>
        /// <param name="fr"></param>
        /// <param name="NamLamViec"></param>
        private void LoadData(FlexCelReport fr, String MaND, String iID_MaDonVi)
        {
            DataRow r;
            DataTable data = DT_rptDuToan_4_TungDonVi(MaND, iID_MaDonVi);
            data.TableName = "ChiTiet";
            fr.AddTable("ChiTiet", data);
            DataTable dtsTM = HamChung.SelectDistinct("dtsTM", data, "sLNS3,sLNS5,sLNS,sL,sK,sM,sTM", "sLNS3,sLNS5,sLNS,sL,sK,sM,sTM,sMoTa", "sLNS,sL,sK,sM,sTM,sTTM");
            DataTable dtsM = HamChung.SelectDistinct("dtsM", dtsTM, "sLNS3,sLNS5,sLNS,sL,sK,sM", "sLNS3,sLNS5,sLNS,sL,sK,sM,sMoTa", "sLNS,sL,sK,sM,sTM");
            DataTable dtsL = HamChung.SelectDistinct("dtsL", dtsM, "sLNS3,sLNS5,sLNS,sL,sK", "sLNS3,sLNS5,sLNS,sL,sK,sMoTa", "sLNS,sL,sK,sM");
            DataTable dtsLNS = HamChung.SelectDistinct("dtsLNS", dtsL, "sLNS", "sLNS3,sLNS5,sLNS,sMoTa", "sLNS,sL");
            DataTable dtsLNS5 = HamChung.SelectDistinct("dtsLNS5", dtsLNS, "sLNS3,sLNS5", "sLNS3,sLNS5,sMoTa");
            for (int i = 0; i < dtsLNS5.Rows.Count; i++)
            {
                r = dtsLNS5.Rows[i];
                r["sMoTa"] = LayMoTa(Convert.ToString(r["sLNS5"]));
            }
            DataTable dtsLNS3 = HamChung.SelectDistinct("dtsLNS3", dtsLNS5, "sLNS3", "sLNS3,sMoTa");

            for (int i = 0; i < dtsLNS3.Rows.Count; i++)
            {
                r = dtsLNS3.Rows[i];
                r["sMoTa"] = LayMoTa(Convert.ToString(r["sLNS3"]));
            }

            fr.AddTable("dtsTM", dtsTM);
            fr.AddTable("dtsM", dtsM);
            fr.AddTable("dtsL", dtsL);
            fr.AddTable("dtsLNS", dtsLNS);
            fr.AddTable("dtsLNS3", dtsLNS3);
            fr.AddTable("dtsLNS5", dtsLNS5);

            data.Dispose();
            dtsTM.Dispose();
            dtsM.Dispose();
            dtsL.Dispose();
            dtsLNS.Dispose();
            dtsLNS3.Dispose();
            dtsLNS5.Dispose();
        }
      
        /// <summary>
        /// Hàm xuất dữ liệu ra file excel
        /// </summary>
        /// <param name="NamLamViec"></param>
        /// <returns></returns>
        public clsExcelResult ExportToExcel(String MaND, String iID_MaDonVi)
        {
            clsExcelResult clsResult = new clsExcelResult();
            ExcelFile xls = CreateReport(Server.MapPath(sFilePath),MaND,iID_MaDonVi);

            using (MemoryStream ms = new MemoryStream())
            {
                xls.Save(ms);
                ms.Position = 0;
                clsResult.ms = ms;
                clsResult.FileName = "DuToan_206_TungDonVi.xls";
                clsResult.type = "xls";
                return clsResult;
            }

        }
        /// <summary>
        /// hàm view PDF
        /// </summary>
        /// <param name="NamLamViec"></param>
        /// <returns></returns>
        public ActionResult ViewPDF(String MaND, String iID_MaDonVi)
        {
            HamChung.Language();
            ExcelFile xls = CreateReport(Server.MapPath(sFilePath), MaND, iID_MaDonVi);
            using (FlexCelPdfExport pdf = new FlexCelPdfExport())
            {
                pdf.Workbook = xls;
                using (MemoryStream ms = new MemoryStream())
                {
                    pdf.BeginExport(ms);
                    pdf.ExportAllVisibleSheets(false, "BaoCao");
                    pdf.EndExport();
                    ms.Position = 0;
                    return File(ms.ToArray(), "application/pdf");
                }
            }
        }
    }
}

