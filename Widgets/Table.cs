using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; 
namespace HandHero.Widgets
{
    internal class CostumTable : Grid
    {


        private Grid CreateGridRow(Grid gridy , int? row, int[] arr, UIElement[] widget = null)
        {
            if (arr.Length != row && !(row is null))
                throw new Exception("THE ARRAY DOESNT FIT ");
            if ((row is null) && arr.Length != 1)
                throw new Exception("THE ARRAY DOESNT FIT ");
            for (int i = 0; i < row; i++)
            {
                RowDefinition tmp = new RowDefinition();
                tmp.Height = new GridLength(arr[i], GridUnitType.Star);
                gridy.RowDefinitions.Add(tmp);
                if (widget != null && widget.Length != 0 && widget.Length == arr.Length)
                {
                    Grid.SetRow(widget[i], i);
                    gridy.Children.Add(widget[i]);
                }
            }
            return gridy;



        }
        private Grid CreateGridColumn( Grid gridy , int? column, int[] arr, UIElement[] widget = null)
        {
            if (arr.Length != column && !(column is null))
                throw new Exception("THE ARRAY DOESNT FIT ");
            if ((column is null) && arr.Length != 1)
                throw new Exception("THE ARRAY DOESNT FIT ");
       
            for (int i = 0; i < column; i++)
            {
                ColumnDefinition tmp = new ColumnDefinition();
                tmp.Width = new GridLength(arr[i], GridUnitType.Star);
                gridy.ColumnDefinitions.Add(tmp);
                if (widget != null && widget.Length != 0 && widget.Length == arr.Length)
                {
                    Grid.SetColumn(widget[i], i);
                    gridy.Children.Add(widget[i]);
                }
            }
            return gridy;



        }
        public CostumTable(  int rows , int cols , UIElement [ , ]  widgets ) { 
        

            UIElement [] widgetsRow = new UIElement[rows];
            for (int i = 0; i < rows; i++) {
                UIElement[] widgetsCols = new UIElement[cols];
                for (int j = 0; j < cols; j++) {

                    widgetsCols[j] = widgets[i,j];
                }
                widgetsRow[i] = this.CreateGridColumn(new Grid(), cols, new int[cols].Select(x => 1).ToArray(), widgetsCols); 
            }
            this.CreateGridRow(this, rows, new int[rows].Select(x => 1).ToArray(), widgetsRow); 


        } 




    }
}
