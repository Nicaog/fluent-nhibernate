﻿using System;
using FluentNHibernate.Infrastructure;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;

namespace FluentNHibernate.Automapping.Steps
{
    public class ReferenceStep : IAutomappingStep
    {
        private readonly Func<Member, bool> findPropertyconvention = p => (
            p.PropertyType.Namespace != "System" && // ignore clr types (won't be entities)
            p.PropertyType.Namespace != "System.Collections.Generic" &&
            p.PropertyType.Namespace != "Iesi.Collections.Generic" &&
	    !p.PropertyType.IsEnum);

        public bool ShouldMap(AutomappingTarget target, Member member)
        {
            if (member.CanWrite)
                return findPropertyconvention(member);

            return false;
        }

        public IMemberMapping Map(AutomappingTarget target, Member member)
        {
            return CreateMapping(member);
        }

        private ManyToOneMapping CreateMapping(Member property)
        {
            var mapping = new ManyToOneMapping { Member = property };

            mapping.SetDefaultValue(x => x.Name, property.Name);
            mapping.SetDefaultValue(x => x.Class, new TypeReference(property.PropertyType));
            mapping.AddDefaultColumn(new ColumnMapping { Name = property.Name + "_id" });

            return mapping;
        }
    }
}
