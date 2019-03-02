using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Midwolf.GamesFramework.Services.Interfaces
{
    /// <summary>
    /// Error service interface can be setup to collect any type of Errors.
    /// </summary>
    /// <typeparam name="T">Type used for collecting errors.</typeparam>
    public interface IErrorService<T> where T : class
    {
        bool HasErrors { get; set; }

        ICollection<T> Errors { get; set; }

        void AddErrorToCollection(T error);

        //Task<TModel> ApplyPatchAsync<TEntity, TPatchDto, TModel>(int id, TPatchDto dto)
        //    where TEntity : class, IEntity
        //    where TModel : class, IBaseDto;
    }
}
