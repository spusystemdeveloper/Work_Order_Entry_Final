using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;

namespace DVO_STORE_DB
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<Item> Inventory { get; set; }
        public ObservableCollection<SelectedItem> Selected { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            LoadData();
        }
        void LoadData()
        {
            Inventory = new ObservableCollection<Item>
            {
                new Item{ Code="55157CSE", Name="BOOK PAPER IK ONE A4 70gsm", Price="₱ 890.00", Avail="0", Soh="-2", QtyCom="0"},
                new Item{ Code="55144RMS", Name="BOOK PAPER IK ONE 8-1/4in x 11-3/4in 80gsm", Price="₱ 203.00", Avail="27,361", Soh="27373", QtyCom="12"},
                new Item{ Code="55158RMS", Name="BOOK PAPER IK ONE 8-1/2in x 11in 70gsm", Price="₱ 171.00", Avail="108,951", Soh="109435", QtyCom="484"},
                new Item{ Code="55159RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 70gsm", Price="₱ 203.00", Avail="95,776", Soh="95987", QtyCom="211"},
                new Item{ Code="55143RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 80gsm", Price="₱ 220.00", Avail="34,585", Soh="34602", QtyCom="17"},
                new Item{ Code="55157RMS", Name="BOOK PAPER IK ONE A4 70gsm", Price="₱ 178.00", Avail="68,534", Soh="71238", QtyCom="2704"},
            };
            InventoryGrid.ItemsSource = Inventory;
            Selected = new ObservableCollection<SelectedItem>
            {
                new SelectedItem{ Code="55143RMS", Name="BOOK PAPER IK ONE 8-1/2in x 13in 80gsm", Qty=5, Price="₱ 220.00", Total="₱ 1,100.00"}
            };
            SelectedGrid.ItemsSource = Selected;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var d = new ContentDialog{ Title="Saved", Content=$"Total {TxtTotal.Text}", CloseButtonText="OK", XamlRoot=this.Content.XamlRoot };
            _ = d.ShowAsync();
        }
        private void InventoryGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item it) Selected.Add(new SelectedItem{ Code=it.Code, Name=it.Name, Qty=1, Price=it.Price, Total=it.Price});
        }
    }
    public class Item{ public string Code{get;set;} public string Name{get;set;} public string Price{get;set;} public string Avail{get;set;} public string Soh{get;set;} public string QtyCom{get;set;} }
    public class SelectedItem{ public string Code{get;set;} public string Name{get;set;} public int Qty{get;set;} public string Price{get;set;} public string Total{get;set;} }
}
