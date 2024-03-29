﻿using scraper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace scraper.Attached
{
    public class DataGridAttachedProperties
    {


        public static IEnumerable<Field>  GetColumnsMi(DataGrid obj)
        {
            return (IEnumerable<Field>) obj.GetValue(ColumnsMiProperty);
        }

        public static void SetColumnsMi(DataGrid obj, int value)
        {
            obj.SetValue(ColumnsMiProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsMiProperty =
            DependencyProperty.RegisterAttached("ColumnsMi", typeof(IEnumerable<Field>), typeof(DataGridAttachedProperties), new PropertyMetadata(null,hndlPropertyChangedCallback));

        private static void hndlPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Adding columns");

            DataGrid dg = d as DataGrid;
            IEnumerable<Field> fields = e.NewValue as IEnumerable<Field>;
            if (fields == null) {
                dg.Columns.Clear();return;
            }
            dg.Columns.Clear();

            foreach (var f in fields)
            {
                if (f.IsDataGridDisabled) continue;
                Debug.WriteLine($"Adding column: {f.UIName}");
                DataGridTextColumn c = new DataGridTextColumn();
                TextBlock htb = new TextBlock();
                htb.Text = f.UIName;
                htb.TextTrimming = TextTrimming.CharacterEllipsis;
                
                c.Header = htb;
                c.Binding = new Binding($"Model.{f.Name}" );
                htb.ToolTip = f.UIName;
                if (f.UserDescription!=null) htb.ToolTip = f.UserDescription;
                c.Width = f.UIHeaderWidth;
                dg.Columns.Add(c);
            }
            return ;
        
        }
    }
}
