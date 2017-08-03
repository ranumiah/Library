using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.Entities;
using Library.Helpers;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorCollectionsController(ILibraryRepository libraryRepository, ILogger<AuthorsController> logger)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection(
            [FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);

            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author collection failed on save.");
            }

            var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",",
                authorCollectionToReturn.Select(a => a.Id));

            _logger.LogInformation("Creating an author collection completed");

            return CreatedAtRoute("GetAuthorCollection",
                new { ids = idsAsString },
                authorCollectionToReturn);
        }

        // Composite Key ==> api/authorcollections/(key1=value1,key2=value2, ...)
        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            _logger.LogInformation("Attempting to get author collection using composite keys");

            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }
    }
}