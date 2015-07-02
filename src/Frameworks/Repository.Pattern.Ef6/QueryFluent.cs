using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Repository.Pattern.Infrastructure;
using Repository.Pattern.Repositories;

namespace Repository.Pattern.Ef6
{
    public sealed class QueryFluent<TEntity> : IQueryFluent<TEntity> where TEntity : class, IObjectState
    {
        #region Private Fields
        private readonly Expression<Func<TEntity, bool>> _expression;
        private readonly List<Expression<Func<TEntity, object>>> _includes;
        private readonly Repository<TEntity> _repository;
        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderBy;
        #endregion Private Fields

        #region Constructors
        public QueryFluent(Repository<TEntity> repository)
        {
            _repository = repository;
            _includes = new List<Expression<Func<TEntity, object>>>();
        }

        public QueryFluent(Repository<TEntity> repository, IQueryObject<TEntity> queryObject) : this(repository) { _expression = queryObject.Query(); }

        public QueryFluent(Repository<TEntity> repository, Expression<Func<TEntity, bool>> expression) : this(repository) { _expression = expression; }
        #endregion Constructors

        public IQueryFluent<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        public IQueryFluent<TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            _includes.Add(expression);
            return this;
        }

        public IEnumerable<TEntity> SelectPage(int page, int pageSize, out int totalCount)
        {
            totalCount = _repository.Select(_expression).Count();
            return _repository.Select(_expression, _orderBy, _includes, page, pageSize);
        }

        public IEnumerable<TEntity> Select() { return _repository.Select(_expression, _orderBy, _includes); }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector) { return _repository.Select(_expression, _orderBy, _includes).Select(selector); }

        public async Task<IEnumerable<TEntity>> SelectAsync() { return await _repository.SelectAsync(_expression, _orderBy, _includes); }

        public IQueryable<TEntity> SqlQuery(string query, params object[] parameters) { return _repository.SelectQuery(query, parameters).AsQueryable(); }
    }
}