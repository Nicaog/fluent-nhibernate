using System;
using System.Linq;
using FluentNHibernate.Automapping.TestFixtures;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers.Builders;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Infrastructure;
using FluentNHibernate.Mapping;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.Testing.FluentInterfaceTests;
using NUnit.Framework;

namespace FluentNHibernate.Testing.ConventionsTests.ApplyingToModel
{
    [TestFixture]
    public class JoinConventionTests
    {
        ConventionsCollection conventions;

        [SetUp]
        public void CreatePersistenceModel()
        {
            conventions = new ConventionsCollection();
        }

        [Test]
        public void ShouldSetTableProperty()
        {
            Convention(x => x.Table("xxx"));

            VerifyModel(x => x.TableName.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetSchemaProperty()
        {
            Convention(x => x.Schema("xxx"));

            VerifyModel(x => x.Schema.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetSubselectProperty()
        {
            Convention(x => x.Subselect("xxx"));

            VerifyModel(x => x.Subselect.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetFetchProperty()
        {
            Convention(x => x.Fetch.Select());

            VerifyModel(x => x.Fetch.ShouldEqual("select"));
        }

        [Test]
        public void ShouldSetInverseProperty()
        {
            Convention(x => x.Inverse());

            VerifyModel(x => x.Inverse.ShouldBeTrue());
        }

        [Test]
        public void ShouldSetOptionalProperty()
        {
            Convention(x => x.Optional());

            VerifyModel(x => x.Optional.ShouldBeTrue());
        }

        #region Helpers

        private void Convention(Action<IJoinInstance> convention)
        {
            conventions.Add(new JoinConventionBuilder().Always(convention));
        }

        private void VerifyModel(Action<JoinMapping> modelVerification)
        {
            var classMap = new ClassMap<ExampleClass>();
            classMap.Id(x => x.Id);
            classMap.Join("table", m => {});

            var instructions = new PersistenceInstructions();
            instructions.AddActions(classMap);
            instructions.UseConventions(conventions);

            var generatedModels = instructions.BuildMappings();
            var modelInstance = generatedModels
                .First(x => x.Classes.FirstOrDefault(c => c.Type == typeof(ExampleClass)) != null)
                .Classes.First()
                .Joins.First();

            modelVerification(modelInstance);
        }

        #endregion
    }
}