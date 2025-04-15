using Inv_M_Sys.Models;
using Inv_M_Sys.ViewModels;
using System.Collections.ObjectModel;
using Xunit;

namespace Inv_M_Sys.Tests
{
    /// <summary>
    /// This class is just for examnarbete, since unit testing take so much time for a six weeks project. this is just example to show the tech. and that i know it.
    /// </summary>
    public class AwaitingPageViewModelTests
    {
        [Fact]
        public void SearchOrders_FiltersOrdersByCustomerName()
        {
            // Arrange
            var vm = new AwaitingPageViewModel();

            vm.SelectedSearchCriteria = "Customer";
            vm.OrdersList = new ObservableCollection<Order>
            {
                new Order { Id = 1, CustomerName = "Alice", DeliveryDate = DateTime.Today, TotalPrice = 100, Status = OrderStatus.Pending },
                new Order { Id = 2, CustomerName = "Bob", DeliveryDate = DateTime.Today, TotalPrice = 200, Status = OrderStatus.Shipped },
                new Order { Id = 3, CustomerName = "Charlie", DeliveryDate = DateTime.Today, TotalPrice = 300, Status = OrderStatus.Cancelled }
            };

            // Backup list to simulate fullOrderList
            var backupList = new ObservableCollection<Order>(vm.OrdersList);
            typeof(AwaitingPageViewModel)
                .GetField("_fullOrderList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(vm, backupList);

            vm.SearchQuery = "Bob";

            // Act
            vm.SearchCommand.Execute(null);

            // Assert
            Assert.Single(vm.OrdersList);
            Assert.Equal("Bob", vm.OrdersList[0].CustomerName);
        }
    }
}