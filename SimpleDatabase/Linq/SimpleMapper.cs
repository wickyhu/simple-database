using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace SimpleDatabase
{
    public class SimpleMapper : BasicMapper
    {
        public Db Db { get; private set; }

        public SimpleMapper(Db db, BasicMapping mapping, QueryTranslator translator)
            : base(mapping, translator)
        {
            Db = db;
        }

        protected override IEnumerable<EntityAssignment> GetAssignments(Expression newOrMemberInit)
        {
            var assignments = new List<EntityAssignment>();
            var minit = newOrMemberInit as MemberInitExpression;
            if (minit != null)
            {
                assignments.AddRange(minit.Bindings.OfType<MemberAssignment>().Select(a => new EntityAssignment(a.Member, a.Expression)));
                newOrMemberInit = minit.NewExpression;
            }
            //var nex = newOrMemberInit as NewExpression;
            //if (nex != null && nex.Members != null)
            //{
            //    assignments.AddRange(
            //        Enumerable.Range(0, nex.Arguments.Count)
            //                  .Where(i => nex.Members[i] != null)
            //                  .Select(i => new EntityAssignment(nex.Members[i], nex.Arguments[i]))
            //                  );
            //}
            return assignments;
        }
       
        protected Expression BuildEntityExpression(Type entityType, IList<EntityAssignment> assignments)
        {
            NewExpression newExpression = null;

            // handle cases where members are not directly assignable            
            ConstructorInfo[] cons = entityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            bool hasNoArgConstructor = cons.Any(c => c.GetParameters().Length == 0);

            if (!hasNoArgConstructor)
            {
                bool found = false;
                if (cons != null)
                {
                    foreach (ConstructorInfo c in cons)
                    {
                        var r = this.BindConstructor(c, assignments);
                        if (r.Remaining.Count < assignments.Count)
                        {
                            found = true;
                            newExpression = r.Expression;
                            //assignments = r.Remaining;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    throw new InvalidOperationException(string.Format("Cannot construct type '{0}' with all mapped includedMembers.", entityType));
                }
            }
            else
            {
                newExpression = Expression.New(entityType);
            }

            Expression result;
            if (assignments.Count > 0)
            {
                if (entityType.IsInterface)
                {
                    assignments = this.MapAssignments(assignments, entityType).ToList();
                }

                List<MemberBinding> bindings = new List<MemberBinding>();
                IEnumerable<EntityAssignment> directBindings = assignments.Where(a => a.Member.ReflectedType.Equals(entityType));
                bindings.AddRange(directBindings.Select(a => Expression.Bind(a.Member, a.Expression)).Cast<MemberBinding>());

                DbTableEntityMapping tm = Db.GetDbTableEntityMapping(entityType);
                if (tm != null && tm.EmbeddedInfos != null && tm.EmbeddedInfos.Count > 0)
                {
                    foreach (EmbeddedInfo ei in tm.EmbeddedInfos.Values)
                    {
                        Type embeddedType = ei.Member.MemberType == MemberTypes.Property ?
                            ((PropertyInfo)ei.Member).PropertyType : ((FieldInfo)ei.Member).FieldType;
                        IEnumerable<EntityAssignment> nestedBindings = assignments.Where(a => a.Member.ReflectedType.Equals(embeddedType));
                        bindings.Add(Expression.Bind(ei.Member, BuildEntityExpression(embeddedType, nestedBindings.ToList())));
                    }
                }
                result = Expression.MemberInit(newExpression,
                    bindings.ToArray()
                    );
            }
            else
            {
                result = newExpression;
            }

            return result;
        }
        

        protected override Expression BuildEntityExpression(MappingEntity entity, IList<EntityAssignment> assignments)
        {
            Expression result = BuildEntityExpression(entity.EntityType, assignments);

            if (entity.ElementType != entity.EntityType)
            {
                result = Expression.Convert(result, entity.ElementType);
            }

            return result;
        }

    }
}
