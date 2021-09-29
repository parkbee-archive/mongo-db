using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ParkBee.MongoDb
{
    public class DbSet<TEntity> : IMongoCollection<TEntity>, IMongoQueryable<TEntity>
        where TEntity : class
    {
        private readonly IMongoCollection<TEntity> _collection;
        private readonly IMongoContextOptionsBuilder _optionsBuilder;
        private readonly MongoContext _context;

        public DbSet(IMongoContextOptionsBuilder optionsBuilder, MongoContext context,
            IMongoCollection<TEntity> collection)
        {
            _optionsBuilder = optionsBuilder;
            _context = context;
            _collection = collection;
        }

        public Task<TEntity> FindByKey(object keyValue, FindOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var findByKeyDefinition = GetFindByKeyFilterDefinition(keyValue);
            return _collection.Find(findByKeyDefinition, options).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<UpdateResult> UpdateByKey(object keyValue, UpdateDefinition<TEntity> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var findByKeyDefinition = GetFindByKeyFilterDefinition(keyValue);
            return _collection.UpdateOneAsync(findByKeyDefinition, update, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceByKey(object keyValue, TEntity replacement, ReplaceOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var findByKeyDefinition = GetFindByKeyFilterDefinition(keyValue);
            return _collection.ReplaceOneAsync(findByKeyDefinition, replacement, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteByKey(object keyValue, DeleteOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var findByKeyDefinition = GetFindByKeyFilterDefinition(keyValue);
            return _collection.DeleteOneAsync(findByKeyDefinition, options, cancellationToken);
        }

        private FilterDefinition<TEntity> GetFindByKeyFilterDefinition(object keyValue)
        {
            var memberExpression = _optionsBuilder.GetFilterByKeyExpression<TEntity>();
            var parameterExpression = Expression.Parameter(typeof(TEntity), "e");
            var left = Expression.Property(parameterExpression, memberExpression.Member.Name);
            var right = Expression.Constant(keyValue, keyValue.GetType());

            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(left, right), parameterExpression
            );
            return new ExpressionFilterDefinition<TEntity>(predicate);
        }

        #region IMongoCollection interface implementations

        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.Aggregate(pipeline, options, cancellationToken);

        public IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session,
            PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.Aggregate(session, pipeline, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.AggregateAsync(pipeline, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session,
            PipelineDefinition<TEntity, TResult> pipeline, AggregateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.AggregateAsync(session, pipeline, options, cancellationToken);

        public void AggregateToCollection<TResult>(PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.AggregateToCollection(pipeline, options, cancellationToken);

        public void AggregateToCollection<TResult>(IClientSessionHandle session,
            PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.AggregateToCollection(session, pipeline, options, cancellationToken);

        public Task AggregateToCollectionAsync<TResult>(PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.AggregateToCollectionAsync(pipeline, options, cancellationToken);

        public Task AggregateToCollectionAsync<TResult>(IClientSessionHandle session,
            PipelineDefinition<TEntity, TResult> pipeline,
            AggregateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);

        public BulkWriteResult<TEntity> BulkWrite(IEnumerable<WriteModel<TEntity>> requests,
            BulkWriteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.BulkWrite(requests, options, cancellationToken);

        public BulkWriteResult<TEntity> BulkWrite(IClientSessionHandle session,
            IEnumerable<WriteModel<TEntity>> requests, BulkWriteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.BulkWrite(session, requests, options, cancellationToken);

        public Task<BulkWriteResult<TEntity>> BulkWriteAsync(IEnumerable<WriteModel<TEntity>> requests,
            BulkWriteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.BulkWriteAsync(requests, options, cancellationToken);

        public Task<BulkWriteResult<TEntity>> BulkWriteAsync(IClientSessionHandle session,
            IEnumerable<WriteModel<TEntity>> requests, BulkWriteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.BulkWriteAsync(session, requests, options, cancellationToken);

        public long Count(FilterDefinition<TEntity> filter, CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.Count(filter, options, cancellationToken);

        public long Count(IClientSessionHandle session, FilterDefinition<TEntity> filter, CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.Count(session, filter, options, cancellationToken);

        public Task<long> CountAsync(FilterDefinition<TEntity> filter, CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountAsync(filter, options, cancellationToken);

        public Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountAsync(session, filter, options, cancellationToken);

        public long CountDocuments(FilterDefinition<TEntity> filter, CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountDocuments(filter, options, cancellationToken);

        public long CountDocuments(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountDocuments(session, filter, options, cancellationToken);

        public Task<long> CountDocumentsAsync(FilterDefinition<TEntity> filter, CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountDocumentsAsync(filter, options, cancellationToken);

        public Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            CountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.CountDocumentsAsync(session, filter, options, cancellationToken);

        public DeleteResult
            DeleteMany(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) =>
            _collection.DeleteMany(filter, cancellationToken);

        public DeleteResult DeleteMany(FilterDefinition<TEntity> filter, DeleteOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteMany(filter, options, cancellationToken);

        public DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            DeleteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteMany(session, filter, options, cancellationToken);

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter,
            CancellationToken cancellationToken = default) => _collection.DeleteManyAsync(filter, cancellationToken);

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter, DeleteOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteManyAsync(filter, options, cancellationToken);

        public Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            DeleteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteManyAsync(session, filter, options, cancellationToken);

        public DeleteResult
            DeleteOne(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) =>
            _collection.DeleteOne(filter, cancellationToken);

        public DeleteResult DeleteOne(FilterDefinition<TEntity> filter, DeleteOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteOne(filter, options, cancellationToken);

        public DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            DeleteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteOne(session, filter, options, cancellationToken);

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter,
            CancellationToken cancellationToken = default) => _collection.DeleteOneAsync(filter, cancellationToken);

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter, DeleteOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteOneAsync(filter, options, cancellationToken);

        public Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            DeleteOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.DeleteOneAsync(session, filter, options, cancellationToken);

        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<TEntity, TField> field,
            FilterDefinition<TEntity> filter, DistinctOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.Distinct(field, filter, options, cancellationToken);

        public IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session,
            FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter,
            DistinctOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.Distinct(session, field, filter, options, cancellationToken);

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TEntity, TField> field,
            FilterDefinition<TEntity> filter, DistinctOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.DistinctAsync(field, filter, options, cancellationToken);

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(IClientSessionHandle session,
            FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter,
            DistinctOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.DistinctAsync(session, field, filter, options, cancellationToken);

        public long EstimatedDocumentCount(EstimatedDocumentCountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.EstimatedDocumentCount(options, cancellationToken);

        public Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.EstimatedDocumentCountAsync(options, cancellationToken);

        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TEntity> filter,
            FindOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindSync(filter, options, cancellationToken);

        public IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter, FindOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindSync(session, filter, options, cancellationToken);

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TEntity> filter,
            FindOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindAsync(filter, options, cancellationToken);

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter, FindOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindAsync(session, filter, options, cancellationToken);

        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<TEntity> filter,
            FindOneAndDeleteOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndDelete(filter, options, cancellationToken);

        public TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            FindOneAndDeleteOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndDelete(session, filter, options, cancellationToken);

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TEntity> filter,
            FindOneAndDeleteOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndDeleteAsync(filter, options, cancellationToken);

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter,
            FindOneAndDeleteOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);

        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<TEntity> filter, TEntity replacement,
            FindOneAndReplaceOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndReplace(filter, replacement, options, cancellationToken);

        public TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter, TEntity replacement,
            FindOneAndReplaceOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndReplace(session, filter, replacement, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TEntity> filter,
            TEntity replacement,
            FindOneAndReplaceOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter, TEntity replacement,
            FindOneAndReplaceOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken);
        }

        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            FindOneAndUpdateOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndUpdate(filter, update, options, cancellationToken);

        public TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update, FindOneAndUpdateOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndUpdate(session, filter, update, options, cancellationToken);

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            FindOneAndUpdateOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(IClientSessionHandle session,
            FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update,
            FindOneAndUpdateOptions<TEntity, TProjection> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);

        public void InsertOne(TEntity document, InsertOneOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertOne(document, options, cancellationToken);

        public void InsertOne(IClientSessionHandle session, TEntity document, InsertOneOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertOne(session, document, options, cancellationToken);

        public Task InsertOneAsync(TEntity document, CancellationToken _cancellationToken) =>
            _collection.InsertOneAsync(document, _cancellationToken);

        public Task InsertOneAsync(TEntity document, InsertOneOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertOneAsync(document, options, cancellationToken);

        public Task InsertOneAsync(IClientSessionHandle session, TEntity document, InsertOneOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertOneAsync(session, document, options, cancellationToken);

        public void InsertMany(IEnumerable<TEntity> documents, InsertManyOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertMany(documents, options, cancellationToken);

        public void InsertMany(IClientSessionHandle session, IEnumerable<TEntity> documents,
            InsertManyOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertMany(session, documents, options, cancellationToken);

        public Task InsertManyAsync(IEnumerable<TEntity> documents, InsertManyOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertManyAsync(documents, options, cancellationToken);

        public Task InsertManyAsync(IClientSessionHandle session, IEnumerable<TEntity> documents,
            InsertManyOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.InsertManyAsync(session, documents, options, cancellationToken);

        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce,
            MapReduceOptions<TEntity, TResult> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.MapReduce(map, reduce, options, cancellationToken);

        public IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<TEntity, TResult> options = null, CancellationToken cancellationToken = default) =>
            _collection.MapReduce(session, map, reduce, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce,
            MapReduceOptions<TEntity, TResult> options = null,
            CancellationToken cancellationToken = default) =>
            _collection.MapReduceAsync(map, reduce, options, cancellationToken);

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(IClientSessionHandle session, BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<TEntity, TResult> options = null, CancellationToken cancellationToken = default) =>
            _collection.MapReduceAsync(session, map, reduce, options, cancellationToken);

        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : TEntity =>
            _collection.OfType<TDerivedDocument>();

        public ReplaceOneResult ReplaceOne(FilterDefinition<TEntity> filter, TEntity replacement,
            ReplaceOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.ReplaceOne(filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(FilterDefinition<TEntity> filter, TEntity replacement, UpdateOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.ReplaceOne(filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            TEntity replacement,
            ReplaceOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.ReplaceOne(session, filter, replacement, options, cancellationToken);

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            TEntity replacement,
            UpdateOptions options, CancellationToken cancellationToken = default) =>
            _collection.ReplaceOne(session, filter, replacement, options, cancellationToken);

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TEntity> filter, TEntity replacement,
            ReplaceOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TEntity> filter, TEntity replacement,
            UpdateOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);

        public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            TEntity replacement,
            ReplaceOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);

        public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            TEntity replacement, UpdateOptions options,
            CancellationToken cancellationToken = default) =>
            _collection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);

        public UpdateResult UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.UpdateMany(filter, update, options, cancellationToken);

        public UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            UpdateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.UpdateMany(session, filter, update, options, cancellationToken);

        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.UpdateManyAsync(filter, update, options, cancellationToken);

        public Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            UpdateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.UpdateManyAsync(session, filter, update, options, cancellationToken);

        public UpdateResult UpdateOne(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.UpdateOne(filter, update, options, cancellationToken);

        public UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            UpdateOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.UpdateOne(session, filter, update, options, cancellationToken);

        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update,
            UpdateOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.UpdateOneAsync(filter, update, options, cancellationToken);

        public Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
            UpdateDefinition<TEntity> update,
            UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _collection.UpdateOneAsync(session, filter, update, options, cancellationToken);
        }

        public IChangeStreamCursor<TResult> Watch<TResult>(
            PipelineDefinition<ChangeStreamDocument<TEntity>, TResult> pipeline, ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.Watch(pipeline, options, cancellationToken);

        public IChangeStreamCursor<TResult> Watch<TResult>(IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<TEntity>, TResult> pipeline,
            ChangeStreamOptions options = null, CancellationToken cancellationToken = default) =>
            _collection.Watch(session, pipeline, options, cancellationToken);

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
            PipelineDefinition<ChangeStreamDocument<TEntity>, TResult> pipeline, ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.WatchAsync(pipeline, options, cancellationToken);

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<TEntity>, TResult> pipeline, ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default) =>
            _collection.WatchAsync(session, pipeline, options, cancellationToken);

        public IMongoCollection<TEntity> WithReadConcern(ReadConcern readConcern) =>
            _collection.WithReadConcern(readConcern);

        public IMongoCollection<TEntity> WithReadPreference(ReadPreference readPreference) =>
            _collection.WithReadPreference(readPreference);

        public IMongoCollection<TEntity> WithWriteConcern(WriteConcern writeConcern) =>
            _collection.WithWriteConcern(writeConcern);

        public CollectionNamespace CollectionNamespace => _collection.CollectionNamespace;
        public IMongoDatabase Database => _collection.Database;
        public IBsonSerializer<TEntity> DocumentSerializer => _collection.DocumentSerializer;
        public IMongoIndexManager<TEntity> Indexes => _collection.Indexes;
        public MongoCollectionSettings Settings => _collection.Settings;

        #endregion

        #region IMongoQueryable Implementation

        public QueryableExecutionModel GetExecutionModel() => _collection.AsQueryable().GetExecutionModel();

        public IAsyncCursor<TEntity> ToCursor(CancellationToken cancellationToken = default)
            => _collection.AsQueryable().ToCursor(cancellationToken);

        public Task<IAsyncCursor<TEntity>> ToCursorAsync(CancellationToken cancellationToken = default)
            => _collection.AsQueryable().ToCursorAsync(cancellationToken);

        public IEnumerator<TEntity> GetEnumerator() => _collection.AsQueryable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.AsQueryable().GetEnumerator();

        public Type ElementType => _collection.AsQueryable().ElementType;
        public Expression Expression => _collection.AsQueryable().Expression;
        public IQueryProvider Provider => _collection.AsQueryable().Provider;

        #endregion
    }
}