﻿using Models;
using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace Repositories
{
    public class DB
    {
        #region Repositories

        private static readonly Dictionary<Type, Repository> Tables = new Dictionary<Type, Repository>()
        {
            [typeof(User)] = new UsersRepository(),
            [typeof(UserType)] = new Repository<UserType>(),
            [typeof(Gender)] = new Repository<Gender>(),

            [typeof(Entry)] = new EntryRepository(),
            [typeof(RelationShip)] = new RelationShipRepository(),

            [typeof(UnverifiedEmail)] = new Repository<UnverifiedEmail>(),
            [typeof(ResetPasswordCommand)] = new Repository<ResetPasswordCommand>(),

            [typeof(Message)] = new MessageRepository(),
        };

        #endregion Repositories

        #region initialization

        static DB() => InitRepositories();

        private static void InitRepositories()
        {
            var serverPath = HostingEnvironment.MapPath(@"~/App_Data/");

            foreach (var item in Tables)
                item.Value.Init(serverPath + item.Key.Name + "s.json");
        }

        #endregion initialization

        // Get repo with T
        public static Repository<T> GetRepo<T>() => Tables.TryGetValue(typeof(T), out var repository)
            ? (Repository<T>)repository
            : throw new NullReferenceException($"No repository found for the type '{typeof(T).Name}'.");
    }
}