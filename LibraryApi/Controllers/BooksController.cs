﻿using LibraryApi.Domain;
using LibraryApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApi.Controllers
{

    public class BooksController : Controller
    {
        LibraryDataContext Context;

        public BooksController(LibraryDataContext context)
        {
            Context = context;
        }

        // Jeff says this is really cool. Maybe look at it again some day.
        [HttpPut("books/{id:int}/numberofpages")]
        public async Task<ActionResult> ChangeNumberOfPages(int id, [FromBody] int numberOfPages)
        {
            // validate the page number
            if(numberOfPages <= 0)
            {
                return BadRequest("Not a valid page number");
            }
            var book = await Context.Books
                .Where(b => b.Id == id && b.InStock)
                .SingleOrDefaultAsync();

            if (book != null)
            {
                book.NumberOfPages = numberOfPages;
                await Context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("books/{bookId:int}")]
        public async Task<ActionResult> RemoveABook(int bookId)
        {
            var bookToRemove = await Context.Books
                .Where(b => b.InStock && b.Id == bookId)
                .SingleOrDefaultAsync();

            if(bookToRemove != null)
            {
                bookToRemove.InStock = false;
                await Context.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpPost("books")]
        public async Task<ActionResult> AddABook([FromBody] PostBookCreate bookToAdd)
        {
            // Need a model for the post [FromBody]
            // Validate the data coming in.
            //     - declarative Validation
            //     - Programmatic a validation
            //     - Reutrn a 400 (Bad Request)
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Add it to the database
            var book = new Book
            {
                Title = bookToAdd.Title,
                Author = bookToAdd.Author,
                Genre = bookToAdd.Genre,
                NumberOfPages = bookToAdd.NumberOfPages,
                InStock = true
            };
            Context.Books.Add(book);
            await Context.SaveChangesAsync();
            // Return (if the post is to a collection)
            //     - a 201 Created status code.
            //     - add a location header to the response. Location: http://localhost:1337/books/3
            //     - add an entity to the response that is EXACTLY what they's get if they followed
            //      the location header.
            var response = new GetABookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                NumberOfPages = book.NumberOfPages
            };
            return CreatedAtRoute("books#getabook", new { bookId = response.Id }, response);
        }

        /// <summary>
        /// Retrieve one of our books
        /// </summary>
        /// <param name="bookId">The Id of the book you want to find</param>
        /// <returns>A book</returns>
        [HttpGet("books/{bookId:int}", Name = "books#getabook")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetABookResponse>> GetABook(int bookId)
        {
            var book = await Context.Books
                .Where(b => b.InStock && b.Id == bookId)
                .Select(b => new GetABookResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    NumberOfPages = b.NumberOfPages
                }).SingleOrDefaultAsync();

            if(book == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(book);
            }
        }

        [HttpGet("books")]
        public async Task<ActionResult<GetABookResponse>> GetAllBooks([FromQuery] string genre)
        {
            var books = Context.Books
                .Where(b => b.InStock)
                .Select(b => new GetBooksResponseItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    NumberOfPages = b.NumberOfPages
                });

            if(genre != null)
            {
                books = books.Where(b => b.Genre == genre);
            }

            var booksList = await books.ToListAsync();

            var response = new GetBooksResponse
            {
                Books = booksList,
                GenreFilter = genre,
                NumberOfBooks = booksList.Count
            };
            return Ok(response);
        }
    }
}
