using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DVO_STORE_DB
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Item> Items { get; set; }
        public ObservableCollection<SelectedItem> Selected { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            Items = new ObservableCollection<Item>
            {
                new Item{ Code="55157CSE", Name="BOOK PAPER IK ONE A4 70gsm 500s 5 RMS (CSE)", Price="₱ 890.00", Avail="0", Soh="-2", QtyCom="0"},
                new Item{ Code="55142CSE", Name="BOOK PAPER IK ONE 8-1/2in x 11in 80gsm 500s 5 RMS (CSE)", Price="₱ 980.00", Avail="0", Soh="0", QtyCom="0"},
                new Item{ Code="55157P60", Name="BOOK PAPER IK ONE A4 70gsm 60/1 (PLT)", Price="₱ 0.00", Avail="0", Soh="0", QtyCom="0"},
                new Item{ Code="55143CSE", Name="BOOK PAPER IK ONE 8-1/2in x 13in 80gsm 500s 5 RMS (CSE)", Price="₱ 1,155.00", Avail="0", Soh="0", QtyCom="0"},
                new Item{ Code="55144RMS", Name="BOOK PAPER IK ONE 8-1/4in x 11-3/4in 80gsm 500s (RMS)", Price="₱ 203.00", Avail="27,361", Soh="27373", QtyCom="12"},
                new Item{ Code="55142RMS", Name="BOOK PAPER IK ONE 8-1/2in x 11in 80gsm 500s (RMS)", Price="₱ 196.00", Avail="17,110", Soh="17122", QtyCom="12"},
                new Item{ Code="55158RMS", Name="BOOK PAPER IK ONE 8-1/2in x 11in 70gsm 500s (RMS)", Price="₱ 171.00", Avail="108,951", Soh="109435", QtyCom="484"},
                new Item{ Code="55159RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 70gsm 500s (RMS)", Price="₱ 203.00", Avail="95,776", Soh="95987", QtyCom="211"},
                new Item{ Code="55143RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 80gsm 500s (RMS)", Price="₱ 220.00", Avail="34,585", Soh="34602", QtyCom="17"},
                new Item{ Code="55157RMS", Name="BOOK PAPER IK ONE A4 70gsm 500s (RMS)", Price="₱ 178.00", Avail="68,534", Soh="71238", QtyCom="2704"},
            };
            DgvInventory.ItemsSource = Items;
            DgvInventory.SelectedIndex = 8;

            Selected = new ObservableCollection<SelectedItem>
            {
                new SelectedItem{ Code="55143RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 80gsm 500s (RMS)", Qty=5, Price="₱ 220.00", Total="₱ 1,100.00", Last="₱ 220.00"}
            };
            DgvSelected.ItemsSource = Selected;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Work Order saved! Total {TxtTotal.Text}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DgvInventory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgvInventory.SelectedItem is Item item)
                MessageBox.Show($"Added {item.Code} to order");
        }
    }

    public class Item
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Avail { get; set; }
        public string Soh { get; set; }
        public string QtyCom { get; set; }
    }

    public class SelectedItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public string Price { get; set; }
        public string Total { get; set; }
        public string Last { get; set; }
    }
}
