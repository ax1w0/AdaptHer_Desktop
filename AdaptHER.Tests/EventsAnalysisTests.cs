using AdaptHER.Class;
using AdaptHER.Model.Models;
using AdaptHER.UI.Pages;
using AdaptHER.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AdaptHER.Tests
{
    [TestClass]
    public class EventsAnalysisPgComboBoxTests
    {
        private EventsAnalysisPg _page;
        private Mock<IFeedbackService> _mockFeedbackService;

        [TestInitialize]
        public void Setup()
        {
            _mockFeedbackService = new Mock<IFeedbackService>();
            _page = new EventsAnalysisPg(_mockFeedbackService.Object);
            _page.InitializeComboBoxes();
        }

        [TestMethod]
        public void JobComboBox_ShouldContainDefaultItem()
        {
            Assert.AreEqual("Нет", (_page.selectJob.Items[0] as ComboBoxValueItem<int>)?.DisplayText);
            Assert.AreEqual(-1, (_page.selectJob.Items[0] as ComboBoxValueItem<int>)?.Value);
        }

        [TestMethod]
        public void GroupComboBox_ShouldContainDefaultItem()
        {
            Assert.AreEqual("Нет", (_page.selectGroup.Items[0] as ComboBoxValueItem<int>)?.DisplayText);
            Assert.AreEqual(-1, (_page.selectGroup.Items[0] as ComboBoxValueItem<int>)?.Value);
        }

        [TestMethod]
        public void ViewComboBox_ShouldHaveTwoItems()
        {
            Assert.AreEqual(2, _page.view.Items.Count);
        }

        [TestMethod]
        public void SelectingJob_ShouldTriggerUpdateModules()
        {
            var updateCalled = false;
            _page.UpdateModulesAction = () => updateCalled = true;
            _page.selectJob.SelectedIndex = 1;
            _page.SelectJob_SelectionChanged(null, null);
            Assert.IsTrue(updateCalled);
        }

        [TestMethod]
        public void SelectingGroup_ShouldTriggerUpdateModules()
        {
            var updateCalled = false;
            _page.UpdateModulesAction = () => updateCalled = true;
            _page.selectGroup.SelectedIndex = 1;
            _page.SelectGroup_SelectionChanged(null, null);
            Assert.IsTrue(updateCalled);
        }

        [TestMethod]
        public void SelectingView_ShouldTriggerUpdateModules()
        {
            var updateCalled = false;
            _page.UpdateModulesAction = () => updateCalled = true;
            _page.view.SelectedIndex = 1;
            _page.View_SelectionChanged(null, null);
            Assert.IsTrue(updateCalled);
        }

        [TestMethod]
        public void InitialViewSelection_ShouldBeFirstItem()
        {
            var updateCalled = false;
            _page.UpdateModulesAction = () => updateCalled = true;
            _page.view.SelectedIndex = 1;
            _page.View_SelectionChanged(null, null);
            Assert.IsTrue(updateCalled);
        }
    }
}