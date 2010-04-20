﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using FluentNHibernate.Infrastructure;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.MappingModel.Identity;
using FluentNHibernate.Utils;
using Machine.Specifications;

namespace FluentNHibernate.Specs
{
    public static class Extensions
    {
        public static T As<T>(this object instance)
        {
            return (T)instance;
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            collection.Any(predicate).ShouldBeTrue();
        }

        public static IEnumerable<HibernateMapping> BuildMappings(this IPersistenceInstructions instructions)
        {
            return new MappingCompiler(new AutomapperV2(new ConventionFinder(instructions.Conventions)), instructions)
                .BuildMappings();
        }

        public static ClassMapping GetClassMapping(this IProvider provider)
        {
            var instructions = new PersistenceInstructions();

            instructions.AddActions(provider.GetAction());

            return instructions.BuildMappings()
                .SelectMany(x => x.Classes)
                .First();
        }

        public static ClassMapping BuildMappingFor<T>(this MappingCompiler compiler)
        {
            return compiler.BuildMappings()
                .SelectMany(x => x.Classes)
                .FirstOrDefault(x => x.Type == typeof(T));
        }

        public static void AddActions(this PersistenceInstructions instructions, params IProvider[] providers)
        {
            instructions.AddActions(providers.Select(x => x.GetAction()));
        }

        public static void AddActions(this PersistenceInstructions instructions, params IMappingAction[] actions)
        {
            instructions.AddActions(actions);
        }

        class StubProvider : IProvider
        {
            readonly IMappingAction action;

            public StubProvider(IMappingAction action)
            {
                this.action = action;
            }

            public IMappingAction GetAction()
            {
                return action;
            }
        }
    }

    public static class MappingAssertions
    {
        public static ClassMappingTester<T> For<T>(this ClassMapping mapping)
        {
            return new ClassMappingTester<T>(mapping);
        }

        public class IdMappingTester<T>
        {
            readonly IIdentityMapping mapping;

            public IdMappingTester(IIdentityMapping mapping)
            {
                this.mapping = mapping;
            }

            public IdMappingTester<T> Of<TMapping>()
            {
                if (!(mapping is TMapping))
                    throw new SpecificationException(string.Format("Should have id of {0} but was {1}.", typeof(TMapping).Name, mapping.GetType().Name));
                return this;
            }

            public IdMappingTester<T> ForMember(Member member)
            {
                if (mapping.As<IdMapping>().Member != member)
                    throw new SpecificationException(string.Format("Should have id with member of {0} but was {1}.", member.Name, mapping.As<IdMapping>().Name));
                return this;   
            }
        }

        public class ClassMappingTester<T>
        {
            readonly ClassMapping mapping;

            public ClassMappingTester(ClassMapping mapping)
            {
                this.mapping = mapping;
            }

            public IdMappingTester<T> ShouldHaveId(Expression<Func<T, object>> exp)
            {
                ShouldHaveId();

                return new IdMappingTester<T>(mapping.Id)
                    .Of<IdMapping>()
                    .ForMember(exp.ToMember());
            }

            public IdMappingTester<T> ShouldHaveId()
            {
                if (mapping.Id == null)
                    throw new SpecificationException(string.Format("Should have id for {0} mapping but does not.", mapping.Type.Name));
                
                return new IdMappingTester<T>(mapping.Id);
            }
        }
    }

    public static class Mapping
    {
        public static ClassMapping For<T>()
        {
            return new ClassMapping { Type = typeof(T) };
        }
    }

    public static class Action
    {
        public static PartialAutomapAction For<T>()
        {
            return new PartialAutomapAction(Mapping.For<T>());
        }
    }

    public class StubProviderSource : IProviderSource
    {
        readonly IEnumerable<IProvider> providers;

        public StubProviderSource(params IProvider[] providers)
        {
            this.providers = providers;
        }

        public StubProviderSource(IEnumerable<IProvider> providers)
        {
            this.providers = providers;
        }

        public CompilationResult Compile(IMappingCompiler mappingCompiler)
        {
            var actions = providers.Select(x => x.GetAction());
            var mappings = actions.SelectMany(x => mappingCompiler.Compile(x));

            return new CompilationResult(mappings);
        }
    }
}
