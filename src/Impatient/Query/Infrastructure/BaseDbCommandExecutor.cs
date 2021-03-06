﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Impatient.Query.Infrastructure
{
    public abstract class BaseDbCommandExecutor : IDbCommandExecutor
    {
        public IEnumerable<TElement> ExecuteEnumerable<TElement>(Action<DbCommand> initializer, Func<DbDataReader, TElement> materializer)
        {
            using (var connection = GetDbConnection())
            using (var command = connection.CreateCommand())
            {
                initializer(command);

                OnCommandInitialized(command);

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return materializer(reader);
                    }
                }
            }
        }

        public TResult ExecuteComplex<TResult>(Action<DbCommand> initializer, Func<DbDataReader, TResult> materializer)
        {
            using (var connection = GetDbConnection())
            using (var command = connection.CreateCommand())
            {
                initializer(command);

                OnCommandInitialized(command);

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    // - 'DefaultIfEmpty' and '___OrDefault' behavior currently relying on the queries for:
                    //   - The default value, through the materializer
                    //   - Exceptions for First/Single/SingleOrDefault/Last/ElementAt

                    reader.Read();

                    return materializer(reader);
                }
            }
        }

        public TResult ExecuteScalar<TResult>(Action<DbCommand> initializer)
        {
            using (var connection = GetDbConnection())
            using (var command = connection.CreateCommand())
            {
                initializer(command);

                OnCommandInitialized(command);

                connection.Open();

                return (TResult)command.ExecuteScalar();
            }
        }

        protected abstract DbConnection GetDbConnection();

        protected virtual void OnCommandInitialized(DbCommand command)
        {
        }
    }
}
