using System.Collections.Generic;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;

namespace Midwolf.GamesFramework.Services
{
    public abstract class ErrorService : IErrorService<Error>
    {
        public bool HasErrors { get; set; }
        public ICollection<Error> Errors { get; set; }

        //public virtual void AddErrorToCollection(string key, string error)
        //{
        //    if (!HasErrors) HasErrors = true;

        //    Errors.AddModelError(key, error);
        //}

        public void AddErrorToCollection(Error error)
        {
            HasErrors = true;

            if (Errors == null)
                Errors = new List<Error>();

            Errors.Add(error);
        }

        /// <summary>
        /// This method will apply updates to any IEntity type given its primary key and Dto which includes the updates.
        /// </summary>
        /// <typeparam name="TEntity">Type being updated</typeparam>
        /// <typeparam name="TDto">DTo which includes the updates</typeparam>
        /// <param name="id">Id of the Entity being updated</param>
        /// <param name="dto">Dto with updates to properties</param>
        /// <returns>The updated entity.</returns>
        //public async Task<TModel> ApplyPatchAsync<TEntity, TPatchDto, TModel>(int id, TPatchDto patchDto) 
        //    where TEntity : class, IEntity
        //    where TModel : class, IBaseDto
        //{
        //    if (patchDto == null)
        //        throw new ArgumentNullException($"{nameof(patchDto)}", $"{nameof(patchDto)} cannot be null.");

        //    var originalEntity = await _context.FindAsync(typeof(TEntity), id);

        //    // create a fresh dto
        //    var baseDto = Activator.CreateInstance(typeof(TModel), new object[] { });

        //    // map the original values from the db to the base model dto.
        //    baseDto = _mapper.Map<TModel>((TEntity)originalEntity);

        //    // map patch to the base dto
        //    baseDto = _mapper.Map(patchDto, baseDto);

        //    // map back to model and return.
        //    baseDto.UpdateDictionaries((TEntity)originalEntity);

        //    return (TModel)baseDto;
        //}
    }
}
