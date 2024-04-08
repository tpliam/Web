﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public int PageSize = 9;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public class PriceRanges
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }
        public IActionResult GetFilteredProducts([FromBody] FilterData filter)
        {
            var filteredProducts = _context.Products.ToList();

            if (filter.PriceRanges != null && filter.PriceRanges.Count > 0 && !filter.PriceRanges.Contains("all"))
            {
                List<PriceRanges> priceRanges = new List<PriceRanges>();
                foreach (var range in filter.PriceRanges)
                {
                    var value = range.Split("-").ToArray();
                    PriceRanges priceRange = new PriceRanges();
                    priceRange.Min = Int32.Parse(value[0]);
                    priceRange.Max = Int32.Parse(value[1]);
                    priceRanges.Add(priceRange);
                }
                filteredProducts = filteredProducts.Where(p => priceRanges.Any(r => p.ProductPrice >= r.Min && p.ProductPrice <= r.Max)).ToList();
            }

            if (filter.Colors != null && filter.Colors.Count > 0 && !filter.Colors.Contains("all"))
            {
                filteredProducts = filteredProducts.Where(p => filter.Colors.Contains(p.Color.ColorName)).ToList();
            }

            if (filter.Sizes != null && filter.Sizes.Count > 0 && !filter.Sizes.Contains("all"))
            {
                filteredProducts = filteredProducts.Where(p => filter.Sizes.Contains(p.Size.SizeName)).ToList();
            }

            return PartialView("_ReturnProducts", filteredProducts);
        }


        // GET: Products
        public async Task<IActionResult> Index(int productPage=1)
        {
            return View(
                new ProductListViewModel
                {
                    Products = _context.Products
                    .Skip((productPage-1)*PageSize)
                    .Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = productPage,
                        TotalItems = _context.Products.Count()
                    }
                }
                );
        }
        [HttpPost]
        public async Task<IActionResult> Search(string keywords, int productPage = 1)
        {
            return View("Index",
                new ProductListViewModel
                {
                    Products = _context.Products
                    .Where(p=>p.ProductName.Contains(keywords))
                    .Skip((productPage - 1) * PageSize)
                    .Take(PageSize),
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = productPage,
                        TotalItems = _context.Products.Count()
                    }
                }
                );
        }

        public async Task<IActionResult> ProductsByCart(int categoryId)
        {
            var applicationDbContext = _context.Products.Where(p=>p.CategoryId == categoryId).Include(p => p.Category).Include(p => p.Color).Include(p => p.Size);
            return View("Index",await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Size)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorName");
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,CategoryId,ProductDescription,ProductPrice,ProductDiscount,ProductPhoto,SizeId,ColorId,IsTrendy,IsArrived")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorName", product.ColorId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeName", product.SizeId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorName", product.ColorId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeName", product.SizeId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,CategoryId,ProductDescription,ProductPrice,ProductDiscount,ProductPhoto,SizeId,ColorId,IsTrendy,IsArrived")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorName", product.ColorId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeName", product.SizeId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Size)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
