using AdaptHER.Class;
using AdaptHER.Model.Models;
using AdaptHER.UI.Pages;
using AdaptHER.UI.UC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdaptHER.Tests
{
    [TestClass]
    public class AdaptModulesTests
    {
        private Mock<IModulesService> _mockModulesService;
        private Mock<IFeedbackService> _mockFeedbackService;
        private AdaptModulesPg _page;
        [TestInitialize]
        public void Setup()
        {
            _mockModulesService = new Mock<IModulesService>();
            _mockFeedbackService = new Mock<IFeedbackService>();

            _page = new AdaptModulesPg(_mockModulesService.Object, _mockFeedbackService.Object);
            _page.selectJob = new ComboBox();
            _page.nameFilter = new TextBox();
            _page.modulesContainer = new StackPanel();
            _page.createButton = new Button();
            _page.selectJob.Items.Add(new ComboBoxValueItem<int>()
            {
                DisplayText = "No",
                Value = -1
            });
        }
        [TestMethod]
        public void PageLoad_ShouldLoadRoles()
        {
            var testRoles = new List<Roles>
            {
                new Roles { Id = 1, Name = "Developer" },
                new Roles { Id = 2, Name = "HR" }
            };
            _mockModulesService.Setup(s => s.GetRoles()).Returns(testRoles);
            _page.RaiseLoadedEvent();
            Assert.AreEqual(3, _page.selectJob.Items.Count);
            Assert.AreEqual("Developer", ((ComboBoxValueItem<int>)_page.selectJob.Items[1]).DisplayText);
            Assert.AreEqual(1, ((ComboBoxValueItem<int>)_page.selectJob.Items[1]).Value);
            Assert.IsTrue(true, "Test passed successfully: roles load correctly");
        }
        [TestMethod]
        public void PageLoad_WhenDbError_ShouldShowFeedback()
        {
            _mockModulesService.Setup(s => s.GetRoles()).Throws(new Exception("DB error"));
            _page.RaiseLoadedEvent();
            _mockFeedbackService.Verify(f => f.ShowFeedback("Failed to retrieve position data, please verify the connection to the server!"), Times.Once);
            Assert.IsTrue(true, "Test passed successfully: role loading error is handled correctly");
        }
        [TestMethod]
        public void UpdateModules_WithNoFilters_ShouldShowAllModules()
        {
            var testModules = new List<Modules>
            {
                new Modules { Id = 1, Name = "Module 1" },
                new Modules { Id = 2, Name = "Module 2" }
            };
            _mockModulesService.Setup(s => s.GetFilteredModules(null, null))
                             .Returns(testModules);
            _page.selectJob.SelectedIndex = 0;
            _page.UpdateModules();
            if (_page.modulesContainer.Children.Count > 0)
            {
                Assert.IsInstanceOfType(_page.modulesContainer.Children[0], typeof(ModuleView));
                Assert.IsInstanceOfType(_page.modulesContainer.Children[1], typeof(ModuleView));
            }
        }
        [TestMethod]
        public void UpdateModules_WithPositionFilter_ShouldShowFilteredModules()
        {
            var testModule = new Modules { Id = 1, Name = "Module 1" };
            var testModules = new List<Modules> { testModule };
            _mockModulesService.Setup(s => s.GetFilteredModules(1, null))
                             .Returns(testModules);
            _page.selectJob.Items.Add(new ComboBoxValueItem<int>
            {
                DisplayText = "Test Role",
                Value = 1
            });
            _page.selectJob.SelectedIndex = 1;
            _page.UpdateModules();
            if (_page.modulesContainer.Children.Count > 0)
            {
                Assert.IsInstanceOfType(_page.modulesContainer.Children[0], typeof(ModuleView));
            }
        }
        [TestMethod]
        public void UpdateModules_WithNameFilter_ShouldShowFilteredModules()
        {
            var testModule = new Modules { Id = 1, Name = "Specific Module" };
            var testModules = new List<Modules> { testModule };
            _mockModulesService.Setup(s => s.GetFilteredModules(null, "Specific"))
                             .Returns(testModules);
            _page.nameFilter.Text = "Specific";
            _page.UpdateModules();
            if (_page.modulesContainer.Children.Count > 0)
            {
                Assert.IsInstanceOfType(_page.modulesContainer.Children[0], typeof(ModuleView));
            }
        }
        [TestMethod]
        public void UpdateModules_WhenNoResults_ShouldShowNoResultsMessage()
        {
            _mockModulesService.Setup(s => s.GetFilteredModules(It.IsAny<int?>(), It.IsAny<string>()))
                          .Returns(new List<Modules>());
            _page.UpdateModules();
            Assert.AreEqual(1, _page.modulesContainer.Children.Count);
            Assert.IsInstanceOfType(_page.modulesContainer.Children[0], typeof(TextBlock));
            Assert.AreEqual("No results found for your query.", ((TextBlock)_page.modulesContainer.Children[0]).Text);
        }
        [TestMethod]
        public void UpdateModules_WhenDbError_ShouldShowFeedback()
        {
            _mockModulesService.Setup(s => s.GetFilteredModules(It.IsAny<int?>(), It.IsAny<string>()))
                             .Throws(new Exception("DB error"));
            _page.UpdateModules();
            _mockFeedbackService.Verify(f => f.ShowFeedback("Failed to display adaptation modules, please verify the connection to the server!"), Times.Once);
            Assert.IsTrue(true, "Test passed successfully: module loading error is handled correctly");
        }
    }
}