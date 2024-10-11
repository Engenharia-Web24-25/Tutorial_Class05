using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Class05.Data;
using Class05.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

namespace Class05.Controllers
{
    public class BooksController : Controller
    {
        private readonly Class05Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(Class05Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _webHostEnvironment = environment;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Book.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,CoverPhoto,Document")] BookViewModel book)
        {
            //Validate the extension of the files
            var PhotoExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var DocumentExtensions= new[] { ".pdf", ".doc", ".docx", ".epub" };

            var extension = Path.GetExtension(book.CoverPhoto.FileName).ToLower();

            if (!PhotoExtensions.Contains(extension))
            {
                ModelState.AddModelError("CoverPhoto", "Please, submit a valid image (jpg, jpeg, png, gif, bmp).");
            }
            extension = Path.GetExtension(book.Document.FileName).ToLower();
            if (!DocumentExtensions.Contains(extension))
            {
                ModelState.AddModelError("Document", "Please, submit a valid document (pdf, doc, docx, epub).");
            }
            if (ModelState.IsValid)
            {
                var newBook=new Book(); //create a new Book and populate whit fields of the book parameter
                newBook.Title= book.Title;
                newBook.CoverPhoto = book.CoverPhoto.FileName;
                newBook.Document = book.Document.FileName;

                //save the files in the corresponding folder
                // Save the CoverPhoto file in the Cover folder
                string coverFileName = Path.GetFileName(book.CoverPhoto.FileName);
                string coverFullPath = Path.Combine(_webHostEnvironment.WebRootPath, "Cover", coverFileName);

                using (var stream = new FileStream(coverFullPath, FileMode.Create))
                {
                    await book.CoverPhoto.CopyToAsync(stream);
                }
                //Save the Document file in the Documents folder
                string docFileName = Path.GetFileName(book.Document.FileName);
                string docFullPath = Path.Combine(_webHostEnvironment.WebRootPath, "Documents", docFileName);

                using (var stream = new FileStream(docFullPath, FileMode.Create))
                {
                    await book.Document.CopyToAsync(stream);
                }

                // Add the book in the database
                _context.Add(newBook);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CoverPhoto,Document")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);

                string coverFileName = Path.GetFileName(book.CoverPhoto);
                string coverFullPath = Path.Combine(_webHostEnvironment.WebRootPath, "Cover", coverFileName);
                System.IO.File.Delete(coverFullPath);

                string DocumentFileName = Path.GetFileName(book.Document);
                string DocumentFullPath = Path.Combine(_webHostEnvironment.WebRootPath, "Cover", DocumentFileName);
                System.IO.File.Delete(DocumentFullPath);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(string? id)
        {
            // 'id' is the filename

            string pathFile = Path.Combine(_webHostEnvironment.WebRootPath, "Documents", id);
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathFile);

            string? mimeType;
            //this code assumes that content type is always obtained.
            //Otherwise, the result should be verified (boolean value)

            if(new FileExtensionContentTypeProvider().TryGetContentType(id, out mimeType) == false)
            { 
                mimeType = "application/force-download"; //if not find the mimeType, use "application/force-download" 
            }

            return File(fileBytes, mimeType);
            // alternative way to force download of unknown mimeTypes
            //return File(fileBytes, mimeType ?? "application/force-download"); 
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.Id == id);
        }
    }
}
