using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IBM.Data.DB2;
using System.IO;

namespace WorkbrainArchivingTool
{
    public partial class Form_MainForm : Form
    {
        public Form_MainForm()
        {
            InitializeComponent();
        }
        
        #region DECLARATIONS
        string globalConnStringDb2 = "Database=wb0001db;UserID=ta00wb;Password=ta00wb!;Server=phdbuprc:51649;Query Timeout=0";

        string strStartDate;
        string strEndDate;

        string strWBRegVal;
        string strEmpUDFVal;
        string strEmpUDFEfDate;
        string strEmpUDF;
        string strArchiveBoundary;

        string strArchiveWS;
        string strArchiveWD;
        string strArchiveCTP;
        string strArchiveEBL;
        string strArchiveWDA;
        string strArchiveOVR;

        string strPrimaryWS;
        string strPrimaryWD;
        string strPrimaryCTP;
        string strPrimaryEBL;
        string strPrimaryWDA;
        string strPrimaryOVR;

        
        #endregion DECLARATIONS

        #region DATES FUNCTIONS
        public void disableDateFields()
        {
            tbStartDate.Enabled = false;
            tbEndDate.Enabled = false;
        }

        public void clearDateFields()
        {
            tbStartDate.Clear();
            tbEndDate.Clear();
        }

        public void dateToday()
        {
            string strDateToday = "Today is ";
            label29.Text = strDateToday + System.DateTime.Today.ToLongDateString();
            label29.Show();
        }

        #endregion DATES FUNCTIONS

        #region FORM FUNCTIONALITIES
        private void Form_MainForm_Load(object sender, EventArgs e)
        {
            dateToday();
        }//btnGenerate()
        private void startDate_DT_ValueChanged(object sender, EventArgs e)
        {
            endDate_DT.Value = startDate_DT.Value.AddDays(6);
            disableDateFields();
            strStartDate = startDate_DT.Value.ToString("yyyy-MM-dd");
            tbStartDate.Text = strStartDate;
            strEndDate = endDate_DT.Value.ToString("yyyy-MM-dd");
            tbEndDate.Text = strEndDate;
        }
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (startDate_DT.Value <= endDate_DT.Value && (startDate_DT.Value.DayOfWeek != DayOfWeek.Sunday))
            {
                MessageBox.Show("Invalid date selection, the start date cannot be greater than or equal to the end date.\n\nSelect a valid value for Start Date (Note that this always starts on a SUNDAY)", "Invalid date", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                archiveQueryWS();
                archiveQueryWD();
                archiveQueryCTP();
                archiveQueryEBL();
                archiveQueryWDA();
                archiveQueryOVR();
                primaryQueryWS();
                primaryQueryWD();
                primaryQueryCTP();
                primaryQueryEBL();
                primaryQueryWDA();
                primaryQueryOVR();
                updateUDF();
                updateWorkbrainRegistry();
                checkBoundaryDate();
            }//else
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            clearDateFields();
        }

        #endregion FORM FUNCTIONALITIES

        #region FORM CONTENTS
        //archive table
        public void archiveQueryWS()
        {
            strArchiveWS = "SELECT COUNT(TB.WRKS_ID)@FROM ARCHIVE.WORK_SUMMARY TB@WHERE WRKS_WORK_DATE@BETWEEN TIMESTAMP('" + tbStartDate.Text + " 0:00:00.0') AND ('" + tbEndDate.Text + " 0:00:00.0')@WITH UR";
            strArchiveWS = strArchiveWS.Replace("@", "" + System.Environment.NewLine);
            rtArchiveWS.Text = strArchiveWS;
        }
        public void archiveQueryWD()
        {
            strArchiveWD = "SELECT COUNT(TB.WRKS_ID)@FROM ARCHIVE.WORK_DETAIL TB @INNER JOIN ARCHIVE.WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR ";
            strArchiveWD = strArchiveWD.Replace("@", "" + System.Environment.NewLine);
            rtArchiveWD.Text = strArchiveWD;
        }
        public void archiveQueryCTP()
        {
            strArchiveCTP = "SELECT COUNT(TB.WRKS_ID)@FROM ARCHIVE.CLOCK_TRAN_PROCESSED TB @INNER JOIN ARCHIVE.WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE  WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR";
            strArchiveCTP = strArchiveCTP.Replace("@", "" + System.Environment.NewLine);
            rtArchiveCTP.Text = strArchiveCTP;
        }
        public void archiveQueryEBL()
        {
            strArchiveEBL = "SELECT COUNT(TB.WRKS_ID)@FROM ARCHIVE.EMPLOYEE_BALANCE_LOG TB @INNER JOIN ARCHIVE.WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE  WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR";
            strArchiveEBL = strArchiveEBL.Replace("@", "" + System.Environment.NewLine);
            rtArchiveEBL.Text = strArchiveEBL;
        }
        public void archiveQueryWDA()
        {
            strArchiveWDA = "SELECT COUNT(TB.WRKDA_ID)@FROM ARCHIVE.WORK_DETAIL_ADJUST TB@WHERE TB.WRKDA_WORK_DATE @BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000') @WITH UR";
            strArchiveWDA = strArchiveWDA.Replace("@", "" + System.Environment.NewLine);
            rtArchiveWDA.Text = strArchiveWDA;
        }
        public void archiveQueryOVR()
        {
            strArchiveOVR = "SELECT COUNT(TB.OVR_ID) @FROM ARCHIVE.OVERRIDE TB@WHERE  ovr_start_date BETWEEN TIMESTAMP('" + tbStartDate.Text + " 0:00:00.0') AND ('" + tbEndDate.Text + " 0:00:00.0')@AND NOT ( ( ovrtyp_id >= 700 @AND ovrtyp_id < 900 ) @AND ( ovrtyp_id >= 1500 @AND ovrtyp_id < 1600 ) @AND ( ovrtyp_id >= 400 @AND ovrtyp_id < 500 ) ) @WITH UR	";
            strArchiveOVR = strArchiveOVR.Replace("@", "" + System.Environment.NewLine);
            rtArchiveOVR.Text = strArchiveOVR;
        }
        
        //primary table

        public void primaryQueryWS()
        {
            strPrimaryWS = "SELECT COUNT(TB.WRKS_ID)@FROM WORK_SUMMARY TB@WHERE WRKS_WORK_DATE@BETWEEN TIMESTAMP('" + tbStartDate.Text + " 0:00:00.0') AND ('" + tbEndDate.Text + " 0:00:00.0')@WITH UR";
            strPrimaryWS = strPrimaryWS.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryWS.Text = strPrimaryWS;
        }
        public void primaryQueryWD()
        {
            strPrimaryWD = "SELECT COUNT(TB.WRKS_ID)@FROM WORK_DETAIL TB @INNER JOIN WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR ";
            strPrimaryWD = strPrimaryWD.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryWD.Text = strPrimaryWD;
        }
        public void primaryQueryCTP()
        {
            strPrimaryCTP = "SELECT COUNT(TB.WRKS_ID)@FROM CLOCK_TRAN_PROCESSED TB @INNER JOIN WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE  WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR";
            strPrimaryCTP = strPrimaryCTP.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryCTP.Text = strPrimaryCTP;
        }
        public void primaryQueryEBL()
        {
            strPrimaryEBL = "SELECT COUNT(TB.WRKS_ID)@FROM EMPLOYEE_BALANCE_LOG TB @INNER JOIN WORK_SUMMARY WS @ON TB.WRKS_ID = WS.WRKS_ID @WHERE  WS.WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND @TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000')@WITH UR";
            strPrimaryEBL = strPrimaryEBL.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryEBL.Text = strPrimaryEBL;
        }
        public void primaryQueryWDA()
        {
            strPrimaryWDA = "SELECT COUNT(TB.WRKDA_ID)@FROM WORK_DETAIL_ADJUST TB@WHERE TB.WRKDA_WORK_DATE @BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.000000') AND TIMESTAMP('" + tbEndDate.Text + " 00:00:00.000000') @WITH UR";
            strPrimaryWDA = strPrimaryWDA.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryWDA.Text = strPrimaryWDA;
        }
        public void primaryQueryOVR()
        {
            strPrimaryOVR = "SELECT COUNT(TB.OVR_ID) @FROM OVERRIDE TB@WHERE  ovr_start_date BETWEEN TIMESTAMP('" + tbStartDate.Text + " 0:00:00.0') AND ('" + tbEndDate.Text + " 0:00:00.0')@AND NOT ( ( ovrtyp_id >= 700 @AND ovrtyp_id < 900 ) @AND ( ovrtyp_id >= 1500 @AND ovrtyp_id < 1600 ) @AND ( ovrtyp_id >= 400 @AND ovrtyp_id < 500 ) ) @WITH UR	";
            strPrimaryOVR = strPrimaryOVR.Replace("@", "" + System.Environment.NewLine);
            rtPrimaryOVR.Text = strPrimaryOVR;
        }
        public void updateUDF()
        {
            strEmpUDFEfDate = endDate_DT.Value.AddDays(1).ToString("yyyy-MM-dd");
            strEmpUDFVal = endDate_DT.Value.AddDays(1).ToString("MM/dd/yyyy");
            strEmpUDF = "UPDATE ta00wb.EMP_UDF_DATA @SET EUDFD_EFF_DATE = '" + strEmpUDFEfDate + " 00:00:00.0',@EUDFD_VALUE = '" + strEmpUDFVal + "'@WHERE EMPUDF_ID = (SELECT EMPUDF_ID@FROM ta00wb.EMP_UDF_DEF@WHERE EMPUDF_NAME = 'LAST_ARCHIVE_PAYROLL_DATA_DATE')@AND EMP_ID in (SELECT DISTINCT(EMP_ID)@FROM ARCHIVE.WORK_SUMMARY@WHERE WRKS_WORK_DATE BETWEEN TIMESTAMP('2013-07-21 00:00:00.0')@AND TIMESTAMP('2013-07-27 23:59:59.0'))";
            strEmpUDF = strEmpUDF.Replace("@", "" + System.Environment.NewLine);
            rtUpdateEmpUDF.Text = strEmpUDF;
        }
        public void updateWorkbrainRegistry()
        {
            strWBRegVal = endDate_DT.Value.AddDays(1).ToString("MM/dd/yyyy");
            string wbRegQuery = "UPDATE TA00WB.WORKBRAIN_REGISTRY@SET WBREG_VALUE ='" + strWBRegVal +"'@WHERE WBREG_NAME = 'ARCHIVE_BOUNDARY_DATE'";
            wbRegQuery = wbRegQuery.Replace("@", "" + System.Environment.NewLine);
            rtUpdateRegistry.Text = wbRegQuery;
        }

        public void checkBoundaryDate()
        {
            strArchiveBoundary = "SELECT WBREG_VALUE FROM TA00WB.WORKBRAIN_REGISTRY@WHERE WBREG_NAME = 'ARCHIVE_BOUNDARY_DATE'@WITH UR@";
            strArchiveBoundary = strArchiveBoundary.Replace("@", "" + System.Environment.NewLine);
            rtCheckBoundary.Text = strArchiveBoundary;
        }
        
        #endregion FORM CONTENTS

        #region WORKBRAIN DATABASE CONNECTION

        public void queryBoundaryDate()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtCheckBoundary.Text;
                tbBoundaryDate.Text = cmd.ExecuteScalar().ToString();
                string strBoundaryLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran Archive Boundary Query. Current boundary date is " + tbBoundaryDate.Text + "@" ;
                strBoundaryLog = strBoundaryLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strBoundaryLog);
                conndb2.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "Check Archive Boundary Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void queryArchiveWS()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveWS.Text;
                tbArchWS.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_SUMMARY Query. " + tbArchWS.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger
                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchWS.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.WORK_SUMMARY Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_SUMMARY Query. Error occured. No" + tbArchWS.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryArchiveWD()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveWD.Text;
                tbArchWD.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query. " + tbArchWD.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger
                
                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchWD.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.WORK_DETAIL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query. Error occured. No" + tbArchWD.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryArchiveCTP()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveCTP.Text;
                tbArchCTP.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.CLOCK_TRAN_PROCESSED Query. " + tbArchCTP.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchCTP.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.CLOCK_TRAN_PROCESSED Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.CLOCK_TRAN_PROCESSED Query. Error occured. No" + tbArchCTP.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryArchiveEBL()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveEBL.Text;
                tbArchEBL.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query. " + tbArchEBL.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchEBL.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.EMPLOYEE_BALANCE_LOG Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query. Error occured. No" + tbArchEBL.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryArchiveWDA()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveWDA.Text;
                tbArchWDA.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL_ADJUST Query. " + tbArchWDA.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchWDA.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.WORK_DETAIL_ADJUST Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL_ADJUST Query. Error occured. No" + tbArchWDA.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryArchiveOVR()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtArchiveOVR.Text;
                tbArchOVR.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query. " + tbArchOVR.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchOVR.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.OVERRIDE Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query. Error occured. No" + tbArchOVR.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }

      
        public void queryPrimaryWS()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryWS.Text;
                tbPrimWS.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_SUMMARY Query. " + tbPrimWS.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger
                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbPrimWS.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "TA00WB.WORK_SUMMARY Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strArchWSLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_SUMMARY Query. Error occured. No" + tbPrimWS.Text + "rows returned.@";
                strArchWSLog = strArchWSLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strArchWSLog);
            }//catch
        }
        public void queryPrimaryWD()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryWD.Text;
                tbPrimWD.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL Query. " + tbPrimWD.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbPrimWS.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "TA00WB.WORK_DETAIL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query. Error occured. No" + tbPrimWS.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryPrimaryCTP()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryCTP.Text;
                tbPrimCTP.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.CLOCK_TRAN_PROCESSED Query. " + tbPrimCTP.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbPrimCTP.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "TA00WB.CLOCK_TRAN_PROCESSED Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.CLOCK_TRAN_PROCESSED Query. Error occured. No" + tbPrimCTP.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryPrimaryEBL()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryEBL.Text;
                tbPrimEBL.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.EMPLOYEE_BALANCE_LOG Query. " + tbPrimEBL.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbPrimEBL.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "ARCHIVE.EMPLOYEE_BALANCE_LOG Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query. Error occured. No" + tbPrimEBL.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryPrimaryWDA()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryWDA.Text;
                tbPrimWDA.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL_ADJUST Query. " + tbPrimWDA.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchWS.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "TA00WB.WORK_DETAIL_ADJUST Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL_ADJUST Query. Error occured. No" + tbPrimWDA.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        public void queryPrimaryOVR()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtPrimaryOVR.Text;
                tbPrimOVR.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.OVERRIDE Query. " + tbPrimOVR.Text + " rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);

                //logger

                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (TextWriter outputFile = new StreamWriter(logFilePath + @"\WBArchiving.txt", true))
                {
                    outputFile.Write(strLog);
                }
                conndb2.Close();
            }
            catch (Exception e)
            {
                tbArchWS.Text = " ";
                MessageBox.Show("An error was encountered during runtime. Please try again.\n\n" + e.GetBaseException().Message, "TA00WB.OVERRIDE Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query. Error occured. No" + tbPrimOVR.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }



        #endregion WORKBRAIN DATABASE CONNECTION

        #region INDIVIDUAL BUTTON CLICK (WORKBRAIN DB QUERIES)
        private void btnCheckBoundary_Click(object sender, EventArgs e)
        {
            queryBoundaryDate();
        }
        private void btnQueryArchWS_Click(object sender, EventArgs e)
        {
            queryArchiveWS();
        }
        private void btnQueryArchWD_Click(object sender, EventArgs e)
        {
            queryArchiveWD();
        }
        private void btnQueryArchCTP_Click(object sender, EventArgs e)
        {
            queryArchiveCTP();
        }
        private void btnQueryArchEBL_Click(object sender, EventArgs e)
        {
            queryArchiveEBL();
        }
        private void btnQueryArchWDA_Click(object sender, EventArgs e)
        {
            queryArchiveWDA();
        }
        private void btnQueryArchOVR_Click(object sender, EventArgs e)
        {
            queryArchiveOVR();
        }

        private void btnQueryPrimWS_Click(object sender, EventArgs e)
        {
            queryPrimaryWS();
        }
        private void btnQueryPrimWD_Click(object sender, EventArgs e)
        {
            queryPrimaryWD();
        }
        private void btnQueryPrimCTP_Click(object sender, EventArgs e)
        {
            queryPrimaryCTP();
        }
        private void btnQueryPrimEBL_Click(object sender, EventArgs e)
        {
            queryPrimaryEBL();
        }
        private void btnQueryPrimWDA_Click(object sender, EventArgs e)
        {
            queryPrimaryWDA();
        }
        private void btnQueryPrimOVR_Click(object sender, EventArgs e)
        {
            queryPrimaryOVR();
        }
        
        #endregion

        
    }
}
