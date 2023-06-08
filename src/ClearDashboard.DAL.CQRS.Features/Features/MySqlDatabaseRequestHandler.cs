﻿using MediatR;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Diagnostics;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.CQRS.Features.Features
{
    /// <summary>
    /// A base class used to query data from Sqlite databases - typically found in the "Resources" folder found in the directory of the executable.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class
        MySqlDatabaseRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
        where TRequest : IRequest<TResponse>
    {
        protected int ReturnValue { get; private set; }
        protected List<CollaborationConfiguration> CollaborationConfigurations { get; set; } = new();

        protected MySqlDatabaseRequestHandler(ILogger logger) : base(logger)
        {
            //no-op
        }

        protected async Task<TData> ExecuteMySqlCommandAndProcessData(string connectionString, string commandText)
        {
            await using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT UserId,RemoteUserName,RemoteEmail,RemotePersonalAccessToken,RemotePersonalPassword,GroupName,NamespaceId"
                + " FROM dashboard.gitlabusers;";
            await using MySqlCommand command = new MySqlCommand(sql, connection);
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var userId = (int)reader.GetValue(0);
                var remoteUserName = reader.GetValue(1).ToString();
                var remoteEmail = reader.GetValue(2).ToString();
                var remotePersonalAccessToken = reader.GetValue(3).ToString();
                var remotePersonalPassword = reader.GetValue(4).ToString();
                var groupName = reader.GetValue(5).ToString();
                var namespaceId = (int)reader.GetValue(6);

                CollaborationConfigurations.Add(new CollaborationConfiguration
                {
                    UserId = userId,
                    RemoteUserName = remoteUserName,
                    RemoteEmail = remoteEmail,
                    RemotePersonalAccessToken = remotePersonalAccessToken,
                    RemotePersonalPassword = remotePersonalPassword,
                    Group = groupName,
                    NamespaceId = namespaceId
                });
            }

            return ProcessData();
        }


        protected async Task<TData> ExecuteMySqlCommand(string connectionString,
            int userId,
            string remoteUserName,
            string remoteEmail,
            string remotePersonalAccessToken,
            string remotePersonalPassword,
            string group,
            int namespaceId)
        {
            try
            {
                await using MySqlConnection connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using MySqlCommand command = new MySqlCommand("INSERT INTO gitlabusers"
                    + " (UserId, RemoteUserName, RemoteEmail, RemotePersonalAccessToken, RemotePersonalPassword, GroupName, NamespaceId)"
                    + $" VALUES ({userId}, \"{remoteUserName}\", \"{remoteEmail}\", \"{remotePersonalAccessToken}\",\"{remotePersonalPassword}\",\"{group}\",{namespaceId});", connection);

                ReturnValue = await command.ExecuteNonQueryAsync();

                //if (ret != 0)
                //{
                //    return true;
                //}
                //else
                //{

                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return ProcessData();
        }
    }
}
