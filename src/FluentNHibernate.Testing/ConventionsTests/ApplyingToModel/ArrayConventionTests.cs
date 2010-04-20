using System;
using System.Linq;
using FluentNHibernate.Automapping.TestFixtures;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers.Builders;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Infrastructure;
using FluentNHibernate.Mapping;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Testing.FluentInterfaceTests;
using NUnit.Framework;

namespace FluentNHibernate.Testing.ConventionsTests.ApplyingToModel
{
    [TestFixture]
    public class ArrayConventionTests
    {
        ConventionsCollection conventions;

        [SetUp]
        public void CreatePersistenceModel()
        {
            conventions = new ConventionsCollection();
        }

        [Test]
        public void ShouldSetAccessProperty()
        {
            Convention(x => x.Access.Property());

            VerifyModel(x => x.Access.ShouldEqual("property"));
        }

        [Test]
        public void ShouldSetBatchSizeProperty()
        {
            Convention(x => x.BatchSize(100));

            VerifyModel(x => x.BatchSize.ShouldEqual(100));
        }

        [Test]
        public void ShouldSetCacheProperty()
        {
            Convention(x => x.Cache.ReadOnly());

            VerifyModel(x => x.Cache.Usage.ShouldEqual("read-only"));
        }

        [Test]
        public void ShouldSetCascadeProperty()
        {
            Convention(x => x.Cascade.None());

            VerifyModel(x => x.Cascade.ShouldEqual("none"));
        }

        [Test]
        public void ShouldSetCheckProperty()
        {
            Convention(x => x.Check("xxx"));

            VerifyModel(x => x.Check.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetCollectionTypeProperty()
        {
            Convention(x => x.CollectionType<string>());

            VerifyModel(x => x.CollectionType.GetUnderlyingSystemType().ShouldEqual(typeof(string)));
        }

        [Test]
        public void ShouldSetFetchProperty()
        {
            Convention(x => x.Fetch.Select());

            VerifyModel(x => x.Fetch.ShouldEqual("select"));
        }

        [Test]
        public void ShouldSetGenericProperty()
        {
            Convention(x => x.Generic());

            VerifyModel(x => x.Generic.ShouldEqual(true));
        }

        [Test]
        public void ShouldSetInverseProperty()
        {
            Convention(x => x.Inverse());

            VerifyModel(x => x.Inverse.ShouldEqual(true));
        }

        [Test]
        public void ShouldSetLazyProperty()
        {
            Convention(x => x.LazyLoad());

            VerifyModel(x => x.Lazy.ShouldEqual(true));
        }

        [Test]
        public void ShouldSetMutableProperty()
        {
            Convention(x => x.ReadOnly());

            VerifyModel(x => x.Mutable.ShouldEqual(false));
        }

        [Test]
        public void ShouldSetNameProperty()
        {
            Convention(x => x.Name("xxx"));

            VerifyModel(x => x.Name.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetOptimisticLockProperty()
        {
            Convention(x => x.OptimisticLock.Dirty());

            VerifyModel(x => x.OptimisticLock.ShouldEqual("dirty"));
        }

        [Test]
        public void ShouldSetPersisterProperty()
        {
            Convention(x => x.Persister<SecondCustomPersister>());

            VerifyModel(x => x.Persister.GetUnderlyingSystemType().ShouldEqual(typeof(SecondCustomPersister)));
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
            Convention(x => x.Subselect("woo"));

            VerifyModel(x => x.Subselect.ShouldEqual("woo"));
        }

        [Test]
        public void ShouldSetTableNameProperty()
        {
            Convention(x => x.Table("xxx"));

            VerifyModel(x => x.TableName.ShouldEqual("xxx"));
        }

        [Test]
        public void ShouldSetWhereProperty()
        {
            Convention(x => x.Where("y = 2"));

            VerifyModel(x => x.Where.ShouldEqual("y = 2"));
        }

        #region Helpers

        private void Convention(Action<IArrayInstance> convention)
        {
            conventions.Add(new ArrayConventionBuilder().Always(convention));
        }

        private void VerifyModel(Action<ArrayMapping> modelVerification)
        {
            var classMap = new ClassMap<ExampleParentClass>();
            classMap.Id(x => x.Id);
            var map = classMap.HasMany(x => x.Examples)
                .AsArray(x => x.Id);

            var instructions = new PersistenceInstructions();
            instructions.AddActions(classMap);
            instructions.UseConventions(conventions);

            var generatedModels = instructions.BuildMappings();
            var modelInstance = generatedModels
                .First(x => x.Classes.FirstOrDefault(c => c.Type == typeof(ExampleParentClass)) != null)
                .Classes.First()
                .Collections.First();

            modelVerification((ArrayMapping)modelInstance);
        }

        #endregion

    }
}