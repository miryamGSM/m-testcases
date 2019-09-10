using System;
using System.Collections.Generic;
using HodderAuthenticationModule.Models.DataIntegration;
using Moq;
using NUnit.Framework;
using Passport.Models.UserInfo;
using System.Linq;
using HodderAuthenticationModule.Context.Models;
using HodderAuthenticationModule.Models.ViewModels.DataIntegration;
using Passport.Models.Application;
using Passport.Models.DataIntegration;
using Passport.Models.InstitutionInfo;

namespace HodderAuthenticationModule.UnitTests.HamService.DataIntegrationTests
{
    [TestFixture]
    public class StudentDataIntegratorTests
    {
        private DataIntegrationTestsContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new DataIntegrationTestsContainer();
            _container.Setup();
        }

        [Test]
        public void Sync_StudentNameChanged_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").FirstName = "Changed";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.FirstName == "Changed"),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_NewUserInSyncedClass_StudentAdded()
        {
            _container.DiStudents.Add(new DiStudent
            {
                Id = "100",
                Gender = "M",
                YearGroupName = _container.DiYearGroups.Single(yg => yg.Id == "3").Name,
                IntegratorYearGroupId = _container.DiYearGroups.Single(yg => yg.Id == "3").Id,
                Upn = "100",
                CanImport = true,
                YearCode = _container.DiYearGroups.Single(yg => yg.Id == "3").YearCode,
                FirstName = "First100",
                MiddleName = "Middle100",
                LastName = "Last100",
                ClassName = _container.DiClasses.Single(yg => yg.Id == "3").Name,
                IntegratorClassId = _container.DiClasses.Single(yg => yg.Id == "3").Id,
                MisId = "mis100",
                MisDateOfBirthAsString  = "2000-02-28"
            });

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "100" && u.FirstName == "First100"),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                Times.Once, "Correct user added.");

            _container.UserAccountServiceMock.Verify(m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "No users updated");
        }

        [Test]
        public void Sync_UserMovedClass_ClassUpdated()
        {
            var diStudent = _container.DiStudents.Single(s => s.Id == "4");
            diStudent.IntegratorClassId = "3";
            diStudent.ClassName = "Class 3";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "4" && u.UserId == 4 && u.EnrolmentSummary.ClassId == 3),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public void Sync_UserMovedYearGroup_YearGroupUpdated()
        {
            var diStudent = _container.DiStudents.Single(s => s.Id == "4");
            diStudent.IntegratorYearGroupId = "2";
            diStudent.YearGroupName = "Year 2";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "4" && u.UserId == 4 && u.EnrolmentSummary.YearGroupId == 2),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public void Sync_PupilNotReturnedFromIntegrator_PupilUnsynced()
        {
            var student = _container.Students.Single(s => s.UserId == 10);
            var diStudent = _container.DiStudents.Single(s => s.Id == "10");
            _container.DiStudents.Remove(diStudent);

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "10" && u.UserId == 10 && u.EnrolmentSummary.YearGroupId == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once);
            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "10" && u.UserId == 10 && u.EnrolmentSummary.ClassId == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public void Sync_PupilPremiumSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").PupilPremium = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.PupilPremium == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_PupilPremiumRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes {PupilPremium = true};
            _container.DiStudents.Single(s => s.Id == "4").PupilPremium = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.PupilPremium == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_ServiceChildrenSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").ServiceChildren = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.ServiceChildren == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_ServiceChildrenRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { ServiceChildren = true };
            _container.DiStudents.Single(s => s.Id == "4").ServiceChildren = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.ServiceChildren == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_FreeSchoolMealsSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").FreeSchoolMeals = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.FreeSchoolMeals == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_FreeSchoolMealsRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { FreeSchoolMeals = true };
            _container.DiStudents.Single(s => s.Id == "4").FreeSchoolMeals = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.FreeSchoolMeals == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_Ever6FreeSchoolMealsSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").Ever6FreeSchoolMeals = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.Ever6FreeSchoolMeals == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_Ever6FreeSchoolMealsRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { Ever6FreeSchoolMeals = true };
            _container.DiStudents.Single(s => s.Id == "4").Ever6FreeSchoolMeals = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.Ever6FreeSchoolMeals == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_MoreAbleSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").MoreAble = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.MoreAble == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_MoreAbleRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { MoreAble = true };
            _container.DiStudents.Single(s => s.Id == "4").MoreAble = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.MoreAble == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_TravellerStatusSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").TravellerStatus = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.TravellerStatus == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_TravellerStatusRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { TravellerStatus = true };
            _container.DiStudents.Single(s => s.Id == "4").TravellerStatus = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.TravellerStatus == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_LookedAfterSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").LookedAfter = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.LookedAfter == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_LookedAfterRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { LookedAfter = true };
            _container.DiStudents.Single(s => s.Id == "4").LookedAfter = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.LookedAfter == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_EverLookedAfterSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").EverLookedAfter = true;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.EverLookedAfter == true),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_EverLookedAfterRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { EverLookedAfter = true };
            _container.DiStudents.Single(s => s.Id == "4").EverLookedAfter = false;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.EverLookedAfter == false),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_SenSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").SenStatus = SenStatus.EducationHealthCarePlan;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.Sen == SenStatus.EducationHealthCarePlan),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_SenRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { Sen = SenStatus.EducationHealthCarePlan };
            _container.DiStudents.Single(s => s.Id == "4").SenStatus = null;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.Sen == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_SenChanged_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { Sen = SenStatus.EducationHealthCarePlan };
            _container.DiStudents.Single(s => s.Id == "4").SenStatus = SenStatus.Statement;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.Sen == SenStatus.Statement),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_HomeLanguageSet_StudentUpdated()
        {
            _container.DiStudents.Single(s => s.Id == "4").HomeLanguage = "Scottish";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.HomeLanguage == "Scottish"),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_HomeLanguageRemoved_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { HomeLanguage = "Welsh" };
            _container.DiStudents.Single(s => s.Id == "4").HomeLanguage = null;

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.HomeLanguage == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_HomeLanguageChanged_StudentUpdated()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { HomeLanguage = "Scottish" };
            _container.DiStudents.Single(s => s.Id == "4").HomeLanguage = "Welsh";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.HomeLanguage == "Welsh"),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_HomeLanguageChangedToEnglish_StudentUpdatedWithNoHomeLanguage()
        {
            _container.Students.Single(s => s.UserId == 4).StudentExtendedAttributes =
                new StudentExtendedAttributes { HomeLanguage = "Scottish" };
            _container.DiStudents.Single(s => s.Id == "4").HomeLanguage = "English";

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.UserId == 4 && u.StudentExtendedAttributes.HomeLanguage == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()), 
                Times.Once, "Correct user updated.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.IsAny<PassportUser>(),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Only one user updated");
        }

        [Test]
        public void Sync_DataIntegrationDataNotReadyException_ReturnsRetry()
        {
            _container.PassportDataIntegrationProviderMock.Setup(m => m.GetStudents(
                    It.IsAny<ChooseStudentSyncModeViewModel>(),
                    It.IsAny<IList<Class>>(), It.IsAny<IList<YearGroup>>()))
                .Throws(new DataIntegrationDataNotReadyException(string.Empty, string.Empty));

            var result = _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            Assert.IsTrue(result.Retry);
        }

        [Test]
        public void Sync_MultiplePassportUsersWithSameIntegratorId_DesyncedAndNotAddedAgain()
        {
            // Single user returned from integrator.
            _container.AddDiStudent("Dup", "DupFirst", "", "DupLast",
                "M", DateTime.Now, _container.DiYearGroups.Single(yg => yg.Id == "1").Id,
                _container.DiClasses.Single(yg => yg.Id == "1").Id, true);

            // Multiple users in passport with the same integrator id.
            _container.AddUser(1000, "Dup", "DubFirst", "", "DupMiddle",
                "M", DateTime.Now, 1, 1);
            _container.AddUser(1001, "Dup", "DubFirst", "", "DupMiddle",
                "M", DateTime.Now, 1, 1);

            var result = _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "Dup" && u.UserId == 0),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Never, "New user add but should not have been.");

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "Dup" && u.UserId == 1000 && u.EnrolmentSummary.YearGroupId == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Student not desynced year group");
            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "Dup" && u.UserId == 1001 && u.EnrolmentSummary.ClassId == null),
                    It.IsAny<ApiApplication>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()),
                Times.Once, "Student not desynced class");
        }

        [Test]
        public void Sync_StudentAddedManually_UserGivenAccessToTargetApplication()
        {
            _container.DiStudents.Add(new DiStudent
            {
                Id = "100",
                Gender = "M",
                YearGroupName = _container.DiYearGroups.Single(yg => yg.Id == "3").Name,
                IntegratorYearGroupId = _container.DiYearGroups.Single(yg => yg.Id == "3").Id,
                Upn = "100",
                CanImport = true,
                YearCode = _container.DiYearGroups.Single(yg => yg.Id == "3").YearCode,
                FirstName = "First100",
                MiddleName = "Middle100",
                LastName = "Last100",
                ClassName = _container.DiClasses.Single(yg => yg.Id == "3").Name,
                IntegratorClassId = _container.DiClasses.Single(yg => yg.Id == "3").Id,
                MisId = "mis100",
                MisDateOfBirthAsString = "2000-02-28"
            });

            _container.RequestInformationMock.SetupGet(m => m.TargetApplicationId)
                .Returns(ApiApplication.AssessmentPlus);

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "100" && u.FirstName == "First100"),
                    It.Is<ApiApplication>(a => a == ApiApplication.AssessmentPlus),
                    It.IsAny<bool>(),
                    It.Is<bool>(grantAccess => grantAccess == true),
                    It.IsAny<bool>()),
                Times.Once, "User given access to target application.");
        }

        [Test]
        public void Sync_StudentAddedDuringAutomatedSync_UserNotGivenAccessToPassport()
        {
            _container.DiStudents.Add(new DiStudent
            {
                Id = "100",
                Gender = "M",
                YearGroupName = _container.DiYearGroups.Single(yg => yg.Id == "3").Name,
                IntegratorYearGroupId = _container.DiYearGroups.Single(yg => yg.Id == "3").Id,
                Upn = "100",
                CanImport = true,
                YearCode = _container.DiYearGroups.Single(yg => yg.Id == "3").YearCode,
                FirstName = "First100",
                MiddleName = "Middle100",
                LastName = "Last100",
                ClassName = _container.DiClasses.Single(yg => yg.Id == "3").Name,
                IntegratorClassId = _container.DiClasses.Single(yg => yg.Id == "3").Id,
                MisId = "mis100",
                MisDateOfBirthAsString = "2000-02-28"
            });

            _container.RequestInformationMock.SetupGet(m => m.TargetApplicationId)
                .Returns(ApiApplication.Passport);

            _container.StudentDataIntegrator.Sync(_container.DataIntegratorConnection);

            _container.UserAccountServiceMock.Verify(
                m => m.SaveUser(It.Is<PassportUser>(u => u.IntegratorId == "100" && u.FirstName == "First100"),
                    It.Is<ApiApplication>(a => a == ApiApplication.Passport),
                    It.IsAny<bool>(),
                    It.Is<bool>(grantAccess => grantAccess == false),
                    It.IsAny<bool>()),
                Times.Once, "User not given access to Passport.");
        }
    }
}