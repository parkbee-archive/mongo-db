using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ParkBee.MongoDb
{
    public class ReferenceClassBsonSerializer<T> : IBsonSerializer<IEnumerable<T>> where T : class, new()
    {
        private readonly Type _type;

        private readonly MemberExpression _idExpression;

        public ReferenceClassBsonSerializer(Type type, Expression<Func<T, object>> idExpression)
        {
            _type = type;
            _idExpression = (idExpression.Body is UnaryExpression unaryExpression?unaryExpression.Operand: idExpression.Body) as MemberExpression;
        }


        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IEnumerable<T> value)
        {
            var idProperty =
                typeof(T).GetProperty(_idExpression.Member.Name, BindingFlags.Instance | BindingFlags.Public);
            if (value == null)
                BsonSerializer.Serialize(context.Writer, (IEnumerable<T>)null);
            else
            {
                var ids = value.Select(e => idProperty.GetValue(e));

                BsonSerializer.Serialize(context.Writer, ids);
            }


        }

        public IEnumerable<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var idProperty =
                typeof(T).GetProperty(_idExpression.Member.Name, BindingFlags.Instance | BindingFlags.Public);

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(idProperty.PropertyType);

            var deserializeMethod =
                typeof(BsonSerializer).GetMethod(nameof(BsonSerializer.Deserialize), new []{typeof(IBsonReader),typeof(Action<BsonDeserializationContext.Builder>)} );
            var bookmark = context.Reader.GetBookmark();
            try
            {
                
                var genericMethod = deserializeMethod.MakeGenericMethod(enumerableType);
               var ids = genericMethod.Invoke(null, new object[] { context.Reader, null }) as IEnumerable<object>;
                if (ids == null)
                    return null;
            
                return ids.Select(e =>
                {
                    var emptyObject = new T();
                    idProperty.SetValue(emptyObject, e);
                    return emptyObject;
                }).ToList();
            }
            catch (TargetInvocationException e) when(e.InnerException is FormatException)
            {
                context.Reader.ReturnToBookmark(bookmark);
                var genericMethod = deserializeMethod.MakeGenericMethod(args.NominalType);
               return genericMethod.Invoke(null, new object[] { context.Reader, null }) as IEnumerable<T>;
            }
            
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, value as IEnumerable<T>);
        }

        public Type ValueType => _type;
    }
}