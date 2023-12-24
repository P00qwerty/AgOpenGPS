﻿using System;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormEditAB : Form
    {
        private readonly FormGPS mf = null;

        private double snapAdj = 0;
        private bool isClosing;

        public FormEditAB(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;

            InitializeComponent();

            this.Text = gStr.gsEditABLine;
            nudSnapDistance.Controls[0].Enabled = false;

            label1.Text = mf.unitsInCm;
        }

        private void FormEditAB_Load(object sender, EventArgs e)
        {
            if (mf.isMetric)
            {
                nudSnapDistance.DecimalPlaces = 0;
                nudSnapDistance.Value = (int)((double)Properties.Settings.Default.setAS_snapDistance * mf.cm2CmOrIn);
            }
            else
            {
                nudSnapDistance.DecimalPlaces = 1;
                nudSnapDistance.Value = (decimal)Math.Round(((double)Properties.Settings.Default.setAS_snapDistance * mf.cm2CmOrIn), 1);
            }

            snapAdj = Properties.Settings.Default.setAS_snapDistance*0.01;

            //label1.Text = mf.unitsInCm;
            btnCancel.Focus();
            //lblHalfSnapFtM.Text = mf.unitsFtM;
            //lblHalfWidth.Text = (mf.tool.width * 0.5 * mf.m2FtOrM).ToString("N2");
            tboxHeading.Text = Math.Round(glm.toDegrees(mf.ABLine.abHeading), 5).ToString();
            lblHalfToolWidth.Text = ((mf.tool.width - mf.tool.overlap) * 0.5 * mf.m2InchOrCm).ToString("N0") + mf.unitsInCm;

            Location = Properties.Settings.Default.setWindow_abLineEditLocation;
        }

        private void tboxHeading_Click(object sender, EventArgs e)
        {
            tboxHeading.Text = "";

            using (FormNumeric form = new FormNumeric(0, 360, Math.Round(glm.toDegrees(mf.ABLine.abHeading), 5)))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    tboxHeading.Text = ((double)form.ReturnValue).ToString();
                    mf.ABLine.abHeading = glm.toRadians((double)form.ReturnValue);
                    mf.ABLine.SetABLineByHeading();
                }
                else tboxHeading.Text = Math.Round(glm.toDegrees(mf.ABLine.abHeading), 5).ToString();
            }

            mf.ABLine.isABValid = false;
        }

        private void nudSnapDistance_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NumericUpDown)sender, this);
            snapAdj = (double)nudSnapDistance.Value * mf.inchOrCm2m;
            Properties.Settings.Default.setAS_snapDistance = snapAdj * 100;
            Properties.Settings.Default.Save();
        }

        private void btnAdjRight_Click(object sender, EventArgs e)
        {
            mf.ABLine.MoveABLine(snapAdj);
        }

        private void btnAdjLeft_Click(object sender, EventArgs e)
        {
            mf.ABLine.MoveABLine(-snapAdj);
        }

        private void bntOk_Click(object sender, EventArgs e)
        {
            isClosing = true;
            if (mf.ABLine.isABLineSet)
            {
                //index to last one.
                int idx = mf.ABLine.numABLineSelected - 1;

                if (idx >= 0)
                {
                    mf.ABLine.lineArr[idx].heading = mf.ABLine.abHeading;
                    //calculate the new points for the reference line and points
                    mf.ABLine.lineArr[idx].ptA.easting = mf.ABLine.refPtA.easting;
                    mf.ABLine.lineArr[idx].ptA.northing = mf.ABLine.refPtA.northing;
                }

                mf.FileSaveABLines();

                mf.panelRight.Enabled = true;
            }
            mf.ABLine.moveDistance = 0;
            mf.ABLine.isABValid = false;

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isClosing = true;
            if (mf.ABLine.isABLineSet && mf.isJobStarted)
            {
                int last = mf.ABLine.numABLineSelected;
                mf.FileLoadABLines();

                mf.ABLine.numABLineSelected = last;
                mf.ABLine.refPtA = mf.ABLine.lineArr[mf.ABLine.numABLineSelected - 1].ptA;
                mf.ABLine.abHeading = mf.ABLine.lineArr[mf.ABLine.numABLineSelected - 1].heading;
                mf.ABLine.SetABLineByHeading();
                mf.ABLine.isABLineSet = true;
                mf.ABLine.moveDistance = 0;

                mf.panelRight.Enabled = true;
                mf.ABLine.isABValid = false;
            }
            Close();
        }

        private void btnContourPriority_Click(object sender, EventArgs e)
        {
            if (mf.ABLine.isABLineSet)
            {
                mf.ABLine.MoveABLine(mf.ABLine.distanceFromCurrentLinePivot);
            }
        }

        private void btnNudgeHalfToolRight_Click(object sender, EventArgs e)
        {
            double dist = mf.tool.width - mf.tool.overlap;

            mf.ABLine.MoveABLine(dist * 0.5);
        }

        private void btnNudgeHalfToolLeft_Click(object sender, EventArgs e)
        {
            double dist = mf.tool.width - mf.tool.overlap;

            mf.ABLine.MoveABLine(-dist * 0.5);
        }

        private void btnNoSave_Click(object sender, EventArgs e)
        {
            isClosing = true;
            mf.ABLine.isABValid = false;
            Close();
        }

        private void cboxDegrees_SelectedIndexChanged(object sender, EventArgs e)
        {
            mf.ABLine.abHeading = glm.toRadians(double.Parse(cboxDegrees.SelectedItem.ToString()));
            mf.ABLine.SetABLineByHeading();
            tboxHeading.Text = Math.Round(glm.toDegrees(mf.ABLine.abHeading), 5).ToString();
        }

        private void FormEditAB_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isClosing)
            {
                e.Cancel = true;
                return;
            }

            Properties.Settings.Default.setWindow_abLineEditLocation = Location;
            Properties.Settings.Default.Save();
        }

        private void btnCancel_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show(gStr.ha_btnCancel, gStr.gsHelp);
        }

        private void btnNoSave_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show(gStr.he_btnNoSave, gStr.gsHelp);
        }

        private void btnOK_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show(gStr.he_btnOK, gStr.gsHelp);
        }

        private void btnContourPriority_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show(gStr.h_btnSnapToPivot, gStr.gsHelp);
        }

    }
}