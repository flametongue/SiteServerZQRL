﻿using System;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web;
using System.Web.UI.WebControls;
using BaiRong.Controls;
using BaiRong.Core;
using SiteServer.ZQRL;
using System.Collections.Generic;
using System.Data;
using SiteServer.ZQRL.Core.Data;

namespace SiteServer.ZQRL.ForegroundPages
{
    public partial class CompanyFundsearchInfos : System.Web.UI.Page
    {
        public TextBox Keyword;
        public Button Search;
        public Repeater rptContents;
        protected string pageHtml;
        protected string key;

        protected int pagesize;
        protected int pagenum;

        protected string returnUrl;

        public TextBox lblYearTime;

        protected string YearTime = string.Empty;
       // protected int FundType = 0;
        protected int provinceType = DataProviderZQRL.CompanyDAO.GetTopProvinceId();
        protected int cityType = -1;
        public Label lblSupportStaff;
        protected string login_name = "匿名";
        protected string login_company = "匿名公司";
        public DropDownList ddlFundtype;
        public DropDownList ddlProvince;
        public DropDownList ddlCity;

        protected void Page_Load(object sender, EventArgs e)
        {
            UsersInfo usersInfo = (UsersInfo)Session["UserInfo"];
            if (usersInfo == null || usersInfo.Number == null || usersInfo.Number == "")
            {
                Response.Redirect("../index.htm");
            }
            else
            {
                key = usersInfo.CompanyId.ToString();
                login_name = usersInfo.Number;

                DataTable dtTable = DataProviderZQRL.CompanyDAO.GetSupportStaffByCompanyId(key);
                string SupportStaff = string.Empty;
                if (dtTable != null && dtTable.Rows.Count > 0)
                {
                    for (int k = 0; k < dtTable.Rows.Count; k++)
                    {
                        SupportStaff += dtTable.Rows[k]["name"].ToString() + dtTable.Rows[k]["phone"].ToString() + ",";
                    }
                }
                lblSupportStaff.Text = SupportStaff;
            }
            pagesize = TranslateUtils.ToInt(Request["pagesize"], 1);
            pagenum = TranslateUtils.ToInt(Request["pagenum"], 5);

            if (!IsPostBack)
            {

                YearTime = DateTime.Now.Year.ToString();
                lblYearTime.Text = YearTime + "年";

                if (Request["YearTime"] != null && Request["YearTime"] != "")
                {
                    YearTime = Request["YearTime"];
                    lblYearTime.Text = YearTime;
                }

                if (Request["fundType"] != null && Request["fundType"] != "")
                {
                    string fundType = Request["fundType"].ToString();
                    this.ddlFundtype.SelectedValue = fundType;
                }

                if (Request["provinceType"] != null && Request["provinceType"] != "")
                {
                    string provinceType = Request["provinceType"].ToString();
                    this.ddlProvince.SelectedValue = provinceType;
                    if (TranslateUtils.ToInt(provinceType) > 0)
                    {
                        this.LoadCity(TranslateUtils.ToInt(provinceType));
                    }

                }

                if (Request["cityType"] != null && Request["cityType"] != "")
                {
                    string cityType = Request["cityType"].ToString();
                    if (TranslateUtils.ToInt(cityType) > 0)
                    {
                        this.ddlCity.SelectedValue = cityType;
                    }

                }
                else
                {
                    LoadCity(provinceType);
                }

                if (Request["YearTime"] != null && Request["YearTime"] != "")
                {
                    YearTime = Request["YearTime"];
                    lblYearTime.Text = YearTime;
                }

                if (Request["fundType"] != null && Request["fundType"] != "")
                {
                    string fundType = Request["fundType"].ToString();
                    this.ddlFundtype.SelectedValue = fundType;
                }

                if (Request["provinceType"] != null && Request["provinceType"] != "")
                {
                    string provinceType = Request["provinceType"].ToString();
                    this.ddlProvince.SelectedValue = provinceType;
                    if (TranslateUtils.ToInt(provinceType) > 0)
                    {
                        this.LoadCity(TranslateUtils.ToInt(provinceType));
                    }

                }

                if (Request["cityType"] != null && Request["cityType"] != "")
                {
                    string cityType = Request["cityType"].ToString();
                    if (TranslateUtils.ToInt(cityType) > 0)
                    {
                        this.ddlCity.SelectedValue = cityType;
                    }

                }
                //页面初始化
                LoadProvince();
                //LoadCity(-1);
                //this.ddlCity.Items.Insert(0, new ListItem("请选择", "0"));
                LoadHtml();

            }

        }

        /// <summary>
        /// 加载信件
        /// </summary>
        private void LoadHtml()
        {
            //分页
            string pageSql = GetSql();
            PageApp pa = new PageApp(pagesize, pagenum, pageSql, " certifytime Desc ");

            this.rptContents.DataSource = pa.Ds;
            this.rptContents.DataBind();
            this.pageHtml = pa.GetNavigateHtml(PageUrl);


        }

        private void LoadProvince()
        {
            this.ddlProvince.DataSource = provinceTable();

            this.ddlProvince.DataTextField = "name";

            this.ddlProvince.DataValueField = "id";

            this.ddlProvince.DataBind();
        }

        private void LoadCity(int provinceType)
        {
            //ListItem list = new ListItem("无", "0");

            this.ddlCity.DataSource = cityTable(provinceType);

            //this.ddlCity.Items.Insert(0, list);

            this.ddlCity.DataTextField = "name";

            this.ddlCity.DataValueField = "id";

            this.ddlCity.DataBind();

            //this.ddlCity.Items.Insert(0,new ListItem("请选择","0"));


        }

        private string GetSql()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" select t1.area,t4.name as area_Name,t1.province,t1.city,t2.name as province_Name,t3.name as city_Name,t1.[yangljfcertify],t1.[gsjfcertify],t1.[yiljfcertify],t1.[syujfcertify],t1.[syejfcertify],t1.[leaveemppro],t1.[gjjcertify],t1.[certifytime],t1.[searchtxt]	,t1.[searthlink],t1.[ylbxpersonalsearch]	,t1.[ybsearthtxt],t1.[ybsearchlink],t1.[ybpersonalsearchway],t1.[gjjsearchtxt],t1.[gjjsearchlink],t1.[gjjaccountsearchway]	,t1.[remark]	 from t_fundsearch t1 inner join t_province t2 on t1.province = t2.id inner join t_city t3 on t1.city = t3.id inner join t_area t4 on t1.area = t4.id  where 1=1  ");

            if (!string.IsNullOrEmpty(YearTime))
            {
                string sTime = YearTime.Replace("年", "-");
                sb.AppendFormat(" and certifytime like '%{0}%' ", sTime);
            }

            sb.AppendFormat(" and province = '{0}' ", this.ddlProvince.SelectedValue);
            if (!string.IsNullOrEmpty(this.ddlCity.SelectedValue))
            {
                sb.AppendFormat(" and city = '{0}' ", this.ddlCity.SelectedValue);

            }
            return sb.ToString();
        }

        private DataTable provinceTable()
        {
            DataSet dsDataSet = null;
            StringBuilder sb = new StringBuilder();

            sb.Append(" select * from t_province ");

            string Sql = sb.ToString();
            using (SqlConnection conn = new SqlConnection(SqlHelper.CONN_STRING))
            {
                conn.Open();
                try
                {
                    dsDataSet = SqlHelper.ExecuteDataset(conn, CommandType.Text, Sql);
                }
                catch (Exception e)
                {
                    throw new Exception("失败！\n" + e.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return dsDataSet.Tables[0];
        }

        private DataTable cityTable(int provinceType)
        {
            DataSet dsDataSet = null;
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select * from t_city where provinceid ={0}", provinceType);

            string Sql = sb.ToString();
            using (SqlConnection conn = new SqlConnection(SqlHelper.CONN_STRING))
            {
                conn.Open();
                try
                {
                    dsDataSet = SqlHelper.ExecuteDataset(conn, CommandType.Text, Sql);
                }
                catch (Exception e)
                {
                    throw new Exception("失败！\n" + e.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            if (cityType == -1)
            {
                if (dsDataSet.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsDataSet.Tables[0].Rows[0][0].ToString()))
                {
                    cityType = TranslateUtils.ToInt(dsDataSet.Tables[0].Rows[0][0].ToString());
                }
                else
                {
                    cityType = 0;
                }

            }

            return dsDataSet.Tables[0];
        }

        protected void ddlProvince_OnSelectedIndexChanged(Object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CompanyFundsearchInfo.aspx?YearTime={0}&provinceType={1}&cityType={2}", this.lblYearTime.Text, this.ddlProvince.SelectedValue, 0));
        }

        protected string _pageUrl;
        protected string PageUrl
        {
            get
            {
                if (StringUtils.IsNullorEmpty(this._pageUrl))
                {
                    this._pageUrl = string.Format("CompanyFundsearchInfo.aspx?YearTime={0}&provinceType={1}&cityType={2}", this.lblYearTime.Text,this.ddlProvince.SelectedValue, this.ddlCity.SelectedValue);
                }
                return this._pageUrl;
            }
        }
    }
}
