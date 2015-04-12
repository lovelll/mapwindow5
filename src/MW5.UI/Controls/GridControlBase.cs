﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.Grouping;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Grid.Grouping;

namespace MW5.UI.Controls
{
    public class GridControlBase: GridGroupingControl
    {
        public GridControlBase()
        {
            InitStyle();

            InitGroupOptions();

            InitCurrentCell();

            InitRowSelection();
        }

        private void InitStyle()
        {
            Appearance.AnyCell.VerticalAlignment = GridVerticalAlignment.Middle;
            Appearance.AnyCell.Borders.All = new GridBorder(GridBorderStyle.None, Color.White);
            GridLineColor = Color.White;
            BrowseOnly = false;
            ShowRowHeaders = false;
            ShowColumnHeaders = true;
        }

        private void InitGroupOptions()
        {
            ShowGroupDropArea = false;
            TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
            TopLevelGroupOptions.ShowCaption = false;
        }

        private void InitCurrentCell()
        {
            Table.TableOptions.ListBoxSelectionCurrentCellOptions = GridListBoxSelectionCurrentCellOptions.None;
            ShowCurrentCellBorderBehavior = GridShowCurrentCellBorder.HideAlways;
            ActivateCurrentCellBehavior = GridCellActivateAction.None;
        }

        private void InitRowSelection()
        {
            
            TableOptions.ListBoxSelectionMode = SelectionMode.One;
            TableOptions.SelectionBackColor = Color.FromArgb(64, 51, 153, 255);
            TableOptions.SelectionTextColor = Color.Black;
            TableOptions.ListBoxSelectionColorOptions = GridListBoxSelectionColorOptions.ApplySelectionColor;

            // any option other than None will disable SelectedRecordsChanged event
            // http ://www.syncfusion.com/forums/46745/grid-grouping-control-selection-color
            TableOptions.AllowSelection = GridSelectionFlags.None;
        }
    }
}
