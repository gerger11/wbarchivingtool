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

        public void clearRichTextBoxes()
        {
            rtArchiveWD.Clear();
            rtArchiveWDA.Clear();
            rtArchiveWS.Clear();
            rtArchiveCTP.Clear();
            rtArchiveEBL.Clear();
            rtArchiveOVR.Clear();

            rtPrimaryWD.Clear();
            rtPrimaryWDA.Clear();
            rtPrimaryWS.Clear();
            rtPrimaryCTP.Clear();
            rtPrimaryEBL.Clear();
            rtPrimaryOVR.Clear();
        }

        public void disableCopyButtons()
        {
            button1.Enabled =          false;
            button2.Enabled =          false;
            button4.Enabled =          false;
            button3.Enabled =          false;
            button6.Enabled =          false;
            button5.Enabled =          false;
            button12.Enabled =         false;
            button11.Enabled =         false;
            button10.Enabled =         false;
            button9.Enabled =          false;
            button8.Enabled =          false;
            button7.Enabled =          false;
            btnCpyEmpUDFData.Enabled = false;
            button14.Enabled =         false;
            button16.Enabled =         false;
            button13.Enabled =         false;
        }

        public void enableCopyButtons()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button3.Enabled = true;
            button6.Enabled = true;
            button5.Enabled = true;
            button12.Enabled = true;
            button11.Enabled = true;
            button10.Enabled = true;
            button9.Enabled = true;
            button8.Enabled = true;
            button7.Enabled = true;
            btnCpyEmpUDFData.Enabled = true;
            button14.Enabled = true;
            button16.Enabled = true;
            button13.Enabled = true;
        }

        #endregion DATES FUNCTIONS

        #region FORM FUNCTIONALITIES
        private void Form_MainForm_Load(object sender, EventArgs e)
        {
            dateToday();
            disableCopyButtons();
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
            if (startDate_DT.Value <= endDate_DT.Value && (startDate_DT.Value.DayOfWeek != DayOfWeek.Sunday) || tbStartDate.Text == "" || tbEndDate.Text == "")
            {
                MessageBox.Show("Invalid date selection, the start date cannot be greater than or equal to the end date.\n\nSelect a valid value for Start Date (Note that this always starts on a SUNDAY)", "Invalid date", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clearRichTextBoxes();
                disableCopyButtons();
            }
            else
            {
                enableCopyButtons();
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
                selectEmpUDF();

                // [gigne00] 20160120 - Disable richtextboxes during query generation

                rtArchiveWD.Enabled = false;
                rtArchiveWDA.Enabled = false;
                rtArchiveWS.Enabled = false;
                rtArchiveCTP.Enabled = false;
                rtArchiveEBL.Enabled = false;
                rtArchiveOVR.Enabled = false;

                rtPrimaryWD.Enabled = false;
                rtPrimaryWDA.Enabled = false;
                rtPrimaryWS.Enabled = false;
                rtPrimaryCTP.Enabled = false;
                rtPrimaryEBL.Enabled = false;
                rtPrimaryOVR.Enabled = false;
            }//else
        }

        // [gigne00] 20160120 - Added functionality for clear button to clear contents of richtextboxes
        private void btnClear_Click(object sender, EventArgs e)
        {
            clearRichTextBoxes();
            disableCopyButtons();
        }
        private void btnClearQueryResults_Click(object sender, EventArgs e)
        {
            lblQueryStat.Text = "------";
            lblArchived.Text = "------";
            lblDeleted.Text = "------";
            lblQueryStat.ForeColor = Color.Black;
            lblArchived.ForeColor = Color.Black;
            lblDeleted.ForeColor = Color.Black;
            clearWBQueriesFields();
        }
        public void lblQueryExecuting()
        {
            lblQueryStat.Text = "QUERY EXECUTING... Please wait...";
            lblQueryStat.ForeColor = Color.Blue;
        }
        public void lblQueryDoneExecuting()
        {
            lblQueryStat.Text = "QUERY EXECUTION DONE!";
            lblQueryStat.ForeColor = Color.Green;
        }
        public void clearWBQueriesFields()
        {
            tbBoundaryDate.Clear();
            tbArchWS.Clear();
            tbArchWD.Clear();
            tbArchCTP.Clear();
            tbArchEBL.Clear();
            tbArchWDA.Clear();
            tbArchOVR.Clear();
            tbPrimWS.Clear();
            tbPrimWD.Clear();
            tbPrimCTP.Clear();
            tbPrimEBL.Clear();
            tbPrimWDA.Clear();
            tbPrimOVR.Clear();
        }

        #endregion FORM FUNCTIONALITIES

        #region BUFFER CHECKERS

        public void lblPrimQueryWS()
        {
            lblDeleted.Text = "Query executing for Work Summary table...";
            lblDeleted.ForeColor = Color.Blue;
        }
        public void lblPrimQueryWD()
        {
            lblDeleted.Text = "Query executing for Work Detail table...";
            lblDeleted.ForeColor = Color.Blue;
        }
        public void lblPrimQueryCTP()
        {
            lblDeleted.Text = "Query executing for Clock Tran Processed table...";
            lblDeleted.ForeColor = Color.Blue;
        }
        public void lblPrimQueryEBL()
        {
            lblDeleted.Text = "Query executing for Emp Balance Log table...";
            lblDeleted.ForeColor = Color.Blue;
        }
        public void lblPrimQueryWDA()
        {
            lblDeleted.Text = "Query executing for Work Detail Adjust table...";
            lblDeleted.ForeColor = Color.Blue;
        }
        public void lblPrimQueryOVR()
        {
            lblDeleted.Text = "Query executing for Override table...";
            lblDeleted.ForeColor = Color.Blue;
        }

        public void lblQueryWS()
        {
            lblArchived.Text = "Query executing for Work Summary table...";
            lblArchived.ForeColor = Color.Blue;
        }
        public void lblQueryWD()
        {
            lblArchived.Text = "Query executing for Work Detail table...";
            lblArchived.ForeColor = Color.Blue;
        }
        public void lblQueryCTP()
        {
            lblArchived.Text = "Query executing for Clock Tran Processed table...";
            lblArchived.ForeColor = Color.Blue;
        }
        public void lblQueryEBL()
        {
            lblArchived.Text = "Query executing for Emp Balance Log table...";
            lblArchived.ForeColor = Color.Blue;
        }
        public void lblQueryWDA()
        {
            lblArchived.Text = "Query executing for Work Detail Adjust table...";
            lblArchived.ForeColor = Color.Blue;
        }
        public void lblQueryOVR()
        {
            lblArchived.Text = "Query executing for Override table...";
            lblArchived.ForeColor = Color.Blue;
        }
        #endregion BUFFER CHECKERS

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
            strEmpUDF = "UPDATE ta00wb.EMP_UDF_DATA @SET EUDFD_EFF_DATE = '" + strEmpUDFEfDate + " 00:00:00.0',@EUDFD_VALUE = '" + strEmpUDFVal + "'@WHERE EMPUDF_ID = (SELECT EMPUDF_ID@FROM ta00wb.EMP_UDF_DEF@WHERE EMPUDF_NAME = 'LAST_ARCHIVE_PAYROLL_DATA_DATE')@AND EMP_ID in (SELECT DISTINCT(EMP_ID)@FROM ARCHIVE.WORK_SUMMARY@WHERE WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.0')@AND TIMESTAMP('" + tbEndDate.Text + " 23:59:59.0'))";
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

        public void selectEmpUDF()
        {
            strEmpUDF = "SELECT COUNT(*) FROM ta00wb.EMP_UDF_DATA @WHERE EMPUDF_ID = (SELECT EMPUDF_ID@FROM ta00wb.EMP_UDF_DEF@WHERE EMPUDF_NAME = 'LAST_ARCHIVE_PAYROLL_DATA_DATE')@AND EMP_ID in (SELECT DISTINCT(EMP_ID)@FROM ARCHIVE.WORK_SUMMARY@WHERE WRKS_WORK_DATE BETWEEN TIMESTAMP('" + tbStartDate.Text + " 00:00:00.0')@AND TIMESTAMP('" + tbEndDate.Text + " 23:59:59.0'))";
            strEmpUDF = strEmpUDF.Replace("@", "" + System.Environment.NewLine);
            rtSelectEmpUDF.Text = strEmpUDF;
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_SUMMARY Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchWS.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_SUMMARY Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbArchWS.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchWD.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbArchWD.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.CLOCK_TRAN_PROCESSED Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchCTP.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.CLOCK_TRAN_PROCESSED Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbArchCTP.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchEBL.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbArchEBL.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL_ADJUST Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchWDA.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL_ADJUST Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbArchWDA.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbArchOVR.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_SUMMARY Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbPrimWS.Text + " rows returned.@";
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
                string strArchWSLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_SUMMARY Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbPrimWS.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbPrimWD.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.WORK_DETAIL Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbPrimWS.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.CLOCK_TRAN_PROCESSED Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbPrimCTP.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.CLOCK_TRAN_PROCESSED Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbPrimCTP.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.EMPLOYEE_BALANCE_LOG Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbPrimEBL.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.EMPLOYEE_BALANCE_LOG Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". "+"Error occured. No" + tbPrimEBL.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL_ADJUST Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.WORK_DETAIL_ADJUST Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " +"Error occured. No" + tbPrimWDA.Text + "rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran TA00WB.OVERRIDE Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbPrimOVR.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " +  "Error occured. No" + tbPrimOVR.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        //in progress gigne00 20151008
        public void querySelectEmpUDFCount()
        {
            DB2Connection conndb2 = new DB2Connection(globalConnStringDb2);
            try
            {
                conndb2.Open();
                DB2Command cmd = conndb2.CreateCommand();
                DB2Transaction trans = conndb2.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandText = "" + rtSelectEmpUDF.Text;
                tbCountEmpUDF.Text = cmd.ExecuteScalar().ToString();
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran COUNT_EMPUDF Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + tbCountEmpUDF.Text + " rows returned.@";
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
                string strLog = System.DateTime.Today.ToShortDateString() + " LOG : Ran ARCHIVE.OVERRIDE Query for " + tbStartDate.Text + " to " + tbEndDate.Text + ". " + "Error occured. No" + tbPrimOVR.Text + "rows returned.@";
                strLog = strLog.Replace("@", "" + System.Environment.NewLine);
                rtLogging.AppendText(strLog);
            }//catch
        }
        #endregion WORKBRAIN DATABASE CONNECTION

        #region INDIVIDUAL BUTTON CLICK (WORKBRAIN DB QUERIES)
        private void btnCheckBoundary_Click(object sender, EventArgs e)
        {
            queryBoundaryDate();
            lblQueryDoneExecuting();            
        }
        private void btnQueryArchWS_Click(object sender, EventArgs e)
        {
            queryArchiveWS();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchWS.Text);
            iHolder = Int64.Parse(tbArchWS.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label22.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label22.Text + " Rows NOT YET archived!";
            }
        }
        private void btnQueryArchWD_Click(object sender, EventArgs e)
        {
            queryArchiveWD();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchWD.Text);
            iHolder = Int64.Parse(tbArchWD.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label23.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label23.Text + " Rows NOT YET archived!";
            }               
        }
        private void btnQueryArchCTP_Click(object sender, EventArgs e)
        {
            queryArchiveCTP();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchCTP.Text);
            iHolder = Int64.Parse(tbArchCTP.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label24.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label24.Text + " Rows NOT YET archived!";
            }
        }
        private void btnQueryArchEBL_Click(object sender, EventArgs e)
        {
            queryArchiveEBL();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchEBL.Text);
            iHolder = Int64.Parse(tbArchEBL.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label25.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label25.Text + " Rows NOT YET archived!";
            }
        }
        private void btnQueryArchWDA_Click(object sender, EventArgs e)
        {
            queryArchiveWDA();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchWDA.Text);
            iHolder = Int64.Parse(tbArchWDA.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label26.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label26.Text + " Rows NOT YET archived!";
            }
        }
        private void btnQueryArchOVR_Click(object sender, EventArgs e)
        {
            queryArchiveOVR();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbArchOVR.Text);
            iHolder = Int64.Parse(tbArchOVR.Text);

            if (iHolder > 0)
            {
                lblArchived.ForeColor = Color.Green;
                lblArchived.Text = label27.Text + " Rows are archived!";
            }
            else if (iHolder == 0)
            {
                lblArchived.ForeColor = Color.Red;
                lblArchived.Text = label27.Text + " Rows NOT YET archived!";
            }
        }
        private void btnQueryPrimWS_Click(object sender, EventArgs e)
        {
            queryPrimaryWS();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimWS.Text);
            iHolder = Int64.Parse(tbPrimWS.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label22.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label22.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryPrimWD_Click(object sender, EventArgs e)
        {
            queryPrimaryWD();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimWD.Text);
            iHolder = Int64.Parse(tbPrimWD.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label23.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label23.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryPrimCTP_Click(object sender, EventArgs e)
        {
            queryPrimaryCTP();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimCTP.Text);
            iHolder = Int64.Parse(tbPrimCTP.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label24.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label24.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryPrimEBL_Click(object sender, EventArgs e)
        {
            queryPrimaryEBL();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimEBL.Text);
            iHolder = Int64.Parse(tbPrimEBL.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label25.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label25.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryPrimWDA_Click(object sender, EventArgs e)
        {
            queryPrimaryWDA();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimWDA.Text);
            iHolder = Int64.Parse(tbPrimWDA.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label26.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label26.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryPrimOVR_Click(object sender, EventArgs e)
        {
            queryPrimaryOVR();
            lblQueryDoneExecuting();
            // [gigne00] : 20151013 - Insert this method after lblQueryDoneExecuting();
            // this monitors if value from the textbox has already been archived/deleted or not
            Int64 iHolder;
            iHolder = Convert.ToInt64(tbPrimOVR.Text);
            iHolder = Int64.Parse(tbPrimOVR.Text);

            if (iHolder == 0)
            {
                lblDeleted.ForeColor = Color.Green;
                lblDeleted.Text = label27.Text + " Rows are deleted!";
            }
            else if (iHolder > 0)
            {
                lblDeleted.ForeColor = Color.Red;
                lblDeleted.Text = label27.Text + " Rows NOT YET deleted!";
            }
        }
        private void btnQueryCountEmpUDF_Click(object sender, EventArgs e)
        {
            querySelectEmpUDFCount();
            lblQueryDoneExecuting();
        }
        #endregion
        
        #region MOUSE DOWN EVENTS
        private void btnCheckBoundary_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
        }
        private void btnQueryArchWS_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryWS();
        }
        private void btnQueryArchWD_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryWD();
        }
        private void btnQueryArchCTP_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryCTP();
        }
        private void btnQueryArchEBL_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryEBL();
        }
        private void btnQueryArchWDA_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryWDA();
        }
        private void btnQueryArchOVR_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblQueryOVR();
        }
        private void btnQueryPrimWS_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryWS();
        }
        private void btnQueryPrimWD_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryWD();
        }
        private void btnQueryPrimCTP_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryCTP();
        }
        private void btnQueryPrimEBL_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryEBL();
        }
        private void btnQueryPrimWDA_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryWDA();
        }
        private void btnQueryPrimOVR_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
            lblPrimQueryOVR();
        }
        private void btnQueryCountEmpUDF_MouseDown(object sender, MouseEventArgs e)
        {
            lblQueryExecuting();
        }
        #endregion MOUSE DOWN EVENTS

        // [gigne00] 20160120 - Added about the tool form
        private void aboutTheToolToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form_AboutTheTool frmAbout = new Form_AboutTheTool();
            frmAbout.ShowDialog();
        }

        // [gigne00] 20160120 - Added copy to clipboard functionality for copy buttons
        private void button15_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtUpdateEmpUDF.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied.ForeColor = Color.Green;
            lblCopied.Text = "Update EmpUDFData copied to clipboard!";
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtUpdateRegistry.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied.ForeColor = Color.Green;
            lblCopied.Text = "Update Registry Script copied to clipboard!";
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtSelectEmpUDF.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied.ForeColor = Color.Green;
            lblCopied.Text = "Select EmpUDFData Script copied to clipboard!";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtCheckBoundary.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied.ForeColor = Color.Green;
            lblCopied.Text = "Check Boundary Date script copied to clipboard!";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveWS.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Work_Summary copied to clipboard!";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveWS.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Work_Detail copied to clipboard!";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveCTP.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Clock_Tran_Processed copied to clipboard!";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveEBL.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Employee_Balance_Log copied to clipboard!";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveWDA.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Work_Detail_Adjust copied to clipboard!";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtArchiveOVR.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied2.ForeColor = Color.Green;
            lblCopied2.Text = "Select script for Archive.Override copied to clipboard!";
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryWS.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Work_Summary copied to clipboard!";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryWD.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Work_Detail copied to clipboard!";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryCTP.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Clock_Tran_Processed copied to clipboard!";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryEBL.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Employee_Balance_Log copied to clipboard!";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryWDA.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Work_Detail_Adjust copied to clipboard!";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();    //Clear if any old value is there in Clipboard        
            Clipboard.SetText(rtPrimaryOVR.Text); //Copy text to Clipboard
            string strClip = Clipboard.GetText(); //Get text from Clipboard
            lblCopied3.ForeColor = Color.Green;
            lblCopied3.Text = "Select script for ta00wb.Override copied to clipboard!";
        }



        #region SFTP CONNECTION FUNCTIONALITY

        #endregion SFTP CONNERCTION FUNCTIONALITY

    }
}
