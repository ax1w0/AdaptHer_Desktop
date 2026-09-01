using AdaptHER.Model;
using AdaptHER.Model.Models;
using AdaptHER.UI.Pages;
using AdaptHER.Class;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AdaptHER.UI.ViewModels;
using System.Windows.Controls.Primitives;

namespace AdaptHER.Tests
{
    [TestClass]
    public class AdaptProgramsPgTests
    {
        private Mock<IFeedbackService> _mockFeedbackService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IModulesProgService> _mockModulesProgService;
        private Mock<IEmployeeService> _mockEmployeeService;
        private AdaptProgramsPg _page;

        [TestInitialize]
        public void Setup()
        {
            _mockFeedbackService = new Mock<IFeedbackService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockModulesProgService = new Mock<IModulesProgService>();
            _mockEmployeeService = new Mock<IEmployeeService>();

            _page = new AdaptProgramsPg(
                 _mockFeedbackService.Object,
                 _mockNotificationService.Object,
                 _mockModulesProgService.Object,
                 _mockEmployeeService.Object)
            {
                selectEmploy = new ComboBox(),
                selectGroup = new ComboBox(),
                selectJob = new ComboBox(),
                selectModules = new ListView { SelectionMode = SelectionMode.Multiple },
                selectTeach = new ListView { SelectionMode = SelectionMode.Multiple },
                newEmployeeSearch = new TextBox(),
                mentorsSearch = new TextBox(),
                FormProg = new Button()
            };
        }
        [TestMethod]
        public void FormProg_ClickBtn_WithMissingRequiredFields_ShouldShowNotifications()
        {
            _mockNotificationService.Setup(n => n.ShowNotification(It.IsAny<Window>(),
                                                                   It.IsAny<string>(),
                                                                   It.IsAny<int>()));
            _page.FormProg_ClickBtn(1, null);
        }
        [TestMethod]
        public void Constructor_ShouldInitializeComponents()
        {
            Assert.IsNotNull(_page.selectEmploy);
            Assert.IsNotNull(_page.selectGroup);
            Assert.IsNotNull(_page.selectJob);
            Assert.IsNotNull(_page.selectModules);
            Assert.IsNotNull(_page.selectTeach);
            Assert.IsNotNull(_page.newEmployeeSearch);
            Assert.IsNotNull(_page.mentorsSearch);
            Assert.IsNotNull(_page.FormProg);
        }
        [TestMethod]
        public void SelectModules_SelectionChanged_ShouldUpdateSelectedModules()
        {
            var testModule = new ComboBoxValueItem<int> { Value = 1, DisplayText = "Module 1" };
            var args = new SelectionChangedEventArgs(Selector.SelectionChangedEvent,
                new List<object>(),
                new List<object> { testModule });
            _page.SelectModules_SelectionChanged(null, args);
            Assert.IsTrue(_page.selectedTeaches.ContainsKey(testModule.Value));
        }
        [TestMethod]
        public void SelectTeach_SelectionChanged_ShouldUpdateSelectedTeaches()
        {
            _page.selectedModule = 1;
            _page.selectedTeaches[1] = new List<ComboBoxValueItem<int>>();
            var testTeach = new ComboBoxValueItem<int> { Value = 1, DisplayText = "Mentor 1" };
            var args = new SelectionChangedEventArgs(Selector.SelectionChangedEvent,
                new List<object>(),
                new List<object> { testTeach });
            _page.SelectTeach_SelectionChanged(null, args);
            Assert.AreEqual(1, _page.selectedTeaches[1].Count);
            Assert.AreEqual(testTeach, _page.selectedTeaches[1][0]);
        }

        [TestMethod]
        public void SearchEmployees_WithEmptyQuery_ShouldShowNoResults()
        {
            _mockEmployeeService.Setup(s => s.SearchEmployees(It.IsAny<string>()))
                               .Returns(new List<ComboBoxValueItem<int>>());
        }
        [TestMethod]
        public void SearchEmployees_WithValidQuery_ShouldUpdateComboBox()
        {
            var testEmployees = new List<ComboBoxValueItem<int>>
            {
                new ComboBoxValueItem<int> { DisplayText = "Ivanov Ivan", Value = 1 }
            };
            _mockEmployeeService.Setup(s => s.SearchEmployees("Ivanov"))
                              .Returns(testEmployees);
        }
        [TestMethod]
        public void SearchMentors_WithValidQuery_ShouldUpdateComboBox()
        {
            var testMentors = new List<ComboBoxValueItem<int>>
            {
                new ComboBoxValueItem<int> { DisplayText = "Petrov Petr", Value = 2 }
            };
            _mockEmployeeService.Setup(s => s.SearchMentors("Petrov"))
                              .Returns(testMentors);
        }
    }
}